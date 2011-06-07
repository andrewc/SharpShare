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

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            byte flag = requestStream.ReadUInt8();
            short forkId = requestStream.ReadInt16();
            long offset = requestStream.ReadInt64();
            long reqCount = requestStream.ReadInt64();

            IAfpFork fork = session.GetFork(forkId);

            if (fork == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            byte[] writeData = requestStream.ReadBytes((uint)reqCount);
            long position = 0;

            if (flag == 1) {
                // End of the fork
                position = (fork.DataProvider.Length - offset);
            } else {
                position = offset;
            }

            fork.DataProvider.Write(position, writeData, 0, writeData.Length);
            responseStream.WriteUInt64((ulong)(position + writeData.LongLength));

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
