using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

/// <summary>
/// Service for copying facebook files from one directory to another and applying dates to the copied files based on the received dates
/// </summary>
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

    private static string relativePostPath = @"your_facebook_activity\posts\" ;

    /// <summary>
    /// Constructor for the FacebookCopier service: requires the root path of the Facebook download and the path where files should be copied to
    /// </summary>
    /// <param name="rootPath">Root path of the Facebook download folder</param>
    /// <param name="photoTargetPath">Path to the photo copy location; this does not need to exist already, however, it must be empty if it does exist</param>
    public FacebookCopier(string rootPath, string photoTargetPath)
    {
        this.rootPath = rootPath;
        if (rootPath.Last() != '\\')
        {
            this.rootPath += "\\";
        }
        this.photoTargetPath = photoTargetPath;
        if (photoTargetPath.Last() != '\\')
        {
            this.photoTargetPath += "\\";
        }

        Helpers.CreateEmptyDirectory(photoTargetPath);
    }

    /// <summary>
    /// Run the copier service
    /// </summary>
    public void Copy()
    {
        CopyPostMedia();
        CopyChatMedia();
    }

    #region Copy Posts

    private void CopyPostMedia()
    {
        Console.Output("Reading post folders");
        var postsFilePaths = Directory.GetFiles(rootPath + relativePostPath, "*.json");

        foreach (var postsFilePath in postsFilePaths)
        {
            var postsFileName = Path.GetFileNameWithoutExtension(postsFilePath);
            if (postsFileName == "your_posts__check_ins__photos_and_videos_1")
            {
                var postJson = System.IO.File.ReadAllText(postsFilePath);
                var posts = JsonSerializer.Deserialize<List<Post>>(postJson);
                foreach (var post in posts)
                {
                    CopyPost(post);
                }
            }
            else if (postsFileName == "archive")
            {
                var archiveJson = System.IO.File.ReadAllText(postsFilePath);
                var archive = JsonSerializer.Deserialize<Archive>(archiveJson);

                foreach (var archivedPost in archive.ArchivedPosts)
                {
                    CopyPost(archivedPost);
                }
            }
            else if (postsFileName == "your_videos")
            {
                var archiveJson = System.IO.File.ReadAllText(postsFilePath);
                var archive = JsonSerializer.Deserialize<Videos>(archiveJson);

                foreach (var archivedPost in archive.ArchivedPosts)
                {
                    CopyFile(archivedPost, null, null);
                }
            }
            else if (postsFileName == "your_uncategorized_photos")
            {
                var archiveJson = System.IO.File.ReadAllText(postsFilePath);
                var archive = JsonSerializer.Deserialize<UncategorizedPhotos>(archiveJson);

                foreach (var archivedPost in archive.OtherPhotos)
                {
                    CopyFile(archivedPost, null, null);
                }
            }
        }
    }

    private void CopyPost(Post post)
    {
        var postText = post.PostData.Select(pd => pd.Text).FirstOrDefault(t => !string.IsNullOrEmpty(t));
        var postName = Helpers.GetValidDirectoryName(postText);
        var postAttachments = post.PostAttachments;
        if (postAttachments == null)
        {
            return;
        }

        foreach (var postAttachment in postAttachments)
        {
            var postMediaAttachments = postAttachment.PostMediaAttachments;
            if (postMediaAttachments == null)
            {
                continue;
            }

            foreach (var postMediaAttachment in postMediaAttachments)
            {
                var postMediaAttachmentMedia = postMediaAttachment.PostMediaAttachments;
                if (postMediaAttachmentMedia == null)
                {
                    continue;
                }
                
                CopyFile(postMediaAttachmentMedia, postName, postText);
            }
        }
    }
    
    private void CopyFile(PostMediaAttachment file, string postName, string postText)
    {
        // Ensure file is not using a Cached file (stored on Facebook CDN servers)
        if (!((ILocatedFile)file).IsLocalFile())
        {
            return;
        }

        // If file is missing an extension: add one on based on the type passed in
        var fileUri = file.URI.Replace("/", "\\");
        var photoPath = rootPath + fileUri;
        var photoPathExtension = Path.GetExtension(photoPath);
        
        var newFilePath = GetCopiedFileTargetPath(file, postName, photoPathExtension);
        CopyFile(photoPath, newFilePath, file.CreateTimestamp);
        

        var directoryPath = Path.GetDirectoryName(newFilePath);
        if (!string.IsNullOrEmpty(postText))
        {
            CreateTextFile(directoryPath, postText);
        }

        var metadata = file.Metadata;
        if (metadata.VideoMetadata != null)
        {
            CopiedVideos++;
        }

        if (metadata.PhotoMetadata != null)
        {
            CopiedPhotos++;
        }
    }

    private void CreateTextFile(string path, string text)
    {
        var textFilePath = path + "\\post.txt";
        var createdFile = System.IO.File.Create(textFilePath);
        var encodedText = new UnicodeEncoding().GetBytes(text);
        createdFile.Write(encodedText);
        createdFile.Close();
    }

    #endregion

    #region Copy Chats

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
            var chatFile = System.Text.Json.JsonSerializer.Deserialize<ChatFile>(chatJson);

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
        
        var newFilePath = GetCopiedFileTargetPath(file, photoPathExtension);
        CopyFile(photoPath, newFilePath, file.CreateTimestamp);
        counter++;
    }

    #endregion

    #region Helpers

    private void CopyFile(string fromPath, string toPath, long createTimestamp)
    {
        var fileExists = System.IO.File.Exists(fromPath);
        if (!fileExists)
        {
            throw new FileNotFoundException("File does not exist");
        }

        var fileType = Path.GetExtension(toPath);
        var newFilePathWithoutExtension = toPath.Replace(fileType, "");
        if (System.IO.File.Exists(toPath))
        {
            var existsIteration = 1;
            while (System.IO.File.Exists(newFilePathWithoutExtension + $" ({existsIteration})" + fileType))
            {
                existsIteration++;
            }
            newFilePathWithoutExtension += $" ({existsIteration})";
        }
        var newFilePath = newFilePathWithoutExtension + Path.GetExtension(toPath);

        var newFileDirectory = newFilePath.Split(Path.GetFileName(newFilePath))[0].Split("/")[0];
        var directoryExists = Directory.Exists(newFileDirectory);
        if (!directoryExists)
        {
            Directory.CreateDirectory(newFileDirectory);
        }

        System.IO.File.Copy(fromPath, newFilePath);
        
        var dateTimeCreated = DateTimeOffset.FromUnixTimeSeconds(createTimestamp).LocalDateTime;
        System.IO.File.SetCreationTime(newFilePath, dateTimeCreated);
        System.IO.File.SetLastWriteTime(newFilePath, DateTime.Now);
    }
    
    private string GetCopiedFileTargetPath(PostMediaAttachment postMediaAttachment, string postName, string fileType)
    {
        var newFilePath = photoTargetPath;
        
        var dateTimeCreated = DateTimeOffset.FromUnixTimeSeconds(postMediaAttachment.CreateTimestamp).LocalDateTime;
        newFilePath += dateTimeCreated.ToString("yyyy") + "\\";

        newFilePath += string.IsNullOrEmpty(postMediaAttachment.Title)
            ? "posts\\"
            : $"albums\\{postMediaAttachment.Title}\\";

        if (!string.IsNullOrEmpty(postName))
        {
            newFilePath += postName + "\\";
        }

        newFilePath += dateTimeCreated.ToString("yyyy-MM-dd");

        return newFilePath + " (Facebook)" + fileType;
    }

    private string GetCopiedFileTargetPath(ILocatedFile locatedFile, string fileType)
    {
        var newFilePath = photoTargetPath;

        var dateTimeCreated = DateTimeOffset.FromUnixTimeSeconds(locatedFile.CreateTimestamp).LocalDateTime;
        newFilePath += dateTimeCreated.ToString("yyyy") + "\\";

        newFilePath += dateTimeCreated.ToString("yyyy-MM-dd");

        return newFilePath + " (Facebook)" + fileType;
    }

    #endregion
}