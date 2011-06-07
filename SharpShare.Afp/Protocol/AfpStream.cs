using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpShare.Storage;
using System.Numerics;
using SharpShare.Storage.Searching;

namespace SharpShare.Afp.Protocol {
    public sealed class AfpStream {
        public static implicit operator AfpStream(Stream stream) {
            return new AfpStream(stream);
        }

        private Dictionary<string, long> _markedOffsets = new Dictionary<string, long>();
        private Stack<long> _markBeginnings = new Stack<long>();

        public AfpStream()
            : this(new MemoryStream()) {

        }
        public AfpStream(byte[] data)
            : this(new MemoryStream(data)) {

        }
        public AfpStream(Stream stream) {
            this.Stream = stream;
            _markBeginnings.Push(0);
        }

        public Stream Stream { get; private set; }

        public bool EOF { get { return (this.Stream.Position == this.Stream.Length); } }

        public void BeginMarking() {
            _markBeginnings.Push(this.Stream.Position);
        }
        public void EndMarking() {
            _markBeginnings.Pop();
        }

        #region Reading

        public string ReadUTF8StringWithHint() {
            uint hint = this.ReadUInt32();
            return this.ReadUTF8String();
        }
        public string ReadUTF8String() {
            ushort length = this.ReadUInt16();
            byte[] data = this.ReadBytes(length);
            return System.Text.Encoding.UTF8.GetString(data);
        }
        public string ReadPascalString() {
            byte length = this.ReadUInt8();
            byte[] data = this.ReadBytes(length);

            return System.Text.Encoding.ASCII.GetString(data);
        }

        public TEnum ReadEnum<TEnum>() where TEnum : struct {
            Type enumType = Enum.GetUnderlyingType(typeof(TEnum));
            long number = this.ReadNumber(enumType);

            object enumValue = Enum.ToObject(typeof(TEnum), number);
            return (TEnum)enumValue;
        }

        public long ReadNumber(Type type) {
            if (type == typeof(byte)) {
                return (long)this.ReadUnsignedNumber(1);
            }
            if (type == typeof(ushort)) {
                return (long)this.ReadUnsignedNumber(2);
            }
            if (type == typeof(uint)) {
                return (long)this.ReadUnsignedNumber(4);
            }

            if (type == typeof(sbyte)) {
                return (long)this.ReadSignedNumber(1);
            }
            if (type == typeof(short)) {
                return (long)this.ReadSignedNumber(2);
            }
            if (type == typeof(int)) {
                return (long)this.ReadSignedNumber(4);
            }

            throw new Exception("Unrecognized type.");
        }
        public ulong ReadUnsignedNumber(uint length) {
            byte[] data = this.ReadBytesAndFlip(length);

            switch (length) {
                case 1:
                    return data[0];
                case 2:
                    return BitConverter.ToUInt16(data, 0);
                case 4:
                    return BitConverter.ToUInt32(data, 0);
                case 8:
                    return BitConverter.ToUInt64(data, 0);
                default:
                    throw new Exception("Invalid length.");
            }
        }
        public long ReadSignedNumber(uint length) {
            byte[] data = this.ReadBytesAndFlip(length);

            switch (length) {
                case 1:
                    return data[0];
                case 2:
                    return BitConverter.ToInt16(data, 0);
                case 4:
                    return BitConverter.ToInt32(data, 0);
                case 8:
                    return BitConverter.ToInt64(data, 0);
                default:
                    throw new Exception("Invalid length.");
            }
        }

        public BigInteger ReadBigInteger(uint size) {
            byte[] data = this.ReadBytes(size);
            Array.Reverse(data);
            BigInteger bigInt = new BigInteger(data);
            return bigInt;
        }
        public void WriteBigInteger(BigInteger bigInt) {
            byte[] data = bigInt.ToByteArray();
            Array.Reverse(data);
            this.WriteBytes(data);
        }

        public long ReadInt64() {
            return (long)this.ReadSignedNumber(8);
        }
        public int ReadInt32() {
            return (int)this.ReadSignedNumber(4);
        }
        public short ReadInt16() {
            return (short)this.ReadSignedNumber(2);
        }
        public sbyte ReadInt8() {
            return (sbyte)this.ReadSignedNumber(1);
        }

