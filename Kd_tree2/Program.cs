// download data
// console program to get user lat long rad 
// distance to the point (heap) and show if the distance is less than rad
using System;
using System.IO;
using System.Globalization;
using Microsoft.VisualBasic.CompilerServices;

class Program
{
    static void Main(string[] args)
    {
        string csvCoordinates = "C:\\Users\\victo\\RiderProjects\\R-tree\\R-tree\\positions.csv";

        string[] lines = File.ReadAllLines(csvCoordinates);

        // get user`s coordinates
        Console.WriteLine("Your latitude>>");
        double userLat = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        Console.WriteLine("Enter your longitude>>");
        double userLon = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        Console.WriteLine("Enter the radius (in km)>>");
        double radius = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

        List<Point, string> placesWithinRadius = kdTree.RangeQuery(userLat, userLon, radius);

        Console.WriteLine("LIST OF PLACES");
        foreach ((Point point, string placeType) in placesWithinRadius)
        {
            Console.WriteLine($" TYPE {placeType} | LOCATION ({point.Latitude}, {point.Longitude}));
        }


        static KdTree ConstructKdTree(string filePath)
        {
            kdTree kdTree = new KdTree();
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                var parts = line.Split(';');
                double latitude = double.Parse(parts[0], CultureInfo.InvariantCulture);
                double longitude = double.Parse(parts[1], CultureInfo.InvariantCulture);
                string placeType = parts[2];
                kdTree.Insert(latitude, longitude, placeType);
            }

            return kdTree;
        }
    }

    public class Point
    {
        public double Latitude { get;}
        public double Longitude { get;}

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
            public double Latitude { get;}
            public double Longitude { get;}
            public string PlaceType { get;}
            public Node Left { get; set;}
            public Node Right { get; set;}
            
        }

        public Node(double latitude, double longitude, string placeType)
        {
            Latitude = latitude;
            Longitude = longitude;
            PlaceType = placeType;
            Left = null;
            Right = nulll;
            
        }
    }

    private Node root;

    public KdTree()
    {
        root = null;
    }
    
    

    

        
