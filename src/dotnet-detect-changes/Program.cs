using System.CommandLine;
using RendleLabs.DetectChanges;

var baseRefOption = new Option<string>("--base-ref", "The base ref to compare against.");
var headRefOption = new Option<string>("--head-ref", "The head ref with changes to scan.");
var baseShaOption = new Option<string>("--base-sha", "The latest commit before the push.");

var projectsArgument = new Argument<string[]>("projects", "Project(s) to scan for changes.");

var rootCommand = new Command("detect")
{
    baseRefOption,
    headRefOption,
    baseShaOption,
    projectsArgument
};

rootCommand.SetHandler((baseRef, headRef, baseSha, projects) =>
{
    try
    {
        new DetectCommand(baseRef, headRef, baseSha, projects).Execute();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        throw;
    }
}, baseRefOption, headRefOption, baseShaOption, projectsArgument);

rootCommand.Invoke(args);
