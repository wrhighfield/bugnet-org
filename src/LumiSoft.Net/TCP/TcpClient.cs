using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using LumiSoft.Net.IO;
using LumiSoft.Net.Log;

namespace LumiSoft.Net.TCP
{
    /// <summary>
    /// This class implements generic TCP client.
    /// </summary>
    public class TcpClient : TcpSession
    {
        private bool mIsConnected;
        private string mId = "";
        private DateTime mConnectTime;
        private IPEndPoint mPLocalEp;
        private IPEndPoint mPRemoteEp;
        private bool mIsSecure;
        private SmartStream mPTcpStream;
        private RemoteCertificateValidationCallback mPCertificateCallback;
        private int mTimeout = 61000;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected TcpClient()
        {
        }

        #region method Dispose

        /// <summary>
        /// Cleans up any resources being used. This method is thread-safe.
        /// </summary>
        public override void Dispose()
        {
            lock (this)
            {
                if (IsDisposed) return;
                try
                {
                    Disconnect();
                }
                catch
                {
                    // ignored
                }

                IsDisposed = true;
            }
        }

        #endregion


        #region method Connect

        /// <summary>
        /// Connects to the specified host. If the hostname resolves to more than one IP address, 
        /// all IP addresses will be tried for connection, until one of them connects.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(string host, int port)
        {
            Connect(host, port, false);
        }

        /// <summary>
        /// Connects to the specified host. If the hostname resolves to more than one IP address, 
        /// all IP addresses will be tried for connection, until one of them connects.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Port to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(string host, int port, bool ssl)
        {
            if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
            if (mIsConnected) throw new InvalidOperationException("TCP client is already connected.");
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("Argument 'host' value may not be null or empty.");
            if (port < 1) throw new ArgumentException("Argument 'port' value must be >= 1.");

            var ips = Dns.GetHostAddresses(host);
            for (var i = 0; i < ips.Length; i++)
                try
                {
                    Connect(null, new IPEndPoint(ips[i], port), ssl);
                    break;
                }
                catch (Exception)
                {
                    if (IsConnected) throw;

                    // Connect failed for specified IP address, if there are some more IPs left, try next, otherwise forward exception.
                    if (i == ips.Length - 1) throw;
                }
        }

        /// <summary>
        /// Connects to the specified remote end point.
        /// </summary>
        /// <param name="remoteEp">Remote IP end point where to connect.</param>
        /// <param name="ssl">Specifies if connects to SSL end point.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Connect(IPEndPoint remoteEp, bool ssl)
        {
            Connect(null, remoteEp, ssl);
        }

        /// <summary>
        /// Connects to the specified remote end point.
        /// </summary>
        /// <param name="localEp">Local IP end point to use. Value null means that system will allocate it.</param>
        /// <param name="remoteEp">Remote IP end point to connect.</param>
        /// <param name="ssl">Specifies if connection switches to SSL affter connect.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null reference.</exception>
        private void Connect(IPEndPoint localEp, IPEndPoint remoteEp, bool ssl)
        {
            Connect(localEp, remoteEp, ssl, null);
        }

        /// <summary>
        /// Connects to the specified remote end point.
        /// </summary>
        /// <param name="localEp">Local IP end point to use. Value null means that system will allocate it.</param>
        /// <param name="remoteEp">Remote IP end point to connect.</param>
        /// <param name="ssl">Specifies if connection switches to SSL affter connect.</param>
        /// <param name="certCallback">SSL server certificate validation callback. Value null means any certificate is accepted.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is already connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null reference.</exception>
        private void Connect(IPEndPoint localEp, IPEndPoint remoteEp, bool ssl,
            RemoteCertificateValidationCallback certCallback)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (mIsConnected) throw new InvalidOperationException("TCP client is already connected.");
            if (remoteEp == null) throw new ArgumentNullException(nameof(remoteEp));

