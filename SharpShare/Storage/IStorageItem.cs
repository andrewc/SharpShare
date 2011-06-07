using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.AccessControl;

namespace SharpShare.Storage {
    [Flags]
    public enum StorageItemAttributes {
        None = 0,
        Hidden = 2
    }
    [Flags]
    public enum StorageItemType {
        Invalid = 0,
        File = 1,
        Container = 2,
        All = (File | Container)
    };

    public interface IStorageItem {
        IStorageProvider Provider { get; }
        string Name { get; }
        string Identifier { get; }
        bool Exists { get; }
        IStorageContainer Parent { get; }
        StorageItemAttributes Attributes { get; set; }
        DateTime DateCreated { get; }
        DateTime DateModified { get; }

        void Delete();
        void Rename(string name);
        void Move(IStorageContainer container);
    }
}
