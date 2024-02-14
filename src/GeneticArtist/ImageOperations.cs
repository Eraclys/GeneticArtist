using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using SkiaSharp;

namespace GeneticArtist;

public static class ImageOperations
{
    public static SKBitmap Rotate(this SKBitmap image, float rotationAngle)
    {
        var rotatedStroke = new SKBitmap(image.Width, image.Height);
        using var canvas = new SKCanvas(rotatedStroke);
        
        canvas.Clear(SKColors.Transparent);
        canvas.Translate(image.Width / 2f, image.Height / 2f);
        canvas.RotateDegrees(rotationAngle);
        canvas.Translate(-image.Width / 2f, -image.Height / 2f);
        canvas.DrawBitmap(image, 0, 0);

        return rotatedStroke;
    }

    public static SKBitmap Scale(this SKBitmap image, float scale)
    {
        var resizedInfo = new SKImageInfo((int)(image.Width * scale), (int)(image.Height * scale));
        return image.Resize(resizedInfo, SKFilterQuality.High);
    }
    
    public static unsafe SKBitmap ColorizeInPlace(this SKBitmap image, SKColor color)
    {
        var pixelCount = image.Width * image.Height;
        var pixels = (SKColor*)image.GetPixels().ToPointer();
        
        for (var i = 0; i < pixelCount; i++)
        {
            var alpha = pixels[i].Alpha;

            if (alpha != 0) // Check if alpha is not 0
            {
                var premultipliedRed = (byte)(color.Red * alpha / 255);
                var premultipliedGreen = (byte)(color.Green * alpha / 255);
                var premultipliedBlue = (byte)(color.Blue * alpha / 255);
                    
                // Preserve the alpha of the original pixel and apply the new RGB values
                pixels[i] = new SKColor(premultipliedRed, premultipliedGreen, premultipliedBlue, alpha);
            }
        }

        // No need to reassign the pixels back to the bitmap, as we've modified the bitmap's memory directly
        return image;
    }
    
    public static unsafe SKColor GetMeanColor(this SKBitmap targetImg, SKBitmap clipImage, SKPoint position)
    {
        var targetImgSpan = (SKColor*)targetImg.GetPixels().ToPointer();
        var clipImageSpan = (SKColor*)clipImage.GetPixels().ToPointer();
        
        var positionX = (int)position.X < 0 ? 0 : (int)position.X;
        var positionY = (int)position.Y < 0 ? 0 : (int)position.Y;
        
        long totalR = 0;
        long totalG = 0;
        long totalB = 0;
        long pixelCount = 0;

        var clipImageHeight = clipImage.Height;
        var clipImageWidth = clipImage.Width;
        var targetImgWidth = targetImg.Width;
        var targetImgHeight = targetImg.Height;

        for (var y = 0; y < clipImageHeight; y++)
        {
            var offsetY = y + positionY;
                
            if (offsetY >= targetImgHeight)
                continue;
                
            for (var x = 0; x < clipImageWidth; x++)
            {
                var offsetX = x + positionX;

                if (offsetX >= targetImgWidth)
                    continue;
                    
                var clipPixel = clipImageSpan[y * clipImageWidth + x];

                if (clipPixel.Alpha == 0) 
                    continue;

                var targetPixel = targetImgSpan[offsetY * targetImgWidth + offsetX];

                totalR += targetPixel.Red;
                totalG += targetPixel.Green;
                totalB += targetPixel.Blue;
                pixelCount++;
            }
        }

        if (pixelCount == 0)
            return SKColors.Black; // Return a default color if no valid pixels were found

        // Calculate mean color
        var meanR = (byte)(totalR / pixelCount);
        var meanG = (byte)(totalG / pixelCount);
        var meanB = (byte)(totalB / pixelCount);

        return new SKColor(meanR, meanG, meanB);
    }
    
    public static double GetDifference(this SKBitmap imgA, SKBitmap imgB)
    {
        if (imgA.Width != imgB.Width || imgA.Height != imgB.Height)
            throw new ArgumentException("Images must be of the same size.");
        
        long totalDiff = 0;
        var pixelCount = imgA.Width * imgA.Height;    
        var byteCount = pixelCount * 4; // Assuming 4 bytes per pixel (RGBA)

        unsafe
        {
            var ptrA = (byte*)imgA.GetPixels().ToPointer();
            var ptrB = (byte*)imgB.GetPixels().ToPointer();
            
            var vectorSizeInt32 = Vector128<uint>.Count;
            var vectorSizeByte = Vector128<byte>.Count;
            var maxLookup = byteCount - vectorSizeByte;

            var i = 0;

            while (i < maxLookup)
            {
                var va = Sse41.ConvertToVector128Int32(Sse2.LoadVector128(ptrA + i));
                var vb = Sse41.ConvertToVector128Int32(Sse2.LoadVector128(ptrB + i));
                var diff = Sse2.Subtract(va, vb);
                var abs = Ssse3.Abs(diff);

                for (var j = 0; j < vectorSizeInt32; j++)
                {
                    totalDiff += abs.GetElement(j);
                }

                i += vectorSizeInt32;
            }

            // Handle remaining pixels
            for (; i < byteCount; i++)
            {
                totalDiff += Math.Abs(ptrA[i] - ptrB[i]);
            }
        }

        // Calculate average difference per color channel
        var avgDiff = totalDiff / (4.0 * pixelCount);

        // Normalize the difference to a [0, 1] range
        return avgDiff / 255.0;
    }
}