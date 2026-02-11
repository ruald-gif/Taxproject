using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using ConsoleApp1;

namespace DriverSystem.Tests
{
    [TestFixture]
    public class AlgorithmTests
    {
        // Проверяем, что алгоритм возвращает 5 водителей
        [Test]
        public void SimpleFinder_Returns5NearestDrivers()
        {
            // Arrange
            var drivers = new List<Driver>();
            for (int i = 0; i < 100; i++)
            {
                drivers.Add(new Driver($"D{i}", new Point(i, i)));
            }
            var order = new Point(50, 50);
            var finder = new SimpleFinder();

            // Act
            var result = finder.FindNearestDrivers(drivers, order, 5);

            // Assert
            Assert.That(result.Count, Is.EqualTo(5));
        }

        // Проверяем, что ближайший водитель - правильный
        [Test]
        public void SimpleFinder_ReturnsClosestDriverFirst()
        {
            // Arrange
            var drivers = new List<Driver>
            {
                new Driver("D1", new Point(100, 100)), // далеко
                new Driver("D2", new Point(0, 0)),     // близко!
                new Driver("D3", new Point(50, 50))    // очень близко!
            };
            var order = new Point(10, 10);
            var finder = new SimpleFinder();

            // Act
            var result = finder.FindNearestDrivers(drivers, order, 2);

            // Assert
            Assert.That(result[0].Id, Is.EqualTo("D2")); // D2 ближе всех
            Assert.That(result[1].Id, Is.EqualTo("D3")); // D3 второй
        }

        // Если водителей меньше 5, возвращаем всех
        [Test]
        public void SimpleFinder_WithLessThan5Drivers_ReturnsAllDrivers()
        {
            // Arrange
            var drivers = new List<Driver>
            {
                new Driver("D1", new Point(10, 10)),
                new Driver("D2", new Point(20, 20))
            };
            var order = new Point(15, 15);
            var finder = new SimpleFinder();

            // Act
            var result = finder.FindNearestDrivers(drivers, order, 5);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // Если список пустой, возвращаем пустой список
        [Test]
        public void SimpleFinder_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var drivers = new List<Driver>();
            var order = new Point(0, 0);
            var finder = new SimpleFinder();

            // Act
            var result = finder.FindNearestDrivers(drivers, order, 5);

            // Assert
            Assert.That(result, Is.Empty);
        }

        // Сравниваем все алгоритмы
        [Test]
        public void AllAlgorithms_ReturnSameDriversSet()
        {
            // Arrange
            var drivers = new List<Driver>();
            var random = new Random(42);

            for (int i = 0; i < 50; i++)
            {
                int x = random.Next(0, 100);
                int y = random.Next(0, 100);
                drivers.Add(new Driver($"D{i:00}", new Point(x, y)));
            }

            var order = new Point(30, 30);

            var simple = new SimpleFinder();
            var grid = new GridPartitionFinder(50);
            var priority = new PriorityQueueFinder();

            // Act
            var result1 = simple.FindNearestDrivers(drivers, order, 5);
            var result2 = grid.FindNearestDrivers(drivers, order, 5);
            var result3 = priority.FindNearestDrivers(drivers, order, 5);

            // проверяем ЧТО 5 водителей
            Assert.That(result2.Count, Is.EqualTo(5), "Grid вернул не 5 водителей");
            Assert.That(result3.Count, Is.EqualTo(5), "Priority вернул не 5 водителей");

            // проверяем расстояния (порядок не важен)
            var dist1 = result1.Select(d => Math.Round(d.GetDistanceTo(order), 2)).OrderBy(d => d).ToList();
            var dist2 = result2.Select(d => Math.Round(d.GetDistanceTo(order), 2)).OrderBy(d => d).ToList();
            var dist3 = result3.Select(d => Math.Round(d.GetDistanceTo(order), 2)).OrderBy(d => d).ToList();

            Assert.That(dist2, Is.EqualTo(dist1), "Grid вернул другие расстояния");
            Assert.That(dist3, Is.EqualTo(dist1), "Priority вернул другие расстояния");

            //просто выводим ID для информации
            var ids1 = result1.Select(d => d.Id).OrderBy(id => id).ToList();
            var ids2 = result2.Select(d => d.Id).OrderBy(id => id).ToList();
            var ids3 = result3.Select(d => d.Id).OrderBy(id => id).ToList();

            Console.WriteLine($"SimpleFinder:  {string.Join(", ", ids1)}");
            Console.WriteLine($"GridPartition: {string.Join(", ", ids2)}");
            Console.WriteLine($"PriorityQueue: {string.Join(", ", ids3)}");
        }
        
    }
}