using System.Diagnostics;
using FlacLibSharp;
using SkiaSharp;
using TagLib;
using File = System.IO.File;

namespace musicTag;

class CoverOptimizer
{
    // Paramètres d'optimisation
    private static readonly int MaxSize = 1200; // 📏 Taille max (800x800 px)
    private static readonly int JpegQuality = 85; // 📌 Compression JPEG (1-100)
    private static readonly string[] ValidExtensions = { ".jpg", ".jpeg", ".png",".webp" };
    private static readonly string[] ValidNames = { "front", "folder", "cover" };
    private static readonly TraceSource Logger = new TraceSource("CoverOptimizer");
    private static readonly string[] MusicExtensions = { ".flac", ".mp3", ".wav" }; // Add other music file extensions here

    
    public static void ProcessMusicFolders(string baseDir)
    {
        Logger.TraceInformation($"Processing music folders in directory: {baseDir}");
        Console.WriteLine($"Processing music folders in directory: {baseDir}");
        foreach (var dir in Directory.EnumerateDirectories(baseDir))
        {
            Logger.TraceInformation($"Processing directory: {dir}");
            Console.WriteLine($"Processing directory: {dir}");
            
            var files = Directory.EnumerateFiles(dir).Where(file => IsValidMusicFile(file));
            if (!files.Any())
            {
                Logger.TraceInformation($"No Music files found in directory: {dir}");
                Console.WriteLine($"    No Music files found in directory: {dir}");
                continue;
            }
            var candidaImages = GetCoverImages(dir);

            var selectedImage = SelectACoverImage(candidaImages);
            if (selectedImage == null)
            {
                Logger.TraceInformation($"No cover images found in directory: {dir}");
                Console.WriteLine($"    No cover images found in directory: {dir}");
                continue;
            }

            if (selectedImage != null && MustBeOptimized(selectedImage, MaxSize))
            {
                selectedImage = CreateArchiveOfImage(selectedImage);
                OptimizeImage(selectedImage);
            }

            foreach (var musicFile in files)
            {
                UpdateMusicFileImage(musicFile, selectedImage);
            }
        }
        // Recursively process subdirectories
        var subdirectories = Directory.GetDirectories(baseDir);
        foreach (var subdirectory in subdirectories)
        {
            ProcessMusicFolders(subdirectory);
        }
    }
    
