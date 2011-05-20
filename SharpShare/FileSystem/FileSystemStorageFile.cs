using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;
using System.IO;

namespace SharpShare.FileSystem {
    public class FileSystemStorageFile : FileSystemStorageItem, IStorageFile {
        public FileSystemStorageFile(string filename, FileSystemStorageProvider provider)
            : base(filename, provider) {
            if ((this.FileInfo.Attributes & System.IO.FileAttributes.Directory) != 0) {
                throw new ArgumentException("Expected a file.");
            }
        }

        public long Length {
            get { return this.FileInfo.Length; }
        }

        public Stream Open() {
            return this.FileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public override void Delete() {
            this.FileInfo.Delete();
        }
    }
}
