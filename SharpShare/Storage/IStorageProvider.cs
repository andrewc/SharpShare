using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Storage {
    public interface IStorageProvider : IStorageContainer {
        /// <summary>
        /// Occurs when anything in this provider changes.
        /// </summary>
        event EventHandler ContentsChanged;

        ulong AvailableBytes { get; }
        ulong TotalBytes { get; }

        IStorageItem Find(string identifier);
    }
}