        public uint ReadUInt32() {
            return (uint)this.ReadUnsignedNumber(4);
        }
        public ushort ReadUInt16() {
            return (ushort)this.ReadUnsignedNumber(2);
        }
        public byte ReadUInt8() {
            return (byte)this.ReadUnsignedNumber(1);
        }

        public byte[] ReadBytes(uint count) {
            byte[] bytes = new byte[count];

            for (int x = 0; x < count; x++) {
                byte[] buffer = new byte[1];
                int rec = this.Stream.Read(buffer, 0, buffer.Length);

                if (rec == 0) {
                    throw new Exception("Reached end.");
                }

                bytes[x] = buffer[0];
            }

            return bytes;
        }
        public byte[] ReadBytesAndFlip(uint count) {
            byte[] bytes = this.ReadBytes(count);
            Array.Reverse(bytes);
            return bytes;
        }
        #endregion

        #region Writing

        public void WriteMacintoshDate(DateTime date) {
            uint macDate = date.ToMacintoshDate();
            this.WriteUInt32(macDate);
        }

        private void WriteStorageItemInfo(AfpShareSession session, IStorageItem item, AfpFileDirectoryBitmap bitmap) {

        }

        public void WriteStorageContainerInfo(IAfpVolume volume, IStorageContainer container, AfpFileDirectoryBitmap bitmap) {
            this.WriteUInt8(1 << 7);

            while ((this.Stream.Length % 2) != 0) {
                this.WriteUInt8(0);
            }

            this.BeginMarking();

            foreach (AfpFileDirectoryBitmap flag in bitmap.EnumerateFlags()) {
                switch (flag) {
                    case AfpFileDirectoryBitmap.kFPAttributeBit:
                        AfpFileDirectoryAttributes attrs = 0;

                        if ((container.Attributes & StorageItemAttributes.Hidden) != 0) {
                            attrs |= AfpFileDirectoryAttributes.kFPInvisibleBit;
                        }

                        this.WriteEnum<AfpFileDirectoryAttributes>(attrs);
                        break;
                    case AfpFileDirectoryBitmap.kFPParentDirIDBit:
                        if (container.Parent == null) {
                            this.WriteUInt32(1);
                        } else if (container.Parent is IStorageProvider) {
                            this.WriteUInt32(2);
                        } else {
                            this.WriteUInt32(volume.GetNode(container.Parent));
                        }
                        break;
                    case AfpFileDirectoryBitmap.kFPOffspringCountBit:
                        this.WriteUInt16((ushort)container.Contents().Count());
                        break;
                    case AfpFileDirectoryBitmap.kFPCreateDateBit:
                        this.WriteMacintoshDate(container.DateCreated);
                        break;
                    case AfpFileDirectoryBitmap.kFPModDateBit:
                        this.WriteMacintoshDate(container.DateModified);
                        break;
                    case AfpFileDirectoryBitmap.kFPBackupDateBit:
                        this.WriteUInt32(0x80000000);
                        break;
                    case AfpFileDirectoryBitmap.kFPFinderInfoBit:
                        this.WriteBytes(new byte[32]);
                        break;
                    case AfpFileDirectoryBitmap.kFPLongNameBit:
                        this.WriteMark("LongName");
                        break;
                    case AfpFileDirectoryBitmap.kFPShortNameBit:
                        this.WriteMark("ShortName");
                        break;
                    case AfpFileDirectoryBitmap.kFPUTF8NameBit:
                        this.WriteMark("UTF8Name");
                        this.WriteBytes(new byte[4]);
                        break;
                    case AfpFileDirectoryBitmap.kFPNodeIDBit:
                        if (container is IStorageProvider) {
                            this.WriteUInt32(2);
                        } else {
                            this.WriteUInt32(volume.GetNode(container));
                        }
                        break;
                    case AfpFileDirectoryBitmap.kFPOwnerIDBit:
                        this.WriteUInt32(0);
                        break;
                    case AfpFileDirectoryBitmap.kFPGroupIDBit:
                        this.WriteUInt32(0);
                        break;
                    case AfpFileDirectoryBitmap.kFPAccessRightsBit:
                        this.WriteEnum<AfpAccessRightsBitmap>(AfpAccessRightsBitmap.All);
                        break;
                    default:
                        Console.WriteLine("Unrecog flag: {0}", flag);
                        break;
                }
            }

            //this.WriteUInt32(0); // Pad

            foreach (AfpFileDirectoryBitmap flag in bitmap.EnumerateFlags()) {
                switch (flag) {
                    case AfpFileDirectoryBitmap.kFPLongNameBit:
                        this.BeginMark("LongName");
                        this.WritePascalString(container.Name, 32);
                        break;
                    case AfpFileDirectoryBitmap.kFPShortNameBit:
                        this.BeginMark("ShortName");
                        this.WritePascalString(container.Name, 12);
                        break;
                    case AfpFileDirectoryBitmap.kFPUTF8NameBit:
                        this.BeginMark("UTF8Name");
                        this.WriteUTF8StringWithHint(container.Name);
                        break;
                }
            }

            this.EndMarking();
        }
        public void WriteStorageFileInfo(IAfpVolume volume, IStorageFile file, AfpFileDirectoryBitmap bitmap, bool writeFileByte = true) {
            if (writeFileByte) {
                this.WriteUInt8(0);
            }

            while ((this.Stream.Length % 2) != 0) {
                this.WriteUInt8(0);
            }

            this.BeginMarking();

            foreach (AfpFileDirectoryBitmap flag in bitmap.EnumerateFlags()) {
                switch (flag) {
                    case AfpFileDirectoryBitmap.kFPAttributeBit:
                        AfpFileDirectoryAttributes attrs = 0;

                        if ((file.Attributes & StorageItemAttributes.Hidden) != 0) {
                            attrs |= AfpFileDirectoryAttributes.kFPInvisibleBit;
                        }

                        this.WriteEnum<AfpFileDirectoryAttributes>(attrs);
                        break;
                    case AfpFileDirectoryBitmap.kFPParentDirIDBit:
                        if (file.Parent == null) {
                            this.WriteUInt32(1);
                        } else if (file.Parent is IStorageProvider) {
                            this.WriteUInt32(2);
                        } else {
                            this.WriteUInt32(volume.GetNode(file.Parent));
                        }
                        break;
                    case AfpFileDirectoryBitmap.kFPCreateDateBit:
                        this.WriteMacintoshDate(file.DateCreated);
                        break;
                    case AfpFileDirectoryBitmap.kFPModDateBit:
                        this.WriteMacintoshDate(file.DateModified);
                        break;
                    case AfpFileDirectoryBitmap.kFPBackupDateBit:
                        this.WriteUInt32(0x80000000);
                        break;
                    case AfpFileDirectoryBitmap.kFPFinderInfoBit:
                        this.WriteBytes(new byte[32]);
                        break;
                    case AfpFileDirectoryBitmap.kFPLongNameBit:
                        this.WriteMark("LongName");

                        break;
                    case AfpFileDirectoryBitmap.kFPShortNameBit:
                        this.WriteMark("ShortName");
                        break;
                    case AfpFileDirectoryBitmap.kFPUTF8NameBit:
                        this.WriteMark("UTF8Name");
                        this.WriteBytes(new byte[4]);
                        break;
                    case AfpFileDirectoryBitmap.kFPNodeIDBit:
                        this.WriteUInt32(volume.GetNode(file));
                        break;
                    case AfpFileDirectoryBitmap.kFPAccessRightsBit:
                        this.WriteEnum<AfpAccessRightsBitmap>(AfpAccessRightsBitmap.All);
                        break;
                    case AfpFileDirectoryBitmap.kFPExtDataForkLenBit:
                        this.WriteUInt64((ulong)file.Length);
                        break;
                    case AfpFileDirectoryBitmap.kFPDataForkLenBit:
                        this.WriteUInt32((uint)file.Length);
                        break;
                    case AfpFileDirectoryBitmap.kFPRsrcForkLenBit:
                        this.WriteUInt32(0);
                        break;
                    case AfpFileDirectoryBitmap.kFPExtRsrcForkLenBit:
                        this.WriteUInt64(0);
                        break;
                    default:
                        Console.WriteLine("Unrecog file flag: {0}", flag);
                        break;
                }
            }

            foreach (AfpFileDirectoryBitmap flag in bitmap.EnumerateFlags()) {
                switch (flag) {
                    case AfpFileDirectoryBitmap.kFPLongNameBit:
                        this.BeginMark("LongName");
                        this.WritePascalString(file.Name, 32);
                        break;
                    case AfpFileDirectoryBitmap.kFPShortNameBit:
                        this.BeginMark("ShortName");
                        this.WritePascalString(file.Name, 12);
                        break;
                    case AfpFileDirectoryBitmap.kFPUTF8NameBit:
                        this.BeginMark("UTF8Name");
                        this.WriteUTF8StringWithHint(file.Name);
                        break;
                }
            }

            this.EndMarking();
        }
        public void WriteVolumeInfo(IAfpVolume volume, AfpVolumeBitmap bitmap) {
            this.BeginMarking();

            foreach (AfpVolumeBitmap flag in bitmap.EnumerateFlags()) {
                switch (flag) {
                    case AfpVolumeBitmap.kFPVolAttributeBit:
                        AfpVolumeAttributesBitmap volBitmap =
                            AfpVolumeAttributesBitmap.kSupportsUTF8Names |
                            AfpVolumeAttributesBitmap.kSupportsACLs |
                            AfpVolumeAttributesBitmap.kSupportsFileIDs |
                            AfpVolumeAttributesBitmap.kSupportsTMLockSteal;

                        ISearchable searchable = (volume.StorageProvider as ISearchable);

                        if (searchable != null && searchable.SearchProvider != null) {
                            volBitmap |= AfpVolumeAttributesBitmap.kSupportsCatSearch;
                        }

                        this.WriteEnum<AfpVolumeAttributesBitmap>(volBitmap);

                        break;
                    case AfpVolumeBitmap.kFPVolSignatureBit:
                        this.WriteUInt16(2); // Fixed directory ID
                        break;
                    case AfpVolumeBitmap.kFPVolCreateDateBit:
                    case AfpVolumeBitmap.kFPVolModDateBit:
                    case AfpVolumeBitmap.kFPVolBackupDateBit:
                        this.WriteMacintoshDate(DateTime.Now);
                        break;
                    case AfpVolumeBitmap.kFPVolIDBit:
                        this.WriteUInt16(volume.Identifier);
                        break;
                    case AfpVolumeBitmap.kFPVolBytesFreeBit:
                        this.WriteUInt32((uint)volume.StorageProvider.AvailableBytes);
                        break;
                    case AfpVolumeBitmap.kFPVolBytesTotalBit:
                        this.WriteUInt32((uint)volume.StorageProvider.TotalBytes);
                        break;
                    case AfpVolumeBitmap.kFPVolNameBit:
                        this.WriteMark("Name");
                        break;
                    case AfpVolumeBitmap.kFPVolExtBytesFreeBit:
                        this.WriteUInt64(volume.StorageProvider.AvailableBytes);
                        break;
                    case AfpVolumeBitmap.kFPVolExtBytesTotalBit:
                        this.WriteUInt64(volume.StorageProvider.TotalBytes);
                        break;
                    case AfpVolumeBitmap.kFPVolBlockSizeBit:
                        this.WriteUInt32(4096); // I guess?
                        break;
                }
            }

            foreach (AfpVolumeBitmap flag in bitmap.EnumerateFlags()) {
                switch (flag) {
                    case AfpVolumeBitmap.kFPVolNameBit:
                        this.BeginMark("Name");
                        this.WritePascalString(volume.StorageProvider.Name);
                        break;
                }
            }

            this.EndMarking();
        }
        public void WriteUTF8StringWithHint(string text) {
            this.WriteUInt32(0);
            this.WriteUTF8String(text);
        }
        public void WriteUTF8String(string text) {

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(text);
            this.WriteUInt16((ushort)data.Length);
            this.WriteBytes(data);
        }
        public void WritePascalString(string text) {
            this.WritePascalString(text, 255);
        }
        public void WritePascalString(string text, int limit) {
            limit = Math.Min(limit, 255);

            if (text.Length > limit) {
                text = text.Substring(0, limit);
            }

            this.WriteUInt8((byte)text.Length);

            byte[] ascii = System.Text.Encoding.ASCII.GetBytes(text);
            this.WriteBytes(ascii);
        }
        public void WriteMark(string name) {
            if (!this.Stream.CanSeek) {
                throw new ArgumentException("Base stream is not seekable.");
            }

            if (_markedOffsets.ContainsKey(name)) {
                throw new ArgumentException("Already marked with this name.");
            }

            _markedOffsets[name] = this.Stream.Position;

            this.WriteUInt16(0);
        }
        public void BeginMark(string name) {
            if (!_markedOffsets.ContainsKey(name)) {
                throw new ArgumentException("Marked name doesn't exist.");
            }

            long offset = _markedOffsets[name];
            _markedOffsets.Remove(name);

            long currentOffset = this.Stream.Position;
            this.Stream.Position = offset;

            this.WriteUInt16((ushort)(currentOffset - _markBeginnings.Peek()));
            this.Stream.Position = currentOffset;
        }

