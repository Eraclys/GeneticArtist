using FluentAssertions;
using SkiaSharp;

namespace GeneticArtist.UnitTests;

public class ImageOperationsTests
{
    public class ColorizeInPlace
    {
        [Fact]
        public Task ShouldReturnColorizedImage()
        {
            var image = ImageLoader.Load(@"TestData\stroke_01.png");

            var scaledImage = image.ColorizeInPlace(SKColors.Firebrick);
        
            return Verify(scaledImage.Bytes);
        }
    }
    
    public class GetDifference
    {
        [Fact]
        public Task ShouldReturnCorrectDifference()
        {
            var imageA = ImageLoader.Load(@"TestData\monaliza.jpg");
            var imageB = ImageLoader.Load(@"TestData\output.png");
        
            var difference = imageA.GetDifference(imageB);
        
            return Verify(difference);
        }
    }
    
    public class RotateImage
    {
        [Fact]
        public Task ShouldReturnScaledImage()
        {
            var image = ImageLoader.Load(@"TestData\monaliza.jpg");
            var originalBytes = image.Bytes;

            var scaledImage = image.Rotate(0.5f);

            image.Bytes.Should().BeEquivalentTo(originalBytes);
        
            return Verify(scaledImage.Bytes);
        }
    }
    
    public class ScaleImage
    {
        [Fact]
        public Task ShouldReturnScaledImage()
        {
            var image = ImageLoader.Load(@"TestData\monaliza.jpg");
            var originalBytes = image.Bytes;

            var scaledImage = image.Scale(0.5f);

            image.Bytes.Should().BeEquivalentTo(originalBytes);
        
            return Verify(scaledImage.Bytes);
        }
    }

    public class GetMeanColor
    {
        [Fact]
        public Task ShouldReturnCorrectColor()
        {
            var image = ImageLoader.Load(@"TestData\monaliza.jpg");
            var stroke = ImageLoader.Load(@"TestData\stroke_01.png");
            
            var originalImageBytes = image.Bytes;
            var originalStrokeBytes = stroke.Bytes;

            var color = image.GetMeanColor(stroke, new SKPoint(image.Width / 2f, image.Height / 2f));

            image.Bytes.Should().BeEquivalentTo(originalImageBytes);
            stroke.Bytes.Should().BeEquivalentTo(originalStrokeBytes);
        
            return Verify(color);
        }
    }
}