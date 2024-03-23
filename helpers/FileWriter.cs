using System.IO;

public class FileWriter
{
    public static void Copy(string sourcePath, string destinationPath)
    {
        var fileExists = System.IO.File.Exists(sourcePath);
        if (!fileExists)
        {
            throw new FileNotFoundException("File does not exist");
        }

        var fileType = Path.GetExtension(destinationPath);
        var newFilePathWithoutExtension = destinationPath.Replace(fileType, "");
        if (System.IO.File.Exists(destinationPath))
        {
            var existsIteration = 1;
            while (System.IO.File.Exists(newFilePathWithoutExtension + $" ({existsIteration})" + fileType))
            {
                existsIteration++;
            }
            newFilePathWithoutExtension += $" ({existsIteration})";
        }
        var newFilePath = newFilePathWithoutExtension + Path.GetExtension(destinationPath);

        var newFileDirectory = newFilePath.Split(Path.GetFileName(newFilePath))[0];
        var directoryExists = Directory.Exists(newFileDirectory);
        if (!directoryExists)
        {
            Directory.CreateDirectory(newFileDirectory);
        }

        System.IO.File.Copy(sourcePath, newFilePath);
    }
}