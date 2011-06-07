using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpShare.Storage {
    [Flags]
    public enum StorageFileAccess {
        None = 0,
        Read = 2,
        Write = 4,
        ReadWrite = (Read | Write)
    };

    public interface IStorageFile : IStorageItem {
        long Length { get; }

        IStorageDataProvider Open(StorageFileAccess access, StorageFileAccess share);
    }
}
