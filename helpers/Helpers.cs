using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class Helpers
{
    public static void CreateEmptyDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            var directoryIsEmpty = !Directory.GetFiles(path).Any() && !Directory.GetDirectories(path).Any();
            if (!directoryIsEmpty)
            {
                throw new Exception ("Target directory is not empty");
            }
        }
        else
        {
            Directory.CreateDirectory(path);
        }
    }

    public static string GetValidDirectoryName(string directoryName)
    {
        if (directoryName == null)
        {
            return null;
        }

        const int CUSTOM_MAX_DIRECTORY_LENGTH = 50;
        directoryName = Regex.Replace(directoryName, "[^\x00-\x7F]+", "")
            .Replace("\n", " ")
            .Replace("?", " ");
        return directoryName.Length >= CUSTOM_MAX_DIRECTORY_LENGTH
            ? directoryName.Substring(0, 50)
            : directoryName;
    }
}