using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GeneticArtist.Chromosomes;
using Microsoft.IO;
using Microsoft.Win32;
using SkiaSharp;

namespace GeneticArtist.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new();

    readonly SKBitmap[] _strokeImages;
    readonly GeneticConfig _geneticConfig;
    
    Artist? _artist;
    SKBitmap _targetImage;
    SKBitmap _canvas;
    Func<IChromosomePainter> _createChromosomePainter;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _geneticConfig = new GeneticConfig
        {  
            MinGenerations = 2,
            MinPopulationSize = 2,
            MaxGenerations = 32,
            MaxPopulationSize = 32,
            MutationProbability = 0.3f,
            MaxIterations = 100000
        };
        
        _targetImage = ImageLoader.Load(@"Examples\Targets\monaliza.jpg");
        _strokeImages = ImageLoader.LoadStrokes(@"Examples\Strokes");
        _canvas = new SKBitmap(_targetImage.Width, _targetImage.Height);
        using var canvasImage = new SKCanvas(_canvas);
        canvasImage.Clear(SKColors.Black);
        
        _createChromosomePainter = () => new StrokeAutoColorChromosome(_targetImage, _strokeImages);

        LabelGeneration.Content = "Iteration 0";
        TargetImage.Source = ConvertSkiaBitmapToWpfImage(_targetImage);
        BestFitImage.Source = ConvertSkiaBitmapToWpfImage(_canvas);
        ButtonStop.IsEnabled = false;
    }

    Func<IChromosomePainter> GetChromosomePainterFactory(PainterType type) => type switch
    {
        PainterType.StrokeAutoColor => () => new StrokeAutoColorChromosome(_targetImage, _strokeImages),
        PainterType.Stroke => () => new StrokeChromosome(_targetImage, _strokeImages),
        PainterType.Polygon => () => new PolygonChromosome(_targetImage),
        PainterType.LineAutoColor => () => new LineChromosome(_targetImage),
        PainterType.PolygonAutoColor => () => new PolygonAutoColorChromosome(_targetImage),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    void SelectTargetImage_Click(object sender, RoutedEventArgs e)
    {      
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };

        if (openFileDialog.ShowDialog() != true) 
            return;
        
        Stop();

        _targetImage = ImageLoader.Load(openFileDialog.FileName);

        if (_targetImage.Width != _canvas.Width || _targetImage.Height != _canvas.Height)
        {
            _canvas = new SKBitmap(_targetImage.Width, _targetImage.Height);
            using var canvasImage = new SKCanvas(_canvas);
            canvasImage.Clear(SKColors.Black);
            BestFitImage.Source = ConvertSkiaBitmapToWpfImage(_canvas);
        }
        
        LabelGeneration.Content = "Iteration 0";
        TargetImage.Source = ConvertSkiaBitmapToWpfImage(_targetImage);
    }
    
    void SelectCanvasImage_Click(object sender, RoutedEventArgs e)
    {      
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
        };

        if (openFileDialog.ShowDialog() != true) 
            return;
        
        Stop();

        _canvas = ImageLoader.Load(openFileDialog.FileName);
        
        LabelGeneration.Content = "Iteration 0";
        BestFitImage.Source = ConvertSkiaBitmapToWpfImage(_canvas);
    }

    void Start_Click(object sender, RoutedEventArgs e) => Start();
    void Stop_Click(object sender, RoutedEventArgs e) => Stop();

    long _lastTimestamp = Stopwatch.GetTimestamp();

    void OnIterationCompleted(int iterationCount, SKBitmap canvas, TimeSpan elapsed)
    {
        if (iterationCount % 100 == 0)
        {
            ImageLoader.SaveBitmapToFile(canvas, $"output_{iterationCount}.png");
        }
        
        var elapsedTime = Stopwatch.GetElapsedTime(_lastTimestamp);

        if (elapsedTime.TotalMilliseconds < 300)
            return;

        Dispatcher.Invoke(() =>
        {
            LabelGeneration.Content = $"Iteration {iterationCount.ToString()}";
            LabelDetail.Content = $"{((int)elapsed.TotalMilliseconds).ToString()} ms per iteration, Skipped: {_artist?.SkipCount.ToString()}, Population: {_artist?.PopulationSize.ToString()}, Generations: {_artist?.Generations.ToString()}";
            BestFitImage.Source = ConvertSkiaBitmapToWpfImage(canvas);
        });
        
        _lastTimestamp = Stopwatch.GetTimestamp();
    }

    static BitmapImage ConvertSkiaBitmapToWpfImage(SKBitmap skiaBitmap)
    {
        using var memoryStream = RecyclableMemoryStreamManager.GetStream();
        // Encode the SkiaSharp bitmap to a memory stream as PNG
        skiaBitmap.Encode(memoryStream, SKEncodedImageFormat.Png, 100);
        memoryStream.Seek(0, SeekOrigin.Begin); // Important: Reset stream position to the beginning

        // Create a BitmapImage in WPF
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Here to ensure the stream is not closed prematurely
        bitmapImage.StreamSource = memoryStream;
        bitmapImage.EndInit();
        bitmapImage.Freeze(); // Optional: Makes the image usable across threads

        return bitmapImage;
    }

    void PainterType_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
        {
            Stop();
            var selectedValue = (PainterType)int.Parse(rb.Tag.ToString());
            _createChromosomePainter = GetChromosomePainterFactory(selectedValue);
        }
    }

    void Start()
    {
        ButtonSelectTargetImage.IsEnabled = false;
        ButtonSelectCanvasImage.IsEnabled = false;
        ButtonStart.IsEnabled = false;
        ButtonStop.IsEnabled = true;
        
        try
        {
            _artist = new Artist(
                _targetImage,
                _createChromosomePainter(), 
                _geneticConfig,
                _canvas,
                OnIterationCompleted,
                _artist?.CurrentBest);
        
            Task.Run(() => _artist.Start());
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
            Stop();
        }
    }

    void Stop()
    {
        _artist?.Stop();
        
        ButtonSelectTargetImage.IsEnabled = true;
        ButtonSelectCanvasImage.IsEnabled = true;
        ButtonStart.IsEnabled = true;
        ButtonStop.IsEnabled = false;
    }

    void Reset()
    {
        Stop();
        
        LabelGeneration.Content = "Iteration 0";
        BestFitImage.Source = ConvertSkiaBitmapToWpfImage(_canvas);
    }
}