using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
   public  class AfpReadRequestHandler : IAfpRequestHandler {
       #region IAfpRequestHandler Members

       public byte CommandCode {
           get { return 60; }
       }

       public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
         
         requestStream.ReadUInt8(); // Pad
           short forkId = requestStream.ReadInt16();
           long offset = requestStream.ReadInt64();
           long readBytes = requestStream.ReadInt64();
           //byte newLineMask = requestStream.ReadUInt8();
           //byte newLineChar = requestStream.ReadUInt8();

           AfpOpenFileInfo info = session.FindFork(forkId);

           if (info == null) {
               return AfpResultCode.FPObjectNotFound;
           }

           info.LockStream();

           try {
               info.Stream.Position = offset;
               byte[] buffer = new byte[readBytes];
               int bytesRead = info.Stream.Read(buffer, 0, buffer.Length);

               responseStream.WriteBytes(buffer);
           } finally {
               info.UnlockStream();
           }
           

           return AfpResultCode.FPNoErr;
       }

       #endregion
   }
}
