using LibGit2Sharp;

namespace RendleLabs.DetectChanges.Git;

public static class Differ
{
    public static HashSet<string>? GetChangedFiles(string projectDirectory, string? baseRef, string? headRef, string? fromCommit)
    {
        var repoRoot = Repository.Discover(projectDirectory);

        if (repoRoot is not { Length: > 0 })
        {
            throw new Exception("Not a Git repo");
        }

        var repository = new Repository(repoRoot);

        TreeChanges? changes = null;
        if (baseRef is { Length: > 0 } && headRef is { Length: > 0 })
        {
            changes = GetChanges(repository, baseRef, headRef);
        }
        else if (fromCommit is { Length: > 0 })
        {
            changes = GetChanges(repository, fromCommit);
        }
        else
        {
            changes = GetChanges(repository);
        }

        if (changes is null) return null;

        var rootDirectory = Path.GetDirectoryName(repoRoot.TrimEnd('/', '\\'))!;
        var comparer = FileSystemIsCaseSensitive(rootDirectory) ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
        var files = new HashSet<string>(comparer);

        foreach (var change in changes)
        {
            files.Add(Path.Combine(rootDirectory, PathHelper.FixSeparator(change.Path)));
        }

        return files;
    }

    private static TreeChanges? GetChanges(Repository repository, string fromCommit)
    {
        using var enumerator = repository.Commits.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var commit = enumerator.Current;
            if (commit?.Id.Sha == fromCommit) break;
        }

        if (enumerator.MoveNext())
        {
            var previous = enumerator.Current;

            if (previous is not null)
            {
                return repository.Diff.Compare<TreeChanges>(repository.Head.Tip.Tree, previous.Tree);
            }
        }

        return null;
    }
    
    private static TreeChanges? GetChanges(Repository repository, string baseRef, string headRef)
    {
        var baseBranch = repository.Branches[baseRef] ?? repository.Branches[$"refs/remotes/origin/{baseRef}"];
        var baseTree = baseBranch?.Tip?.Tree;
        var headBranch = repository.Branches[headRef] ?? repository.Branches[$"refs/remotes/origin/{headRef}"];
        var headTree = headBranch?.Tip?.Tree;

        if (baseTree is null || headTree is null) return null;

        return repository.Diff.Compare<TreeChanges>(baseTree, headTree);
    }

    private static TreeChanges? GetChanges(Repository repository)
    {
        using var enumerator = repository.Commits.GetEnumerator();
        var latest = enumerator.MoveNext() ? enumerator.Current : null;

        if (latest is null) return null;

        var previous = enumerator.MoveNext() ? enumerator.Current : null;

        if (previous is null) return null;

        return repository.Diff.Compare<TreeChanges>(latest.Tree, previous.Tree);
    }
    
    /// <summary>
    /// Test whether the file-system containing a directory is case-sensitive
    /// </summary>
    /// <param name="projectDirectory"></param>
    /// <returns></returns>
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