namespace RendleLabs.DetectChanges;

public static class PathHelper
{
    public static string FixSeparator(string original)
    {
        return Path.DirectorySeparatorChar switch
        {
            '/' => original.Replace('\\', '/'),
            '\\' => original.Replace('/', '\\'),
            _ => original
        };
    }
}