using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpEnumerateExt2RequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 68; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Padding

            ushort volumeId = requestStream.ReadUInt16();
            uint directoryId = requestStream.ReadUInt32();
            AfpFileDirectoryBitmap fileBitmap = requestStream.ReadEnum<AfpFileDirectoryBitmap>();
            AfpFileDirectoryBitmap directoryBitmap = requestStream.ReadEnum<AfpFileDirectoryBitmap>();

            short reqCount = requestStream.ReadInt16();
            int startIndex = requestStream.ReadInt32();
            int maxReplySize = requestStream.ReadInt32();

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

            IStorageContainer lookAtContainer = null;

            if (string.IsNullOrEmpty(path)) {
                lookAtContainer = container;
            } else {
                lookAtContainer = (container.Content(path) as IStorageContainer);
            }

            if (lookAtContainer == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            var contents = lookAtContainer
                .ListContents()
                .Skip(startIndex - 1)
                .Take(reqCount)
                .ToList();

            if (contents.Count == 0) {
                return AfpResultCode.FPEOFErr;
            }

            responseStream.WriteEnum<AfpFileDirectoryBitmap>(fileBitmap);
            responseStream.WriteEnum<AfpFileDirectoryBitmap>(directoryBitmap);
            responseStream.WriteInt16((short)contents.Count);

            foreach (IStorageItem item in contents) {
                AfpStream resultRecord = new AfpStream();

                resultRecord.WriteUInt16(0); // Length

                if (item is IStorageContainer) {
                    resultRecord.WriteStorageContainerInfo(session, (IStorageContainer)item, directoryBitmap);
                } else {
                    resultRecord.WriteStorageFileInfo(session, (IStorageFile)item, fileBitmap);
                }

                while ((resultRecord.Stream.Length % 2) != 0) {
                    resultRecord.WriteUInt8(0);
                }

                resultRecord.Stream.Position = 0;
                resultRecord.WriteUInt16((byte)resultRecord.Stream.Length);

                byte[] recordData = resultRecord.ToByteArray();
                responseStream.WriteBytes(recordData);
            }

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
