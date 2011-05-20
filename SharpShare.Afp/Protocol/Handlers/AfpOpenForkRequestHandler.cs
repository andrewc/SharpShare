using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpOpenForkRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 26; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            byte flag = requestStream.ReadUInt8();
            ushort volumeId = requestStream.ReadUInt16();
            uint directoryId = requestStream.ReadUInt32();

            AfpFileDirectoryBitmap bitmap = requestStream.ReadEnum<AfpFileDirectoryBitmap>();
            AfpAccessModes accessMode = requestStream.ReadEnum<AfpAccessModes>();
            AfpPathType pathType = requestStream.ReadEnum<AfpPathType>();
            string path = null;

            switch (pathType) {
                case AfpPathType.kFPLongName:
                case AfpPathType.kFPShortName:
                    path = requestStream.ReadPascalString();
                    break;
                case AfpPathType.kFPUTF8Name:
                    path = requestStream.ReadUTF8StringWithHint();
                    break;
            }

            IStorageProvider provider = session.GetVolume(volumeId);

            if (provider == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageContainer container = null;

            if (directoryId == 2) {
                container = provider;
            } else {
                container = (IStorageContainer)session.GetNode(directoryId);
            }

            if (container == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageFile file = (container.Content(path) as IStorageFile);

            if (file == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            AfpOpenFileInfo info = session.OpenFork(
                file, accessMode);

            if (info == null) {
                return AfpResultCode.FPDenyConflict;
            }

            responseStream.WriteEnum<AfpFileDirectoryBitmap>(bitmap);
            responseStream.WriteInt16(info.Identifier);
            responseStream.WriteStorageFileInfo(session, file, bitmap);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
