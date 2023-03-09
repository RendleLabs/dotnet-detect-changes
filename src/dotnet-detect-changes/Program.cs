using System.CommandLine;
using RendleLabs.DetectChanges;

var baseOption = new Option<string>("--base", "The base ref to compare against.");
var headOption = new Option<string>("--head", "The head ref with changes to scan.");

var projectsArgument = new Argument<string[]>("projects", "Project(s) to scan for changes.");

var rootCommand = new Command("detect")
{
    baseOption,
    headOption,
    projectsArgument
};

rootCommand.SetHandler((baseOptionValue, headOptionValue, projectsArgumentValue) =>
{
    new DetectCommand(baseOptionValue, headOptionValue, projectsArgumentValue).Execute();
}, baseOption, headOption, projectsArgument);

rootCommand.Invoke(args);