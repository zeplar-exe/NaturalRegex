using System.ComponentModel;
using NaturalRegex;
using Spectre.Console.Cli;

var app = new CommandApp<NatRegCommand>();
return app.Run(args);

public sealed class NatRegCommand : Command<NatRegCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("File contianing NatReg statements.")]
        [CommandArgument(0, "<targetFile>")]
        public string TargetFile { get; init; }

        [Description("File containing newline-delimited var=value statements to be injected into the interpreter")]
        [CommandOption("-e|--env <ENVIRONMENT>")]
        public List<string>? Environemnt { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var environment = new NatRegEnvironment()
            .WithStandardRegexProcedures()
            .WithStandardAsciiSets();

        if (settings.Environemnt != null)
        {
            foreach (var env in settings.Environemnt)
            {
                if (File.Exists(env))
                {
                    foreach (var line in File.ReadAllLines(env))
                    {
                        var parts = line.Split('=');

                        if (parts.Length != 2)
                        {
                            Console.WriteLine($"Invalid environment declaration 'line' in {env}, expected var=value");
                        }
                        
                        NatRegExpression exp;
                        var expString = parts[1];

                        if (expString.StartsWith('[') && expString.EndsWith(']'))
                            exp = new LiteralSetExpression(expString.Substring(1, expString.Length - 2));
                        else
                            exp = new RegexExpression(expString);

                        environment = environment.WithVariable(parts[0], exp);
                    }
                }
                else
                {
                    Console.WriteLine($"Environment file {env} does not exist.");
                }
            }
        }

        if (!File.Exists(settings.TargetFile))
        {
            Console.WriteLine($"Target file {settings.TargetFile} does not exist.");

            return 1;
        }
        
        var input = File.ReadAllText(settings.TargetFile);
        string regex;

        try
        {
            regex = NatReg.GenerateRegex(input, environment);
        }
        catch (NatRegCompileException e)
        {
            Console.WriteLine($"[{e.GetType().Name}]: {e.Message}");

            return 1;
        }
        
        Console.WriteLine(regex);
        
        return 0;
    }
}