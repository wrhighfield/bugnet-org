using System;
using System.IO;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using LumiSoft.Net.IO;

namespace LumiSoft.Net.MIME
{
    /// <summary>
    /// This class represents MIME application/pkcs7-mime body. Defined in RFC 5751 3.2.
    /// </summary>
    public class MIME_b_ApplicationPkcs7Mime : MIME_b_Application
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MIME_b_ApplicationPkcs7Mime() : base(MIME_MediaTypes.Application.pkcs7_mime)
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
        protected new static MIME_b Parse(MIME_Entity owner, MIME_h_ContentType defaultContentType, SmartStream stream)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (defaultContentType == null) throw new ArgumentNullException(nameof(defaultContentType));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var retVal = new MIME_b_ApplicationPkcs7Mime();
            NetUtils.StreamCopy(stream, retVal.EncodedStream, 32000);

            return retVal;
        }

        #endregion


        #region method GetCertificates

        /// <summary>
        /// Gets certificates contained in pkcs 7.
        /// </summary>
        /// <returns>Returns certificates contained in pkcs 7. Returns null if no certificates.</returns>
        public X509Certificate2Collection GetCertificates()
        {
            if (Data == null) return null;

            var signedCms = new SignedCms();
            signedCms.Decode(Data);

            return signedCms.Certificates;
        }

        #endregion

        #region method VerifySignature

        /// <summary>
        /// Checks if signature is valid and data not altered.
        /// </summary>
        /// <returns>Returns true if signature is valid, otherwise false.</returns>
        /// <remarks>This method is valid only if <b>Content-Type</b> parameter <b>smime-type=signed-data</b>.</remarks>
        /// <exception cref="InvalidOperationException">Is raised when <b>smime-type != signed-data</b>.</exception>
        public bool VerifySignature()
        {
            if (!string.Equals(Entity.ContentType.Parameters["smime-type"], "signed-data",
                    StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidOperationException(
                    "The VerifySignature method is only valid if Content-Type parameter smime-type=signed-data.");

            // Check this.Data exists.
            if (Data == null) return false;

            try
            {
                var signedCms = new SignedCms();
                signedCms.Decode(Data);
                signedCms.CheckSignature(true);

                return true;
            }
            catch
            {
            }

            return false;
        }

        #endregion

        #region method GetSignedMime

        /// <summary>
        /// Gets signed mime content. Value null means no content.
        /// </summary>
        /// <returns>Returns signed mime content. Value null means no content.</returns>
        /// <remarks>This method is valid only if <b>Content-Type</b> parameter <b>smime-type=signed-data</b>.</remarks>
        /// <exception cref="InvalidOperationException">Is raised when <b>smime-type != signed-data</b>.</exception>
        public MIME_Message GetSignedMime()
        {
            if (!string.Equals(Entity.ContentType.Parameters["smime-type"], "signed-data",
                    StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidOperationException(
                    "The VerifySignature method is only valid if Content-Type parameter smime-type=signed-data.");

            if (Data != null)
            {
                var signedCms = new SignedCms();
                signedCms.Decode(Data);

                return MIME_Message.ParseFromStream(new MemoryStream(signedCms.ContentInfo.Content));
            }

            return null;
        }

        #endregion

        #region method GetEnvelopedMime

        /// <summary>
        /// Decrypts enveloped mime content.
        /// </summary>
        /// <param name="cert">Decrypting certificate.</param>
        /// <returns>Returns decrypted enveloped mime content.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>cert</b> is null reference.</exception>
        /// <exception cref="InvalidOperationException">Is raised when <b>smime-type != enveloped-data</b>.</exception>
        public MIME_Message GetEnvelopedMime(X509Certificate2 cert)
        {
            if (cert == null) throw new ArgumentNullException(nameof(cert));
            if (!string.Equals(Entity.ContentType.Parameters["smime-type"], "enveloped-data",
                    StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidOperationException(
                    "The VerifySignature method is only valid if Content-Type parameter smime-type=enveloped-data.");

            var envelopedCms = new EnvelopedCms();
            envelopedCms.Decode(Data);

            var certificates = new X509Certificate2Collection(cert);
            envelopedCms.Decrypt(certificates);

            return MIME_Message.ParseFromStream(new MemoryStream(envelopedCms.Encode()));
        }

        #endregion


        // public void CreateSigned(X509Certificate2 cert,MIME_Entity entity)

        // public void CreateEnveloped(X509Certificate2 cert,MIME_Entity entity)


        #region Properties implementation

        #endregion
    }
}