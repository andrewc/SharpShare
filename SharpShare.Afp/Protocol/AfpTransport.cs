using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace SharpShare.Afp.Protocol {
    public sealed class AfpTransport {
        public event EventHandler<AfpTransportCommandReceivedEventArgs> CommandReceived = delegate { };

        private byte[] _buffer = new byte[1];
        private MemoryStream _currentHeader = new MemoryStream();
        private bool _closed;
        private ushort _requestId;
        private object _synclock = new object();
        private Timer _tickleTimer;

        public AfpTransport(Socket socket) {
            this.Socket = socket;
            this.BeginReceive();

            _tickleTimer = new Timer(TickleFired, null, 20000, -1);
        }

        private Socket Socket { get; set; }

        private void TickleFired(object state) {
            if (_closed) {
                return;
            }

            this.SendRequest(DsiCommand.Tickle, new byte[0]);

            _tickleTimer.Change(20000, -1);
        }
        public void SendReply(DsiHeader header, AfpResultCode resultCode, byte[] payload) {
            lock (_synclock) {
                AfpStream finalStream = new AfpStream();

                header.WriteReply(resultCode, payload, finalStream);

                byte[] result = finalStream.ToByteArray();

                this.Socket.Send(result);
            }
        }
        public void SendRequest(DsiCommand command, byte[] payload) {
            DsiHeader header = new DsiHeader() {
                command = command,
                flags = DsiFlags.Request,
                requestId = ++_requestId,
                errorCodeOrWriteOffset = 0,
                totalDataLength = (uint)payload.Length
            };

            AfpStream stream = new AfpStream();
            header.Write(stream);
            stream.WriteBytes(payload);

            lock (_synclock) {
                this.Socket.Send(stream.ToByteArray());
            }
        }

        public void Close() {
            if (_closed) {
                return;
            }

            _closed = true;
            this.Socket.Close();
        }
        private void DataReceived(IAsyncResult ar) {
            try {
                lock (_synclock) {
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

                        new Action(() => {
                            this.OnCommandReceived(header, payload);
                        }).BeginInvoke(null, null);

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

        private void OnCommandReceived(DsiHeader header, byte[] payload) {
            if (header.command == DsiCommand.Tickle) {

            } else {
                AfpTransportCommandReceivedEventArgs args = new AfpTransportCommandReceivedEventArgs(header, payload);
                try {
                    CommandReceived(this, args);
                } catch {

                }
            }
        }
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
