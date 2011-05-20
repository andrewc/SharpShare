using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;
using System.Collections.ObjectModel;

namespace SharpShare {
    public interface IShareServer {
        void Start();
        void Stop();

        string Name { get; }

        ReadOnlyCollection<IStorageProvider> Shares { get; }
        ReadOnlyCollection<IShareSession> Sessions { get; }

        void AddShare(IStorageProvider provider);
        void RemoveShare(IStorageProvider provider);
    }
}
