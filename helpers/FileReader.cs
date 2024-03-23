using System.Collections.Generic;
using System.IO;

public class FileReader
{
    private string path;

    public FileReader(string path)
    {
        this.path = path;
    }

    public static string[] GetDirectories(string path)
    {
        return Directory.Exists(path)
            ? Directory.GetDirectories(path)
            : new string[0];
    }

    public static string[] GetDirectories(string rootPath, string[] relativePaths1, string[] relativePaths2)
    {
        var directories = new List<string>();
        foreach (var relativePath1 in relativePaths1)
        {
            foreach (var relativePath2 in relativePaths2)
            {
                var chatFolderPath = rootPath + relativePath1 + relativePath2;
                directories.AddRange(GetDirectories(chatFolderPath));
            }
        }
        return directories.ToArray();
    }
}
