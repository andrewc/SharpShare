using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpShare.Storage {
    public interface IStorageFile : IStorageItem {
        long Length { get; }

        Stream Open();
    }
}
