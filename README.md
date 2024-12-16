# Using Uno Platform with MVUX and Skia

## Prototipo actual - `/Actual`

Dentro de la carpeta `/Actual` se encuentra el proyecto en su estado actual con las siguientes funcionalidades:

- Se ejecuta pasando como parametro el archivo .svg que se desea visualizar y se lee el archivo usando SkiaSharp.Svg (el cual ha sido importado en el proyecto).
  Constructor de `MainPage`. Fichero `/Actual/Actual/MainPage.xaml.cs`:
  ```cs
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
  ```

- Se renderiza la imagen dentro de un SKXamlCanvas usando la funcion PaintSurface que se ejecuta cada vez que se refresca el canvas:
  Insertamos en el archivo `/Actual/Actual/MainPage.xaml` el canvas:
  ```xaml
  <!-- Canvas con borde decorativo -->
    <Border Grid.Column="0"
            Grid.Row="0"
            BorderBrush="Gray"
            BorderThickness="2"
            Margin="10">
      <sk:SKXamlCanvas x:Name="canvas"
                        
                        PaintSurface="OnPaintSurface"/>
    </Border>
  ```
  Y creamos la funcion `OnPaintSurface` en el archivo `/Actual/Actual/MainPage.xaml.cs`:
  ```cs
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
      // Escalar el SVG según el zoom
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
  ```
- Se hace zoom gracias al codigo anterior y a la funcion `PointerWheelChanged` del `SKXamlCanvas`:
  Actualizamos en el archivo `/Actual/Actual/MainPage.xaml` el `SKXamlCanvas`:
  ```
  <sk:SKXamlCanvas x:Name="canvas"
                             
                             PaintSurface="OnPaintSurface"
                             PointerWheelChanged="OnSurfacePointerWheelChanged" />
  ```
  Y añadimos la funcion `OnSurfacePointerWheelChanged` en el archivo `/Actual/Actual/MainPage.xaml.cs`:
  ```cs
  private void OnSurfacePointerWheelChanged(object sender, PointerRoutedEventArgs e)
  {
    var delta = e.GetCurrentPoint(canvas).Properties.MouseWheelDelta;

    if (delta > 0) // Zoom in
    {
      _scale *= 1.1f;
    }
    else if (delta < 0) //Zoom out
    {
      _scale *= 0.9f;
    }

    // Forzar el redibujado después de ajustar el zoom
    canvas.Invalidate();
  }
  ```

- Se mueve la imagen aprentando y arrastrando gracias al uso de la funcion `PointerPressed`, `PointerMoved` y `PointerReleased` del `SKXamlCanvas`:
  Actualizamos en el archivo `/Actual/Actual/MainPage.xaml` el `SKXamlCanvas`:
  ```
  <sk:SKXamlCanvas x:Name="canvas"
                             
                             PaintSurface="OnPaintSurface"
                             PointerMoved="OnSurfacePointerMoved"
                             PointerPressed="OnSurfacePointerPressed"
                             PointerReleased="OnSurfacePointerReleased"
                             PointerWheelChanged="OnSurfacePointerWheelChanged" />
  ```
  Y añadimos las funciones `OnSurfacePointerMoved`, `OnSurfacePointerPressed` y `OnSurfacePointerReleased` en el archivo `/Actual/Actual/MainPage.xaml.cs`:
  ```cs
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
  ```

https://github.com/user-attachments/assets/4cf3d076-cb84-4862-a259-f58df6897659

## Diseño esperado - `/Expected`

