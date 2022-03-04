using CommandLine;
using dumpsym;

// ReSharper disable StringLiteralTypo

namespace dumpsym_ghidra;

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

        writer.WriteLine(@"
#dumpsym_ghidra
#@author https://github.com/aybe
#@category PSX

def rename(address, label):

    adr = toAddr(address)
    
    if getSymbolAt(adr) is None:
        print(""WARNING: Symbol not found at 0x{:08x}."".format(address))

    createLabel(adr, label, True, ghidra.program.model.symbol.SourceType.USER_DEFINED)

    print(""SUCCESS: Create label at 0x{:08x} with name '{}'."".format(address, label))
");

        foreach (var symbol in symbols)
        {
            writer.WriteLine($"rename(0x{symbol.Position:X8}, \"{options.Prefix}{symbol.Name}{options.Suffix}\")");
        }

        var contents = writer.ToString();

        File.WriteAllText(options.Target, contents);

        return 0;
    }
}