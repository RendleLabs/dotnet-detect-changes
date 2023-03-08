namespace RendleLabs.DetectChanges;

public static class PathHelper
{
    /// <summary>
    /// Fixes directory separator characters in paths
    /// </summary>
    /// <param name="original">The original string</param>
    /// <returns>Fixed string</returns>
    public static string FixSeparator(string original)
    {
        return Path.DirectorySeparatorChar switch
        {
            '/' => original.Replace('\\', '/'),
            '\\' => original.Replace('/', '\\'),
            _ => original
        };
    }

    public static string UseGitSeparator(string original) => original.Replace('\\', '/');

    public static bool IsProjectFile(string path) => path.EndsWith("proj", StringComparison.CurrentCultureIgnoreCase) && File.Exists(path);
}