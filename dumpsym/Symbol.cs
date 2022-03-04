namespace dumpsym;

public readonly struct Symbol
{
    public int Position { get; }

    public string Name { get; }

    public Symbol(int position, string name)
    {
        Position = position;
        Name = name;
    }

    public override string ToString()
    {
        return $"{nameof(Position)}: {Position}, {nameof(Name)}: {Name}";
    }
}