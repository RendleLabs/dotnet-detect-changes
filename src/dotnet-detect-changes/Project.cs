using System.Xml.Linq;
using RendleLabs.DetectChanges.Git;

namespace RendleLabs.DetectChanges;

public class Project
{
    private readonly XDocument _document;
    private readonly string _projectDirectory;

    private Project(string projectFilePath, XDocument document)
    {
        _projectDirectory = Path.GetDirectoryName(projectFilePath)!;
        _document = document;
    }

    public static Project Load(string projectFilePath)
    {
        using var reader = File.OpenRead(projectFilePath);
        var document = XDocument.Load(reader, LoadOptions.None);
        return new Project(projectFilePath, document);
    }

    public IEnumerable<string> EnumerateFiles(HashSet<string>? seen = null)
    {
        if (_document.Root is null) return Enumerable.Empty<string>();
        
        seen ??= new HashSet<string>(Differ.FileSystemIsCaseSensitive(_projectDirectory)
            ? StringComparer.CurrentCulture
            : StringComparer.CurrentCultureIgnoreCase);


        return EnumerateOwnFiles().Concat(EnumerateExplicitFiles(seen)).Concat(EnumerateReferencedProjectFiles(seen));
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
            if (!excluded.Contains(file))
            {
                yield return file;
            }
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
                var referencedProject = Load(referencedProjectPath);
                foreach (var file in referencedProject.EnumerateFiles(seen))
                {
                    yield return file;
                }
            }
        }
    }

    public IEnumerable<string> EnumerateReferencedProjects()
    {
        if (_document.Root is null) yield break;

        foreach (var itemGroup in _document.Root.Elements("ItemGroup"))
        {
            foreach (var projectReference in itemGroup.Elements("ProjectReference"))
            {
                if (projectReference.Attribute("Include")?.Value is { Length: > 0 } include)
                {
                    yield return Path.GetFullPath(include, _projectDirectory);
                }
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