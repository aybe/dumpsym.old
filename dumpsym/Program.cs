using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable StringLiteralTypo

namespace dumpsym;

public static class Program
{
    private static readonly byte[] ClassTypes =
    {
        0x6B, 0x6A, 0x69, 0x68, 0x67, 0x66, 0x65, 0x13, 0x12,
        0x11, 0x10, 0x0F, 0x0E, 0x0D, 0x0C, 0x0B, 0x0A, 0x09,
        0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00
    };

    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("dumpsym 2.02 (c) 1997 SN Systems Software Ltd");
            Console.WriteLine("Usage: dumpsym sym_file");
            return 1;
        }

        var path = args[0];

        if (!File.Exists(path))
        {
            Console.WriteLine($"Error: Can't open file '{path}' for input.");
            return 1;
        }

        using var stream = File.OpenRead(path);

        var text = Parse(stream);

        Console.WriteLine(text);

        return 0;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    public static string Parse(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new BinaryReader(stream, Encoding.ASCII, true);
        using var writer = new StringWriter();

        var magic = Encoding.ASCII.GetString(reader.ReadBytes(3));

        if (magic != "MND")
        {
            throw new InvalidDataException("Stream is not in SN-SYM format.");
        }

        writer.WriteLine();
        writer.WriteLine($"Header : {magic} version {reader.ReadByte()}");
        writer.WriteLine($"Target unit {reader.ReadByte()}");

        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();

        string ReadString()
        {
            return new string(reader.ReadChars(reader.ReadByte()));
        }

        ushort ReadUInt16()
        {
            return reader.ReadUInt16();
        }

        uint ReadUInt32()
        {
            return reader.ReadUInt32();
        }

        var line = 0u;

        while (true)
        {
            int tag;

            var offset = 0u;

            while (true)
            {
                if (reader.BaseStream.Length - reader.BaseStream.Position >= 5) // fix useless position write at EOF
                {
                    writer.Write($"{reader.BaseStream.Position:x6}: ");

                    offset = ReadUInt32();

                    tag = reader.BaseStream.ReadByte();
                }
                else
                {
                    tag = -1;
                }

                if (tag != 8)
                {
                    break;
                }

                writer.WriteLine($"${offset:x8} {8:x} MX-info {reader.ReadByte():X}");
            }

            if (tag == -1)
            {
                break;
            }

            if (tag <= 0x7F)
            {
                writer.WriteLine($"${offset:x8} {tag:x} {ReadString()}");
                continue;
            }

            switch (tag)
            {
                case 0x80:
                    writer.WriteLine($"${offset:x8} {0x80:x} Inc SLD linenum (to {++line})");
                    break;
                case 0x82:
                    var add1 = reader.ReadByte();
                    line += add1;
                    writer.WriteLine($"${offset:x8} {tag:x} Inc SLD linenum by byte {add1} (to {line})");
                    break;
                case 0x84:
                    var add2 = ReadUInt16();
                    line += add2;
                    writer.WriteLine($"${offset:x8} {tag:x} Inc SLD linenum by word {add2} (to {line})");
                    break;
                case 0x86:
                    line = ReadUInt32();
                    writer.WriteLine($"${offset:x8} {tag:x} Set SLD linenum to {line}");
                    break;
                case 0x88:
                    line = ReadUInt32();
                    writer.WriteLine($"${offset:x8} {tag:x} Set SLD to line {line} of file {ReadString()}");
                    break;
                case 0x8A:
                    writer.WriteLine($"${offset:x8} {tag:x} End SLD info");
                    break;
                case 0x8C:
                    writer.WriteLine($"${offset:x8} {0x8C:x} Function start");
                    writer.WriteLine($"    fp = {ReadUInt16()}");
                    writer.WriteLine($"    fsize = {ReadUInt32()}");
                    writer.WriteLine($"    retreg = {ReadUInt16()}");
                    writer.WriteLine($"    mask = ${ReadUInt32():x8}");
                    writer.WriteLine($"    maskoffs = {ReadUInt32()}");
                    writer.WriteLine($"    line = {ReadUInt32()}");
                    writer.WriteLine($"    file = {ReadString()}");
                    writer.WriteLine($"    name = {ReadString()}");
                    break;
                case 0x8E:
                    writer.WriteLine($"${offset:x8} {tag:x} Function end   line {ReadUInt32()}");
                    break;
                case 0x90:
                    writer.WriteLine($"${offset:x8} {tag:x} Block start  line = {ReadUInt32()}");
                    break;
                case 0x92:
                    writer.WriteLine($"${offset:x8} {tag:x} Block end  line = {ReadUInt32()}");
                    break;
                case 0x94:
                    writer.Write($"${offset:x8} {0x94:x} Def ");
                    ParseClass(ReadUInt16(), writer);
                    ParseType(ReadUInt16(), writer);
                    writer.WriteLine($"size {ReadUInt32()} name {ReadString()}");
                    break;
                case 0x96:
                    writer.Write($"${offset:x8} {tag:x} Def2 ");

                    ParseClass(ReadUInt16(), writer);

                    ParseType(ReadUInt16(), writer);

                    writer.Write($"size {ReadUInt32()} ");

                    var count = ReadUInt16();

                    writer.Write($"dims {count} ");

                    for (var i = 0; i < count; i++)
                    {
                        writer.Write($"{ReadUInt32()} ");
                    }

                    writer.WriteLine($"tag {ReadString()} name {ReadString()}");
                    break;
                case 0x98:
                    writer.WriteLine($"${offset:x8} overlay length {ReadUInt32():x8} id ${ReadUInt32():x}");
                    break;
                case 0x9A:
                    writer.WriteLine($"${offset:x8} set overlay");
                    break;
                case 0x9C:
                    writer.WriteLine($"${offset:x8} {tag:x} Function2 start");
                    writer.WriteLine($"    fp = {ReadUInt16()}");
                    writer.WriteLine($"    fsize = {ReadUInt32()}");
                    writer.WriteLine($"    retreg = {ReadUInt16()}");
                    writer.WriteLine($"    mask = ${ReadUInt32():x8}");
                    writer.WriteLine($"    maskoffs = {ReadUInt32()}");
                    writer.WriteLine($"    fmask = ${ReadUInt32():x8}");
                    writer.WriteLine($"    fmaskoffs = {ReadUInt32()}");
                    writer.WriteLine($"    line = {ReadUInt32()}");
                    writer.WriteLine($"    file = {ReadString()}");
                    writer.WriteLine($"    name = {ReadString()}");
                    break;
                case 0x9E:
                    writer.WriteLine($"${offset:x8} {0x9E:x} Mangled name \"{ReadString()}\" is \"{ReadString()}");
                    break;
                default:
                    writer.WriteLine($"??? {tag} ???");
                    writer.WriteLine();
                    break;
            }
        }

        var text = writer.ToString();

        return text;
    }

    private static void ParseClass(int classType1, TextWriter writer)
    {
        bool found;

        writer.Write("class ");

        if (classType1 + 1 > 0x6B)
        {
            writer.Write($"?{classType1}? ");
            return;
        }

        var index1 = 28;
        var index2 = 0;

        do
        {
            if (index1 == 0)
            {
                break;
            }

            found = ClassTypes[index2] == (byte)(classType1 + 1);
            index1--;
            index2++;
        } while (!found);

        switch (index1)
        {
            case 0:
                writer.Write($"?{classType1}? ");
                break;
            case 1:
                writer.Write("EFCN ");
                break;
            case 2:
                writer.Write("NULL ");
                break;
            case 3:
                writer.Write("AUTO ");
                break;
            case 4:
                writer.Write("EXT ");
                break;
            case 5:
                writer.Write("STAT ");
                break;
            case 6:
                writer.Write("REG ");
                break;
            case 7:
                writer.Write("EXTDEF ");
                break;
            case 8:
                writer.Write("LABEL ");
                break;
            case 9:
                writer.Write("ULABEL ");
                break;
            case 10:
                writer.Write("MOS ");
                break;
            case 11:
                writer.Write("ARG ");
                break;
            case 12:
                writer.Write("STRTAG ");
                break;
            case 13:
                writer.Write("MOU ");
                break;
            case 14:
                writer.Write("UNTAG ");
                break;
            case 15:
                writer.Write("TPDEF ");
                break;
            case 16:
                writer.Write("USTATIC ");
                break;
            case 17:
                writer.Write("ENTAG ");
                break;
            case 18:
                writer.Write("MOE ");
                break;
            case 19:
                writer.Write("REGPARM ");
                break;
            case 20:
                writer.Write("FIELD ");
                break;
            case 21:
                writer.Write("BLOCK ");
                break;
            case 22:
                writer.Write("FCN ");
                break;
            case 23:
                writer.Write("EOS ");
                break;
            case 24:
                writer.Write("FILE ");
                break;
            case 25:
                writer.Write("LINE ");
                break;
            case 26:
                writer.Write("ALIAS ");
                break;
            case 27:
                writer.Write("HIDDEN ");
                break;
        }
    }

    private static void ParseType(int classType2, TextWriter writer)
    {
        writer.Write("type ");

        while ((classType2 & 0xFFF0) != 0)
        {
            var type1 = (classType2 >> 4) & 3; // eax

            switch (type1)
            {
                case 1:
                    writer.Write("PTR ");
                    break;
                case 2:
                    writer.Write("FCN ");
                    break;
                default:
                {
                    if (type1 > 2)
                    {
                        writer.Write("ARY ");
                    }
                }
                    break;
            }

            classType2 = ((classType2 >> 2) & 0xFFF0) + (classType2 & 0xF);
        }

        switch (classType2)
        {
            case 0:
                writer.Write("NULL ");
                break;
            case 1:
                writer.Write("VOID ");
                break;
            case 2:
                writer.Write("CHAR ");
                break;
            case 3:
                writer.Write("SHORT ");
                break;
            case 4:
                writer.Write("INT ");
                break;
            case 5:
                writer.Write("LONG ");
                break;
            case 6:
                writer.Write("FLOAT ");
                break;
            case 7:
                writer.Write("DOUBLE ");
                break;
            case 8:
                writer.Write("STRUCT ");
                break;
            case 9:
                writer.Write("UNION ");
                break;
            case 10:
                writer.Write("ENUM ");
                break;
            case 11:
                writer.Write("MOE ");
                break;
            case 12:
                writer.Write("UCHAR ");
                break;
            case 13:
                writer.Write("USHORT ");
                break;
            case 14:
                writer.Write("UINT ");
                break;
            case 15:
                writer.Write("ULONG ");
                break;
        }
    }
}