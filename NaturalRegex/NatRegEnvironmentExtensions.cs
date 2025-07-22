namespace NaturalRegex;

public static class NatRegEnvironmentExtensions
{
    public static NatRegEnvironment WithStandardRegexProcedures(this NatRegEnvironment procedures)
    {
        return procedures
            .WithProcedure("start of line", _ => new ConstantExpression("^"), 0)
            .WithProcedure("end of line", _ => new ConstantExpression("$"), 0)
            .WithProcedure(["capture group", "group"], 
                expressions => new ModifierExpression(expressions[0], s => $"({s})"), 1)
            .WithProcedure(["zero or more", "any amount", "any amount of times"], 
                expressions => new ModifierExpression(expressions[0], s => $"(?:{s})*"), 1)
            .WithProcedure(["one or more", "at least one", "at least once"], 
                expressions => new ModifierExpression(expressions[0], s => $"(?:{s})+"), 1)
            .WithProcedure(["once or none", "at most once", "optional", "optionally", "once at most", "maybe"], 
                expressions => new ModifierExpression(expressions[0], s => $"(?:{s})?"), 1)
            .WithProcedure(["one of", "or"], 
                expressions => new OrExpression(expressions))
            .WithProcedure(["content of group", "group content", "group"], 
                expressions => new ModifierExpression(expressions[0], s => $"({s})?"), 1)
            .WithProcedure(["n or more", "at least n", "n or more times", "at least n times"], 
                expressions => new RangeTimesExpression(expressions, true, false), 2)
            .WithProcedure(["repeat n times", "n times"], 
                expressions => new NumberTimesExpression(expressions), 2)
            .WithProcedure(["up to n", "up to n times"], 
                expressions => new RangeTimesExpression(expressions, false, true), 2)
            .WithProcedure(["between n and m", "between n and m times"], 
                expressions => new RangeTimesExpression(expressions, true, true), 3)
            .WithProcedure(["lookahead", "positive lookahead", "followed by"], 
                expressions => new ModifierExpression(expressions[0], s => $"(?={s})"), 1)
            .WithProcedure(["negative lookahead", "not followed by", "not"], 
                expressions => new ModifierExpression(expressions[0], s => $"(?<={s})"), 1)
            .WithProcedure(["lookbehind", "positive lookbehind"], 
                expressions => new ModifierExpression(expressions[0], s => $"(?!{s})"), 1)
            .WithProcedure(["negative lookbehind"], 
                expressions => new ModifierExpression(expressions[0], s => $"(?<!{s})"), 1)
            .WithProcedure(["lazy", "lazily"], 
                expressions => new ModifierExpression(expressions[0], s => $"{s}?"), 1);
    }
    
    public static NatRegEnvironment WithStandardAsciiSets(this NatRegEnvironment procedures)
    {
        return procedures
            .WithVariable("any alphanumeric", new LiteralSetExpression("a-zA-Z0-9"))
            .WithVariable("any lowercase", new LiteralSetExpression("a-z"))
            .WithVariable("any uppercase", new LiteralSetExpression("A-Z"))
            .WithVariable("any numeral", new LiteralSetExpression("0-9"))
            .WithVariable("any digit", new LiteralSetExpression("\\d"))
            .WithVariable("any number", new RegexExpression("(?:[0-9]+(?:\\.[0-9]+)?)"))
            .WithVariable("any line break", new LiteralSetExpression("\\r\\n"))
            .WithVariable("any newline", new LiteralSetExpression("\\r\\n"))
            .WithVariable("lowercase alphanumeric", new LiteralSetExpression("a-z0-9"))
            .WithVariable("uppercase alphanumeric", new LiteralSetExpression("A-Z0-9"))
            .WithVariable("any word character", new LiteralSetExpression("\\w"))
            .WithVariable("any character", new RegexExpression("."));
    }

    private class ConstantExpression : NatRegExpression
    {
        public string Constant { get; }

        public ConstantExpression(string constant)
        {
            Constant = constant;
        }

        public override string ToRegex()
        {
            return Constant;
        }
    }

    
    private class OrExpression : NatRegExpression
    {
        public NatRegExpression?[] Expressions { get; }

        public OrExpression(NatRegExpression?[] expressions)
        {
            Expressions = expressions;
        }

        public override string ToRegex()
        {
            var ors = string.Join('|', Expressions.Select(e => $"(?:{e.ToRegex()})"));

            return $"(?:{ors})";
        }
    }

    private class ModifierExpression : NatRegExpression
    {
        public NatRegExpression? Expression { get; }
        public Func<string, string> Modifier { get; }

        public ModifierExpression(NatRegExpression? expression, Func<string, string> modifier)
        {
            Expression = expression;
            Modifier = modifier;
        }

        public override string ToRegex()
        {
            return Modifier(Expression?.ToRegex() ?? "");
        }
    }

    private class NumberTimesExpression : NatRegExpression
    {
        public NatRegExpression[] Arguments { get; }

        public NumberTimesExpression(NatRegExpression[] arguments)
        {
            Arguments = arguments;
        }
        
        public override string ToRegex()
        {
            if (Arguments[1] is NumberExpression numberExpression)
            {
                if (numberExpression.Value % 1 != 0)
                    throw new NatRegArgumentException("Expected an integer number, but got a decimal for repetition number.");
                    
                return $"(?:{Arguments[0].ToRegex()}{{{numberExpression.Value}}})";
            }
            else
            {
                throw new NatRegArgumentException($"Expected an integer, but got {Arguments[1].GetType()} for repetition number.");
            }
        }
    }

    private class RangeTimesExpression : NatRegExpression
    {
        public NatRegExpression[] Arguments { get; }
        public bool UseMin { get; }
        public bool UseMax { get; }

        public RangeTimesExpression(NatRegExpression[] arguments, bool useMin, bool useMax)
        {
            Arguments = arguments;
            UseMin = useMin;
            UseMax = useMax;
        }

        public override string ToRegex()
        {
            int? min = null;
            int? max = null;
            
            if (UseMin)
            {
                if (Arguments[1] is NumberExpression numberExpression)
                {
                    if (numberExpression.Value % 1 != 0)
                        throw new NatRegArgumentException("Expected an integer number, but got a decimal for range minimum.");
                    
                    min = (int)numberExpression.Value;
                }
                else
                {
                    throw new NatRegArgumentException($"Expected an integer, but got {Arguments[1].GetType()} for range minimum.");
                }
            }

            if (UseMax)
            {
                if (Arguments[2] is NumberExpression numberExpression)
                {
                    if (numberExpression.Value % 1 != 0)
                        throw new NatRegArgumentException("Expected an integer number, but got a decimal for range maximum.");
                    
                    max = (int)numberExpression.Value;
                }
                else
                {
                    throw new NatRegArgumentException($"Expected an integer, but got {Arguments[1].GetType()} for range maximum.");
                }
            }

            return $"(?:{Arguments[0].ToRegex()}{{{min.ToString() ?? ""},{max.ToString() ?? ""}}})";
        }
    }
}