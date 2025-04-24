using System.Text.RegularExpressions;

namespace NaturalRegex;

public class RegexExpression : NatRegExpression
{
    public string Value { get; set; }

    public RegexExpression(string value)
    {
        Value = value;
    }

    public override string ToRegex()
    {
        return Regex.Escape(Value);
    }
}