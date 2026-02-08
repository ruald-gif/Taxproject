using System;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {

        }      
    }
    public struct Point
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public double DistanceTo(Point orderPoint)//orderPoint - точка заказа
        {
            double dx = X - orderPoint.X;
            double dy = Y - orderPoint.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
    public class Driver
    {
        public string Id { get; }//уникальный ID водителя
        public Point LocationDriver { get; set; }
        public Driver(string id, Point location)
        {
            Id = id;
            LocationDriver = location;
        }
        public double GetDistanceTo(Point orderPoint)
        {
            return LocationDriver.DistanceTo(orderPoint);
        }
    }
}