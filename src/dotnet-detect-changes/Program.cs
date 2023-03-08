using LibGit2Sharp;
using RendleLabs.DetectChanges;
using RendleLabs.DetectChanges.Git;
using Project = RendleLabs.DetectChanges.Project;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: changedetector PROJECT [PROJECT]");
    return 1;
}

var first = Path.GetFullPath(args[0]);
if (PathHelper.IsProjectFile(first))
{
    first = Path.GetDirectoryName(first)!;
}

var repoRoot = Path.GetDirectoryName(Repository.Discover(first).TrimEnd('/', '\\'))!;

var changedFiles = Differ.GetChangedFiles(first);
if (changedFiles is null)
{
    Console.WriteLine("changed=true");
    return 0;
}

foreach (var arg in args)
{
    var filePath = Path.GetFullPath(arg);
    if (!PathHelper.IsProjectFile(filePath))
    {
        if (Directory.Exists(filePath))
        {
            var projectFiles = Directory.EnumerateFiles(filePath, "*proj")
                .Where(PathHelper.IsProjectFile)
                .ToArray();
            
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

    var project = Project.Load(filePath, repoRoot);

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