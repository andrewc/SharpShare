using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpWriteExtRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 61; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            byte flag = requestStream.ReadUInt8();
            short forkId = requestStream.ReadInt16();
            long offset = requestStream.ReadInt64();
            long reqCount = requestStream.ReadInt64();

            byte[] writeData = requestStream.ReadBytes((uint)reqCount);

            AfpOpenFileInfo info = session.FindFork(forkId);

            if (info == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            info.LockStream();
            try {
                if (flag == 1) {
                    // End of the fork
                    info.Stream.Seek(offset, System.IO.SeekOrigin.End);
                } else {
                    info.Stream.Seek(offset, System.IO.SeekOrigin.Begin);
                }

                //Console.WriteLine("Write to {0}, offset: {1}, len: {2}", info.File.Name, offset, reqCount);
                info.Stream.Write(writeData, 0, writeData.Length);
                responseStream.WriteUInt64((ulong)info.Stream.Position );
            } finally {
                info.UnlockStream();
            }

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
