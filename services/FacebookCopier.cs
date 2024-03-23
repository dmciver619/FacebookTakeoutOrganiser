using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

public class FacebookCopier : IFacebookCopier
{
    private string rootPath;
    private string photoTargetPath;
    private static string[] relativeActivityPaths = new string[] { @"your_activity_across_facebook\", @"your_facebook_activity\" };
    private static string[] relativeChatFolderPaths = new string[] { @"messages\inbox\", @"messages\filtered_threads\", @"messages\message_requests\", @"messages\archived_threads\" };

    public FacebookCopier(string rootPath, string photoTargetPath)
    {
        this.rootPath = rootPath;
        this.photoTargetPath = photoTargetPath;

        // DEBUG
        this.rootPath = @"C:\Users\Dylan\Downloads\Facebook Attempt 2\";
        this.photoTargetPath = @"C:\Users\Dylan\Downloads\Facebook Photos\";

        Helpers.GetOrCreateEmptyDirectory(photoTargetPath);
    }

    public void CopyPhotos()
    {
        Console.Output("Reading chat folders");
        var chats = FileReader.GetDirectories(rootPath, relativeActivityPaths, relativeChatFolderPaths);
        var numberOfChats = (double)chats.Count();

        Console.Output($"{numberOfChats} chat folders found.");
        Console.Output($"0% Completed (0 out of {numberOfChats})");
        for (int i = 0; i < numberOfChats; i++)
        {
            CopyPhotosFromChat(chats.ElementAt(i));
            
            var percentCompleted = (i + 1) / numberOfChats * 100;
            if (percentCompleted % 10 == 0)
            {
                Console.Output($"{percentCompleted}% Completed ({i + 1} out of {numberOfChats})");
            }
        }
    }

    private void CopyPhotosFromChat(string chatDir)
    {
        var chatJsonFiles = Directory.GetFiles(chatDir, "*.json", SearchOption.AllDirectories);
        foreach (var chatJsonFile in chatJsonFiles)
        {
            CopyPhotosFromChatJsonFile(chatJsonFile);
        }
    }

    private void CopyPhotosFromChatJsonFile(string chatJsonFilePath)
    {
        var chatJsonString = File.ReadAllText(chatJsonFilePath);
        var chatJson = JsonSerializer.Deserialize<ChatFile>(chatJsonString);

        if (chatJson.Image == null)
        {
            return;
        }

        var fileUri = rootPath + chatJson.Image.URI.Replace("/", "\\");
        var fileExists = File.Exists(fileUri);
        if (!fileExists)
        {
            throw new FileNotFoundException("File does not exist");
        }

        CopyChatPhotos(chatJson.Messages);
    }

    private void CopyChatPhotos(ICollection<ChatEntry> chatEntries)
    {
        var chatsWithPhotos = chatEntries
            .Where(m => m.Photos != null && m.Photos.Count > 0)
            .ToList();
        
        foreach (var chatWithPhotos in chatsWithPhotos)
        {
            var chatPhotos = chatWithPhotos.Photos;
            foreach (var chatPhoto in chatPhotos)
            {
                // If the photo URI is accessing Facebook's CDN: skip
                if (chatPhoto.URI.StartsWith("https:"))
                {
                    continue;
                }

                var photoPath = rootPath + chatPhoto.URI.Replace("/", "\\");
                var dateTimeCreated = DateTimeOffset.FromUnixTimeSeconds(chatPhoto.CreateTimestamp).LocalDateTime;
                var newFileName = dateTimeCreated.ToString("yyyy-MM-dd");
                var newFilePath = photoTargetPath + newFileName + " (Facebook)" + Path.GetExtension(photoPath);
                FileWriter.CopyImage(photoPath, newFilePath);
                File.SetCreationTime(newFilePath, DateTime.Now);
                File.SetLastWriteTime(newFilePath, dateTimeCreated);
            }
        }
    }
}