using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class FacebookCopier : IFacebookCopier
{
    public int CopiedAudio { get; private set; } = 0;
    public int CopiedFiles { get; private set; } = 0;
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
        CopyChatMedia();
    }

    private void CopyChatMedia()
    {
        Console.Output("Reading chat folders");
        var chats = FileReader.GetDirectories(rootPath, relativeActivityPaths, relativeChatFolderPaths);
        var numberOfChats = (double)chats.Count();

        Console.Output($"{numberOfChats} chat folders found.");
        Console.Output($"0% Completed (0 out of {numberOfChats})");
        var lastPercentOutput = 0;
        for (int i = 0; i < numberOfChats; i++)
        {
            CopyFromChat(chats.ElementAt(i));
            
            var percentCompleted = (int)((i + 1) / numberOfChats * 100);
            var interval = percentCompleted % 10;
            if (interval == 0 &&  percentCompleted != lastPercentOutput)
            {
                lastPercentOutput = percentCompleted;
                Console.Output($"{percentCompleted}% Completed ({i + 1} out of {numberOfChats})");
            }
        }
    }

    private void CopyFromChat(string chatDir)
    {
        var chatPaths = Directory.GetFiles(chatDir, "*.json", SearchOption.AllDirectories);
        foreach (var chatPath in chatPaths)
        {
            var chatJson = System.IO.File.ReadAllText(chatPath);
            var chatFile = JsonSerializer.Deserialize<ChatFile>(chatJson);

            CopyChatMedia(chatFile);
        }
    }
    private void CopyChatMedia(ChatFile chatFile)
    {
        var chatPhoto = chatFile.Image;
        if (chatPhoto != null)
        {
            var copiedPhotos = CopiedPhotos;
            CopyFile(chatPhoto, ref copiedPhotos);
            CopiedPhotos = copiedPhotos;
        }

        foreach (var message in chatFile.Messages)
        {
            CopyChatMedia(message);
        }
    }

    private void CopyChatMedia(ChatEntry chatEntry)
    {
        var audio = chatEntry.Audio;
        if (audio != null)
        {
            var copiedAudio = CopiedAudio;
            CopyFile(audio.Cast<ILocatedFile>().ToList(), ref copiedAudio);
            CopiedAudio = copiedAudio;
        }

        var files = chatEntry.Files;
        if (files != null)
        {
            var copiedFiles = CopiedFiles;
            CopyFile(files.Cast<ILocatedFile>().ToList(), ref copiedFiles);
            CopiedFiles = copiedFiles;
        }

        var photos = chatEntry.Photos;
        if (photos != null)
        {
            var copiedPhotos = CopiedPhotos;
            CopyFile(photos.Cast<ILocatedFile>().ToList(), ref copiedPhotos);
            CopiedPhotos = copiedPhotos;
        }

        var videos = chatEntry.Videos;
        if (videos != null)
        {
            var copiedVideos = CopiedVideos;
            CopyFile(videos.Cast<ILocatedFile>().ToList(), ref copiedVideos);
            CopiedVideos = copiedVideos;
        }
    }

    private void CopyFile(List<ILocatedFile> files, ref int counter)
    {
        foreach (var file in files)
        {
            CopyFile(file, ref counter);
        }
    }

    private void CopyFile(ILocatedFile file, ref int counter)
    {
        // Ensure file is not using a Cached file (stored on Facebook CDN servers)
        if (!file.IsLocalFile())
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
                default:
                    if (fileUri.Split("\\").Last().Contains("image"))
                    {
                        photoPathExtension = ".jpg";
                    }
                    break;
            }
        }
        
        var newFilePath = GetCopiedFileTargetPath(file.CreateTimestamp, photoPathExtension);
        FileWriter.Copy(photoPath, newFilePath);
        
        var dateTimeCreated = DateTimeOffset.FromUnixTimeSeconds(file.CreateTimestamp).LocalDateTime;
        System.IO.File.SetCreationTime(newFilePath, dateTimeCreated);
        System.IO.File.SetLastWriteTime(newFilePath, DateTime.Now);
        counter++;
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