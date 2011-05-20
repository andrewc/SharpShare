using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using SharpShare.FileSystem;

namespace SharpShare.Service {
    public partial class Service : ServiceBase {
        private IShareServer _server;

        public Service() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            _server = new SharpShare.Afp.AfpServer("Skynet");

            FileSystemStorageProvider provider = new FileSystemStorageProvider("X:\\", "Data");

            _server.AddShare(provider);

            provider = new FileSystemStorageProvider("X:\\Backups\\Time Capsule", "Time Capsule");

            _server.AddShare(provider);

            _server.Start();
        }

        protected override void OnStop() {
            _server.Stop();
        }
    }
}
