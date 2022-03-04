using CommandLine;
using dumpsym;

// ReSharper disable StringLiteralTypo

namespace dumpsym_ida;

internal static class Program
{
    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).MapResult(Generate, _ => 1);
    }

    private static int Generate(Options options)
    {
        if (options.Source.Equals(options.Target, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Source and target files must be different.");
            return 1;
        }

        Console.WriteLine($@"Reading source file ""{options.Source}""...");

        List<Symbol> symbols;

        using (var stream = File.OpenRead(options.Source))
        {
            symbols = SymbolUtility.GetSymbols(stream);
        }

        Console.WriteLine($@"Writing target file ""{options.Target}""...");

        using var writer = new StringWriter();

        writer.WriteLine("#dumpsym_ida");
        writer.WriteLine("#@author https://github.com/aybe");
        writer.WriteLine("#@category PSX");
        writer.WriteLine("import idc");

        // we're going to increment dupe names ourselves instead of letting IDA do it
        // this is because it wouldn't report failed renaming operations at tail byte
        // clicking these errors allow the user to go the problem and fix it manually

        var dictionary = new Dictionary<string, int>();

        foreach (var symbol in symbols)
        {
            var key = symbol.Name;

            if (dictionary.ContainsKey(key))
            {
                dictionary[key]++;
            }
            else
            {
                dictionary.Add(key, default);
            }

            if (dictionary.ContainsKey(key))
            {
                var val = dictionary[key];
                if (val > 0)
                {
                    key = $"{key}_{val - 1}";
                }
            }

            writer.WriteLine(
                $"idc.set_name(0x{symbol.Position:X8}, \"{options.Prefix}{key}{options.Suffix}\")");
        }

        var contents = writer.ToString();

        File.WriteAllText(options.Target, contents);

        return 0;
    }
}