using SkiaSharp;

namespace musicTag
{
    public static class ImageTools
    {
        /// <summary>
        /// Resize the image to the specified width and height with specified quality.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="quality">The quality of the resizing (e.g., Low, Medium, High).</param>
        /// <returns>The resized image.</returns>
        public static SKBitmap ResizeImage(SKBitmap image, int width, int height, SKFilterQuality quality = SKFilterQuality.High)
        {
            var resizedImage = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(resizedImage))
            {
                var paint = new SKPaint
                {
                    FilterQuality = quality,
                    IsAntialias = true
                };
                canvas.DrawBitmap(image, new SKRect(0, 0, width, height), paint);
            }
            return resizedImage;
        }
        // CHercher dans un dossier les images avec un nom tel que Front, Folder ou Cover sans prendre en compte la casse

        
        // si il y a plusieurs images, prendre la plus grande
        // si l'image la plus grande est plus grande que 1200x1200, la redimensionner
        

        /// <summary>
        /// Load an image from a file and resize it with specified quality.
        /// </summary>
        /// <param name="filePath">The path to the image file.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="quality">The quality of the resizing (e.g., Low, Medium, High).</param>
        /// <returns>The resized image.</returns>
        public static SKBitmap LoadAndResizeImage(string filePath, int width, int height, SKFilterQuality quality = SKFilterQuality.High)
        {
            using (var input = System.IO.File.OpenRead(filePath))
            {
                var image = SKBitmap.Decode(input);
                return ResizeImage(image, width, height, quality);
            }
        }
        
        public static void SaveImage(SKBitmap image, string filePath, SKEncodedImageFormat format)
        {
            using (var output = System.IO.File.OpenWrite(filePath))
            {
                image.Encode(output, format, 100);
            }
            
        }
    }
}