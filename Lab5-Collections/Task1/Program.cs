using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Task1
{
    public class Employee
    {
        public string? FullName { get; set; }
        public string? Position { get; set; }
        public decimal Salary { get; set; }
        public string? Email { get; set; }
    }

    public class EmployeeManagement<T> : IList<T>, IReadOnlyList<T>
    {
        private readonly List<T> employees = new();

        public T this[int index]
        {
            get => employees[index];
            set => employees[index] = value;
        }

        public int Count
            => employees.Count;

        public bool IsReadOnly
            => false;

        public void Add(T item)
            => employees.Add(item);

        public void Clear()
            => employees.Clear();

        public bool Contains(T item)
            => employees.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
            => employees.CopyTo(array, arrayIndex);

        public int IndexOf(T item)
            => employees.IndexOf(item);

        public void Insert(int index, T item)
            => employees.Insert(index, item);

        public bool Remove(T item)
            => employees.Remove(item);

        public void RemoveAt(int index)
            => employees.RemoveAt(index);

        public IEnumerator<T> GetEnumerator()
            => employees.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        // Пошук за умовою
        public IEnumerable<T> Search(Func<T, bool> predicate)
            => employees.Where(predicate);

        // Оновлення інформації про співробітника за індексом
        public void UpdateEmployeeInfo(int index, T newInfo)
            => employees[index] = newInfo;

        // Сортування за ключем
        public void SortBy(Func<T, IComparable> keySelector)
            => employees.Sort((x, y) => keySelector(x).CompareTo(keySelector(y)));

        // Пошук за умовою (перегрузка для рядка)
        public IEnumerable<T> Search(string searchTerm)
            => Search(e => e?.ToString()?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false);

        // Сортування за ключем (перегрузка для властивості рядка)
        public void SortBy(Func<T, string> keySelector)
            => employees.Sort((x, y) => string.Compare(keySelector(x), keySelector(y), StringComparison.OrdinalIgnoreCase));
    }

    class Program
    {
        static void Main()
        {
            EmployeeManagement<Employee> employeeManagement = new();

            // Додавання співробітників
            Employee employee1 = new() { FullName = "John Doe", Position = "Developer", Salary = 50000, Email = "john@example.com" };
            Employee employee2 = new() { FullName = "Jane Smith", Position = "Manager", Salary = 60000, Email = "jane@example.com" };

            employeeManagement.Add(employee1);
            employeeManagement.Add(employee2);

            // Видалення співробітників
            employeeManagement.Remove(employee1);

            // Пошук співробітників за параметрами
            var searchResults = employeeManagement.Search(e => e.Position == "Manager");
            Console.WriteLine("Search results:");
            foreach (var result in searchResults)
            {
                Console.WriteLine($"Name: {result.FullName}, Position: {result.Position}");
            }

            // Сортування співробітників за параметрами
            employeeManagement.SortBy(e => e.Salary);
        }
    }
}
