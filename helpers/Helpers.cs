using System;
using System.IO;
using System.Linq;

public class Helpers
{
    public static void GetOrCreateEmptyDirectory(string path)
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
}