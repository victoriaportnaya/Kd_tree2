using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        var sw = new Stopwatch();
        sw.Start();

        string csvCoordinates = "C:\\Users\\victo\\RiderProjects\\R-tree\\R-tree\\positions.csv";

        KdTree kdTree = ConstructKdTree(csvCoordinates);

        // get user's coordinates
        Console.WriteLine("Enter your latitude:");
        double userLat = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        Console.WriteLine("Enter your longitude:");
        double userLon = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        Console.WriteLine("Enter the radius (in meters):");
        double radius = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        List<(Point, string)> placesWithinRadius = kdTree.RangeQuery(userLat, userLon, radius);

        if (placesWithinRadius.Count == 0)
        {
            Console.WriteLine("No places found within radius.");
        }
        else
        {
            Console.WriteLine("List of places within radius:");
            var nearestPoint = kdTree.NearestNeighbor(userLat, userLon);
            Console.WriteLine($"The nearest point: TYPE {nearestPoint.Item2} | LOCATION ({nearestPoint.Item1.Latitude}, {nearestPoint.Item1.Longitude})");
            foreach ((Point point, string placeType) in placesWithinRadius)
            {
                Console.WriteLine($"TYPE {placeType} | LOCATION ({point.Latitude}, {point.Longitude})");
            }
        }

        sw.Stop();
        Console.WriteLine($"Elapsed time: {sw.Elapsed}");
    }

    static KdTree ConstructKdTree(string filePath)
    {
        List<(double, double, string)> points = new List<(double, double, string)>();
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            var parts = line.Split(';');
            if (!double.TryParse(parts[0], out double latitude)) continue;
            if (!double.TryParse(parts[1], out double longitude)) continue;
            string placeType = parts[2];
            points.Add((latitude, longitude, placeType));
        }
        return new KdTree(points);
    }
}

public class Point
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Point(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}

public class KdTree
{
    private class Node
    {
        public double Latitude { get; }
        public double Longitude { get; }
        public string PlaceType { get; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public Node(double latitude, double longitude, string placeType)
        {
            Latitude = latitude;
            Longitude = longitude;
            PlaceType = placeType;
            Left = null;
            Right = null;
        }
    }

    private Node root;

    public KdTree(List<(double, double, string)> points)
    {
        root = ConstructKdTree(points, 0);
    }

    public List<(Point, string)> RangeQuery(double latitude, double longitude, double radius)
    {
        List<(Point, string)> placesWithinRadius = new List<(Point, string)>();
        RangeQuery(root, latitude, longitude, radius, 0, placesWithinRadius);
        return placesWithinRadius;
    }

    private void RangeQuery(Node node, double latitude, double longitude, double radius, int depth,
        List<(Point, string)> result)
    {
        if (node == null)
            return;

        double lat1 = latitude * Math.PI / 180.0;
        double lon1 = longitude * Math.PI / 180.0;
        double lat2 = node.Latitude * Math.PI / 180.0;
        double lon2 = node.Longitude * Math.PI / 180.0;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = 6371 * c;
        if (distance <= radius)
        {
            result.Add((new Point(node.Latitude, node.Longitude), node.PlaceType));
        }

        int cd = depth % 2;

        var abs = Math.Abs(latitude - node.Latitude); // optimize 
        if (cd == 0)
        {
            if (abs <= radius && Math.Abs(longitude - node.Longitude) <= radius)
                RangeQuery(node.Left, latitude, longitude, radius, depth + 1, result);
            if (abs <= radius && Math.Abs(longitude - node.Longitude) <= radius)
                RangeQuery(node.Right, latitude, longitude, radius, depth + 1, result);
        }
        else
        {
            if (Math.Abs(longitude - node.Longitude) <= radius && abs <= radius)
                RangeQuery(node.Left, latitude, longitude, radius, depth + 1, result);
            if (Math.Abs(longitude - node.Longitude) <= radius && abs <= radius)
                RangeQuery(node.Right, latitude, longitude, radius, depth + 1, result);
        }
    }

    public (Point, string) NearestNeighbor(double latitude, double longitude)
    {
        if (root == null)
            throw new InvalidOperationException("Kd-tree is empty!");

        Point target = new Point(latitude, longitude);
        Node nearestNode = NearestNeighbor(root, target, root, 0);

        return (new Point(nearestNode.Latitude, nearestNode.Longitude), nearestNode.PlaceType);
    }

    private Node NearestNeighbor(Node currentNode, Point target, Node currentBest, int depth)
    {
        if (currentNode == null)
            return currentBest;

        Node nextBranch = null;
        Node otherBranch = null;

        int cd = depth % 2;

        if (cd == 0)
        {
            if (target.Latitude <= currentNode.Latitude)
            {
                nextBranch = currentNode.Left;
                otherBranch = currentNode.Right;
            }
            else
            {
                nextBranch = currentNode.Right;
                otherBranch = currentNode.Left;
            }
        }
        else
        {
            if (target.Longitude <= currentNode.Longitude)
            {
                nextBranch = currentNode.Left;
                otherBranch = currentNode.Right;
            }
            else
            {
                nextBranch = currentNode.Right;
                otherBranch = currentNode.Left;
            }
        }

        currentBest = ClosestNode(target, NearestNeighbor(nextBranch, target, currentBest, depth + 1), currentBest);

        double currentBestDistance = CalculateDistance(target.Latitude, target.Longitude, currentBest.Latitude, currentBest.Longitude);

        double nodeDistance = CalculateDistance(target.Latitude, target.Longitude, currentNode.Latitude, currentNode.Longitude);

        if (nodeDistance < currentBestDistance)
            currentBest = currentNode;

        if (Math.Abs((cd == 0 ? target.Latitude - currentNode.Latitude : target.Longitude - currentNode.Longitude)) < currentBestDistance)
            currentBest = ClosestNode(target, NearestNeighbor(otherBranch, target, currentBest, depth + 1), currentBest);

        return currentBest;
    }

    private Node ConstructKdTree(List<(double, double, string)> points, int depth)
    {
        if (points == null || points.Count == 0)
        {
            return null;
        }

        int cd = depth % 2;
        points = cd == 0 ? points.OrderBy(p => p.Item1).ToList() : points.OrderBy(p => p.Item2).ToList();

        int median = points.Count / 2;

        Node node = new Node(points[median].Item1, points[median].Item2, points[median].Item3);

        node.Left = ConstructKdTree(points.GetRange(0, median), depth + 1);
        node.Right = ConstructKdTree(points.GetRange(median + 1, points.Count - (median + 1)), depth + 1);

        return node;
    }

    private Node ClosestNode(Point target, Node node1, Node node2)
    {
        if (node1 == null)
            return node2;
        if (node2 == null)
            return node1;

        double distance1 = CalculateDistance(target.Latitude, target.Longitude, node1.Latitude, node1.Longitude);
        double distance2 = CalculateDistance(target.Latitude, target.Longitude, node2.Latitude, node2.Longitude);

        return distance1 < distance2 ? node1 : node2;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadius = 6371; // Earth radius in kilometers
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = EarthRadius * c;
        return distance;
    }
}