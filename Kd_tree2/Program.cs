using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string csvCoordinates = "C:\\Users\\victo\\RiderProjects\\R-tree\\R-tree\\positions.csv";

        KdTree kdTree = ConstructKdTree(csvCoordinates);

        // get user`s coordinates
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
            foreach ((Point point, string placeType) in placesWithinRadius)
            {
                Console.WriteLine($"TYPE {placeType} | LOCATION ({point.Latitude}, {point.Longitude})");
            }
        }
    }

    static KdTree ConstructKdTree(string filePath)
    {
        KdTree kdTree = new KdTree();
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length != 3) continue;
            if (!double.TryParse(parts[0], out double latitude)) continue;
            if (!double.TryParse(parts[1], out double longitude)) continue;
            string placeType = parts[2];
            kdTree.Insert(latitude, longitude, placeType);
        }
        return kdTree;
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

    public KdTree()
    {
        root = null;
    }

    public void Insert(double latitude, double longitude, string placeType)
    {
        root = Insert(root, latitude, longitude, placeType, 0);
    }

    private Node Insert(Node node, double latitude, double longitude, string placeType, int depth)
    {
        if (node == null)
            return new Node(latitude, longitude, placeType);
        int cd = depth % 2;

        if (cd == 0)
        {
            if (latitude < node.Latitude)
                node.Left = Insert(node.Left, latitude, longitude, placeType, depth + 1);
            else
                node.Right = Insert(node.Right, latitude, longitude, placeType, depth + 1);
        }
        else
        {
            if (longitude < node.Longitude)
                node.Left = Insert(node.Left, latitude, longitude, placeType, depth + 1);
            else
                node.Right = Insert(node.Right, latitude, longitude, placeType, depth + 1);
        }

        return node;
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

        double dlat = lat2 - lat1;
        double dlon = lon2 - lon1;

        double a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = 6371000 * c;
        if (distance <= radius)
        {
            result.Add((new Point(node.Latitude, node.Longitude), node.PlaceType));
        }

        int cd = depth % 2;

        if (cd == 0)
        {
            if (latitude - radius <= node.Latitude)
                RangeQuery(node.Left, latitude, longitude, radius, depth + 1, result);
            if (latitude + radius > node.Latitude)
                RangeQuery(node.Right, latitude, longitude, radius, depth + 1, result);
        }
        else
        {
            if (longitude - radius <= node.Longitude)
                RangeQuery(node.Left, latitude, longitude, radius, depth + 1, result);
            if (longitude + radius > node.Longitude)
                RangeQuery(node.Right, latitude, longitude, radius, depth + 1, result);
        }
    }


}
