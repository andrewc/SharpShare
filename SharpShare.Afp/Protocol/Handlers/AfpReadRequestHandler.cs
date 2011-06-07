using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpReadRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 60; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Pad

            short forkId = requestStream.ReadInt16();
            long offset = requestStream.ReadInt64();
            long readBytes = requestStream.ReadInt64();
            //byte newLineMask = requestStream.ReadUInt8();
            //byte newLineChar = requestStream.ReadUInt8();

            IAfpFork fork = session.GetFork(forkId);

            if (fork == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            byte[] buffer = new byte[readBytes];
            int bytesRead = fork.DataProvider.Read(offset, buffer, 0, (int)readBytes);

            responseStream.WriteBytes(buffer);

            if (bytesRead < readBytes) {
                return AfpResultCode.FPEOFErr;
            }

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
