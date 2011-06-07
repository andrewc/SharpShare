using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using SharpShare.Management;
using SharpShare.Management.Configuration;

namespace SharpShare.Service {
    public partial class Service : ServiceBase {
        private ShareManager _shareManager = new ShareManager();

        public Service() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            _shareManager.Start();

            ShareConfiguration dataShare = new FileSystemShareConfiguration() {
                Name = "Data",
                Path = "X:\\"
            };

            ShareConfiguration backupShare = new FileSystemShareConfiguration() {
                Name = "Time Capsule",
                Path = "X:\\Backups\\Time Capsule"
            };

            ServerConfiguration afpServer = new AppleFilingProtocolServerConfiguration() {
                Name = "Skynet",
                TimeMachineShares = { "Time Capsule" }
            };

            ServiceConfiguration service = new ServiceConfiguration() {
                Servers = { afpServer },
                Shares = { dataShare, backupShare }
            };

            _shareManager.Configuration = service;
        }

        protected override void OnStop() {
            _shareManager.Stop();
        }
    }
}
