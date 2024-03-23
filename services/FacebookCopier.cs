using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class FacebookCopier : IFacebookCopier
{
    public int CopiedPhotos { get; private set; } = 0;
    public int CopiedVideos { get; private set; } = 0;
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



    public void Copy()
    {
        Console.Output("Reading chat folders");
        var chats = FileReader.GetDirectories(rootPath, relativeActivityPaths, relativeChatFolderPaths);
        var numberOfChats = (double)chats.Count();

        Console.Output($"{numberOfChats} chat folders found.");
        Console.Output($"0% Completed (0 out of {numberOfChats})");
        for (int i = 0; i < numberOfChats; i++)
        {
            CopyFromChat(chats.ElementAt(i));
            
            var percentCompleted = (i + 1) / numberOfChats * 100;
            if (percentCompleted % 10 == 0)
            {
                Console.Output($"{percentCompleted}% Completed ({i + 1} out of {numberOfChats})");
            }
        }
    }

    private void CopyFromChat(string chatDir)
    {
        var chatJsonFiles = Directory.GetFiles(chatDir, "*.json", SearchOption.AllDirectories);
        foreach (var chatJsonFile in chatJsonFiles)
        {
            CopyFromChatJsonFile(chatJsonFile);
        }
    }

    private void CopyFromChatJsonFile(string chatJsonFilePath)
    {
        var chatJsonString = File.ReadAllText(chatJsonFilePath);
        var chatJson = JsonSerializer.Deserialize<ChatFile>(chatJsonString);

        var chatPhoto = chatJson.Image;
        if (chatPhoto != null)
        {
            CopyFile(chatPhoto);
            CopiedPhotos++;
        }


        CopyChatPhotos(chatJson.Messages);
        CopyChatVideos(chatJson.Messages);
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
                CopyFile(chatPhoto);
                CopiedPhotos++;
            }
        }
    }

    private void CopyChatVideos(ICollection<ChatEntry> chatEntries)
    {
        foreach(var chatEntry in chatEntries)
        {
            var videos = chatEntry.Videos;
            if (videos == null || videos.Count == 0)
            {
                continue;
            }

            foreach (var video in videos)
            {
                CopyFile(video);
                CopiedVideos++;
            }
        }
    }

    private void CopyFile(ILocatedFile file)
    {
        // Ensure file is not using a Cached file (stored on Facebook CDN servers)
        if (!IsLocalFile(file.URI))
        {
            return;
        }

        // If file is missing an extension: add one on based on the type passed in
        var fileUri = file.URI.Replace("/", "\\");
        var photoPath = rootPath + fileUri;
        var photoPathExtension = Path.GetExtension(photoPath);
        if (string.IsNullOrEmpty(photoPathExtension))
        {
            var type = file.GetType().ToString();
            switch (type)
            {
                case "Video":
                    photoPathExtension = ".mp4";
                    break;
                case "Photo":
                    photoPathExtension = ".jpg";
                    break;
            }
        }
        
        var newFilePath = GetCopiedFileTargetPath(file.CreateTimestamp, photoPathExtension);
        FileWriter.Copy(photoPath, newFilePath);
        
        var dateTimeCreated = DateTimeOffset.FromUnixTimeSeconds(file.CreateTimestamp).LocalDateTime;
        File.SetCreationTime(newFilePath, dateTimeCreated);
        File.SetLastWriteTime(newFilePath, DateTime.Now);
        CopiedVideos++;
    }

    private bool IsLocalFile (string fileUri)
    {
        return !fileUri.StartsWith("https");
    }

    private string GetCopiedFileTargetPath(long createTimestamp_s, string fileType)
    {
        var newFilePath = photoTargetPath;

        var dateTimeCreated = DateTimeOffset.FromUnixTimeSeconds(createTimestamp_s).LocalDateTime;
        newFilePath += dateTimeCreated.ToString("yyyy") + "\\";

        newFilePath += dateTimeCreated.ToString("yyyy-MM-dd");

        return newFilePath + " (Facebook)" + fileType;
    }
}