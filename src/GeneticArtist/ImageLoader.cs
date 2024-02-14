using SkiaSharp;

namespace GeneticArtist;

public static class ImageLoader
{
    public static SKBitmap Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        using var stream = new SKFileStream(filePath);
        return SKBitmap.Decode(stream);
    }
    
    public static SKBitmap[] LoadStrokes(string imageDirectory)
    {
        var directoryInfo = new DirectoryInfo(imageDirectory);
        
        if (!directoryInfo.Exists)
            throw new DirectoryNotFoundException("Directory not found");
        
        var files = directoryInfo.GetFiles("*.png");
        var images = new SKBitmap[files.Length];

        for (var index = 0; index < files.Length; index++)
        {
            var file = files[index];
            images[index] = LoadImageAsGrayscaleWithTransparency(file.FullName);
        }

        return images;
    }
    
    static SKBitmap LoadImageAsGrayscaleWithTransparency(string filePath)
    {
        using var image = Load(filePath);
        var newImage = new SKBitmap(image.Width, image.Height);
        using var canvas = new SKCanvas(newImage);
        canvas.Clear(SKColors.Transparent);
    
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                var brightness = (byte)((pixel.Red + pixel.Green+ pixel.Blue)/3);
                newImage.SetPixel(x, y, new SKColor(255, 255, 255, brightness));
            }
        }
        
        return newImage;
    }
    
    public static void SaveBitmapToFile(SKBitmap bitmap, string filePath)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        
        data.SaveTo(stream);
    }
}