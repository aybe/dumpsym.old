using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace dumpsym_ghidra;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
internal sealed class Options
{
    [Option("source", Required = true, HelpText = "Path to the source .SYM file.")]
    public string Source { get; set; } = null!;

    [Option("target", Required = true, HelpText = "Path to the target .PY file.")]
    public string Target { get; set; } = null!;

    [Option("prefix", Required = false, HelpText = "Optional. Prefix for symbol names.")]
    public string? Prefix { get; set; }

    [Option("suffix", Required = false, HelpText = "Optional. Suffix for symbol names.")]
    public string? Suffix { get; set; }
}