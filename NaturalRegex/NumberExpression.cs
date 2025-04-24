using System.Globalization;
using System.Text.RegularExpressions;

namespace NaturalRegex;

public class NumberExpression : NatRegExpression
{
    public double Value { get; set; }

    public NumberExpression(double value)
    {
        Value = value;
    }

    public override string ToRegex()
    {
        return Regex.Escape(Value.ToString(CultureInfo.CurrentCulture));
    }
}