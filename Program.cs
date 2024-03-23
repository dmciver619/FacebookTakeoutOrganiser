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
            facebookCopier.Copy();

            Console.Output($"{facebookCopier.CopiedPhotos} photos, {facebookCopier.CopiedVideos} videos, {facebookCopier.CopiedAudio} audio files and {facebookCopier.CopiedFiles} files copied");
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