using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;
using System.Collections.ObjectModel;
using SharpShare.Security;

namespace SharpShare {
    public enum ShareServerState {
        Invalid = 0,
        Stopped,
        Running,
        ShuttingDown
    }

    public interface IShareServer {
        event EventHandler<ShareSessionEventArgs> SessionCreated;
        event EventHandler<ShareSessionEventArgs> SessionEnded;

        void Start();
        void Shutdown();
        void Stop();

        ShareServerState State { get; }

        string Name { get; }

        IList<IStorageProvider> Shares { get; }

        ReadOnlyCollection<IShareSession> Sessions { get; }

        IShareAuthenticationProvider AuthenticationProvider { get; set; }
    }

    public sealed class ShareSessionEventArgs : EventArgs {
        public ShareSessionEventArgs(IShareSession session) {
            this.Session = session;
        }

        public IShareSession Session { get; private set; }
    }
}
