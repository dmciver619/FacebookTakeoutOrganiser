using System.IO;

public class FileWriter
{
    public static void Copy(string sourcePath, string destinationPath)
    {
        var fileExists = File.Exists(sourcePath);
        if (!fileExists)
        {
            throw new FileNotFoundException("File does not exist");
        }

        var fileType = Path.GetExtension(destinationPath);
        var newFilePathWithoutExtension = destinationPath.Replace(fileType, "");
        if (File.Exists(destinationPath))
        {
            var existsIteration = 1;
            while (File.Exists(newFilePathWithoutExtension + $" ({existsIteration})" + fileType))
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

        File.Copy(sourcePath, newFilePath);
    }
}