        public void WriteEnum<TEnum>(TEnum value) where TEnum : struct {
            Type enumType = Enum.GetUnderlyingType(typeof(TEnum));
            long number = Convert.ToInt64(value);

            if (enumType == typeof(byte)) {
                this.WriteUnsignedNumber((ulong)number, 1);
            } else if (enumType == typeof(ushort)) {
                this.WriteUnsignedNumber((ulong)number, 2);
            } else if (enumType == typeof(uint)) {
                this.WriteUnsignedNumber((ulong)number, 4);
            } else if (enumType == typeof(sbyte)) {
                this.WriteSignedNumber(number, 1);
            } else if (enumType == typeof(short)) {
                this.WriteSignedNumber(number, 2);
            } else if (enumType == typeof(int)) {
                this.WriteSignedNumber(number, 4);
            } else {
                throw new Exception("Invalid enum type.");
            }
        }
        public void WriteUnsignedNumber(ulong number, int length) {
            switch (length) {
                case 1:
                    this.WriteBytes(new byte[] { (byte)number });
                    break;
                case 2:
                    this.FlipAndWriteBytes(BitConverter.GetBytes((ushort)number));
                    break;
                case 4:
                    this.FlipAndWriteBytes(BitConverter.GetBytes((uint)number));
                    break;
                case 8:
                    this.FlipAndWriteBytes(BitConverter.GetBytes((ulong)number));
                    break;
                default:
                    throw new Exception("Invalid length.");
            }
        }
        public void WriteSignedNumber(long number, int length) {
            switch (length) {
                case 1:
                    this.WriteBytes(new byte[] { (byte)number });
                    break;
                case 2:
                    this.FlipAndWriteBytes(BitConverter.GetBytes((short)number));
                    break;
                case 4:
                    this.FlipAndWriteBytes(BitConverter.GetBytes((int)number));
                    break;
                default:
                    throw new Exception("Invalid length.");
            }
        }

