using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ConsoleApp1;
using System.Collections.Generic;
using System.Linq;

namespace DriverSystem.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class DriverFinderBenchmarks
    {
        private List<Driver> _drivers;
        private Point _orderLocation;

        [Params(100, 1000, 5000)]
        public int DriverCount;

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(42);
            _drivers = new List<Driver>();

            for (int i = 0; i < DriverCount; i++)
            {
                int x = random.Next(0, 1000);
                int y = random.Next(0, 1000);
                _drivers.Add(new Driver($"D{i:0000}", new Point(x, y)));
            }

            _orderLocation = new Point(500, 500);
        }

        [Benchmark]
        public List<Driver> SimpleFinder()
        {
            var finder = new SimpleFinder();
            return finder.FindNearestDrivers(_drivers, _orderLocation, 5);
        }

        [Benchmark]
        public List<Driver> GridPartitionFinder()
        {
            var finder = new GridPartitionFinder(100);
            return finder.FindNearestDrivers(_drivers, _orderLocation, 5);
        }

        [Benchmark]
        public List<Driver> PriorityQueueFinder()
        {
            var finder = new PriorityQueueFinder();
            return finder.FindNearestDrivers(_drivers, _orderLocation, 5);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<DriverFinderBenchmarks>();
        }
    }
}