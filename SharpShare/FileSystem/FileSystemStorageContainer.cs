using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;
using System.IO;

namespace SharpShare.FileSystem {
    public class FileSystemStorageContainer : FileSystemStorageItem, IStorageContainer {
        public FileSystemStorageContainer(string filename, FileSystemStorageProvider provider)
            : base(filename, provider) {

            if ((this.FileInfo.Attributes & System.IO.FileAttributes.Directory) == 0) {
                throw new ArgumentException("Expected directory.");
            }

        }

        public override IStorageContainer Parent {
            get {
                FileSystemStorageProvider fileProvider = (FileSystemStorageProvider)this.Provider;
                if (this.DirectoryInfo.Parent.FullName == fileProvider.DirectoryInfo.FullName) {
                    return this.Provider;
                }

                return new FileSystemStorageContainer(this.DirectoryInfo.Parent.FullName, fileProvider);
            }
        }
        public override StorageItemAttributes Attributes {
            get {
                StorageItemAttributes attrs = StorageItemAttributes.None;

                if ((this.DirectoryInfo.Attributes & FileAttributes.Hidden) != 0) {
                    attrs |= StorageItemAttributes.Hidden;
                }

                return attrs;
            }
        } 
        public DirectoryInfo DirectoryInfo {
            get { return new DirectoryInfo(this.FileInfo.FullName); }
        }

        public IStorageItem Content(string name) {
            string path = Path.Combine(this.DirectoryInfo.FullName, name);

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            if (directoryInfo.Exists) {
                return new FileSystemStorageContainer(path, (FileSystemStorageProvider)this.Provider);
            }

            FileInfo fileInfo = new FileInfo(path);

            if (!fileInfo.Exists) {
                return null;
            }

            return new FileSystemStorageFile(path, (FileSystemStorageProvider)this.Provider);
        }
        public IStorageContainer CreateContainer(string name) {
            return new FileSystemStorageContainer(this.DirectoryInfo.CreateSubdirectory(name).FullName, (FileSystemStorageProvider)this.Provider);
        }
        public IStorageFile CreateFile(string name) {
            string filename = System.IO.Path.Combine(this.DirectoryInfo.FullName, name);
            if (System.IO.File.Exists(filename)) {
                return new FileSystemStorageFile(filename, (FileSystemStorageProvider)this.Provider);
            }
            System.IO.File.WriteAllBytes(filename, new byte[] { });

            return new FileSystemStorageFile(filename, (FileSystemStorageProvider)this.Provider);
        }
        public IEnumerable<IStorageItem> ListContents() {
            foreach (var folder in this.DirectoryInfo.GetDirectories()) {
                if ((folder.Attributes & FileAttributes.System) != 0) {
                    continue;
                }

                yield return new FileSystemStorageContainer(folder.FullName, (FileSystemStorageProvider)this.Provider);
            }

            foreach (var file in this.DirectoryInfo.GetFiles()) {
                if ((file.Attributes & FileAttributes.System) != 0) {
                    continue;
                }

                yield return new FileSystemStorageFile(file.FullName, (FileSystemStorageProvider)this.Provider);
            }
        }
        public override void Delete() {
            this.DirectoryInfo.Delete();
        }
    }
}
