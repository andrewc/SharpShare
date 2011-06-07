using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Afp;
using System.Security.AccessControl;
using System.Security.Principal;
using SharpShare.Diagnostics;
using SharpShare.Management;
using SharpShare.Management.Configuration;

namespace SharpShare.ConsoleTest {
    class Program {
        private static object SyncLock = new object();
        private static ShareManager _shareManager;

        static void Main(string[] args) {
            try {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BufferHeight = 120;
                Console.BufferWidth = 190;
                Console.WindowHeight = 60;
                Console.WindowWidth = 180;
            } catch { }

            Console.Clear();

            Log.EntryAdded += new Log.EntryAddedHandler(Log_EntryAdded);

            _shareManager = new ShareManager();

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

            //IShareServer server = _shareManager.Servers[0];

            //server.AuthenticationProvider = new TestAuth();

            //AfpShareServer afpShareServer = (AfpShareServer)server;
            //afpShareServer.AuthenticationMethods.Clear();
            //afpShareServer.AuthenticationMethods.Add(new Afp.Protocol.Security.AfpCleartextUserAuthenticationMethod());

            for (; ; ) {
                string line = Console.ReadLine();
                if (line == "x") {
                    if (_shareManager.IsRunning) {
                        Console.WriteLine("Stopping...");
                        _shareManager.Stop();
                        Console.WriteLine("Stopped.");
                    } else {
                        Console.WriteLine("Starting...");
                        _shareManager.Start();
                    }
                } else {
                    break;
                }
            }

            _shareManager.Stop();
        }

        private static void Log_EntryAdded(LogEntry entry) {
            lock (SyncLock) {
                if (entry.Type == EntryType.Debug) {
                    return;
                }

                ConsoleColor color = ConsoleColor.Black;

                string type = null;
                if (entry.Type == EntryType.Information)
                    type = "INFO";
                else
                    type = entry.Type.ToString().ToUpper();

                switch (entry.Type) {
                    case EntryType.Information:
                        color = ConsoleColor.DarkGreen;
                        break;
                    case EntryType.Debug:
                        color = ConsoleColor.DarkBlue;
                        break;
                    case EntryType.Warning:
                        color = ConsoleColor.Red;
                        break;
                    case EntryType.Error:
                        color = ConsoleColor.DarkRed;
                        break;
                }

                string sender = "Unknown";
                if (entry.Sender != null) {
                    if (entry.Sender is ILogProvider)
                        sender = (entry.Sender as ILogProvider).Name;
                    else
                        sender = entry.Sender.ToString();
                }

                if (sender.Length > 15)
                    sender = sender.Substring(0, 15);

                Console.Write("{0,-16}", sender);
                Console.ForegroundColor = color;
                Console.Write("{0,8}  ", type);
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("{0}", entry.Message);
                Console.WriteLine();

                if (entry.Exception != null) {
                    Console.WriteLine();
                    Console.Write(entry.Exception.ToString());
                    Console.WriteLine();
                }
                if (entry.DebugObject != null) {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.WriteLine();
                    Console.Write(entry.DebugObject.ToString());
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Black;
                }
            }
        }

    }
}