            var wait = new ManualResetEvent(false);
            using (var op = new ConnectAsyncOp(localEp, remoteEp, ssl, certCallback))
            {
                op.CompletedAsync += delegate(object s1, EventArgs<ConnectAsyncOp> e1) { wait.Set(); };
                if (!ConnectAsync(op)) wait.Set();
                wait.WaitOne();
                wait.Close();

                if (op.Error != null) throw op.Error;
            }
        }

        #endregion

        #region method ConnectAsync

        #region class ConnectAsyncOP

        /// <summary>
        /// This class represents <see cref="TcpClient.ConnectAsync"/> asynchronous operation.
        /// </summary>
        public class ConnectAsyncOp : IDisposable, IAsyncOp
        {
            private readonly object mPLock = new object();
            private Exception mPException;
            private IPEndPoint mPLocalEp;
            private IPEndPoint mPRemoteEp;
            private bool mSsl;
            private RemoteCertificateValidationCallback mPCertCallback;
            private TcpClient mPTcpClient;
            private Socket mPSocket;
            private Stream mPStream;
            private bool mRiseCompleted;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="localEp">Local IP end point to use. Value null means that system will allocate it.</param>
            /// <param name="remoteEp">Remote IP end point to connect.</param>
            /// <param name="ssl">Specifies if connection switches to SSL affter connect.</param>
            /// <param name="certCallback">SSL server certificate validation callback. Value null means any certificate is accepted.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>remoteEP</b> is null reference.</exception>
            public ConnectAsyncOp(IPEndPoint localEp, IPEndPoint remoteEp, bool ssl,
                RemoteCertificateValidationCallback certCallback)
            {
                mPLocalEp = localEp;
                mPRemoteEp = remoteEp ?? throw new ArgumentNullException(nameof(localEp));
                mSsl = ssl;
                mPCertCallback = certCallback;
            }

            #region method Dispose

            /// <summary>
            /// Cleans up any resource being used.
            /// </summary>
            public void Dispose()
            {
                if (State == AsyncOpState.Disposed) return;
                SetState(AsyncOpState.Disposed);

                mPException = null;
                mPLocalEp = null;
                mPRemoteEp = null;
                mSsl = false;
                mPCertCallback = null;
                mPTcpClient = null;
                mPSocket = null;
                mPStream = null;

                CompletedAsync = null;
            }

            #endregion


            #region method Start

            /// <summary>
            /// Starts operation processing.
            /// </summary>
            /// <param name="owner">Owner TCP client.</param>
            /// <returns>Returns true if asynchronous operation in progress or false if operation completed synchronously.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> is null reference.</exception>
            internal bool Start(TcpClient owner)
            {
                mPTcpClient = owner ?? throw new ArgumentNullException(nameof(owner));

                SetState(AsyncOpState.Active);

                try
                {
                    // Create socket.
                    if (mPRemoteEp.AddressFamily == AddressFamily.InterNetwork)
                    {
                        mPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        mPSocket.ReceiveTimeout = mPTcpClient.mTimeout;
                        mPSocket.SendTimeout = mPTcpClient.mTimeout;
                    }
                    else if (mPRemoteEp.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        mPSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                        mPSocket.ReceiveTimeout = mPTcpClient.mTimeout;
                        mPSocket.SendTimeout = mPTcpClient.mTimeout;
                    }

                    // Bind socket to the specified end point.
                    if (mPLocalEp != null) mPSocket.Bind(mPLocalEp);

                    mPTcpClient.LogAddText("Connecting to " + mPRemoteEp.ToString() + ".");

                    // Start connecting.
                    mPSocket.BeginConnect(mPRemoteEp, BeginConnectCompleted, null);
                }
                catch (Exception x)
                {
                    mPException = x;
                    CleanupSocketRelated();
                    mPTcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(AsyncOpState.Completed);

                    return false;
                }

                // Set flag rise CompletedAsync event flag. The event is raised when async op completes.
                // If already completed sync, that flag has no effect.
                lock (mPLock)
                {
                    mRiseCompleted = true;

                    return State == AsyncOpState.Active;
                }
            }

