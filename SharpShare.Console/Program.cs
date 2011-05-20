using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Afp;
using SharpShare.FileSystem;

namespace SharpShare.ConsoleTest {
    class Program {
        static void Main(string[] args) {
            IShareServer server = new AfpServer("Skynet");

            FileSystemStorageProvider provider = new FileSystemStorageProvider("X:\\", "Data");

            server.AddShare(provider);

            provider = new FileSystemStorageProvider("X:\\Backups\\Time Capsule", "Time Capsule");

            server.AddShare(provider);

            server.Start();

            Console.ReadKey();

            server.Stop();
        }
    }
}
