using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Task2
{
    [Serializable]
    public class Book : ISerializable
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Genre { get; set; }
        public int YearPublished { get; set; }

        public Book()
        {
        }

        public Book(string title, string author, string genre, int yearPublished)
        {
            Title = title;
            Author = author;
            Genre = genre;
            YearPublished = yearPublished;
        }

        protected Book(SerializationInfo info, StreamingContext context)
        {
            Title = info.GetString(nameof(Title));
            Author = info.GetString(nameof(Author));
            Genre = info.GetString(nameof(Genre));
            YearPublished = info.GetInt32(nameof(YearPublished));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Title), Title);
            info.AddValue(nameof(Author), Author);
            info.AddValue(nameof(Genre), Genre);
            info.AddValue(nameof(YearPublished), YearPublished);
        }
    }

    public class BookManagement<T> : ICollection<T>, IReadOnlyCollection<T>, ISerializable, IDeserializationCallback
    {
        private LinkedList<T> books = new();

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return books.ElementAt(index);
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var node = books.Find(books.ElementAt(index));
                if (node != null)
                    node.Value = value;
            }
        }

        public int Count => books.Count;

        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public void Add(T item) => books.AddLast(item);
        public bool Remove(T item) => books.Remove(item);
        public void Clear() => books.Clear();
        public bool Contains(T item) => books.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => books.CopyTo(array, arrayIndex);

        public IEnumerable<T> Search(Func<T, bool> predicate) => books.Where(predicate);

        public void Insert(int index, T item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index == Count)
                Add(item);
            else
            {
                var node = books.Find(books.ElementAt(index));
                if (node != null)
                    books.AddBefore(node, item);
            }
        }

        public void InsertAtBeginning(T item) => books.AddFirst(item);
        public void InsertAtEnd(T item) => books.AddLast(item);
        public void RemoveFromBeginning() => books.RemoveFirst();
        public void RemoveFromEnd() => books.RemoveLast();

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var node = books.Find(books.ElementAt(index));
            if (node != null)
                books.Remove(node);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(books), books.ToList());
        }

        public void OnDeserialization(object? sender)
        {
            if (books == null)
                books = new LinkedList<T>();
            else
                books = new LinkedList<T>(books);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return books.GetEnumerator();
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
            BookManagement<Book> bookManagement = new();

            Book book1 = new() { Title = "Book1", Author = "Author1", Genre = "Genre1", YearPublished = 2022 };
            Book book2 = new() { Title = "Book2", Author = "Author2", Genre = "Genre2", YearPublished = 2023 };

            bookManagement.Add(book1);
            bookManagement.Add(book2);

            bookManagement.Remove(book1);
        }
    }
}
