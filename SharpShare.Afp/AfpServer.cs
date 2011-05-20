using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using SharpShare.Storage;
using SharpShare.Afp.Protocol;
using System.IO;
using System.Collections.ObjectModel;
using Bonjour;
using System.Threading;

namespace SharpShare.Afp {
    public class AfpServer : IShareServer, IAfpServer {
        private Guid _signature;
        private List<AfpSession> _sessions = new List<AfpSession>();
        private List<IStorageProvider> _shares = new List<IStorageProvider>();
        private short _currentForkId = 1;
        private Dictionary<IStorageFile, List<AfpOpenFileInfo>> _openFiles = new Dictionary<IStorageFile, List<AfpOpenFileInfo>>();
        private Socket _listenSocket;
        private IDNSSDService _netService;
        private DNSSDEventManager _netServiceEventManager;

        public AfpServer(string name) {
            this.Name = name;
            _signature = Guid.NewGuid();
        }

        public void Start() {
            if (_listenSocket != null) {
                throw new Exception("Server is already started.");
            }

            new Thread(NetServiceWorker).Start();

            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, 548));
            _listenSocket.Listen(10);

            _listenSocket.BeginAccept(ConnectionReceived, null);
        }

        void _netServiceEventManager_DomainLost(DNSSDService service, DNSSDFlags flags, uint ifIndex, string domain) {
        
        }

        void _netServiceEventManager_ServiceRegistered(DNSSDService service, DNSSDFlags flags, string name, string regtype, string domain) {
   
        }

        void _netServiceEventManager_ServiceLost(DNSSDService browser, DNSSDFlags flags, uint ifIndex, string serviceName, string regtype, string domain) {
          
        }

        public void Stop() {
            if (_listenSocket == null) {
                throw new Exception("Server is not started.");
            }

            _listenSocket.Close();
            _listenSocket = null;
        }

        public string Name {
            get;
            private set;
        }

        public string[] Versions {
            get { return new[] { "AFP3.3" }; }
        }
        public Guid Signature {
            get { return _signature; }
        }

        public ReadOnlyCollection<IShareSession> Sessions {
            get { return _sessions.Cast<IShareSession>().ToList().AsReadOnly(); }
        }
        public ReadOnlyCollection<IStorageProvider> Shares {
            get {
                return _shares.AsReadOnly();
            }
        }

        public void AddShare(IStorageProvider provider) {
            _shares.Add(provider);
        }
        public void RemoveShare(IStorageProvider provider) {
            _shares.Remove(provider);
        }

        private void AddSession(AfpSession session) {
            lock (_sessions) {
                _sessions.Add(session);
            }
        }
        internal void RemoveSession(AfpSession session) {
            lock (_sessions) {
                //_sessions.Remove(session);

                Console.WriteLine("Session {0} removed.", session.Token);
            }
        }

        internal AfpSession FindSession(Guid token) {
            lock (_sessions) {
                return _sessions
                    .Where(s => s.Token == token)
                    .FirstOrDefault();
            }
        }
        internal void CloseFork(AfpOpenFileInfo info) {
            lock (_openFiles) {
                var list = _openFiles[info.File];

                info.Stream.Dispose();

                list.Remove(info);
            }
        }
        internal AfpOpenFileInfo OpenFork(IAfpSession session, IStorageFile file, AfpAccessModes accessMode) {
            lock (_openFiles) {
                List<AfpOpenFileInfo> infos = null;

                if (_openFiles.ContainsKey(file)) {
                    infos = _openFiles[file];
                }
                else {
                    infos = new List<AfpOpenFileInfo>();
                    _openFiles[file] = infos;
                }

                Stream stream = file.Open();

                AfpOpenFileInfo fileInfo = new AfpOpenFileInfo(
                    _currentForkId++,
                    accessMode,
                    session,
                    file,
                    stream);

                infos.Add(fileInfo);

                return fileInfo;
            }
        }
        internal AfpOpenFileInfo FindFork(short identifier) {
            lock (_openFiles) {
                return _openFiles
                    .SelectMany(o => o.Value)
                    .Where(o => o.Identifier == identifier)
                    .FirstOrDefault();
            }
        }

        private void NetServiceWorker(object state) {
            _netService = new DNSSDService();
            _netServiceEventManager = new DNSSDEventManager();
            _netServiceEventManager.ServiceLost += new _IDNSSDEvents_ServiceLostEventHandler(_netServiceEventManager_ServiceLost);
            _netServiceEventManager.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(_netServiceEventManager_ServiceRegistered);
            _netServiceEventManager.DomainLost += new _IDNSSDEvents_DomainLostEventHandler(_netServiceEventManager_DomainLost);

           IDNSSDService service = _netService.Register(DNSSDFlags.kDNSSDFlagsDefault, 0, this.Name, "_afpovertcp._tcp", "", "", 548, null, _netServiceEventManager);

            while (_listenSocket != null) {
                service = service.Register(DNSSDFlags.kDNSSDFlagsDefault, 0, this.Name, "_afpovertcp._tcp", "", "", 548, null, _netServiceEventManager);
                Thread.Sleep(5000);
            }
        }


        private void ConnectionReceived(IAsyncResult ar) {
            Socket newSocket = _listenSocket.EndAccept(ar);

            AfpTransport transport = new AfpTransport(newSocket);
            transport.CommandReceived += CommandReceived;

            _listenSocket.BeginAccept(ConnectionReceived, null);
        }
        private void CommandReceived(object sender, AfpTransportCommandReceivedEventArgs eventArgs) {
            AfpTransport transport = (AfpTransport)sender;

            switch (eventArgs.Header.command) {
                case DsiCommand.GetStatus:
                    AfpStream statusStream = new AfpStream();

                    transport.SendReply(eventArgs.Header, AfpResultCode.FPNoErr, this.WriteServerInfo());

                    break;
                case DsiCommand.OpenSession:
                    transport.CommandReceived -= CommandReceived;

                    AfpSession session = new AfpSession(this, transport);
                    this.AddSession(session);

                    AfpStream sessionStream = new AfpStream();
                    sessionStream.WriteUInt8(0);
                    sessionStream.WriteUInt8(0);
                    sessionStream.WriteUInt32(10485760);

                    transport.SendReply(eventArgs.Header, AfpResultCode.FPNoErr, sessionStream.ToByteArray());

                    break;
                case DsiCommand.Command:
                    transport.Close();
                    break;
            }
        }

        private byte[] WriteServerInfo() {
            AfpServer server = this;
            AfpStream responseStream = new AfpStream();
            string[] uams = new[] { AfpUams.kNoUserAuthStr };
            AfpServerFlagsBitmap serverFlags =
                AfpServerFlagsBitmap.kSupportsSrvrSig |
                AfpServerFlagsBitmap.kSupportsTCP;

            responseStream.WriteMark("MachineType");
            responseStream.WriteMark("AFPVersionsCount");
            responseStream.WriteMark("UAMCount");
            responseStream.WriteMark("VolumeIconAndMask");

            responseStream.WriteEnum(serverFlags);
            responseStream.WritePascalString(server.Name);

            responseStream.WriteMark("ServerSignature");
            responseStream.WriteMark("NetworkAddressesCount");
            responseStream.WriteMark("DirectoryNamesCount");
            //responseStream.WriteMark("UTF8ServerName");

            responseStream.BeginMark("MachineType");
            responseStream.WritePascalString("Xserve");

            responseStream.BeginMark("AFPVersionsCount");
            responseStream.WriteUInt8((byte)server.Versions.Length);
            foreach (string version in server.Versions) {
                responseStream.WritePascalString(version);
            }

            responseStream.BeginMark("UAMCount");
            responseStream.WriteUInt8((byte)uams.Length);
            foreach (string uam in uams) {
                responseStream.WritePascalString(uam);
            }

            responseStream.BeginMark("ServerSignature");
            responseStream.WriteBytes(server.Signature.ToByteArray().Take(16).ToArray());

            responseStream.BeginMark("NetworkAddressesCount");
            responseStream.WriteUInt8(0);

            responseStream.BeginMark("DirectoryNamesCount");
            responseStream.WriteUInt8(0);

            //responseStream.BeginMark("UTF8ServerName");
            //responseStream.WriteUTF8String(server.Name);

            responseStream.BeginMark("VolumeIconAndMask");
            responseStream.WriteBytes(new byte[256]);

            return responseStream.ToByteArray();
        }

    }
}
