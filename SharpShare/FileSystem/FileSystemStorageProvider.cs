using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;
using System.IO;

namespace SharpShare.FileSystem {
    public class FileSystemStorageProvider : FileSystemStorageContainer, IStorageProvider {
        private DriveInfo _driveInfo;
        private string _name;

        public FileSystemStorageProvider(string directory, string name)
            : base(directory, null) {
            _name = name;
            _driveInfo = new DriveInfo(this.FileInfo.FullName);
        }

        public override string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return _driveInfo.Name;
                }

                return _name;
            }
        }
        public override IStorageProvider Provider {
            get {
                return this;
            }
        }

        public override IStorageContainer Parent {
            get {
                return null;
            }
        }
        public override StorageItemAttributes Attributes {
            get {
                return StorageItemAttributes.None;
            }
        }
        public short ProviderIdentifier {
            get {
                return 5;
            }
        }
        public ulong AvailableBytes {
            get {
                return (ulong)_driveInfo.AvailableFreeSpace;
            }
        }

        public ulong TotalBytes {
            get { return (ulong)_driveInfo.TotalSize; }
        }
    }
}
