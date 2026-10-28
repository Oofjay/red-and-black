using System.IO;
using System.Text;
using UnityEngine;
using System;

public class BetterBinaryReader : BinaryReader
{
    protected readonly byte[] Buffer;

    protected int BufferOffset = 0;
    public int ReadOffset = 0;

    protected readonly int BufferSize = 4096;

    public BetterBinaryReader(Stream InputStream) : base(InputStream, new UTF8Encoding(), false)
    {
        Buffer = new byte[BufferSize];

        FillBuffer();
    }

    public override byte ReadByte()
    {
        if (BufferOffset + 1 >= Buffer.Length)
        {
            FillBuffer();
        }

        byte Value = Buffer[BufferOffset];

        ++BufferOffset;
        ++ReadOffset;

        return Value;
    }

    public byte PeekByte()
    {
        if (BufferOffset + 1 >= Buffer.Length)
        {
            FillBuffer();
        }

        byte Value = Buffer[BufferOffset];

        return Value;
    }

    public override byte[] ReadBytes(int Number)
    {
        if (Number > 4096)
        {
            throw new IOException("Cannot read more than 4096 bytes with ReadBytes, use ReadManyBytes instead");
        }

        if (BufferOffset + Number >= Buffer.Length)
        {
            FillBuffer();
        }

        byte[] Value = new byte[Number];

        for (int i = 0; i < Number; ++i)
        {
            Value[i] = Buffer[BufferOffset + i];
        }

        BufferOffset += Number;
        ReadOffset += Number;

        return Value;
    }

    public byte[] ReadManyBytes(int Number)
    {
        Stream InternalStream = BaseStream;

        InternalStream.Position = ReadOffset;

        byte[] Value = new byte[Number];
        InternalStream.Read(Value, 0, Number);

        Seek((int)InternalStream.Position, SeekOrigin.Begin);

        return Value;
    }

    public override short ReadInt16()
    {
        if (BufferOffset + 2 >= Buffer.Length)
        {
            FillBuffer();
        }

        short Value = (short)(Buffer[BufferOffset + 1] | Buffer[BufferOffset] << 8);

        BufferOffset += 2;
        ReadOffset += 2;

        return Value;
    }

    public override ushort ReadUInt16()
    {
        if (BufferOffset + 2 >= Buffer.Length)
        {
            FillBuffer();
        }

        ushort Value = (ushort)(Buffer[BufferOffset + 1] | Buffer[BufferOffset] << 8);

        BufferOffset += 2;
        ReadOffset += 2;

        return Value;
    }

    public override int ReadInt32()
    {
        if (BufferOffset + 4 >= Buffer.Length)
        {
            FillBuffer();
        }

        int Value = (Buffer[BufferOffset + 3] | Buffer[BufferOffset + 2] << 8 | Buffer[BufferOffset + 1] << 16 | Buffer[BufferOffset] << 24);

        BufferOffset += 4;
        ReadOffset += 4;

        return Value;
    }

    public override uint ReadUInt32()
    {
        if (BufferOffset + 4 >= Buffer.Length)
        {
            FillBuffer();
        }

        uint Value = (uint)(Buffer[BufferOffset + 3] | Buffer[BufferOffset + 2] << 8 | Buffer[BufferOffset + 1] << 16 | Buffer[BufferOffset] << 24);

        BufferOffset += 4;
        ReadOffset += 4;

        return Value;
    }

    public override float ReadSingle()
    {
        if (BufferOffset + 4 >= Buffer.Length)
        {
            FillBuffer();
        }

        byte[] FloatBytes = new byte[] { Buffer[BufferOffset + 3], Buffer[BufferOffset + 2], Buffer[BufferOffset + 1], Buffer[BufferOffset] };

        float Value = BitConverter.ToSingle(FloatBytes, 0);

        BufferOffset += 4;
        ReadOffset += 4;

        return Value;
    }

