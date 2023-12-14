using System;
using System.Collections.Generic;

namespace Lab4.Containers
{
    public sealed class Queue<T>
    {
        private readonly LinkedList<T> items = new();

        public void Enqueue(T item)
        {
            items.AddLast(item);
        }

        public T Dequeue()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            if (items.First == null)
            {
                throw new InvalidOperationException("The queue is in an invalid state.");
            }

            T item = items.First.Value;
            items.RemoveFirst();
            return item;
        }

        public T Peek()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            if (items.First == null)
            {
                throw new InvalidOperationException("The queue is in an invalid state.");
            }

            return items.First.Value;
        }

        public bool IsEmpty()
        {
            return items.Count == 0;
        }

        public int Count
        {
            get { return items.Count; }
        }

        public void Clear()
        {
            items.Clear();
        }

        public T[] ToArray()
        {
            T[] array = new T[Count];
            items.CopyTo(array, 0);
            return array;
        }
    }
}
