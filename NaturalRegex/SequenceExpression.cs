namespace NaturalRegex;

public class SequenceExpression : NatRegExpression
{
    public NatRegExpression?[] Expressions { get; set; }

    public SequenceExpression(NatRegExpression?[] expressions)
    {
        Expressions = expressions;
    }

    public override string ToRegex()
    {
        return string.Join("", Expressions.Select(e => e?.ToRegex() ?? ""));
    }
}