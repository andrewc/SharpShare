using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Storage {
    [Flags]
    public enum StorageItemAttributes {
        None = 0,
        Hidden = 2
    }
    public interface IStorageItem {
        IStorageProvider Provider { get; }
        string Name { get; }
        IStorageContainer Parent { get; }
        StorageItemAttributes Attributes { get; }
        DateTime DateCreated { get; }
        DateTime DateModified { get; }

        void Delete();
        void Rename(string name);
        void Move(IStorageContainer container);
    }
}
