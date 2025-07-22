namespace NaturalRegex;

public abstract class NatRegCompileException : Exception
{
    public NatRegCompileException(string message) : base(message)
    {
        
    }
}

public class NatRegMissingReferenceException : Exception
{
    public NatRegMissingReferenceException(string message)
    {
        
    }
}