namespace NaturalRegex;

public class NatRegArgumentCountException : NatRegCompileException
{
    public NatRegArgumentCountException(int expectedCount, int actualCount, string procedureName) : 
        base($"{procedureName} expected {expectedCount} arguments, but got {actualCount}.")
    {
        
    }
}