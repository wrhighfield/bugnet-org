﻿using System;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME
{
    /// <summary>
    /// This class represents MIME unknown bodies.
    /// </summary>
    public class MIME_b_Unknown : MIME_b_SinglepartBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mediaType">MIME media type.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>mediaType</b> is null reference.</exception>
        public MIME_b_Unknown(string mediaType) : base(new MIME_h_ContentType(mediaType))
        {
        }


        #region static method Parse

        /// <summary>
        /// Parses body from the specified stream
        /// </summary>
        /// <param name="owner">Owner MIME entity.</param>
        /// <param name="defaultContentType">Default content-type for this body.</param>
        /// <param name="stream">Stream from where to read body.</param>
        /// <returns>Returns parsed body.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>stream</b>, <b>defaultContentType</b> or <b>strean</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when any parsing errors.</exception>
        protected new static MIME_b Parse(MIME_Entity owner,MIME_h_ContentType defaultContentType,SmartStream stream)
        {
            if(owner == null){
                throw new ArgumentNullException(nameof(owner));
            }
            if(defaultContentType == null){
                throw new ArgumentNullException(nameof(defaultContentType));
            }
            if(stream == null){
                throw new ArgumentNullException(nameof(stream));
            }

            string mediaType = null;
            try{
                mediaType = owner.ContentType.TypeWithSubtype;
            }
            catch{
                mediaType = "unparsable/unparsable";
            }

            var retVal = new MIME_b_Unknown(mediaType);
            NetUtils.StreamCopy(stream,retVal.EncodedStream,32000);

            return retVal;
        }

        #endregion
    }
}
