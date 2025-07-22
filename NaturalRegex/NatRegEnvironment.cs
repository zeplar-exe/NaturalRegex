namespace NaturalRegex;

public class NatRegEnvironment
{
    private Dictionary<string, NatRegExpression> Environment { get; } = new();

    public NatRegEnvironment WithVariable(string name, NatRegExpression value)
    {
        Environment[name] = value;
        
        return this;
    }
    
    public NatRegEnvironment WithProcedure(string name, Func<NatRegExpression[], NatRegExpression> func, int expectedArgumentCount = -1)
    {
        return WithProcedure([name], func, expectedArgumentCount);
    }

    public NatRegEnvironment WithProcedure(string[] names, Func<NatRegExpression[], NatRegExpression> func, int expectedArgumentCount = -1)
    {
        foreach (var name in names)
        {
            Environment[name] = new ProcedureExpression(func, expectedArgumentCount);
        }

        return this;
    }

    public bool HasVariable(string name)
    {
        return Environment.ContainsKey(name);
    }
    
    public bool TryApply(string name, NatRegExpression[] arguments, out NatRegExpression? result)
    {
        result = null;
        
        var expression = Environment.GetValueOrDefault(name);

        if (expression == null)
            return false;

        if (expression is ProcedureExpression procedureExpression)
        {
            if (arguments.Length != procedureExpression.ExpectedArgumentCount && procedureExpression.ExpectedArgumentCount >= 0)
                throw new NatRegArgumentCountException(procedureExpression.ExpectedArgumentCount, arguments.Length, name);
            
            result = procedureExpression.Func(arguments);
        }
        else
        {
            result = expression;
        }
            
        return true;
    }
    
    public NatRegExpression? ResolveReference(string reference, NatRegExpression[] arguments)
    {
        return TryApply(reference, arguments, out var result) ? result : null;
    }
    
    private class ProcedureExpression : NatRegExpression
    {
        public Func<NatRegExpression[], NatRegExpression> Func { get; }
        public int ExpectedArgumentCount { get; }

        public ProcedureExpression(Func<NatRegExpression[], NatRegExpression> func, int expectedArgumentCount)
        {
            Func = func;
            ExpectedArgumentCount = expectedArgumentCount;
        }
        
        public override string ToRegex()
        {
            throw new NotImplementedException();
        }
    }
}