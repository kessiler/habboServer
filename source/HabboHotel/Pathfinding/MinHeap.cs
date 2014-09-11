using System;
namespace Cyber.HabboHotel.PathFinding
{
	internal class MinHeap<T> where T : IComparable<T>
	{
		private int count;
		private int capacity;
		private T temp;
		private T mheap;
		private T[] array;
		private T[] tempArray;
		public int Count
		{
			get
			{
				return this.count;
			}
		}
		public MinHeap() : this(16)
		{
		}
		public MinHeap(int capacity)
		{
			this.count = 0;
			this.capacity = capacity;
			this.array = new T[capacity];
		}
		public void BuildHead()
		{
			checked
			{
				for (int i = this.count - 1 >> 1; i >= 0; i--)
				{
					this.MinHeapify(i);
				}
			}
		}
		public void Add(T item)
		{
			checked
			{
				this.count++;
				if (this.count > this.capacity)
				{
					this.DoubleArray();
				}
				this.array[this.count - 1] = item;
				int num = this.count - 1;
				int num2 = num - 1 >> 1;
				while (num > 0 && this.array[num2].CompareTo(this.array[num]) > 0)
				{
					this.temp = this.array[num];
					this.array[num] = this.array[num2];
					this.array[num2] = this.temp;
					num = num2;
					num2 = num - 1 >> 1;
				}
			}
		}
		private void DoubleArray()
		{
			this.capacity <<= 1;
			this.tempArray = new T[this.capacity];
			MinHeap<T>.CopyArray(this.array, this.tempArray);
			this.array = this.tempArray;
		}
		private static void CopyArray(T[] source, T[] destination)
		{
			checked
			{
				for (int i = 0; i < source.Length; i++)
				{
					destination[i] = source[i];
				}
			}
		}
		public T Peek()
		{
			if (this.count == 0)
			{
				throw new InvalidOperationException("Heap is empty");
			}
			return this.array[0];
		}
		public T ExtractFirst()
		{
			if (this.count == 0)
			{
				throw new InvalidOperationException("Heap is empty");
			}
			this.temp = this.array[0];
			checked
			{
				this.array[0] = this.array[this.count - 1];
				this.count--;
				this.MinHeapify(0);
				return this.temp;
			}
		}
		private void MinHeapify(int position)
		{
			checked
			{
				while (true)
				{
					int num = (position << 1) + 1;
					int num2 = num + 1;
					int num3;
					if (num < this.count && this.array[num].CompareTo(this.array[position]) < 0)
					{
						num3 = num;
					}
					else
					{
						num3 = position;
					}
					if (num2 < this.count && this.array[num2].CompareTo(this.array[num3]) < 0)
					{
						num3 = num2;
					}
					if (num3 == position)
					{
						break;
					}
					this.mheap = this.array[position];
					this.array[position] = this.array[num3];
					this.array[num3] = this.mheap;
					position = num3;
				}
			}
		}
	}
}
