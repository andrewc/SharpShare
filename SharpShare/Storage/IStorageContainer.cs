using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Storage {
    public interface IStorageContainer : IStorageItem {
        IQueryable<IStorageItem> Contents();
        IStorageItem Content(string name);
        IStorageContainer CreateContainer(string name);
        IStorageFile CreateFile(string name);
    }
}
