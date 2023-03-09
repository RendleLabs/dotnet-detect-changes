using LibGit2Sharp;
using RendleLabs.DetectChanges.Git;

namespace RendleLabs.DetectChanges;


public class DetectCommand 
{
    private readonly string? _baseRef;
    private readonly string? _headRef;
    private readonly string _baseSha;
    private readonly string[] _projects;

    public DetectCommand(string? baseRef, string? headRef, string baseSha, string[] projects)
    {
        _baseRef = baseRef;
        _headRef = headRef;
        _baseSha = baseSha;
        _projects = projects;
    }
    
    public bool Execute()
    {
        var first = Path.GetFullPath(_projects[0]);
        if (PathHelper.IsProjectFile(first))
        {
            first = Path.GetDirectoryName(first)!;
        }

        var repoRoot = Path.GetDirectoryName(Repository.Discover(first).TrimEnd('/', '\\'))!;

        var changedFiles = Differ.GetChangedFiles(first, _baseRef, _headRef, _baseSha);
        
        if (changedFiles is null)
        {
            Console.WriteLine("changed=true");
            return true;
        }

        foreach (var arg in _projects)
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
                    return true;
                }
            }
        }

        Console.WriteLine("changed=false");
        return true;
    }
}