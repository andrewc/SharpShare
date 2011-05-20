using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpMoveRenameRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 23; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Padding

            ushort volumeId = requestStream.ReadUInt16();
            uint sourceDirId = requestStream.ReadUInt32();
            uint destDirId = requestStream.ReadUInt32();
            AfpPathType sourcePathType = requestStream.ReadEnum<AfpPathType>();
            string sourcePathName = null;

            switch (sourcePathType) {
                case AfpPathType.kFPLongName:
                case AfpPathType.kFPShortName:
                    sourcePathName = requestStream.ReadPascalString();
                    break;
                case AfpPathType.kFPUTF8Name:
                    sourcePathName = requestStream.ReadUTF8StringWithHint();
                    break;
            }

            AfpPathType destPathType = requestStream.ReadEnum<AfpPathType>();
            string destPathName = null;
            switch (destPathType) {
                case AfpPathType.kFPLongName:
                case AfpPathType.kFPShortName:
                    destPathName = requestStream.ReadPascalString();
                    break;
                case AfpPathType.kFPUTF8Name:
                    destPathName = requestStream.ReadUTF8StringWithHint();
                    break;
            }

            AfpPathType newPathType = requestStream.ReadEnum<AfpPathType>();
            string newPathName = null;
            switch (newPathType) {
                case AfpPathType.kFPLongName:
                case AfpPathType.kFPShortName:
                    newPathName = requestStream.ReadPascalString();
                    break;
                case AfpPathType.kFPUTF8Name:
                    newPathName = requestStream.ReadUTF8StringWithHint();
                    break;
            }

            IStorageProvider provider = session.GetVolume(volumeId);

            IStorageContainer sourceContainer = sourceDirId == 2 ? provider : (IStorageContainer)session.GetNode(sourceDirId);
            IStorageContainer destinationContainer = destDirId == 2 ? provider : (IStorageContainer)session.GetNode(destDirId);

            if (sourceContainer == null || destinationContainer == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageItem sourceItem = null;

            if (!string.IsNullOrEmpty(destPathName)) {
                destinationContainer = destinationContainer.Content(destPathName) as IStorageContainer;

                if (destinationContainer == null) {
                    return AfpResultCode.FPObjectNotFound;
                }
            }

            if (!string.IsNullOrEmpty(sourcePathName)) {
                sourceItem = sourceContainer.Content(sourcePathName);
            } else {
                sourceItem = sourceContainer;
            }

            if (sourceItem == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            if (string.IsNullOrEmpty(newPathName)) {
                newPathName = sourceItem.Name;
            }

            if (destinationContainer.Content(newPathName) != null) {
                return AfpResultCode.FPObjectExists;
            }

            sourceItem.Move(destinationContainer);
            sourceItem.Rename(newPathName);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
