using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpOpenVolumeRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 24; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Pad

            AfpVolumeBitmap volumeBitmap = requestStream.ReadEnum<AfpVolumeBitmap>();
            string volumeName = requestStream.ReadPascalString();

            IStorageProvider provider = session.Server.Shares
                .Where(s => s.Name == volumeName)
                .FirstOrDefault();

            if (provider == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            responseStream.WriteEnum(volumeBitmap);
            responseStream.WriteVolumeInfo(session, provider, volumeBitmap);

            if (!session.OpenVolumes.Contains(provider)) {
                session.OpenVolumes.Add(provider);
            }

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
