using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public double DistanceTo(Point orderPoint)// orderPoint - точка заказа
        {
            double dx = X - orderPoint.X;
            double dy = Y - orderPoint.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
    public class Driver
    {
        public string Id { get; }// уникальный ID водителя
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
    public interface IDriverFinder
    {
        
        List<Driver> FindNearestDrivers(List<Driver> drivers, Point orderLocation, int count = 5);

        string AlgorithmName { get; }
    }

    public class SimpleFinder:IDriverFinder
    {
        public string AlgorithmName => "SimpleFinder";
        public List<Driver> FindNearestDrivers(List<Driver> drivers, Point orderLocation, int count = 5)
        {
            // Если водителей меньше или равно нужному количеству - возвращаем всех
            if (drivers == null || drivers.Count <= count)
                return drivers ?? new List<Driver>();

            // 1. Создаем список пар "водитель-расстояние"
            var driversWithDistance = new List<(Driver Driver, double Distance)>();

            foreach (var driver in drivers)
            {
                double distance = driver.GetDistanceTo(orderLocation);
                driversWithDistance.Add((driver, distance));
            }

            // 2. Сортируем по расстоянию (от ближнего к дальнему)
            driversWithDistance.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            // 3. Берем первых count водителей
            var result = new List<Driver>();
            for (int i = 0; i < count; i++)
            {
                result.Add(driversWithDistance[i].Driver);
            }

            return result;
        }
    }
    public class GridPartitionFinder : IDriverFinder
    {
        private readonly int _gridSize;//переменная для хранения размера ячейки сетки
        //readonly — можно установить значение только в конструкторе, потом нельзя изменить
        public string AlgorithmName => $"GridPartitionFinder";
        public GridPartitionFinder(int gridSize = 50)//значение по умолчанию
        {
            _gridSize = gridSize;
        }
        public List<Driver> FindNearestDrivers(List<Driver> drivers, Point orderLocation, int count = 5)
        {
            if (drivers == null || drivers.Count <= count)
                return drivers ?? new List<Driver>();//вернет drivers, если он не null, иначе вернет new List<Driver>()

            // 1. Создаем словарь: ключ - ячейка сетки, значение - водители в этой ячейке
            var grid = new Dictionary<(int X, int Y), List<Driver>>();
            //пример: grid[(1,1)] = New List<Driver>(); (1,1) - ключ. New List<Driver>() - значение

            foreach (var driver in drivers)
            {
                // Определяем ячейку для водителя
                var cellKey = (driver.LocationDriver.X / _gridSize,
                              driver.LocationDriver.Y / _gridSize);

                if (!grid.ContainsKey(cellKey))//проверка есть ли в grid ключ cellkey
                    grid[cellKey] = new List<Driver>();

                grid[cellKey].Add(driver);
            }
            var orderCell = (orderLocation.X / _gridSize, orderLocation.Y / _gridSize);// ячейка заказа

            var result = new List<Driver>();
            int radius = 0;

            while (result.Count < count)
            {
                // Получаем все ячейки на текущем радиусе
                var cellsToCheck = GetCellsAtRadius(orderCell, radius);

                // Собираем водителей из этих ячеек
                var driversInRadius = new List<Driver>();
                foreach (var cell in cellsToCheck)
                {
                    if (grid.ContainsKey(cell))
                        driversInRadius.AddRange(grid[cell]);//addrange добавляет весь список водителей
                }

                // Если нашли водителей в этом радиусе
                if (driversInRadius.Any())//Any возвращает true если в списке хотя бы один элемент
                {
                    // Сортируем их по расстоянию и берем нужное количество
                    var sorted = driversInRadius
                        .OrderBy(d => d.GetDistanceTo(orderLocation))//Сортируем driversInRadius
                        .Take(count - result.Count);//берем нужное количество водителей из driversInRadius

                    result.AddRange(sorted);
                }

                // Если прошли все ячейки и не набрали нужное количество
                if (radius > Math.Max(grid.Keys.Max(c => Math.Abs(c.X - orderCell.Item1)),
                                     grid.Keys.Max(c => Math.Abs(c.Y - orderCell.Item2))))
                    break;

                radius++;
            }

            return result;
        }
        private List<(int X, int Y)> GetCellsAtRadius((int X, int Y) center, int radius)
        {
            var cells = new List<(int, int)>();// Складываем найденные ячейки

            // Если радиус 0 - только центральная ячейка
            if (radius == 0)
            {
                cells.Add(center);
                return cells;
            }

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // Добавляем ТОЛЬКО ячейки на границе radius
                    // (все внутренние ячейки уже были проверены на меньших радиусах)
                    if (Math.Abs(dx) == radius || Math.Abs(dy) == radius)
                    {
                        cells.Add((center.X + dx, center.Y + dy));
                    }
                }
            }

            return cells;
        }
    }

    public class PriorityQueueFinder : IDriverFinder
    {
        public string AlgorithmName => "PriorityQueue";

        public List<Driver> FindNearestDrivers(List<Driver> drivers, Point orderLocation, int count = 5)
        {
            if (drivers == null || drivers.Count <= count)
                return drivers ?? new List<Driver>();

            //  Создаем приоритетную очередь (максимум наверху)
            var queue = new PriorityQueue<Driver, double>(Comparer<double>.Create((a, b) => b.CompareTo(a)));

            //  Проходим по всем водителям
            foreach (var driver in drivers)
            {
                double distance = driver.GetDistanceTo(orderLocation);

                if (queue.Count < count)
                {
                    // Если очередь не заполнена - добавляем
                    queue.Enqueue(driver, distance);
                }
                else if (distance < queue.Peek().Priority)
                {
                    // Если текущий водитель ближе чем самый дальний в очереди
                    queue.Dequeue(); // Удаляем самого дальнего
                    queue.Enqueue(driver, distance); // Добавляем текущего
                }
            }

            //  Извлекаем результаты из очереди
            var result = new List<Driver>();
            while (queue.Count > 0)
            {
                result.Add(queue.Dequeue().Element);
            }

            // Разворачиваем (т.к. в очереди дальние были первыми)
            result.Reverse();

            return result;
        }
    }

    // Вспомогательный класс приоритетной очереди
    public class PriorityQueue<TElement, TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _elements;
        private readonly IComparer<TPriority> _comparer;

        public int Count => _elements.Count;

        public PriorityQueue() : this(Comparer<TPriority>.Default) { }

        public PriorityQueue(IComparer<TPriority> comparer)
        {
            _elements = new List<(TElement, TPriority)>();
            _comparer = comparer;
        }

        public void Enqueue(TElement element, TPriority priority)
        {
            _elements.Add((element, priority));//добавление нового элемента в конец списка
            int i = _elements.Count - 1;//индекс нового элемента

            // Просеивание вверх
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_comparer.Compare(_elements[parent].Priority, _elements[i].Priority) <= 0)
                    break;

                (_elements[i], _elements[parent]) = (_elements[parent], _elements[i]);//меняем местами значения
                i = parent;
            }
        }

        public (TElement Element, TPriority Priority) Dequeue()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            var result = _elements[0];
            _elements[0] = _elements[^1];// ^1 = последний элемент
            _elements.RemoveAt(_elements.Count - 1);// Удаляем последний

            int i = 0;
            // Просеивание вниз
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left < _elements.Count &&
                    _comparer.Compare(_elements[left].Priority, _elements[smallest].Priority) < 0)
                    smallest = left;

                if (right < _elements.Count &&
                    _comparer.Compare(_elements[right].Priority, _elements[smallest].Priority) < 0)
                    smallest = right;

                if (smallest == i) break;

                (_elements[i], _elements[smallest]) = (_elements[smallest], _elements[i]);
                i = smallest;
            }

            return result;
        }

        public (TElement Element, TPriority Priority) Peek()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            return _elements[0];
        }
    }

}