    private static bool IsValidMusicFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return MusicFileExtensionHelper.ExtensionMap.ContainsValue(extension);
    }

    public static void ExtractFlacImage(string flacFilePath, string outputImagePath)
    {
        // Load the FLAC file
        var file = TagLib.File.Create(flacFilePath);

        // Access the Vorbis comment metadata
        var tag = file.Tag as TagLib.Flac.Metadata;
        if (tag != null && tag.Pictures.Length > 0)
        {
            // Extract the image data from the Vorbis comment
            var picture = tag.Pictures[0];
            byte[] imageData = picture.Data.Data;

            // Save the image data to a file
            File.WriteAllBytes(outputImagePath, imageData);
            Console.WriteLine($"Image extracted and saved to: {outputImagePath}");
        }
        else
        {
            Console.WriteLine("No image found in the FLAC file.");
        }
    }

    public static void UpdateMusicFileImage(string musicFilePath, string imagePath)
    {
        // Load the FLAC file
        var file = TagLib.File.Create(musicFilePath);

        // Read the image file and convert it to a byte array
        byte[] imageData = File.ReadAllBytes(imagePath);

        // Create a new picture
        var picture = new TagLib.Picture
        {
            MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg, // or "image/png" depending on the image format
            Type = TagLib.PictureType.FrontCover,
            Description = "Cover",
            Data = imageData
        };

        // // Add the picture to the Vorbis comment
        // var tag = file.Tag as TagLib.Ogg.XiphComment;
        // Access the tag information
        
     
     
        var extension = Path.GetExtension(musicFilePath).ToLower();
        var musicFileExtension = MusicFileExtensionHelper.ExtensionMap
            .FirstOrDefault(x => x.Value == extension).Key;
        Tag tag;
        switch (musicFileExtension)
        {
            case MusicFileExtension.Flac:
                tag = file.Tag;
                break;
            case MusicFileExtension.Mp3:
                tag = file.Tag; 
                break;
            case MusicFileExtension.Wav:
                tag = file.Tag;                break;
            case MusicFileExtension.Wma:
                tag = file.Tag;                break;
            case MusicFileExtension.Ogg:
                    tag = file.Tag;
                break;
            case MusicFileExtension.Alac:
                tag = file.Tag;
                break;
            case MusicFileExtension.M4a:
                tag = file.Tag;
                break;
            default:
                throw new NotSupportedException($"Unsupported file extension: {extension}");
        }
        if (tag != null)
        {
            tag.Pictures = new TagLib.IPicture[] { picture };
        }
        
        // Print the artist, album, and title
        Console.WriteLine($"Artist: {tag.FirstPerformer}");
        Console.WriteLine($"Album: {tag.Album}");
        Console.WriteLine($"Title: {tag.Title}");

        // Save the FLAC file
        file.Save();
    }

    private static string RenameImage(string filePath, string newName)
    {
        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        string extension = Path.GetExtension(filePath);
        string newFilePath = Path.Combine(directory, newName + extension);
        File.Move(filePath, newFilePath);
        return newFilePath;
    }

    private static string? SelectACoverImage(List<string>? candidaImages)
    {
        if (candidaImages == null || candidaImages.Count == 0)
        {
            return null;
        }

        if (candidaImages.Count == 1)
        {
            return RenameImage(candidaImages[0], "cover");
        }

        // Compare images based on file size and resolution
        var images = candidaImages.Select(file =>
        {
            using (var inputStream = File.OpenRead(file))
            using (var bitmap = SKBitmap.Decode(inputStream))
            {
                return new
                {
                    file,
                    size = new FileInfo(file).Length,
                    resolution = bitmap.Width * bitmap.Height
                };
            }
        }).ToList();

        // Order by resolution first, then by size
        candidaImages = images.OrderByDescending(x => x.resolution)
            .ThenByDescending(x => x.size)
            .Select(x => x.file)
            .ToList();

        // Archive the other "cover" if exists to avoid conflicts
        ArchiveCover(candidaImages);

        // Rename the selected image
        candidaImages[0] = RenameImage(candidaImages[0], "cover");


        return candidaImages.FirstOrDefault();
    }

    static void SaveImage(SKBitmap image, string filePath, SKEncodedImageFormat format)
    {
        using (var output = System.IO.File.OpenWrite(filePath))
        {
            image.Encode(output, format, 100);
        }
    }

    /// <summary>
    /// Chercher dans un dossier les images avec un nom tel que Front, Folder ou Cover sans prendre en compte la casse
    /// et retourne l'image la plus grande
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public static List<string>? GetCoverImages(string directory)

    {
        try
        {
            string[] files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
            List<string> coverImages = [];
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                string extension = Path.GetExtension(file).ToLower();
                if (ValidExtensions.Contains(extension) && ValidNames.Contains(fileName))
                {
                    coverImages.Add(file);
                }
            }


            return coverImages;
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    private static void ArchiveCover(List<string> coverImages)
    {
        for (int i = 1; i < coverImages.Count; i++)
        {
            if (Path.GetFileNameWithoutExtension(coverImages[i])
                .Equals("cover", StringComparison.OrdinalIgnoreCase))
            {
                string newFilePath = AddSuffixToFilePath(coverImages[i], "old");
                File.Move(coverImages[i], newFilePath);
            }
        }
    }

    static string AddSuffixToFilePath(string filePath, string suffix)
    {
        string directory = Path.GetDirectoryName(filePath);
        string filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);

        // Génère un nouveau nom en ajoutant la résolution
        string newFilename = $"{filenameWithoutExt}_{suffix}{extension}";
        return Path.Combine(directory, newFilename);
    }

    static string GetNewFilePath(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        return Path.Combine(directory, "cover.jpg");
    }

    static void OptimizeImage(string imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath)) return;


            using (var inputStream = File.OpenRead(imagePath))
            using (var bitmap = SKBitmap.Decode(inputStream))
            {
                int width = bitmap.Width, height = bitmap.Height;

                // Vérifier si la taille doit être réduite
                if (width > MaxSize || height > MaxSize)
                {
                    float scale = Math.Min((float)MaxSize / width, (float)MaxSize / height);
                    width = (int)(width * scale);
                    height = (int)(height * scale);
                }

                using (var resizedBitmap = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High))
                {
                    if (resizedBitmap == null)
                    {
                        Console.WriteLine($"⚠️ Erreur lors du redimensionnement : {imagePath}");
                        return;
                    }

                    string newPath = GetNewFilePath(imagePath);
                    SKEncodedImageFormat format =
                        imagePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                        new FileInfo(imagePath).Length > 500 * 1024
                            ? SKEncodedImageFormat.Jpeg
                            : SKEncodedImageFormat.Png;

                    if (imagePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                        new FileInfo(imagePath).Length > 500 * 1024)
                    {
                        newPath = Path.ChangeExtension(newPath, ".jpg");
                        format = SKEncodedImageFormat.Jpeg;
                        File.Delete(imagePath);
                    }

                    using (var image = SKImage.FromBitmap(resizedBitmap))
                    using (var data = image.Encode(format, JpegQuality))
                    using (var outputStream = File.OpenWrite(newPath))
                    {
                        data.SaveTo(outputStream);
                    }

                    // Set file permissions to ensure it is viewable
                    File.SetAttributes(newPath, FileAttributes.Normal);
                    File.SetLastWriteTime(newPath, DateTime.Now);
                    // RenameImage(newPath, "cover");
                    Console.WriteLine($"✅ Optimisé : {newPath}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur sur {imagePath} : {ex.Message}");
        }
    }

    static bool MustBeOptimized(String filePath, int resolution)
    {
        using (var inputStream = File.OpenRead(filePath))
        using (var bitmap = SKBitmap.Decode(inputStream))
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            long fileSizeInBytes = new FileInfo(filePath).Length;
            double sizeInMb = fileSizeInBytes / (1024.0 * 1024.0);
            return width > resolution || height > resolution || sizeInMb > 0.5;
        }
    }

    static string? CreateArchiveOfImage(string filePath)
    {
        string? newPath = null;


        using (var inputStream = File.OpenRead(filePath))
        using (var bitmap = SKBitmap.Decode(inputStream))
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            long fileSizeInBytes = new FileInfo(filePath).Length;
            double sizeInMb = fileSizeInBytes / (1024.0 * 1024.0);


            string suffix = width > height ? $"{width}_{sizeInMb:F1}" : $"{height}_{sizeInMb:F1}";
            newPath = AddSuffixToFilePath(filePath, suffix);
            File.Move(filePath, newPath);
            Console.WriteLine($"🔄 Renommé : {filePath} → {newPath}");
        }

        return newPath ?? filePath;
    }
}