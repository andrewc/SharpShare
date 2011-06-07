using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpGetFileDirectoryParamsRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 34; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Pad

            ushort volumeId = requestStream.ReadUInt16();
            uint directoryID = requestStream.ReadUInt32();
            AfpFileDirectoryBitmap fileBitmap = requestStream.ReadEnum<AfpFileDirectoryBitmap>();
            AfpFileDirectoryBitmap directoryBitmap = requestStream.ReadEnum<AfpFileDirectoryBitmap>();
            AfpPathType pathType = requestStream.ReadEnum<AfpPathType>();
            string pathname = null;

            switch (pathType) {
                case AfpPathType.kFPLongName:
                case AfpPathType.kFPShortName:
                    pathname = requestStream.ReadPascalString();
                    break;
                case AfpPathType.kFPUTF8Name:
                    pathname = requestStream.ReadUTF8StringWithHint();
                    break;
            }

            IAfpVolume volume = session.GetVolume(volumeId);

            if (volume == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageContainer container = null;

            if (directoryID == 2) {
                container = volume.StorageProvider;
            } else if (directoryID == 1) {
                if (pathname == volume.StorageProvider.Name) {
                    container = volume.StorageProvider;
                }
            } else {
                container = volume.GetNode(directoryID) as IStorageContainer;
            }

            if (container == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageItem item = null;

            if (directoryID == 1) {
                item = container;
            } else {
                if (string.IsNullOrEmpty(pathname)) {
                    item = container;
                } else {
                    item = container.Content(pathname);
                }
            }

            if (item == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            responseStream.WriteEnum(fileBitmap);
            responseStream.WriteEnum(directoryBitmap);

            if (item is IStorageContainer) {
                responseStream.WriteStorageContainerInfo(volume, (IStorageContainer)item, directoryBitmap);
            } else {
                responseStream.WriteStorageFileInfo(volume, (IStorageFile)item, fileBitmap);
            }

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
