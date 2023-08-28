﻿using System;
using System.Text;

namespace LumiSoft.Net.MIME
{
    /// <summary>
    /// Represents "Content-Disposition:" header. Defined in RFC 2183.
    /// </summary>
    /// <example>
    /// <code>
    /// RFC 2183.
    ///     In the extended BNF notation of [RFC 822], the Content-Disposition
    ///     header field is defined as follows:
    ///
    ///     disposition := "Content-Disposition" ":" disposition-type *(";" disposition-parm)
    ///
    ///     disposition-type := "inline" / "attachment" / extension-token
    ///                         ; values are not case-sensitive
    ///
    ///     disposition-parm := filename-parm 
    ///                         / creation-date-parm
    ///                         / modification-date-parm
    ///                         / read-date-parm
    ///                         / size-parm
    ///                         / parameter
    ///
    ///     filename-parm := "filename" "=" value
    ///
    ///     creation-date-parm := "creation-date" "=" quoted-date-time
    /// 
    ///     modification-date-parm := "modification-date" "=" quoted-date-time
    ///
    ///     read-date-parm := "read-date" "=" quoted-date-time
    ///
    ///     size-parm := "size" "=" 1*DIGIT
    ///
    ///     quoted-date-time := quoted-string
    ///                         ; contents MUST be an RFC 822 `date-time'
    ///                         ; numeric timezones (+HHMM or -HHMM) MUST be used
    /// </code>
    /// </example>
    public class MIME_h_ContentDisposition : MIME_h
    {
        private bool                       m_IsModified;
        private string                     m_ParseValue;
        private string                     m_DispositionType = "";
        private MIME_h_ParameterCollection m_pParameters;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="dispositionType">The disposition-type. Known values are in <see cref="MIME_DispositionTypes">MIME_DispositionTypes</see>.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>dispositionType</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public MIME_h_ContentDisposition(string dispositionType)
        {
            if(dispositionType == null){
                throw new ArgumentNullException(nameof(dispositionType));
            }
            if(dispositionType == string.Empty){
                throw new ArgumentException("Argument 'dispositionType' value must be specified.");
            }

            m_DispositionType = dispositionType;

            m_pParameters = new MIME_h_ParameterCollection(this);
            m_IsModified  = true;
        }

        /// <summary>
        /// Internal parser constructor.
        /// </summary>
        private MIME_h_ContentDisposition()
        {
            m_pParameters = new MIME_h_ParameterCollection(this);
        }


        #region static method Parse

        /// <summary>
        /// Parses header field from the specified value.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Content-Type: text/plain'.</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public static MIME_h_ContentDisposition Parse(string value)
        {
            if(value == null){
                throw new ArgumentNullException(nameof(value));
            }

            // We should not have encoded words here, but some email clients do this, so encoded them if any.
            var valueDecoded = MIME_Encoding_EncodedWord.DecodeS(value);

            var retVal = new MIME_h_ContentDisposition();
            
            var name_value = valueDecoded.Split(new[]{':'},2);
            if(name_value.Length != 2){
                throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
            }

            var r = new MIME_Reader(name_value[1]);
            var type = r.Token();
            if(type == null){
                throw new ParseException("Invalid Content-Disposition: header field value '" + value + "'.");
            }
            retVal.m_DispositionType = type.Trim();

            retVal.m_pParameters.Parse(r);

            retVal.m_ParseValue = value;

            return retVal;
        }

        #endregion
        

        #region override method ToString
                
        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit characters. Value null means parameters not encoded.</param>
        /// <param name="reEncode">If true always specified encoding is used. If false and header field value not modified, original encoding is kept.</param>
        /// <returns>Returns header field as string.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder,Encoding parmetersCharset,bool reEncode)
        {
            if(reEncode || IsModified){
                return "Content-Disposition: " + m_DispositionType + m_pParameters.ToString(parmetersCharset) + "\r\n";
            }

            return m_ParseValue;
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets if this header field is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public override bool IsModified => m_IsModified || m_pParameters.IsModified;

        /// <summary>
        /// Returns always "Content-Disposition".
        /// </summary>
        public override string Name => "Content-Disposition";

        /// <summary>
        /// Gets the disposition-type. Known values are in <see cref="MIME_DispositionTypes">MIME_DispositionTypes</see>.
        /// </summary>
        public string DispositionType => m_DispositionType;

        /// <summary>
        /// Gets Content-Type parameters collection.
        /// </summary>
        public MIME_h_ParameterCollection Parameters => m_pParameters;

        /// <summary>
        /// Gets or sets the suggested file name. Value null means not specified. Defined in RFC 2183 2.3.
        /// </summary>
        public string Param_FileName
        {
            get => Parameters["filename"];

            set => m_pParameters["filename"] = value;
        }

        /// <summary>
        /// Gets or sets the creation date for a file. Value DateTime.MinValue means not specified. Defined in RFC 2183 2.4.
        /// </summary>
        public DateTime Param_CreationDate
        {
            get{ 
                var value = Parameters["creation-date"];
                if(value == null){
                    return DateTime.MinValue;
                }

                return MIME_Utils.ParseRfc2822DateTime(value);
            }

            set{ 
                if(value == DateTime.MinValue){
                    Parameters.Remove("creation-date");
                }
                else{
                    Parameters["creation-date"] = MIME_Utils.DateTimeToRfc2822(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the modification date of a file. Value DateTime.MinValue means not specified. Defined in RFC 2183 2.5.
        /// </summary>
        public DateTime Param_ModificationDate
        {
            get{
                var value = Parameters["modification-date"];
                if(value == null){
                    return DateTime.MinValue;
                }

                return MIME_Utils.ParseRfc2822DateTime(value);
            }

            set{ 
                if(value == DateTime.MinValue){
                    Parameters.Remove("modification-date");
                }
                else{
                    Parameters["modification-date"] = MIME_Utils.DateTimeToRfc2822(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the last read date of a file. Value DateTime.MinValue means not specified. Defined in RFC 2183 2.6.
        /// </summary>
        public DateTime Param_ReadDate
        {
            get{
                var value = Parameters["read-date"];
                if(value == null){
                    return DateTime.MinValue;
                }

                return MIME_Utils.ParseRfc2822DateTime(value);
            }

            set{ 
                if(value == DateTime.MinValue){
                    Parameters.Remove("read-date");
                }
                else{
                    Parameters["read-date"] = MIME_Utils.DateTimeToRfc2822(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of a file. Value -1 means not specified. Defined in RFC 2183 2.7.
        /// </summary>
        public long Param_Size
        {
            get{
                var value = Parameters["size"];
                if(value == null){
                    return -1;
                }

                return Convert.ToInt64(value);
            }

            set{ 
                if(value < 0){
                    Parameters.Remove("size");
                }
                else{
                    Parameters["size"] = value.ToString();
                }
            }
        }

        #endregion
    }
}
