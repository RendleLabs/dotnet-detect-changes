using System.Xml.Linq;
using LibGit2Sharp;
using RendleLabs.DetectChanges.Git;

namespace RendleLabs.DetectChanges;

public class Project
{
    private readonly XDocument _document;
    private readonly string _repoRoot;
    private readonly string _projectDirectory;
    private readonly Repository _repository;

    private Project(string projectFilePath, XDocument document, string repoRoot)
    {
        _projectDirectory = PathHelper.FixSeparator(Path.GetDirectoryName(projectFilePath)!);
        _document = document;
        _repoRoot = PathHelper.FixSeparator(repoRoot);
        _repository = new Repository(repoRoot);
    }

    public static Project Load(string projectFilePath, string repoRoot)
    {
        using var reader = File.OpenRead(projectFilePath);
        var document = XDocument.Load(reader, LoadOptions.None);
        return new Project(projectFilePath, document, repoRoot);
    }

    public IEnumerable<string> EnumerateFiles(HashSet<string>? seen = null)
    {
        if (_document.Root is null) return Enumerable.Empty<string>();

        seen ??= new HashSet<string>(Differ.FileSystemIsCaseSensitive(_projectDirectory)
            ? StringComparer.CurrentCulture
            : StringComparer.CurrentCultureIgnoreCase);


        return EnumerateOwnFiles().Concat(EnumerateExplicitFiles(seen)).Concat(EnumerateReferencedProjectFiles(seen).Concat(EnumerateImports(seen)));
    }

    private IEnumerable<string> EnumerateOwnFiles()
    {
        var excluded = GetExcludedFiles();

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
        };

        foreach (var file in Directory.EnumerateFiles(_projectDirectory, "*", options))
        {
            if (excluded.Contains(file)) continue;

            var relativePath = Path.GetRelativePath(_repoRoot, file);
            if (_repository.Ignore.IsPathIgnored(PathHelper.UseGitSeparator(relativePath))) continue;
            
            yield return file;
        }
    }

    private IEnumerable<string> EnumerateExplicitFiles(HashSet<string> seen)
    {
        if (_document.Root is null) yield break;

        foreach (var itemGroup in _document.Root.Elements("ItemGroup"))
        {
            foreach (var item in itemGroup.Elements())
            {
                if (item.Name.LocalName is "ProjectReference" or "PackageReference") continue;
                if (item.Attribute("Include")?.Value is { Length: > 0 } include)
                {
                    var includePath = Path.Combine(include, _projectDirectory);
                    if (File.Exists(includePath) && seen.Add(includePath))
                    {
                        yield return includePath;
                    }
                }
            }
        }
    }

    private IEnumerable<string> EnumerateReferencedProjectFiles(HashSet<string> seen)
    {
        foreach (var referencedProjectPath in EnumerateReferencedProjects())
        {
            if (seen.Add(referencedProjectPath))
            {
                var referencedProject = Load(referencedProjectPath, _repoRoot);
                foreach (var file in referencedProject.EnumerateFiles(seen))
                {
                    yield return file;
                }
            }
        }
    }

    private IEnumerable<string> EnumerateReferencedProjects()
    {
        if (_document.Root is null) yield break;

        foreach (var itemGroup in _document.Root.Elements("ItemGroup"))
        {
            foreach (var projectReference in itemGroup.Elements("ProjectReference"))
            {
                if (projectReference.Attribute("Include")?.Value is { Length: > 0 } include)
                {
                    include = PathHelper.FixSeparator(include);
                    yield return Path.GetFullPath(include, _projectDirectory);
                }
            }
        }
    }

    private IEnumerable<string> EnumerateImports(HashSet<string> seen)
    {
        var directory = _projectDirectory;

        while (directory is { Length: > 0 })
        {
            var props = Path.Combine(directory, "Directory.Build.props");
            if (File.Exists(props) && seen.Add(props)) yield return props;

            var targets = Path.Combine(directory, "Directory.Build.targets");
            if (File.Exists(targets) && seen.Add(targets)) yield return targets;
            
            var packages = Path.Combine(directory, "Directory.Packages.props");
            if (File.Exists(packages) && seen.Add(packages)) yield return packages;

            if (directory == _repoRoot) break;

            directory = Path.GetDirectoryName(directory);
        }

        if (_document.Root is null) yield break;
        
        foreach (var import in _document.Root.Elements("Import"))
        {
            if (import.Attribute("Project")?.Value is { Length: > 0 } project)
            {
                project = PathHelper.FixSeparator(project);
                var path = Path.GetFullPath(project, _projectDirectory);
                if (seen.Add(path)) yield return path;
            }
        }
    }

    private HashSet<string> GetExcludedFiles()
    {
        var comparer = Differ.FileSystemIsCaseSensitive(_projectDirectory) ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;

        var excluded = new HashSet<string>(comparer);

        foreach (var itemGroup in _document.Root.Elements("ItemGroup"))
        {
            foreach (var element in itemGroup.Elements())
            {
                if (element.Attribute("Remove")?.Value is { Length: > 0 } removed)
                {
                    excluded.Add(Path.GetFullPath(removed, _projectDirectory));
                }
            }
        }

        return excluded;
    }
}