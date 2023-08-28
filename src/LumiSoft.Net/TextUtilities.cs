using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LumiSoft.Net
{
	/// <summary>
	/// This class provides use-full text methods.
	/// </summary>
	public class TextUtilities
	{      
		#region static method QuoteString

		/// <summary>
        /// Output string and escapes fishy('\',"') chars.
        /// </summary>
        /// <param name="text">Text to quote.</param>
        /// <returns></returns>
        public static string QuoteString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            // String is already quoted-string.
            if(!string.IsNullOrWhiteSpace(text) && text.StartsWith("\"") && text.EndsWith("\"")){
                return text;
            }

            var retVal = new StringBuilder();

            foreach (var c in text)
            {
                switch (c)
                {
                    case '\\':
                        retVal.Append(@"\\");
                        break;
                    case '\"':
                        retVal.Append("\\\"");
                        break;
                    default:
                        retVal.Append(c);
                        break;
                }
            }

            return "\"" + retVal + "\"";
        }

		#endregion
         
        #region static method UnQuoteString

        /// <summary>
        /// Unquotes and un-escapes escaped chars specified text. For example "xxx" will become to 'xxx', "escaped quote \"", will become to escaped 'quote "'.
        /// </summary>
        /// <param name="text">Text to unquote.</param>
        /// <returns></returns>
        public static string UnQuoteString(string text)
        {
            var startPosInText = 0;
            var endPosInText   = text.Length;
           
            //--- Trim. We can't use standard string.Trim(), it's slow. ----//
            for(var i=0;i<endPosInText;i++){
                var c = text[i];
                if(c == ' ' || c == '\t'){
                    startPosInText++;
                }
                else{
                    break;
                }
            }
            for(var i=endPosInText-1;i>0;i--){
                var c = text[i];
                if(c == ' ' || c == '\t'){
                    endPosInText--;
                }
                else{
                    break;
                }
            }
            //--------------------------------------------------------------//
         
            // All text trimmed
            if(endPosInText - startPosInText <= 0){
                return "";
            }
            
            // Remove starting and ending quotes.         
            if(text[startPosInText] == '\"'){
				startPosInText++;
			}
			if(text[endPosInText - 1] == '\"'){
				endPosInText--;
			}

            // Just '"'
            if(endPosInText == startPosInText - 1){
                return "";
            }

            var chars = new char[endPosInText - startPosInText];
            
            var posInChars = 0;
            var charIsEscaped = false;
            for(var i=startPosInText;i<endPosInText;i++)
            {
                var c = text[i];

                switch (charIsEscaped)
                {
                    // Escaping char
                    case false when c == '\\':
                        charIsEscaped = true;
                        break;
                    // Escaped char
                    case true:
                        // TODO: replace \n,\r,\t,\v ???
                        chars[posInChars] = c;
                        posInChars++;
                        charIsEscaped = false;
                        break;
                    // Normal char
                    default:
                        chars[posInChars] = c;
                        posInChars++;
                        break;
                }
            }

            return new string(chars,0,posInChars);
        }

        #endregion

        #region static method EscapeString

        /// <summary>
        /// Escapes specified chars in the specified string.
        /// </summary>
        /// <param name="text">Text to escape.</param>
        /// <param name="charsToEscape">Chars to escape.</param>
		public static string EscapeString(string text,char[] charsToEscape)
        {
            // Create worst scenario buffer, assume all chars must be escaped
            var buffer = new char[text.Length * 2];
            var nChars = 0;
            foreach(var c in text){
                if (charsToEscape.Any(escapeChar => c == escapeChar))
                {
                    buffer[nChars] = '\\';
                    nChars++;
                }

                buffer[nChars] = c;
                nChars++;
            }

            return new string(buffer,0,nChars);
        }

        #endregion

        #region static method UnEscapeString

        /// <summary>
        /// Un-escapes all escaped chars.
        /// </summary>
        /// <param name="text">Text to un-escape.</param>
        /// <returns></returns>
        public static string UnEscapeString(string text)
        {
            // Create worst scenario buffer, non of the chars escaped.
            var buffer = new char[text.Length];
            var nChars = 0;
            var escapedChar = false;
            foreach(var c in text){
                if(!escapedChar && c == '\\'){
                    escapedChar = true;
                }
                else{
                    buffer[nChars] = c;
                    nChars++;
                    escapedChar = false;
                }                
            }

            return new string(buffer,0,nChars);
        }

        #endregion


		#region static method SplitQuotedString

        /// <summary>
		/// Splits string into string arrays. This split method won't split quoted strings, but only text outside of quoted string.
		/// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
		/// </summary>
		/// <param name="text">Text to split.</param>
		/// <param name="splitChar">Char that splits text.</param>
		/// <returns></returns>
		public static IEnumerable<string> SplitQuotedString(string text,char splitChar)
		{
            return SplitQuotedString(text,splitChar,false);
        }

		/// <summary>
		/// Splits string into string arrays. This split method won't split quoted strings, but only text outside of quoted string.
		/// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
		/// </summary>
		/// <param name="text">Text to split.</param>
		/// <param name="splitChar">Char that splits text.</param>
		/// <param name="unquote">If true, split parse will be unquoted if they are quoted.</param>
		/// <returns></returns>
		public static IEnumerable<string> SplitQuotedString(string text,char splitChar,bool unquote)
		{
			return SplitQuotedString(text,splitChar,unquote,int.MaxValue);
		}

        /// <summary>
		/// Splits string into string arrays. This split method won't split quoted strings, but only text outside of quoted string.
		/// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
		/// </summary>
		/// <param name="text">Text to split.</param>
		/// <param name="splitChar">Char that splits text.</param>
		/// <param name="unquote">If true, split parse will be unquoted if they are quoted.</param>
        /// <param name="count">Maximum number of substrings to return.</param>
		/// <returns>Returns split string.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>text</b> is null reference.</exception>
		public static string[] SplitQuotedString(string text,char splitChar,bool unquote,int count)
		{
            if(text == null){
                throw new ArgumentNullException(nameof(text));
            }

			var  splitParts     = new List<string>();  // Holds split parts
            var           startPos       = 0;
			var          inQuotedString = false;               // Holds flag if position is quoted string or not
            var          lastChar       = '0';

            for(var i=0;i<text.Length;i++){
			    var c = text[i];

                // We have exceeded maximum allowed split parts.
                if(splitParts.Count + 1 >= count){
                    break;
                }

                // We have quoted string start/end.
                if(lastChar != '\\' && c == '\"'){
                    inQuotedString = !inQuotedString;
                }
                // We have escaped or normal char.
                //else{

                // We ignore split char in quoted-string.
                if(!inQuotedString){
                    // We have split char, do split.
                    if(c == splitChar)
                    {
                        splitParts.Add(unquote
                            ? UnQuoteString(text.Substring(startPos, i - startPos))
                            : text.Substring(startPos, i - startPos));

                        // Store new split part start position.
                        startPos = i + 1;
                    }
                }
                //else{

                lastChar = c;
			}
                        
			// Add last split part to split parts list
            splitParts.Add(unquote
                ? UnQuoteString(text.Substring(startPos, text.Length - startPos))
                : text.Substring(startPos, text.Length - startPos));

            return splitParts.ToArray();
		}

		#endregion


		#region method QuotedIndexOf

		/// <summary>
		/// Gets first index of specified char. The specified char in quoted string is skipped.
		/// Returns -1 if specified char doesn't exist.
		/// </summary>
		/// <param name="text">Text in what to check.</param>
		/// <param name="indexChar">Char what index to get.</param>
		/// <returns></returns>
		public static int QuotedIndexOf(string text,char indexChar)
		{
			const int retVal = -1;
			var inQuotedString = false; // Holds flag if position is quoted string or not			
			for(var i=0;i<text.Length;i++){
				var c = text[i];

				if(c == '\"'){
					// Start/end quoted string area
					inQuotedString = !inQuotedString;
				}

				// Current char is what index we want and it isn't in quoted string, return it's index
				if(!inQuotedString && c == indexChar){
					return i;
				}
			}

			return retVal;
		}

		#endregion


		#region static method SplitString

		/// <summary>
		/// Splits string into string arrays.
		/// </summary>
		/// <param name="text">Text to split.</param>
		/// <param name="splitChar">Char Char that splits text.</param>
		/// <returns></returns>
		public static string[] SplitString(string text,char splitChar)
		{
			var splitParts = new ArrayList();  // Holds split parts

			var lastSplitPoint = 0;
			var textLength     = text.Length;
			for(var i=0;i<textLength;i++)
            {
                if (text[i] != splitChar) continue;
                // Add current currentSplitBuffer value to split parts list
                splitParts.Add(text.Substring(lastSplitPoint,i - lastSplitPoint));

                lastSplitPoint = i + 1;
            }
			// Add last split part to split parts list
			if(lastSplitPoint <= textLength){
				splitParts.Add(text.Substring(lastSplitPoint));
			}

			var retVal = new string[splitParts.Count];
			splitParts.CopyTo(retVal,0);

			return retVal;
		}

		#endregion


        #region static method IsToken

        /// <summary>
        /// Gets if specified string is valid "token" value.
        /// </summary>
        /// <param name="value">String value to check.</param>
        /// <returns>Returns true if specified string value is valid "token" value.</returns>
        /// <exception cref="ArgumentNullException">Is raised if <b>value</b> is null.</exception>
        public static bool IsToken(string value)
        {
            if(value == null){
                throw new ArgumentNullException(nameof(value));
            }

            /* This syntax is taken from rfc 3261, but token must be universal so ... .
                token    =  1*(alphanum / "-" / "." / "!" / "%" / "*" / "_" / "+" / "`" / "'" / "~" )
                alphanum = ALPHA / DIGIT
                ALPHA    =  %x41-5A / %x61-7A   ; A-Z / a-z
                DIGIT    =  %x30-39             ; 0-9
            */

            var tokenChars = new[]{'-','.','!','%','*','_','+','`','\'','~'};
            return (from c in value where (c < 0x41 || c > 0x5A) && (c < 0x61 || c > 0x7A) && (c < 0x30 || c > 0x39) select tokenChars.Any(tokenChar => c == tokenChar)).All(validTokenChar => validTokenChar);
        }

        #endregion
    }
}
