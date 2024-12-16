using SkiaSharp;
using SkiaSharp.Views.Windows;
using Windows.Foundation;
using Microsoft.UI.Xaml.Input;

namespace Actual;

public sealed partial class MainPage : Page
{
    private float _scale = 1.0f;
    private SkiaSharp.Extended.Svg.SKSvg? _svg;

    private Point _currentPosition;
    private Point _lastPointerPosition;
    private bool _isDragging = false;

    private float _offsetX = 0; // Desplazamiento horizontal del dibujo
    private float _offsetY = 0;
    public MainPage()
    {
        this.InitializeComponent();

        var args = Environment.GetCommandLineArgs();
        if (args.Length == 2)
        {
            var svgFilePath = args[1];
            if (File.Exists(svgFilePath))
            {
                using var stream = File.OpenRead(svgFilePath);
                _svg = new SkiaSharp.Extended.Svg.SKSvg();
                _svg.Load(stream);
            }
        }
    }

    private void OnSurfacePointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var clickPosition = e.GetCurrentPoint(canvas).Position;

        LastClickPositionX.Text = $"X: {clickPosition.X:F1}";
        LastClickPositionY.Text = $"Y: {clickPosition.Y:F1}";

        _lastPointerPosition = clickPosition;
        _isDragging = true;

        canvas.Invalidate();
    }

    private void OnSurfacePointerMoved(object sender, PointerRoutedEventArgs e)
    {
        _currentPosition = e.GetCurrentPoint(canvas).Position;

        if (_isDragging)
        {
            // Calcular el desplazamiento acumulado
            var deltaX = (float)(_currentPosition.X - _lastPointerPosition.X);
            var deltaY = (float)(_currentPosition.Y - _lastPointerPosition.Y);

            // Actualizar offsets
            _offsetX += deltaX;
            _offsetY += deltaY;

            _lastPointerPosition = _currentPosition;

            canvas.Invalidate();
        }
    }

    private void OnSurfacePointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
    }

    private void OnSurfacePointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint(canvas).Properties.MouseWheelDelta;

        if (delta > 0)
        {
            // Hacer zoom in (aumentar escala)
            _scale *= 1.1f;
        }
        else if (delta < 0)
        {
            // Hacer zoom out (disminuir escala)
            _scale *= 0.9f;
        }

        // Forzar el redibujado después de ajustar el zoom
        canvas.Invalidate();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        Render(canvas, new Size(info.Width, info.Height), SKColors.LightBlue);
    }

    private void Render(SKCanvas canvas, Size size, SKColor backgroundColor)
    {
        var scale = 1.0f;
        if (this.XamlRoot != null)
        {
            var xamlRoot = this.XamlRoot;
            scale = (float)xamlRoot.RasterizationScale;
        }
        var scaledSize = new SKSize((float)size.Width / scale, (float)size.Height / scale);

        // Escalar según la densidad
        canvas.Scale(scale);

        // Limpiar el canvas con el color de fondo
        canvas.Clear(backgroundColor);

        if (_svg?.Picture != null)
        {
            // Escalar el SVG según el zoom (_scale)
            var pictureBounds = _svg.Picture.CullRect;
            var svgScale = _scale; // Usar el zoom actual

            // Centramos la imagen en el canvas
            var xOffset = (scaledSize.Width - pictureBounds.Width * svgScale) / 2 + _offsetX;
            var yOffset = (scaledSize.Height - pictureBounds.Height * svgScale) / 2 + _offsetY;

            var matrix = SKMatrix.CreateScale(svgScale, svgScale);
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)xOffset, (float)yOffset));

            canvas.DrawPicture(_svg.Picture, ref matrix);
        }
    }
}