            #endregion


            #region method SetState

            /// <summary>
            /// Sets operation state.
            /// </summary>
            /// <param name="state">New state.</param>
            private void SetState(AsyncOpState state)
            {
                if (State == AsyncOpState.Disposed) return;

                lock (mPLock)
                {
                    State = state;

                    if (State == AsyncOpState.Completed && mRiseCompleted) OnCompletedAsync();
                }
            }

            #endregion

            #region method BeginConnectCompleted

            /// <summary>
            /// This method is called when "BeginConnect" has completed.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void BeginConnectCompleted(IAsyncResult ar)
            {
                try
                {
                    mPSocket.EndConnect(ar);

                    mPTcpClient.LogAddText("Connected, localEP='" + mPSocket.LocalEndPoint.ToString() +
                                           "'; remoteEP='" + mPSocket.RemoteEndPoint.ToString() + "'.");

                    // Start SSL handshake.
                    if (mSsl)
                    {
                        mPTcpClient.LogAddText("Starting SSL handshake.");

                        mPStream = new SslStream(new NetworkStream(mPSocket, true), false,
                            RemoteCertificateValidationCallback);
                        ((SslStream) mPStream).BeginAuthenticateAsClient("dummy", BeginAuthenticateAsClientCompleted,
                            null);
                    }
                    // We are done.
                    else
                    {
                        mPStream = new NetworkStream(mPSocket, true);

                        InternalConnectCompleted();
                    }
                }
                catch (Exception x)
                {
                    mPException = x;
                    CleanupSocketRelated();
                    mPTcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(AsyncOpState.Completed);
                }
            }

            #endregion

            #region method BeginAuthenticateAsClientCompleted

            /// <summary>
            /// This method is called when "BeginAuthenticateAsClient" has completed.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void BeginAuthenticateAsClientCompleted(IAsyncResult ar)
            {
                try
                {
                    ((SslStream) mPStream).EndAuthenticateAsClient(ar);

                    mPTcpClient.LogAddText("SSL handshake completed sucessfully.");

                    InternalConnectCompleted();
                }
                catch (Exception x)
                {
                    mPException = x;
                    CleanupSocketRelated();
                    mPTcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(AsyncOpState.Completed);
                }
            }

            #endregion

            #region method RemoteCertificateValidationCallback

            /// <summary>
            /// This method is called when we need to validate remote server certificate.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="certificate">Certificate.</param>
            /// <param name="chain">Certificate chain.</param>
            /// <param name="sslPolicyErrors">SSL policy errors.</param>
            /// <returns>Returns true if certificate validated, otherwise false.</returns>
            private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate,
                X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                // User will handle it.
                if (mPCertCallback != null) return mPCertCallback(sender, certificate, chain, sslPolicyErrors);

                return sslPolicyErrors == SslPolicyErrors.None ||
                       (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) > 0;
                // Do not allow this client to communicate with unauthenticated servers.
            }

            #endregion

            #region method CleanupSocketRelated

            /// <summary>
            /// Cleans up any socket related resources.
            /// </summary>
            private void CleanupSocketRelated()
            {
                try
                {
                    mPStream?.Dispose();
                    if (mPSocket == null) return;
                    mPSocket.Close();
                }
                catch
                {
                    // ignored
                }
            }

            #endregion

            #region method InternalConnectCompleted

            /// <summary>
            /// Is called when when connecting has finished.
            /// </summary>
            private void InternalConnectCompleted()
            {
                mPTcpClient.mIsConnected = true;
                mPTcpClient.mId = Guid.NewGuid().ToString();
                mPTcpClient.mConnectTime = DateTime.Now;
                mPTcpClient.mPLocalEp = (IPEndPoint) mPSocket.LocalEndPoint;
                mPTcpClient.mPRemoteEp = (IPEndPoint) mPSocket.RemoteEndPoint;
                mPTcpClient.mPTcpStream = new SmartStream(mPStream, true);
                mPTcpClient.mPTcpStream.Encoding = Encoding.UTF8;

                mPTcpClient.OnConnected(CompleteConnectCallback);
            }

