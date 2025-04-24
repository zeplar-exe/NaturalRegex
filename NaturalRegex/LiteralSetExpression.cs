namespace NaturalRegex;

public class LiteralSetExpression : NatRegExpression
{
    public string Characters { get; set; }

    public LiteralSetExpression(string characters)
    {
        Characters = characters;
    }

    public override string ToRegex()
    {
        return $"[{Characters}]";
    }
}