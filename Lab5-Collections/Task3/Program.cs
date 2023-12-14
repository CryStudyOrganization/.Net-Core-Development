using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Task3
{
    [Serializable]
    public class EmployeeManager<TKey, TValue> : IDictionary<TKey, TValue>, ISerializable, IDeserializationCallback
        where TKey : notnull
    {
        private Dictionary<TKey, TValue> employeeData = new();

        public TValue this[TKey key]
        {
            get => employeeData[key];
            set => employeeData[key] = value;
        }

        public ICollection<TKey> Keys => employeeData.Keys;
        public ICollection<TValue> Values => employeeData.Values;
        public int Count => employeeData.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            employeeData.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            employeeData.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            employeeData.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return employeeData.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return employeeData.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)employeeData).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return employeeData.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return employeeData.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return employeeData.Remove(item.Key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return employeeData.TryGetValue(key, out value!);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(employeeData), employeeData);
        }

        public void OnDeserialization(object? sender)
        {
            if (employeeData == null)
                employeeData = new Dictionary<TKey, TValue>();
            else
                employeeData = new Dictionary<TKey, TValue>(employeeData);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class Program
    {
        static void Main()
        {
            EmployeeManager<string, string> employeeManager = new()
            {
                // Додавання логіну та паролю
                { "john_doe", "hashedPassword123" },
                { "jane_smith", "hashedSecurePassword" }
            };

            // Зміна інформації про логін і пароль
            employeeManager["john_doe"] = "newHashedPassword123";

            // Видалення логіну співробітника
            employeeManager.Remove("jane_smith");

            // Отримання інформації про пароль за логіном
            if (employeeManager.TryGetValue("john_doe", out var password))
            {
                Console.WriteLine($"Password for john_doe: {password}");
            }
        }
    }
}
