using System;
using System.Collections.Generic;
using System.Collections;

namespace Lab4.Containers
{
    public sealed class PriorityQueue<TElement, TPriority>
    {
        private readonly List<(TElement element, TPriority priority)> heap = new();

        public int Count => heap.Count;

        public PriorityQueue() { }

        public PriorityQueue(IComparer<TPriority> comparer)
        {
            Comparer = comparer;
        }

        public PriorityQueue(IEnumerable<(TElement element, TPriority priority)> items)
        {
            heap.AddRange(items);
            BuildHeap();
        }

        public IComparer<TPriority> Comparer { get; } = Comparer<TPriority>.Default;

        public void Enqueue(TElement element, TPriority priority)
        {
            heap.Add((element, priority));
            HeapifyUp();
        }

        public (TElement element, TPriority priority) Dequeue()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            var min = heap[0];
            var lastIndex = heap.Count - 1;
            heap[0] = heap[lastIndex];
            heap.RemoveAt(lastIndex);
            HeapifyDown();
            return min;
        }

        public (TElement element, TPriority priority) Peek()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            return heap[0];
        }

        public bool IsEmpty()
        {
            return heap.Count == 0;
        }

        public void Clear()
        {
            heap.Clear();
        }

        private void BuildHeap()
        {
            for (int i = heap.Count / 2 - 1; i >= 0; i--)
            {
                HeapifyDown(i);
            }
        }

        private void HeapifyUp()
        {
            int currentIndex = heap.Count - 1;
            while (currentIndex > 0)
            {
                int parentIndex = (currentIndex - 1) / 2;
                if (Comparer.Compare(heap[currentIndex].priority, heap[parentIndex].priority) >= 0)
                {
                    break;
                }
                Swap(currentIndex, parentIndex);
                currentIndex = parentIndex;
            }
        }

        private void HeapifyDown(int startIndex = 0)
        {
            int currentIndex = startIndex;
            int leftChildIndex;
            int rightChildIndex;

            while (true)
            {
                leftChildIndex = (currentIndex << 1) + 1;
                rightChildIndex = (currentIndex << 1) + 2;
                int smallestIndex = currentIndex;

                if (leftChildIndex < heap.Count && Comparer.Compare(heap[leftChildIndex].priority, heap[smallestIndex].priority) < 0)
                {
                    smallestIndex = leftChildIndex;
                }

                if (rightChildIndex < heap.Count && Comparer.Compare(heap[rightChildIndex].priority, heap[smallestIndex].priority) < 0)
                {
                    smallestIndex = rightChildIndex;
                }

                if (smallestIndex == currentIndex)
                {
                    break;
                }

                Swap(currentIndex, smallestIndex);
                currentIndex = smallestIndex;
            }
        }

        private void Swap(int index1, int index2)
        {
            (heap[index2], heap[index1]) = (heap[index1], heap[index2]);
        }
    }
}