using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using SharpShare.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SharpShare.Afp.Protocol {
    public delegate void AfpTransportReplyHandler(DsiHeader header, byte[] payload);

    public sealed class AfpTransport : ILogProvider {
        public event EventHandler<AfpTransportCommandReceivedEventArgs> CommandReceived = delegate { };
        public event EventHandler<AfpTransportCommandReceivedEventArgs> CommandSent = delegate { };
        public event EventHandler Closed = delegate { };

        private byte[] _buffer = new byte[1];
        private MemoryStream _currentHeader = new MemoryStream();
        private ConcurrentDictionary<ushort, AfpTransportReplyHandler> _replyHandlers = new ConcurrentDictionary<ushort, AfpTransportReplyHandler>();
        private bool _closed;
        private ushort _requestId;
        private object _synclock = new object();
        private Timer _tickleTimer;
        private string _name;

        public AfpTransport(Socket socket) {
            _name = socket.RemoteEndPoint.ToString();

            this.Socket = socket;
            this.BeginReceive();

            _tickleTimer = new Timer(TickleFired, null, 20000, -1);
        }

        private Socket Socket { get; set; }

        public void SendReply(DsiHeader header, AfpResultCode resultCode, byte[] payload) {
            AfpStream finalStream = new AfpStream();

            DsiHeader replyHeader = header.WriteReply(resultCode, payload, finalStream);
            this.OnCommandSent(replyHeader, payload);

            byte[] result = finalStream.ToByteArray();

            this.SendBuffer(result);
        }

        public void SendRequest(DsiCommand command, byte[] payload, AfpTransportReplyHandler replyHandler = null) {
            DsiHeader header = new DsiHeader() {
                command = command,
                flags = DsiFlags.Request,
                requestId = this.NextRequestId(),
                errorCodeOrWriteOffset = 0,
                totalDataLength = (uint)payload.Length
            };

            AfpStream stream = new AfpStream();
            header.Write(stream);
            stream.WriteBytes(payload);

            if (replyHandler != null) {
                _replyHandlers[header.requestId] = replyHandler;
            }

            byte[] result = stream.ToByteArray();

            this.SendBuffer(result);
        }

        private void SendBuffer(byte[] buffer) {
            this.Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, DataSent, null);
        }

        public void Close() {
            lock (_synclock) {
                if (_closed) {
                    return;
                }

                Log.Add(this, EntryType.Information, "Connection '{0}' closed.", this);

                _closed = true;

                if (this.Socket.Connected) {
                    try {
                        this.Socket.Shutdown(SocketShutdown.Both);
                    } catch { }

                    try {
                        this.Socket.Disconnect(false);
                    } catch { }
                }

                this.Socket.Dispose();

                this.OnClosed();
            }
        }

        private void DataSent(IAsyncResult ar) {
            try {
                this.Socket.EndSend(ar);
            } catch (Exception ex) {
                Log.Add(this, EntryType.Error, "Transport '{0}' socket send exception: {1}", this, ex);
            }
        }
        private void DataReceived(IAsyncResult ar) {
            try {
                lock (_synclock) {
                    if (_closed) {
                        return;
                    }

                    int bytesReceived = this.Socket.EndReceive(ar);

                    if (bytesReceived == 0) {
                        this.Close();
                        return;
                    }

                    _currentHeader.WriteByte(_buffer[0]);

                    if (_currentHeader.Length == 16) {
                        _currentHeader.Position = 0;
                        DsiHeader header = DsiHeader.Read(_currentHeader);

                        byte[] payload = new byte[header.totalDataLength];

                        for (int currentOffset = 0; currentOffset < payload.Length; ) {
                            bytesReceived = this.Socket.Receive(payload, currentOffset, payload.Length - currentOffset, SocketFlags.None);

                            if (bytesReceived == 0) {
                                this.Close();
                                return;
                            }

                            currentOffset += bytesReceived;
                        }

                        Task.Factory.StartNew(() => {
                            this.OnCommandReceived(header, payload);
                        });

                        _currentHeader.SetLength(0);
                    }

                    this.BeginReceive();
                }
            } catch {
                this.Close();
            }
        }

        private void BeginReceive() {
            this.Socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, DataReceived, null);
        }

        private ushort NextRequestId() {
            lock (_synclock) {
                ushort id = _requestId;

                if (_requestId == ushort.MaxValue) {
                    _requestId = 0;
                } else {
                    _requestId++;
                }

                return id;
            }
        }

        private void TickleFired(object state) {
            lock (_synclock) {
                if (_closed) {
                    return;
                }

                try {
                    this.SendRequest(DsiCommand.Tickle, new byte[0]);
                } catch { }

                _tickleTimer.Change(20000, -1);
            }
        }

        private void OnCommandSent(DsiHeader header, byte[] payload) {
            AfpTransportCommandReceivedEventArgs args = new AfpTransportCommandReceivedEventArgs(header, payload);
            try {
                CommandSent(this, args);
            } catch { }
        }

        private void OnCommandReceived(DsiHeader header, byte[] payload) {
            if (header.flags == DsiFlags.Reply) {
                // Find reply handler.
                AfpTransportReplyHandler handler = null;

                if (!_replyHandlers.TryRemove(header.requestId, out handler)) {
                    // BUG? Request ID flipped in replies from Mac OS X Snow Leopard?
                    byte[] requestIdData = BitConverter.GetBytes(header.requestId);
                    Array.Reverse(requestIdData);
                    header.requestId = BitConverter.ToUInt16(requestIdData, 0);

                    _replyHandlers.TryRemove(header.requestId, out handler);
                }

                if (handler != null) {
                    try {
                        handler(header, payload);
                    } catch { }

                    return;
                }
            }

            switch (header.command) {
                case DsiCommand.Tickle:

                    break;
                default: {
                        AfpTransportCommandReceivedEventArgs args = new AfpTransportCommandReceivedEventArgs(header, payload);
                        try {
                            CommandReceived(this, args);
                        } catch { }

                        break;
                    }
            }
        }

        private void OnClosed() {
            try {
                Closed(this, EventArgs.Empty);
            } catch {

            }
        }

        public override string ToString() {
            return _name;
        }

        #region ILogProvider Members

        string ILogProvider.Name {
            get { return "AFP Transport"; }
        }

        #endregion
    }

    public sealed class AfpTransportCommandReceivedEventArgs : EventArgs {
        public AfpTransportCommandReceivedEventArgs(DsiHeader header, byte[] payload) {
            this.Header = header;
            this.Payload = payload;
        }

        public DsiHeader Header { get; private set; }
        public byte[] Payload { get; private set; }
    }
}
