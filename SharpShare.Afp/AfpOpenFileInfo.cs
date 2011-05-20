using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;
using System.IO;
using SharpShare.Afp.Protocol;
using System.Threading;

namespace SharpShare.Afp {
    public sealed class AfpOpenFileInfo {
        private object _streamLock = new object();

        public AfpOpenFileInfo(
            short identifier,
            AfpAccessModes accessModes,
            IAfpSession session, 
            IStorageFile file,
            Stream stream) {
            this.File = file;
            this.Stream = stream;
            this.Session = session;
            this.Identifier = identifier;
            this.AccessModes = accessModes;
        }

        public short Identifier { get; private set; }
        public AfpAccessModes AccessModes { get; private set; }
        public IAfpSession Session { get; private set; }
        public IStorageFile File { get; private set; }
        public Stream Stream { get; private set; }

        public void LockStream() {
            Monitor.Enter(_streamLock);
        }
        public void UnlockStream() {
            Monitor.Exit(_streamLock);
        }
    }
}
