namespace musicTag;

public enum ImageFileExtension
{
    Jpg,
    Jpeg,
    Png,
    Webp,
    
}

public static class ImageFileExtensionExtensions
{
    public static string ToFileExtension(this ImageFileExtension imageFileExtension)
    {
        return imageFileExtension switch
        {
            ImageFileExtension.Jpg => ".jpg",
            ImageFileExtension.Jpeg => ".jpeg",
            ImageFileExtension.Png => ".png",
            ImageFileExtension.Webp => ".webp",
            _ => throw new ArgumentOutOfRangeException(nameof(imageFileExtension), imageFileExtension, null)
        };
    }
}