            #endregion

            #region method CompleteConnectCallback

            /// <summary>
            /// This method is called when this derrived class OnConnected processing has completed.
            /// </summary>
            /// <param name="error">Exception happened or null if no errors.</param>
            private void CompleteConnectCallback(Exception error)
            {
                mPException = error;

                SetState(AsyncOpState.Completed);
            }

            #endregion


            #region Properties implementation

            /// <summary>
            /// Gets asynchronous operation state.
            /// </summary>
            public AsyncOpState State { get; private set; } = AsyncOpState.WaitingForStart;

            /// <summary>
            /// Gets error happened during operation. Returns null if no error.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>AsyncOP_State.Completed</b> state.</exception>
            public Exception Error
            {
                get
                {
                    if (State == AsyncOpState.Disposed) throw new ObjectDisposedException(GetType().Name);
                    if (State != AsyncOpState.Completed)
                        throw new InvalidOperationException(
                            "Property 'Error' is accessible only in 'AsyncOP_State.Completed' state.");

                    return mPException;
                }
            }

            /// <summary>
            /// Gets connected socket.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>AsyncOP_State.Completed</b> state.</exception>
            public Socket Socket
            {
                get
                {
                    if (State == AsyncOpState.Disposed) throw new ObjectDisposedException(GetType().Name);
                    if (State != AsyncOpState.Completed)
                        throw new InvalidOperationException(
                            "Property 'Socket' is accessible only in 'AsyncOP_State.Completed' state.");
                    if (mPException != null) throw mPException;

                    return mPSocket;
                }
            }

            /// <summary>
            /// Gets connected TCP stream.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>AsyncOP_State.Completed</b> state.</exception>
            public Stream Stream
            {
                get
                {
                    if (State == AsyncOpState.Disposed) throw new ObjectDisposedException(GetType().Name);
                    if (State != AsyncOpState.Completed)
                        throw new InvalidOperationException(
                            "Property 'Stream' is accessible only in 'AsyncOP_State.Completed' state.");
                    if (mPException != null) throw mPException;

                    return mPStream;
                }
            }

            #endregion

            #region Events implementation

            /// <summary>
            /// Is called when asynchronous operation has completed.
            /// </summary>
            public event EventHandler<EventArgs<ConnectAsyncOp>> CompletedAsync;

            #region method OnCompletedAsync

            /// <summary>
            /// Raises <b>CompletedAsync</b> event.
            /// </summary>
            private void OnCompletedAsync()
            {
                CompletedAsync?.Invoke(this, new EventArgs<ConnectAsyncOp>(this));
            }

            #endregion

