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
            eConsole.WriteLine($" TYPE {placeType} | LOCATION ({point.Latitude}, {point.Longitude}));
        }
    }
}

        
