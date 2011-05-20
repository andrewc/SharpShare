using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;
using System.IO;

namespace SharpShare.FileSystem {
    public abstract class FileSystemStorageItem : IStorageItem {
        private FileInfo _fileInfo;
        private FileSystemStorageProvider _provider;

        protected FileSystemStorageItem(string filename, FileSystemStorageProvider provider) {
            _fileInfo = new FileInfo(filename);
            _provider = provider;
        }

        public FileInfo FileInfo { get { return _fileInfo; } }

        public virtual int Identifier {
            get {
                return this.FileInfo.FullName.GetHashCode();
            }
        }
        public virtual string Name {
            get { return this.FileInfo.Name; }
        }

        public DateTime DateCreated { get { return this.FileInfo.CreationTime; } }
        public DateTime DateModified { get { return this.FileInfo.LastWriteTime; } }

        public virtual IStorageProvider Provider { get { return _provider; } }

        public virtual IStorageContainer Parent {
            get {
                FileSystemStorageProvider fileProvider = (FileSystemStorageProvider)this.Provider;
                if (this.FileInfo.Directory.FullName == fileProvider.DirectoryInfo.FullName) {
                    return this.Provider;
                }

                return new FileSystemStorageContainer(this.FileInfo.Directory.FullName, fileProvider);
            }
        }
        public virtual StorageItemAttributes Attributes {
            get {
                StorageItemAttributes attrs = StorageItemAttributes.None;

                if ((this.FileInfo.Attributes & FileAttributes.Hidden) != 0) {
                    attrs |= StorageItemAttributes.Hidden;
                }

                return attrs;
            }
        }
        public abstract void Delete();

        public void Move(IStorageContainer container) {
            FileSystemStorageContainer fsContainer = container as FileSystemStorageContainer;

            if (fsContainer == null) {
                throw new ArgumentException("Can only move to a file system storage container.");
            }

            if (this.FileInfo.Directory.FullName == fsContainer.DirectoryInfo.FullName) {
                return;
            }

            this.FileInfo.MoveTo(Path.Combine(fsContainer.DirectoryInfo.FullName, this.FileInfo.Name));
        }
        public void Rename(string name) {
            this.FileInfo.MoveTo(Path.Combine(Path.GetDirectoryName(this.FileInfo.FullName), name));
        }
        public override int GetHashCode() {
            return this.FileInfo.FullName.GetHashCode();
        }
        public override bool Equals(object obj) {
            FileSystemStorageItem other = (obj as FileSystemStorageItem);

            if (other == null) {
                return false;
            }

            return other.FileInfo.FullName == this.FileInfo.FullName;
        }
    }
}