        public void WriteInt64(long value) {
            this.WriteSignedNumber(value, 8);
        }
        public void WriteInt32(int value) {
            this.WriteSignedNumber(value, 4);
        }
        public void WriteInt16(short value) {
            this.WriteSignedNumber(value, 2);
        }

        public void WriteUInt8(byte value) {
            this.WriteUnsignedNumber(value, 1);
        }
        public void WriteUInt16(ushort value) {
            this.WriteUnsignedNumber(value, 2);
        }
        public void WriteUInt32(uint value) {
            this.WriteUnsignedNumber(value, 4);
        }
        public void WriteUInt64(ulong value) {
            this.WriteUnsignedNumber(value, 8);
        }

        public void WriteBytes(byte[] bytes) {
            this.Stream.Write(bytes, 0, bytes.Length);
        }
        public void FlipAndWriteBytes(byte[] bytes) {
            Array.Reverse(bytes);
            this.WriteBytes(bytes);
        }

        public uint ReadPadding(uint alignment = 2) {
            if (this.EOF) {
                return 0;
            }

            uint padBytes = (uint)(this.Stream.Position % alignment);
            this.ReadBytes((uint)padBytes);
            return padBytes;
        }
        public uint WritePadding(uint alignment = 2) {
            uint padBytes = ((uint)this.Stream.Length % alignment);
            this.WriteBytes(new byte[padBytes]);
            return padBytes;
        }

        #endregion

        public byte[] ToByteArray() {
            MemoryStream memStream = (this.Stream as MemoryStream);
            if (memStream == null) {
                throw new InvalidOperationException("Base stream is not a memory stream.");
            }

            return memStream.ToArray();
        }
    }
}
