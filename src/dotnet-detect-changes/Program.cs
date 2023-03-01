using System.Reflection.Emit;
using System.Xml.Linq;
using LibGit2Sharp;
using RendleLabs.DetectChanges;
using RendleLabs.DetectChanges.Git;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: changedetector PROJECT");
    return 1;
}

var first = Path.GetFullPath(args[0]);
if (first.EndsWith(".csproj"))
{
    first = Path.GetDirectoryName(first)!;
}

var changedFiles = Differ.GetChangedFiles(first);
if (changedFiles is null)
{
    Console.WriteLine("changed=true");
    return 0;
}

foreach (var arg in args)
{
    var filePath = Path.GetFullPath(arg);
    if (!filePath.EndsWith(".csproj", StringComparison.CurrentCultureIgnoreCase))
    {
        if (Directory.Exists(filePath))
        {
            var projectFiles = Directory.EnumerateFiles(filePath, "*.csproj").ToArray();
            if (projectFiles.Length == 1)
            {
                filePath = projectFiles[0];
            }
            else
            {
                continue;
            }
        }
    }

    var project = Project.Load(filePath);

    foreach (var projectFile in project.EnumerateFiles())
    {
        if (changedFiles.Contains(projectFile))
        {
            Console.WriteLine("changed=true");
            return 0;
        }
    }
}

Console.WriteLine("changed=false");
return 0;