    protected void FillBuffer()
    {
        Stream InternalStream = BaseStream;

        if (InternalStream == null)
        {
            throw new IOException("Stream is not open when filling the buffer");
        }

        int AmountToRead = BufferSize;
        int FileSize = (int)InternalStream.Length;

        if (ReadOffset + BufferSize > FileSize)
        {
            AmountToRead = FileSize - ReadOffset;
        }

        //Debug.Log($"Filling buffer with {AmountToRead} bytes, offset: {BufferOffset}, file size: {FileSize}, ReadOffset: {ReadOffset}");

        InternalStream.Position = ReadOffset;
        InternalStream.Read(Buffer, 0, AmountToRead);

        BufferOffset = 0;
    }

    public void Seek(int Offset, SeekOrigin Origin)
    {
        int OldReadOffset = ReadOffset;
        int OldBufferOffset = BufferOffset;

        switch (Origin)
        {
            case SeekOrigin.Begin:
                ReadOffset = Offset;
                BufferOffset += Offset - OldReadOffset;
                break;
            case SeekOrigin.Current:
                ReadOffset += Offset;
                BufferOffset += Offset;
                break;
            case SeekOrigin.End:
                throw new IOException("SeekOrigin.End is unsupported");
        }

        /* Already gets checked in FillBuffer > InternalStream.Read
        if (ReadOffset > BaseStream.Length)
        {
            throw new EndOfStreamException("Tried to seek to " + ReadOffset + " in a file with a length of " + BaseStream.Length);
        }*/

        // If the new offset is outside of the buffer
        if (ReadOffset < OldReadOffset - OldBufferOffset || ReadOffset > OldReadOffset - OldBufferOffset + BufferSize)
        {
            FillBuffer();
        }
    }
}

public class BetterBinaryWriter: BinaryWriter
{
    protected readonly byte[] Buffer;

    //protected int BufferOffset = 0;
    //public int ReadOffset = 0;

    protected readonly int BufferSize = 8;

    public BetterBinaryWriter(Stream InputStream) : base(InputStream, new UTF8Encoding(), false)
    {
        Buffer = new byte[BufferSize];
    }

    public override void Write(byte Value)
    {
        Buffer[0] = Value;

        OutStream.Write(Buffer, 0, 1);
    }

    public override void Write(byte[] Value)
    {
        OutStream.Write(Value, 0, Value.Length);
    }

    public override void Write(short Value)
    {
        Buffer[0] = (byte)(Value >> 8);
        Buffer[1] = (byte)Value;

        OutStream.Write(Buffer, 0, 2);
    }

    public override void Write(ushort Value)
    {
        Buffer[0] = (byte)(Value >> 8);
        Buffer[1] = (byte)Value;

        OutStream.Write(Buffer, 0, 2);
    }

    public override void Write(int Value)
    {
        Buffer[0] = (byte)(Value >> 24);
        Buffer[1] = (byte)(Value >> 16);
        Buffer[2] = (byte)(Value >> 8);
        Buffer[3] = (byte)Value;

        OutStream.Write(Buffer, 0, 4);
    }

    public override void Write(uint Value)
    {
        Buffer[0] = (byte)(Value >> 24);
        Buffer[1] = (byte)(Value >> 16);
        Buffer[2] = (byte)(Value >> 8);
        Buffer[3] = (byte)Value;

        OutStream.Write(Buffer, 0, 4);
    }

    public override void Write(float Value)
    {
        byte[] FloatBytes = BitConverter.GetBytes(Value);
        Buffer[0] = FloatBytes[3];
        Buffer[1] = FloatBytes[2];
        Buffer[2] = FloatBytes[1];
        Buffer[3] = FloatBytes[0];

        OutStream.Write(Buffer, 0, 4);
    }

    public override void Write(bool Value)
    {
        Buffer[0] = System.Convert.ToByte(Value);

        OutStream.Write(Buffer, 0, 1);
    }

    public void WriteZeroes(int NumZeroes)
    {
        byte[] ZeroesBuffer = new byte[NumZeroes];

        OutStream.Write(ZeroesBuffer, 0, NumZeroes);
    }
}
