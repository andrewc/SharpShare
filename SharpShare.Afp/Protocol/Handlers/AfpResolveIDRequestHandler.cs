using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpResolveIDRequestHandler : IAfpRequestHandler {
        public byte CommandCode {
            get { return 41; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Pad

            ushort volumeId = requestStream.ReadUInt16();
            uint fileId = requestStream.ReadUInt32();
            AfpFileDirectoryBitmap fileBitmap = requestStream.ReadEnum<AfpFileDirectoryBitmap>();

            IAfpVolume volume = session.GetVolume(volumeId);

            if (volume == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageFile item = volume.GetNode(fileId) as IStorageFile;

            if (item == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            responseStream.WriteEnum(fileBitmap);

            responseStream.WriteStorageFileInfo(volume, item, fileBitmap, false);

            return AfpResultCode.FPNoErr;
        }
    }
}
