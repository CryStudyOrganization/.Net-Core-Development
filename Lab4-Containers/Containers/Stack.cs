using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab4.Containers
{
    public sealed class Stack<T> : IEnumerable<T>, IReadOnlyCollection<T>, ICollection
    {
        private T[] array;
        private int top;

        public Stack()
        {
            array = new T[4];
            top = 0;
            Count = 0;
        }

        public int Count { get; private set; }

        public bool IsEmpty => Count == 0;

        public void Push(T item)
        {
            if (Count == array.Length)
            {
                Array.Resize(ref array, array.Length * 2);
            }

            array[top++] = item;
            Count++;
        }

        public T Pop()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("The stack is empty.");
            }

            Count--;
            return array[--top];
        }

        public T Peek()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("The stack is empty.");
            }

            return array[top - 1];
        }

        public void Clear()
        {
            Array.Clear(array, 0, Count);
            top = 0;
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Rank != 1)
                throw new ArgumentException("Multidimensional arrays are not supported.");

            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

            if (array.Length - index < Count)
                throw new ArgumentException("The number of elements in the stack exceeds the capacity of the destination array.");

            int arrayIndex = index;
            foreach (T item in this)
            {
                array.SetValue(item, arrayIndex);
                arrayIndex++;
            }
        }

        public bool IsSynchronized => false;

        public object SyncRoot => this;
    }
}