            #endregion
        }

        #endregion

        /// <summary>
        /// Starts connecting to remote end point.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        /// <returns>Returns true if aynchronous operation is pending (The <see cref="ConnectAsyncOp.CompletedAsync"/> event is raised upon completion of the operation).
        /// Returns false if operation completed synchronously.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
        private bool ConnectAsync(ConnectAsyncOp op)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (op.State != AsyncOpState.WaitingForStart)
                throw new ArgumentException(
                    "Invalid argument 'op' state, 'op' must be in 'AsyncOP_State.WaitingForStart' state.", nameof(op));

            return op.Start(this);
        }

        #endregion

        #region method Disconnect

        /// <summary>
        /// Disconnects connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override void Disconnect()
        {
            if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
            if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");
            mIsConnected = false;

            mPLocalEp = null;
            mPRemoteEp = null;
            mPTcpStream.Dispose();
            mIsSecure = false;
            mPTcpStream = null;

            LogAddText("Disconnected.");
        }

        #endregion

        #region method BeginDisconnect

        /// <summary>
        /// Internal helper method for asynchronous Disconnect method.
        /// </summary>
        private delegate void DisconnectDelegate();

        /// <summary>
        /// Starts disconnecting connection.
        /// </summary>
        /// <param name="callback">Callback to call when the asynchronous operation is complete.</param>
        /// <param name="state">User data.</param>
        /// <returns>An IAsyncResult that references the asynchronous disconnect.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");

            var asyncMethod = new DisconnectDelegate(Disconnect);
            var asyncState = new AsyncResultState(this, asyncMethod, callback, state);
            asyncState.SetAsyncResult(asyncMethod.BeginInvoke(new AsyncCallback(asyncState.CompletedCallback), null));

            return asyncState;
        }

        #endregion

        #region method EndDisconnect

        /// <summary>
        /// Ends a pending asynchronous disconnect request.
        /// </summary>
        /// <param name="asyncResult">An IAsyncResult that stores state information and any user defined data for this asynchronous operation.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>asyncResult</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when argument <b>asyncResult</b> was not returned by a call to the <b>BeginDisconnect</b> method.</exception>
        /// <exception cref="InvalidOperationException">Is raised when <b>EndDisconnect</b> was previously called for the asynchronous connection.</exception>
        public void EndDisconnect(IAsyncResult asyncResult)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (asyncResult == null) throw new ArgumentNullException(nameof(asyncResult));

            if (!(asyncResult is AsyncResultState asyncResultState) || asyncResultState.AsyncObject != this)
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginDisconnect method.");
            if (asyncResultState.IsEndCalled)
                throw new InvalidOperationException(
                    "EndDisconnect was previously called for the asynchronous connection.");

            asyncResultState.IsEndCalled = true;
            if (asyncResultState.AsyncDelegate is DisconnectDelegate @delegate)
                @delegate.EndInvoke(asyncResultState.AsyncResult);
            else
                throw new ArgumentException(
                    "Argument asyncResult was not returned by a call to the BeginDisconnect method.");
        }

        #endregion


        #region method SwitchToSecure

        /// <summary>
        /// Switches session to secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected or is already secure.</exception>
        protected void SwitchToSecure()
        {
            if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
            if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");
            if (mIsSecure) throw new InvalidOperationException("TCP client is already secure.");

            LogAddText("Switching to SSL.");

            // FIX ME: if ssl switching fails, it closes source stream or otherwise if ssl successful, source stream leaks.

            var sslStream = new SslStream(mPTcpStream.SourceStream, true, RemoteCertificateValidationCallback);
            sslStream.AuthenticateAsClient("dummy");

            // Close old stream, but leave source stream open.
            mPTcpStream.IsOwner = false;
            mPTcpStream.Dispose();

            mIsSecure = true;
            mPTcpStream = new SmartStream(sslStream, true);
        }

        #region method RemoteCertificateValidationCallback

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // User will handle it.
            if (mPCertificateCallback != null)
                return mPCertificateCallback(sender, certificate, chain, sslPolicyErrors);

            return sslPolicyErrors == SslPolicyErrors.None ||
                   (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) > 0;
            // Do not allow this client to communicate with unauthenticated servers.
        }

        #endregion

        #endregion

        #region method SwitchToSecureAsync

        #region class SwitchToSecureAsyncOP

        /// <summary>
        /// This class represents <see cref="TcpClient.SwitchToSecureAsync"/> asynchronous operation.
        /// </summary>
        protected class SwitchToSecureAsyncOp : IDisposable, IAsyncOp
        {
            private object mPLock = new object();
            private bool mRiseCompleted;
            private Exception mPException;
            private RemoteCertificateValidationCallback mPCertCallback;
            private TcpClient mPTcpClient;
            private SslStream mPSslStream;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="certCallback">SSL server certificate validation callback. Value null means any certificate is accepted.</param>
            public SwitchToSecureAsyncOp(RemoteCertificateValidationCallback certCallback)
            {
                mPCertCallback = certCallback;
            }

            #region method Dispose

            /// <summary>
            /// Cleans up any resource being used.
            /// </summary>
            public void Dispose()
            {
                if (State == AsyncOpState.Disposed) return;
                SetState(AsyncOpState.Disposed);

                mPException = null;
                mPCertCallback = null;
                mPSslStream = null;

                CompletedAsync = null;
            }

            #endregion


            #region method Start

            /// <summary>
            /// Starts operation processing.
            /// </summary>
            /// <param name="owner">Owner TCP client.</param>
            /// <returns>Returns true if asynchronous operation in progress or false if operation completed synchronously.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> is null reference.</exception>
            internal bool Start(TcpClient owner)
            {
                mPTcpClient = owner ?? throw new ArgumentNullException(nameof(owner));

                SetState(AsyncOpState.Active);

                try
                {
                    mPSslStream = new SslStream(mPTcpClient.mPTcpStream.SourceStream, false,
                        RemoteCertificateValidationCallback);
                    mPSslStream.BeginAuthenticateAsClient("dummy", BeginAuthenticateAsClientCompleted, null);
                }
                catch (Exception x)
                {
                    mPException = x;
                    SetState(AsyncOpState.Completed);
                }

                // Set flag rise CompletedAsync event flag. The event is raised when async op completes.
                // If already completed sync, that flag has no effect.
                lock (mPLock)
                {
                    mRiseCompleted = true;

                    return State == AsyncOpState.Active;
                }
            }

            #endregion


            #region method SetState

            /// <summary>
            /// Sets operation state.
            /// </summary>
            /// <param name="state">New state.</param>
            private void SetState(AsyncOpState state)
            {
                if (State == AsyncOpState.Disposed) return;

                lock (mPLock)
                {
                    State = state;

                    if (State == AsyncOpState.Completed && mRiseCompleted) OnCompletedAsync();
                }
            }

            #endregion

            #region method RemoteCertificateValidationCallback

            /// <summary>
            /// This method is called when we need to validate remote server certificate.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="certificate">Certificate.</param>
            /// <param name="chain">Certificate chain.</param>
            /// <param name="sslPolicyErrors">SSL policy errors.</param>
            /// <returns>Returns true if certificate validated, otherwise false.</returns>
            private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate,
                X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                // User will handle it.
                if (mPCertCallback != null) return mPCertCallback(sender, certificate, chain, sslPolicyErrors);

                return sslPolicyErrors == SslPolicyErrors.None ||
                       (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) > 0;
                // Do not allow this client to communicate with unauthenticated servers.
            }

            #endregion

            #region method BeginAuthenticateAsClientCompleted

            /// <summary>
            /// This method is called when "BeginAuthenticateAsClient" has completed.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void BeginAuthenticateAsClientCompleted(IAsyncResult ar)
            {
                try
                {
                    mPSslStream.EndAuthenticateAsClient(ar);

                    // Close old stream, but leave source stream open.
                    mPTcpClient.mPTcpStream.IsOwner = false;
                    mPTcpClient.mPTcpStream.Dispose();

                    mPTcpClient.mIsSecure = true;
                    mPTcpClient.mPTcpStream = new SmartStream(mPSslStream, true);
                }
                catch (Exception x)
                {
                    mPException = x;
                }

                SetState(AsyncOpState.Completed);
            }

            #endregion


            #region Properties implementation

            /// <summary>
            /// Gets asynchronous operation state.
            /// </summary>
            public AsyncOpState State { get; private set; } = AsyncOpState.WaitingForStart;

            /// <summary>
            /// Gets error happened during operation. Returns null if no error.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>AsyncOP_State.Completed</b> state.</exception>
            public Exception Error
            {
                get
                {
                    if (State == AsyncOpState.Disposed) throw new ObjectDisposedException(GetType().Name);
                    if (State != AsyncOpState.Completed)
                        throw new InvalidOperationException(
                            "Property 'Error' is accessible only in 'AsyncOP_State.Completed' state.");

                    return mPException;
                }
            }

            #endregion

            #region Events implementation

            /// <summary>
            /// Is called when asynchronous operation has completed.
            /// </summary>
            public event EventHandler<EventArgs<SwitchToSecureAsyncOp>> CompletedAsync;

            #region method OnCompletedAsync

            /// <summary>
            /// Raises <b>CompletedAsync</b> event.
            /// </summary>
            private void OnCompletedAsync()
            {
                CompletedAsync?.Invoke(this, new EventArgs<SwitchToSecureAsyncOp>(this));
            }

            #endregion

            #endregion
        }

        #endregion

        /// <summary>
        /// Starts switching connection to secure.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        /// <returns>Returns true if aynchronous operation is pending (The <see cref="SwitchToSecureAsyncOp.CompletedAsync"/> event is raised upon completion of the operation).
        /// Returns false if operation completed synchronously.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected or connection is already secure.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
        protected bool SwitchToSecureAsync(SwitchToSecureAsyncOp op)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
            if (!IsConnected) throw new InvalidOperationException("You must connect first.");
            if (IsSecureConnection) throw new InvalidOperationException("Connection is already secure.");
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (op.State != AsyncOpState.WaitingForStart)
                throw new ArgumentException(
                    "Invalid argument 'op' state, 'op' must be in 'AsyncOP_State.WaitingForStart' state.", nameof(op));

            return op.Start(this);
        }

        #endregion


        #region virtual method OnConnected

        /// <summary>
        /// This method is called after TCP client has successfully connected.
        /// </summary>
        protected virtual void OnConnected()
        {
        }

        /// <summary>
        /// Represents callback to be called when to complete connect operation.
        /// </summary>
        /// <param name="error">Exception happened or null if no errors.</param>
        protected delegate void CompleteConnectCallback(Exception error);

        /// <summary>
        /// This method is called when TCP client has successfully connected.
        /// </summary>
        /// <param name="callback">Callback to be called to complete connect operation.</param>
        protected virtual void OnConnected(CompleteConnectCallback callback)
        {
            try
            {
                OnConnected();

                callback(null);
            }
            catch (Exception x)
            {
                callback(x);
            }
        }

        #endregion


        #region method ReadLine

        /// <summary>
        /// Reads and logs specified line from connected host.
        /// </summary>
        /// <returns>Returns read line.</returns>
        protected string ReadLine()
        {
            var args = new SmartStream.ReadLineAsyncOP(new byte[32000], SizeExceededAction.JunkAndThrowException);
            TcpStream.ReadLine(args, false);
            if (args.Error != null) throw args.Error;
            var line = args.LineUtf8;
            if (args.BytesInBuffer > 0)
                LogAddRead(args.BytesInBuffer, line);
            else
                LogAddText("Remote host closed connection.");

            return line;
        }

        #endregion

        #region method WriteLine

        /// <summary>
        /// Sends and logs specified line to connected host.
        /// </summary>
        /// <param name="line">Line to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>line</b> is null reference.</exception>
        protected void WriteLine(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));

            var countWritten = TcpStream.WriteLine(line);
            LogAddWrite(countWritten, line);
        }

        #endregion


        #region mehtod LogAddRead

        /// <summary>
        /// Logs read operation.
        /// </summary>
        /// <param name="size">Number of bytes readed.</param>
        /// <param name="text">Log text.</param>
        protected internal void LogAddRead(long size, string text)
        {
            try
            {
                if (Logger == null) return;
                Logger.AddRead(
                    Id,
                    AuthenticatedUserIdentity,
                    size,
                    text,
                    LocalEndPoint,
                    RemoteEndPoint
                );
            }
            catch
            {
                // We skip all logging errors, normally there shouldn't be any.
            }
        }

        #endregion

        #region method LogAddWrite

        /// <summary>
        /// Logs write operation.
        /// </summary>
        /// <param name="size">Number of bytes written.</param>
        /// <param name="text">Log text.</param>
        protected internal void LogAddWrite(long size, string text)
        {
            try
            {
                if (Logger == null) return;
                Logger.AddWrite(
                    Id,
                    AuthenticatedUserIdentity,
                    size,
                    text,
                    LocalEndPoint,
                    RemoteEndPoint
                );
            }
            catch
            {
                // We skip all logging errors, normally there shouldn't be any.
            }
        }

        #endregion

        #region method LogAddText

        /// <summary>
        /// Logs free text entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        protected void LogAddText(string text)
        {
            try
            {
                if (Logger == null) return;
                Logger.AddText(
                    IsConnected ? Id : "",
                    IsConnected ? AuthenticatedUserIdentity : null,
                    text,
                    IsConnected ? LocalEndPoint : null,
                    IsConnected ? RemoteEndPoint : null
                );
            }
            catch
            {
                // We skip all logging errors, normally there shouldn't be any.
            }
        }

        #endregion

        #region method LogAddException

        /// <summary>
        /// Logs exception.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="x">Exception happened.</param>
        protected internal void LogAddException(string text, Exception x)
        {
            try
            {
                if (Logger == null) return;
                Logger.AddException(
                    IsConnected ? Id : "",
                    IsConnected ? AuthenticatedUserIdentity : null,
                    text,
                    IsConnected ? LocalEndPoint : null,
                    IsConnected ? RemoteEndPoint : null,
                    x
                );
            }
            catch
            {
                // We skip all logging errors, normally there shouldn't be any.
            }
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets TCP client logger. Value null means no logging.
        /// </summary>
        public Logger Logger { get; set; }


        /// <summary>
        /// Gets if TCP client is connected.
        /// </summary>
        public override bool IsConnected => mIsConnected;

        /// <summary>
        /// Gets session ID.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override string Id
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
                if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");

                return mId;
            }
        }

        /// <summary>
        /// Gets the time when session was connected.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override DateTime ConnectTime
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
                if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");

                return mConnectTime;
            }
        }

        /// <summary>
        /// Gets the last time when data was sent or received.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override DateTime LastActivity
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
                if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");

                return mPTcpStream.LastActivity;
            }
        }

        /// <summary>
        /// Gets session local IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override IPEndPoint LocalEndPoint
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
                if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");

                return mPLocalEp;
            }
        }

        /// <summary>
        /// Gets session remote IP end point.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override IPEndPoint RemoteEndPoint
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
                if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");

                return mPRemoteEp;
            }
        }

        /// <summary>
        /// Gets if this session TCP connection is secure connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override bool IsSecureConnection
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
                if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");

                return mIsSecure;
            }
        }

        /// <summary>
        /// Gets TCP stream which must be used to send/receive data through this session.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TCP client is not connected.</exception>
        public override SmartStream TcpStream
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("TCP_Client");
                if (!mIsConnected) throw new InvalidOperationException("TCP client is not connected.");

                return mPTcpStream;
            }
        }

        /// <summary>
        /// Gets or sets remote callback which is called when remote server certificate needs to be validated.
        /// Value null means not specified.
        /// </summary>
        public RemoteCertificateValidationCallback ValidateCertificateCallback
        {
            get => mPCertificateCallback;

            set => mPCertificateCallback = value;
        }

        /// <summary>
        /// Gets or sets default TCP read/write timeout.
        /// </summary>
        /// <remarks>This timeout applies only synchronous TCP read/write operations.</remarks>
        public int Timeout
        {
            get => mTimeout;

            set => mTimeout = value;
        }

        #endregion

        #region method OnError

        protected void OnError(Exception x)
        {
            try
            {
                if (Logger != null)
                {
                    //m_pLogger.AddException(x);
                }
            }
            catch
            {
                // ignored
            }
        }

        #endregion
    }
}