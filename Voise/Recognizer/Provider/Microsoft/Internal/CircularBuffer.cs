using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Voise.Recognizer.Provider.Microsoft.Internal
{
    /// <summary>
    /// A buffer implemented like a queue with a maximum length that will not allocate
    /// extra memory to enqueue data, once maximum length is reached, no more data can
    /// be enqueued.
    /// </summary>
    /// <typeparam name="T">Type of the elements that will hold the buffer</typeparam>
    public class CircularBuffer<T> : IEnumerable<T>, IDisposable
    {
        /// <summary>
        /// Array containing the elements of the buffer
        /// </summary>
        private T[] _buffer;

        private object _sync;

        /// <summary>
        /// Gets the maximum length of the buffer
        /// </summary>
        public int Length { get { return _buffer.Length; } }

        /// <summary>
        /// Gets the index of the buffer where the next "Add" operation will be executed
        /// </summary>
        public int Head { get; private set; }

        /// <summary>
        /// Gets the index of the buffer where the next "Get" operation will be executed
        /// </summary>
        public int Tail { get; private set; }

        /// <summary>
        /// Gets the amount of elements in the buffer
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// A buffer implemented like a queue with a maximum length that will not allocate
        /// extra memory to enqueue data, once maximum length is reached, no more data can
        /// be enqueued.
        /// </summary>
        /// <param name="capacity">Maximum amount of elements that the buffer will hold</param>
        public CircularBuffer(int capacity)
        {
            _buffer = new T[capacity];
            _sync = new object();
        }

        public void Dispose()
        {
            _buffer = null;
            _sync = null;
        }

        /// <summary>
        /// Changes the Write index of the buffer.
        /// </summary>
        /// <param name="amount">amount that the Write index will be incremented.</param>
        private void IncrementHead(int amount)
        {
            lock (_sync)
            {
                if (Head + amount >= Length)
                {
                    Head = Head + amount - Length;
                }
                else
                {
                    Head += amount;
                }
                Count += amount;
            }
        }

        /// <summary>
        /// Changes the Read index of the buffer.
        /// </summary>
        /// <param name="amount">Amount that the Read index will be incremented.</param>
        private void IncrementTail(int amount)
        {
            lock (_sync)
            {
                if (Tail + amount >= Length)
                {
                    Tail = Tail + amount - Length;
                }
                else
                {
                    Tail += amount;
                }
                Count -= amount;
            }
        }

        /// <summary>
        /// Adds a new element to the buffer and increases the Write index by one,  returns
        /// false if there is not enough space to add the elements.
        /// </summary>
        /// <param name="value">Value that will be added to the buffer</param>
        /// <returns></returns>
        public bool TryAdd(T value)
        {
            lock (_sync)
            {
                bool succesfullyAdded = false;
                if (Count < Length)
                {
                    _buffer[Head] = value;
                    succesfullyAdded = true;
                    IncrementHead(1);
                }
                return succesfullyAdded;
            }
        }

        /// <summary>
        /// Adds an array of elements to the buffer and increases the Write index 
        /// acording to the length parameter, returns false if there is not enough space
        /// to add the elements.
        /// </summary>
        /// <param name="values">Array containing the values that will be added</param>
        /// <param name="index">Index where is located the first element that will be copied from the array</param>
        /// <param name="length">Amount of elements that will be copied from the array</param>
        /// <returns></returns>
        public bool TryAdd(T[] values, int index, int length)
        {
            lock (_sync)
            {
                bool succesfullyAdded = false;
                if (this.Count + length <= this.Length && values.Count() >= index + length)
                {
                    int copyLength = Math.Min(this.Length - Head, length);
                    Array.Copy(values, index, _buffer, Head, copyLength);
                    IncrementHead(copyLength);
                    if (copyLength < length)
                        succesfullyAdded = TryAdd(values, index + copyLength, length - copyLength);
                    else
                        succesfullyAdded = true;
                }
                return succesfullyAdded;
            }
        }

        /// <summary>
        /// Gets the first element from the buffer and increases the Read index by one, returns
        /// false if there are not any element in the buffer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(out T value)
        {
            lock (_sync)
            {
                bool succesfullyRemoved = false;
                value = default(T);
                if (Count > 0)
                {
                    value = _buffer[Tail];
                    _buffer[Tail] = default(T);
                    IncrementTail(1);
                    succesfullyRemoved = true;
                }
                return succesfullyRemoved;
            }
        }

        /// <summary>
        /// Copies values from the buffer to the readBuffer parameter at the specified index, 
        /// returns false if the length parameter is major to the amount of elements in the buffer
        /// </summary>
        /// <param name="readBuffer">Buffer where the elements will be copied</param>
        /// <param name="index">Index of the readBuffer where the elements will be copied</param>
        /// <param name="length">amount of elements that will be read from the buffer</param>
        /// <returns></returns>
        public bool TryGet(T[] readBuffer, int index, int length)
        {
            lock (_sync)
            {
                bool succesfullyRemoved = false;
                if (this.Count - length >= 0 && readBuffer.Count() >= index + length)
                {
                    int copyLength = Math.Min(this.Length - Tail, length);
                    Array.Copy(_buffer, Tail, readBuffer, index, copyLength);
                    IncrementTail(copyLength);
                    if (copyLength < length)
                        succesfullyRemoved = TryGet(readBuffer, index + copyLength, length - copyLength);
                    else
                        succesfullyRemoved = true;
                }
                return succesfullyRemoved;
            }
        }

        /// <summary>
        /// Delete values from the buffer and increases the Read index
        /// </summary>
        /// <param name="amount">Amount of elements that will be skipped</param>
        public void Skip(int amount)
        {
            lock (_sync)
            {
                amount = Math.Min(amount, Count);
                for (int i = 0; i < amount; i++)
                {
                    _buffer[Tail] = default(T);
                    IncrementTail(1);
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int bufferIndex = Tail;
            for (int i = 0; i < Count; i++, bufferIndex++)
            {
                if (bufferIndex >= Length)
                    bufferIndex = 0;
                yield return _buffer[bufferIndex];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int bufferIndex = Tail;
            for (int i = 0; i < Count; i++, bufferIndex++)
            {
                if (bufferIndex >= Length)
                    bufferIndex = 0;
                yield return _buffer[bufferIndex];
            }
        }

        public T this[int index]
        {
            get
            {
                if (Count >= index)
                {
                    if (index + Tail >= Length)
                        index = Tail + index - Length;
                    else
                        index += Tail;
                    return _buffer[index];
                }
                else
                    return default(T);
            }
        }
    }
}
