using System.Collections;
using System.Collections.Generic;

namespace LumiSoft.Net
{
    /// <summary>
    /// Represents a collection that can be accessed either with the key or with the index. 
    /// </summary>
    public class KeyValueCollection<TKeyK, TValue> : IEnumerable
    {
        private readonly Dictionary<TKeyK, TValue> mPDictionary;
        private readonly List<TValue> mPList;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public KeyValueCollection()
        {
            mPDictionary = new Dictionary<TKeyK, TValue>();
            mPList = new List<TValue>();
        }


        #region method Add

        /// <summary>
        /// Adds the specified key and value to the collection.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void Add(TKeyK key, TValue value)
        {
            mPDictionary.Add(key, value);
            mPList.Add(value);
        }

        #endregion

        #region method Remove

        /// <summary>
        /// Removes the value with the specified key from the collection.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Returns if key found and removed, otherwise false.</returns>
        public bool Remove(TKeyK key)
        {
            if (!mPDictionary.TryGetValue(key, out var value)) return false;
            mPDictionary.Remove(key);
            mPList.Remove(value);

            return true;
        }

        #endregion

        #region method Clear

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            mPDictionary.Clear();
            mPList.Clear();
        }

        #endregion

        #region method ContainsKey

        /// <summary>
        /// Gets if the collection contains the specified key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Returns true if the collection contains specified key.</returns>
        public bool ContainsKey(TKeyK key)
        {
            return mPDictionary.ContainsKey(key);
        }

        #endregion

        #region method TryGetValue

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found.</param>
        /// <returns>Returns true if the collection contains specified key and value stored to <b>value</b> argument.</returns>
        public bool TryGetValue(TKeyK key, out TValue value)
        {
            return mPDictionary.TryGetValue(key, out value);
        }

        #endregion

        #region method TryGetValueAt

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">Zero based item index.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found.</param>
        /// <returns>Returns true if the collection contains specified key and value stored to <b>value</b> argument.</returns>
        public bool TryGetValueAt(int index, out TValue value)
        {
            value = default;

            if (mPList.Count <= 0 || index < 0 || index >= mPList.Count) return false;
            value = mPList[index];

            return true;
        }

        #endregion

        #region method ToArray

        /// <summary>
        /// Copies all elements to new array, all elements will be in order they added. This method is thread-safe.
        /// </summary>
        /// <returns>Returns elements in a new array.</returns>
        public TValue[] ToArray()
        {
            lock (mPList)
            {
                return mPList.ToArray();
            }
        }

        #endregion

        #region interface IEnumerator

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns>Returns IEnumerator interface.</returns>
        public IEnumerator GetEnumerator()
        {
            return mPList.GetEnumerator();
        }

        #endregion

        #region Properties implementation

        /// <summary>
        /// Gets number of items int he collection.
        /// </summary>
        public int Count => mPList.Count;

        /// <summary>
        /// Gets item with the specified key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Returns item with the specified key. If the specified key is not found, a get operation throws a KeyNotFoundException.</returns>
        public TValue this[TKeyK key] => mPDictionary[key];

        #endregion
    }
}