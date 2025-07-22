using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace NaturalRegex;

public static class NatReg
{
    private class Listener : NatRegBaseListener
    {
        private NatRegEnvironment Environment { get; }
        
        public string RegexString { get; private set; } = "";

        public Listener(NatRegEnvironment environment)
        {
            Environment = environment;
        }

        public override void ExitDefineStatement(NatRegParser.DefineStatementContext context)
        {
            var name = context.STRING().GetText();
            name = name.Substring(1, name.Length - 2);
            var value = context.expression();

            var expressionListener = new ExpressionListener(Environment);
            var walker = new ParseTreeWalker();
        
            walker.Walk(expressionListener, value);
            
            Environment.WithVariable(name, expressionListener.Expression);
        }

        public override void ExitMatchStatement(NatRegParser.MatchStatementContext context)
        {
            var expression = context.expression();
            
            var expressionListener = new ExpressionListener(Environment);
            var walker = new ParseTreeWalker();
        
            walker.Walk(expressionListener, expression);
            
            var natExpression = expressionListener.Expression;
            
            RegexString += natExpression?.ToRegex() ?? "";
        }
        
        private class ExpressionListener : NatRegBaseListener
        {
            private NatRegEnvironment Environment { get; }
            
            public NatRegExpression? Expression { get; private set; }

            public ExpressionListener(NatRegEnvironment environment)
            {
                Environment = environment;
            }

            public override void ExitRegexExpression(NatRegParser.RegexExpressionContext context)
            {
                var str = context.REGEX().GetText();
                str = str.Substring(1, str.Length - 2);
                
                Expression = new RegexExpression(str);
            }

            public override void ExitNumberExpression(NatRegParser.NumberExpressionContext context)
            {
                Expression = new NumberExpression(double.Parse(context.NUMBER().GetText()));
            }

            public override void ExitReference(NatRegParser.ReferenceContext context)
            {
                var name = "";
                
                foreach (var word in context.WORD())
                    name += word.GetText() + " ";
                
                name = name.Trim();
                
                var expressions = new List<NatRegExpression?>();

                foreach (var expression in context.expression())
                {
                    var listener = new ExpressionListener(Environment);
                    var walker = new ParseTreeWalker();
                    
                    walker.Walk(listener, expression);
                    
                    expressions.Add(listener.Expression);
                }
                
                Expression = Environment.ResolveReference(name, expressions.ToArray());

                if (Expression == null)
                    throw new NatRegMissingReferenceException($"The reference to {name} could not be resolved.");
            }

            public override void ExitSequenceExpression(NatRegParser.SequenceExpressionContext context)
            {
                var expressions = new List<NatRegExpression?>();

                foreach (var expression in context.expression())
                {
                    var listener = new ExpressionListener(Environment);
                    var walker = new ParseTreeWalker();
                    
                    walker.Walk(listener, expression);
                    
                    expressions.Add(listener.Expression);
                }
                
                Expression = new SequenceExpression(expressions.ToArray());
            }

            public override void ExitLiteralSetExpression(NatRegParser.LiteralSetExpressionContext context)
            {
                var str = context.LITERAL_SET().GetText();
                str = str.Substring(1, str.Length - 2);
                
                Expression = new LiteralSetExpression(str);
            }

            public override void ExitAddExpression(NatRegParser.AddExpressionContext context)
            {
                var exp1 = context.expression(0);
                var exp2 = context.expression(1);
                
                var expressionListener = new ExpressionListener(Environment);
                var walker = new ParseTreeWalker();
        
                walker.Walk(expressionListener, exp1);
                
                var natExp1 = expressionListener.Expression;
                
                walker.Walk(expressionListener, exp2);
                
                var natExp2 = expressionListener.Expression;
                
                if (context.op.Text == "+")
                {
                    if (natExp1 is LiteralSetExpression literalSetExpression1 &&
                        natExp2 is LiteralSetExpression literalSetExpression2)
                        Expression = new LiteralSetExpression(literalSetExpression1.Characters + literalSetExpression2.Characters);
                    else if (natExp1 is NumberExpression numberExpression1 &&
                             natExp2 is NumberExpression numberExpression2)
                        Expression = new NumberExpression(numberExpression1.Value + numberExpression2.Value);
                    else if (natExp1 is RegexExpression stringExpression1 &&
                             natExp2 is RegexExpression stringExpression2)
                        Expression = new RegexExpression(stringExpression1.Value + stringExpression2.Value);
                    else if (natExp1 is SequenceExpression sequenceExpression1 &&
                             natExp2 is SequenceExpression sequenceExpression2)
                        Expression = new SequenceExpression(sequenceExpression1.Expressions
                            .Concat(sequenceExpression2.Expressions).ToArray());
                    else
                        throw new NatRegArgumentException(
                            $"Cannot add two different types of expressions ({natExp1?.GetType()} and {natExp2?.GetType()}).");
                }
                else if (context.op.Text == "-")
                {
                    if (natExp1 is LiteralSetExpression literalSetExpression1 &&
                        natExp2 is LiteralSetExpression literalSetExpression2)
                        Expression = new LiteralSetExpression(string.Join("", literalSetExpression1.Characters.Except(literalSetExpression2.Characters)));
                    else if (natExp1 is NumberExpression numberExpression1 &&
                             natExp2 is NumberExpression numberExpression2)
                        Expression = new NumberExpression(numberExpression1.Value - numberExpression2.Value);
                    else if (natExp1 is RegexExpression && natExp2 is RegexExpression)
                        throw new NatRegArgumentException($"Cannot subtract two strings.");
                    else if (natExp1 is SequenceExpression && natExp2 is SequenceExpression)
                        throw new NatRegArgumentException($"Cannot subtract two sequences.");
                    else
                        throw new NatRegArgumentException($"Cannot add two different types of expressions ({natExp1?.GetType()} and {natExp2?.GetType()}).");
                }
            }
        }
    }
    
    public static string GenerateRegex(string inputString, NatRegEnvironment environment)
    {
        var input = new AntlrInputStream(inputString);
        var lexer = new NatRegLexer(input);
        var parser = new NatRegParser(new CommonTokenStream(lexer));
        
        var listener = new Listener(environment);
        var walker = new ParseTreeWalker();
        
        walker.Walk(listener, parser.program());
        
        return listener.RegexString;
    }
}