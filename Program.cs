using System.IO;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rootUri = GetRootUriFromUser();
            var targetUri = GetTargetUriFromUser();

            var facebookCopier = new FacebookCopier(rootUri, targetUri);
            facebookCopier.CopyPhotos();
        }

        private static string GetRootUriFromUser()
        {
            Console.Output("Please input the root of your Facebook dyi file: ");
            var rootUri = Console.Input();

            while (!IsRootUriValid(rootUri))
            {
                Console.Output("Location is not valid, please enter a valid root uri: ");
                rootUri = Console.Input();
            }
            return rootUri;
        }

        private static bool IsRootUriValid(string rootUri)
        {
            return Directory.Exists(rootUri);
        }

        private static string GetTargetUriFromUser()
        {
            Console.Output("Please input the target for your photos: ");
            return Console.Input();
        }
    }
}

// https://stackoverflow.com/questions/5337683/how-to-set-extended-file-properties
// using Microsoft.WindowsAPICodePack.Shell;
// using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

// string filePath = @"C:\temp\example.docx";
// var file = ShellFile.FromFilePath(filePath);

// // Read and Write:

// string[] oldAuthors = file.Properties.System.Author.Value;
// string oldTitle = file.Properties.System.Title.Value;

// file.Properties.System.Author.Value = new string[] { "Author #1", "Author #2" };
// file.Properties.System.Title.Value = "Example Title";

// // Alternate way to Write:

// ShellPropertyWriter propertyWriter =  file.Properties.GetPropertyWriter();
// propertyWriter.WriteProperty(SystemProperties.System.Author, new string[] { "Author" });
// propertyWriter.Close();