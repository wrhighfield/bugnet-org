﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LumiSoft.Net.MIME;

namespace LumiSoft.Net.Mail
{
    /// <summary>
    /// This class represents <b>mailbox-list</b>. Defined in RFC 5322 3.4.
    /// </summary>
    /// <example>
    /// <code>
    /// RFC 5322.
    ///     mailbox-list =  (mailbox *("," mailbox)) / obs-mbox-list
    /// </code>
    /// </example>
    public class Mail_t_MailboxList : IEnumerable
    {
        private bool m_IsModified;
        private List<Mail_t_Mailbox> m_pList;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Mail_t_MailboxList()
        {
            m_pList = new List<Mail_t_Mailbox>();
        }


        #region static method Parse

        /// <summary>
        /// Parses <b>mailbox-list</b> from specified string value.
        /// </summary>
        /// <param name="value">The <b>mailbox-list</b> string value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when <b>value</b> is not valid <b>mailbox-list</b> value.</exception>
        public static Mail_t_MailboxList Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var r = new MIME_Reader(value);
            var retVal = new Mail_t_MailboxList();
            while (true)
            {
                var word = r.QuotedReadToDelimiter(new[] {',', '<'});
                // We processed all data.
                if (string.IsNullOrEmpty(word) && r.Available == 0) break;
                // name-addr

                if (r.Peek(true) == '<')
                    retVal.Add(new Mail_t_Mailbox(
                        word != null
                            ? MIME_Encoding_EncodedWord.DecodeS(TextUtilities.UnQuoteString(word.Trim()))
                            : null, r.ReadParenthesized()));
                // addr-spec
                else
                    retVal.Add(new Mail_t_Mailbox(null, word));

                // We have more addresses.
                if (r.Peek(true) == ',') r.Char(false);
            }

            return retVal;
        }

        #endregion


        #region method Insert

        /// <summary>
        /// Inserts a address into the collection at the specified location.
        /// </summary>
        /// <param name="index">The location in the collection where you want to add the item.</param>
        /// <param name="value">Address to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when <b>index</b> is out of range.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        public void Insert(int index, Mail_t_Mailbox value)
        {
            if (index < 0 || index > m_pList.Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (value == null) throw new ArgumentNullException(nameof(value));

            m_pList.Insert(index, value);
            m_IsModified = true;
        }

        #endregion

        #region method Add

        /// <summary>
        /// Adds specified address to the end of the collection.
        /// </summary>
        /// <param name="value">Address to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference value.</exception>
        public void Add(Mail_t_Mailbox value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            m_pList.Add(value);
            m_IsModified = true;
        }

        #endregion

        #region method Remove

        /// <summary>
        /// Removes specified item from the collection.
        /// </summary>
        /// <param name="value">Address to remove.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference value.</exception>
        public void Remove(Mail_t_Mailbox value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            m_pList.Remove(value);
        }

        #endregion

        #region method Clear

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            m_pList.Clear();
            m_IsModified = true;
        }

        #endregion

        /// <summary>
        /// Returns an IList
        /// </summary>
        /// <returns></returns>
        public IList<Mail_t_Mailbox> ToList()
        {
            return m_pList;
        }

        #region method ToArray

        /// <summary>
        /// Copies addresses to new array.
        /// </summary>
        /// <returns>Returns addresses array.</returns>
        public Mail_t_Mailbox[] ToArray()
        {
            return m_pList.ToArray();
        }

        #endregion

        #region override method ToString

        /// <summary>
        /// Returns address-list as string.
        /// </summary>
        /// <returns>Returns address-list as string.</returns>
        public override string ToString()
        {
            var retVal = new StringBuilder();
            for (var i = 0; i < m_pList.Count; i++)
                if (i == m_pList.Count - 1)
                    retVal.Append(m_pList[i].ToString());
                else
                    retVal.Append(m_pList[i].ToString() + ",");

            return retVal.ToString();
        }

        #endregion


        #region method AcceptChanges

        /// <summary>
        /// Resets IsModified property to false.
        /// </summary>
        internal void AcceptChanges()
        {
            m_IsModified = false;
        }

        #endregion


        #region interface IEnumerator

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pList.GetEnumerator();
        }

        #endregion

        #region Properties implementation

        /// <summary>
        /// Gets if list has modified since it was loaded.
        /// </summary>
        public bool IsModified => m_IsModified;

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count => m_pList.Count;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>Returns the element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when <b>index</b> is out of range.</exception>
        public Mail_t_Mailbox this[int index]
        {
            get
            {
                if (index < 0 || index >= m_pList.Count) throw new ArgumentOutOfRangeException(nameof(index));

                return m_pList[index];
            }
        }

        #endregion
    }
}