using System.CommandLine;
using RendleLabs.DetectChanges;

var baseOption = new Option<string>("--base", "The base ref to compare against.");
var headOption = new Option<string>("--head", "The head ref with changes to scan.");
var fromCommitOption = new Option<string>("--from-commit", "The first commit in the push.");

var projectsArgument = new Argument<string[]>("projects", "Project(s) to scan for changes.");

var rootCommand = new Command("detect")
{
    baseOption,
    headOption,
    fromCommitOption,
    projectsArgument
};

rootCommand.SetHandler((baseOptionValue, headOptionValue, fromCommitOptionValue, projectsArgumentValue) =>
{
    try
    {
        new DetectCommand(baseOptionValue, headOptionValue, fromCommitOptionValue, projectsArgumentValue).Execute();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        throw;
    }
}, baseOption, headOption, fromCommitOption, projectsArgument);

rootCommand.Invoke(args);
