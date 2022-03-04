using System.Globalization;
using System.Text.RegularExpressions;

namespace dumpsym;

public static class SymbolUtility
{
    public static List<Symbol> GetSymbols(Stream stream)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        var parse = Program.Parse(stream);

        using var reader = new StringReader(parse);

        var symbols = new List<Symbol>(1000);

        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            var match = Regex.Match(line, @"\$(\w{8})\s2\s(\w+)");

            if (!match.Success)
                continue;

            var v1 = match.Groups[1].Value;
            var v2 = match.Groups[2].Value;

            var i1 = int.Parse(v1, NumberStyles.HexNumber);

            symbols.Add(new Symbol(i1, v2));
        }

        symbols.Sort((x, y) => x.Position.CompareTo(y.Position));

        return symbols;
    }
}