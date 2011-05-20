using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    public enum DsiFlags : byte {
        Request = 0,
        Reply = 1
    }
    public enum DsiCommand : byte {
        Invalid = 0,
        Attention = 8,
        CloseSession = 1,
        Command = 2,
        GetStatus = 3,
        OpenSession = 4,
        Tickle = 5,
        Write = 6
    }

    public struct DsiHeader {
        public DsiFlags flags;
        public DsiCommand command;
        public ushort requestId;
        public uint errorCodeOrWriteOffset;
        public uint totalDataLength;
        public uint reserved;

        public void WriteReply(AfpResultCode resultCode, byte[] resultPayload, AfpStream writeToStream) {
            DsiHeader replyHeader = this.CreateReply(resultCode, (uint)resultPayload.Length);

    //        if (replyHeader.totalDataLength == 97) {
      //          System.Diagnostics.Debugger.Break();
        //    }

            replyHeader.Write(writeToStream);
            writeToStream.WriteBytes(resultPayload);
        }
        public DsiHeader CreateReply(Afp.Protocol.AfpResultCode resultCode, uint resultPayloadLength) {
            DsiHeader newHeader = this;
            newHeader.flags = DsiFlags.Reply;
            newHeader.errorCodeOrWriteOffset = (uint)resultCode;
            newHeader.totalDataLength = resultPayloadLength;
            return newHeader;
        }
        public void Write(AfpStream stream) {
            stream.WriteEnum(flags);
            stream.WriteEnum(command);
            stream.WriteUInt16(requestId);
            stream.WriteUInt32(errorCodeOrWriteOffset);
            stream.WriteUInt32(totalDataLength);
            stream.WriteUInt32(reserved);
        }

        public static DsiHeader Read(AfpStream stream) {
            DsiHeader header = new DsiHeader();
            header.flags = stream.ReadEnum<DsiFlags>();
            header.command = stream.ReadEnum<DsiCommand>();
            if (header.command == DsiCommand.Invalid) {
                return header;
            }

            header.requestId = stream.ReadUInt16();
            header.errorCodeOrWriteOffset = stream.ReadUInt32();
            header.totalDataLength = stream.ReadUInt32();
            header.reserved = stream.ReadUInt32();

            return header;
        }
    }
}
