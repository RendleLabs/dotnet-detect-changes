using LibGit2Sharp;

namespace RendleLabs.DetectChanges.Git;

public static class Differ
{
    public static HashSet<string>? GetChangedFiles(string projectDirectory)
    {
        var repoRoot = Repository.Discover(projectDirectory);

        if (repoRoot is not { Length: > 0 })
        {
            throw new Exception("Not a Git repo");
        }

        var repository = new Repository(repoRoot);

        var rootDirectory = Path.GetDirectoryName(repoRoot.TrimEnd('/', '\\'))!;

        var commit = repository.Commits.First();
        Console.WriteLine($"Latest:   {commit.Sha} {commit.Message} {commit.MessageShort}");
        var previousCommit = repository.Commits.Skip(1).FirstOrDefault();
        if (previousCommit is null) return null;
        Console.WriteLine($"Previous: {previousCommit.Sha} {previousCommit.Message} {previousCommit.MessageShort}");

        var comparer = FileSystemIsCaseSensitive(rootDirectory) ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
        var files = new HashSet<string>(comparer);
        
        var changes = repository.Diff.Compare<TreeChanges>(commit.Tree, previousCommit.Tree);

        foreach (var change in changes)
        {
            var path = change.Path;
            if (Path.DirectorySeparatorChar == '/')
            {
                path = path.Replace('\\', '/');
            }
            else if (Path.DirectorySeparatorChar == '\\')
            {
                path = path.Replace('/', '\\');
            }

            files.Add(Path.Combine(rootDirectory, path));
        }

        return files;
    }

    public static bool FileSystemIsCaseSensitive(string projectDirectory)
    {
        var guid = Guid.NewGuid();
        var filePath = Path.Combine(projectDirectory, $"TMP{guid:N}.tmp");
        var testPath = Path.Combine(projectDirectory, $"tmp{guid:N}.TMP");
        try
        {
            File.WriteAllText(filePath, " ");
            return !File.Exists(testPath);
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}