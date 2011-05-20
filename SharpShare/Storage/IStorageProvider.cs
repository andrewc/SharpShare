using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Storage {
    public interface IStorageProvider : IStorageContainer {
        ulong AvailableBytes { get; }
        ulong TotalBytes { get; }
    }
}
