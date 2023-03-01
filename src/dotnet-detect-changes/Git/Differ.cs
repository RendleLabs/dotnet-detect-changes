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

        using var enumerator = repository.Commits.GetEnumerator();
        var latest = enumerator.MoveNext() ? enumerator.Current : null;

        if (latest is null) return null;

        var previous = enumerator.MoveNext() ? enumerator.Current : null;

        if (previous is null) return null;

        if (latest.Message.StartsWith($"Merge {previous.Sha} into "))
        {
            latest = previous;
            previous = enumerator.MoveNext() ? enumerator.Current : null;
            if (previous is null) return null;
        }
        Console.WriteLine($"Latest:   {latest.Sha} {latest.Message} {latest.MessageShort}");
        Console.WriteLine($"Previous: {previous.Sha} {previous.Message} {previous.MessageShort}");

        // var commit = repository.Commits.First();
        // Console.WriteLine($"Latest:   {commit.Sha} {commit.Message} {commit.MessageShort}");
        // var previousCommit = repository.Commits.Skip(1).FirstOrDefault();
        // if (previousCommit is null) return null;

        var comparer = FileSystemIsCaseSensitive(rootDirectory) ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
        var files = new HashSet<string>(comparer);
        
        var changes = repository.Diff.Compare<TreeChanges>(latest.Tree, previous.Tree);

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