using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SharpShare.Afp.Protocol;
using System.IO;
using SharpShare.Storage;

namespace SharpShare.Afp {
    public sealed class AfpSession : IAfpSession {
        private Guid _token;
        private bool _justTransferred;
        private AfpTransport _transport;
        private Dictionary<IStorageProvider, ushort> _volumeIds = new Dictionary<IStorageProvider, ushort>();
        private Dictionary<IStorageContainer, uint> _directoryIds = new Dictionary<IStorageContainer, uint>();
        private Dictionary<IStorageItem, uint> _nodeIds = new Dictionary<IStorageItem, uint>();
        private List<IStorageProvider> _openVolumes = new List<IStorageProvider>();
        private object _synclock = new object();

        public AfpSession(AfpServer server, AfpTransport transport) {
            this.Server = server;
            this.Transport = transport;
        }

        public AfpServer Server { get; private set; }
        public Socket Socket { get; private set; }
        public Guid Token { get { return _token; } }
        public AfpTransport Transport {
            get {
                return _transport;
            }
            set {
                if (_transport != null) {
                    _transport.CommandReceived -= CommandReceived;
                }

                _transport = value;

                if (_transport != null) {
                    _transport.CommandReceived += CommandReceived;
                }
            }
        }

        public bool Opened { get; private set; }

        public void Close() {
            if (this.Transport != null) {
                this.Transport.Close();
            }

            this.Server.RemoveSession(this);
        }

        private void CommandReceived(object sender, AfpTransportCommandReceivedEventArgs eventArgs) {
            switch (eventArgs.Header.command) {
                case DsiCommand.CloseSession:
                    this.Transport.SendReply(eventArgs.Header, AfpResultCode.FPNoErr, new byte[] { });

                    break;
                case DsiCommand.Write:
                case DsiCommand.Command:
                    AfpResultCode resultCode = AfpResultCode.FPCallNotSupported;

                    byte commandCode = eventArgs.Payload[0];
                    IAfpRequestHandler handler = AfpRequestHandler.Find(commandCode);

                    AfpStream responseStream = new AfpStream();

                    //if (!(handler is Afp.Protocol.Handlers.AfpReadRequestHandler) &&
                    //    !(handler is Afp.Protocol.Handlers.AfpWriteExtRequestHandler)) {
                    //    Console.WriteLine("AFP Command: {0} ({1})", commandCode, handler == null ? "no handler found" : handler.GetType().Name);
                    //}

                    if (handler != null) {
                        AfpStream requestStream = new AfpStream(eventArgs.Payload);
                        requestStream.ReadUInt8(); // Command code

                        resultCode = handler.Process(
                            this, eventArgs.Header, requestStream, responseStream);
                    } else {
                        Console.WriteLine("AFP Command: {0} ({1})", commandCode, handler == null ? "no handler found" : handler.GetType().Name);
                    }

                    this.Transport.SendReply(eventArgs.Header, resultCode, responseStream.ToByteArray());
                    if (_justTransferred) {
                        this.Transport = null;
                        this.Close();
                    }

                    break;
            }
        }

        #region IAfpSession Members

        internal ushort GetVolumeIdentifier(IStorageProvider provider) {
            if (_volumeIds.ContainsKey(provider)) {
                return _volumeIds[provider];
            }

            return _volumeIds[provider] = (ushort)(_volumeIds.Count + 500);
        }

        internal Guid AssignToken() {
            if (_token == Guid.Empty) {
                _token = Guid.NewGuid();
            }

            return _token;
        }

        internal uint GetNodeIdentifier(IStorageItem item) {
            if (_nodeIds.ContainsKey(item)) {
                return _nodeIds[item];
            }

            return _nodeIds[item] = (ushort)(_nodeIds.Count + 200);
        }
        internal IStorageItem GetNode(uint id) {
            return _nodeIds.Where(n => n.Value == id).Select(n => n.Key).FirstOrDefault();
        }
        internal IStorageProvider GetVolume(ushort id) {
            foreach (var pair in _volumeIds) {
                if (pair.Value == id) {
                    if (!_openVolumes.Contains(pair.Key)) {
                        return null;
                    }

                    return pair.Key;
                }
            }

            return null;
        }

        internal AfpOpenFileInfo OpenFork(IStorageFile file, AfpAccessModes accessMode) {
            return this.Server.OpenFork(this, file, accessMode);
        }
        internal AfpOpenFileInfo FindFork(short identifier) {
            return this.Server.FindFork(identifier);
        }
        internal void CloseFork(AfpOpenFileInfo info) {
            this.Server.CloseFork(info);
        }
        internal IList<IStorageProvider> OpenVolumes {
            get {
                return _openVolumes;
            }
        }
        internal string UsingAfpVersion {
            get;
            set;
        }

        internal string UsingUam {
            get;
            set;
        }

        internal IAfpSession TransferTo(Guid token) {
            lock (_synclock) {
                AfpSession otherSession = this.Server.FindSession(token);

                if (otherSession != null) {
                    AfpTransport oldTransport = otherSession.Transport;
                    oldTransport.Close();

                    otherSession.Transport = this.Transport;

                    _justTransferred = true;
                }

                return otherSession;
            }
        }

        #endregion
    }
}
