# Using Uno Platform with MVUX and Skia

## Prototipo actual - [`/Actual`](Actual/)

Dentro de la carpeta [`/Actual`](Actual/) se encuentra el proyecto en su estado actual con las siguientes funcionalidades:

- Se ejecuta pasando como parametro el archivo .svg que se desea visualizar y se lee el archivo usando SkiaSharp.Svg (el cual ha sido importado en el proyecto).
  Constructor de `MainPage`. Fichero [`/Actual/Actual/MainPage.xaml.cs`](Actual/Actual/MainPage.xaml.cs):
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
  Insertamos en el archivo [`/Actual/Actual/MainPage.xaml`](Actual/Actual/MainPage.xaml) el canvas:
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
  Y creamos la funcion `OnPaintSurface` en el archivo [`/Actual/Actual/MainPage.xaml.cs`](Actual/Actual/MainPage.xaml.cs):
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
  Actualizamos en el archivo [`/Actual/Actual/MainPage.xaml`](Actual/Actual/MainPage.xaml) el `SKXamlCanvas`:
  ```
  <sk:SKXamlCanvas x:Name="canvas"
                             
                             PaintSurface="OnPaintSurface"
                             PointerWheelChanged="OnSurfacePointerWheelChanged" />
  ```
  Y añadimos la funcion `OnSurfacePointerWheelChanged` en el archivo [`/Actual/Actual/MainPage.xaml.cs`](Actual/Actual/MainPage.xaml.cs):
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
  Actualizamos en el archivo [`/Actual/Actual/MainPage.xaml`](Actual/Actual/MainPage.xaml) el `SKXamlCanvas`:
  ```
  <sk:SKXamlCanvas x:Name="canvas"
                             
                             PaintSurface="OnPaintSurface"
                             PointerMoved="OnSurfacePointerMoved"
                             PointerPressed="OnSurfacePointerPressed"
                             PointerReleased="OnSurfacePointerReleased"
                             PointerWheelChanged="OnSurfacePointerWheelChanged" />
  ```
  Y añadimos las funciones `OnSurfacePointerMoved`, `OnSurfacePointerPressed` y `OnSurfacePointerReleased` en el archivo [`/Actual/Actual/MainPage.xaml.cs`](Actual/Actual/MainPage.xaml.cs):
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

## Diseño esperado - [`/Expected`](Expected/)

El objetivo seria hacer uso del entorno proporcionado por Uno Platform para dividir el codigo en los componentes indicados en su [tutorial de MVUX](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Mvux/Overview.html?tabs=viewmodel%2Cmodel).

- Dentro de la carpeta [`/Expected/Expected/Presentation`](Expected/Expected/Presentation) tenemos el [`MainModel`](Expected/Expected/Presentation/MainModel.cs).
  En este ejemplo el MainModel tiene una propiedad usada para obtener la temperatura actual proporcionada por WeatherService.
  ```cs
  public partial record MainModel(WeatherService WeatherService)
  {
    public IFeed<WeatherInfo> CurrentWeather => Feed.Async(this.WeatherService.GetCurrentWeather);
  }
  ```

- Dentro de la carpeta [`/Expected/Expected/Presentation`](Expected/Expected/Presentation) tenemos el MainPage.
  En el archivo [`/Expected/Expected/Presentation/MainPage.xaml.cs`](Expected/Expected/Presentation/MainPage.xaml.cs) vinculamos el DataContext al modelo MainViewModel generado a partir de [MainModel](Expected/Expected/Presentation/MainModel.cs):
  ```cs
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      DataContext = new MainViewModel(new WeatherService());
    }
  }
  ```
  Y en el archivo [`/Expected/Expected/Presentation/MainPage.xaml`](Expected/Expected/Presentation/MainPage.xaml) añadimos un FeedView el cual estara vinculado a nuestra propiedad:
  ```
  <mvux:FeedView Source="{Binding CurrentWeather}" x:Name="WeatherFeedView">
    <mvux:FeedView.ValueTemplate>
      <DataTemplate>
        <TextBlock>
          <Run Text="Current temperature: " />
          <Run Text="{Binding Data.Temperature}" />
        </TextBlock>
      </DataTemplate>
    </mvux:FeedView.ValueTemplate>
    <mvux:FeedView.ProgressTemplate>
      <DataTemplate>
        <ProgressRing />
      </DataTemplate>
    </mvux:FeedView.ProgressTemplate>
    <mvux:FeedView.ErrorTemplate>
      <DataTemplate>
        <TextBlock Text="Error" />
      </DataTemplate>
    </mvux:FeedView.ErrorTemplate>
    <mvux:FeedView.NoneTemplate>
      <DataTemplate>
        <TextBlock Text="No Results" />
      </DataTemplate>
    </mvux:FeedView.NoneTemplate>
  </mvux:FeedView>
  ```
- El servicio [`/Expected/Expected/Services/WeatherService.cs`](Expected/Expected/Services/WeatherService.cs) es el encargado de suministrar los datos que en este caso son de tipo WeatherInfo:
  ```cs
  public partial record WeatherInfo(int Temperature);

  public interface IWeatherService
  {
    ValueTask<WeatherInfo> GetCurrentWeather(CancellationToken ct);
  }

  public class WeatherService : IWeatherService
  {
    public async ValueTask<WeatherInfo> GetCurrentWeather(CancellationToken ct)
    {
      await Task.Delay(TimeSpan.FromSeconds(1), ct);
      return new WeatherInfo(new Random().Next(-40, 40));
    }
  }
  ```

## Problematica
El problema es el siguiente:
- En el ejemplo se usa un boton para accionar el refresco, en mi caso el refresco se realiza dentro del propio canvas por medio de eventos alcanzados en el canvas.
- En el ejemplo se vincula un simple texto, en mi caso deberia ¿vincular el SKXamlCanvas? ¿crear una clase hija de SKXamlCanvas?

## Beneficios
- Como indica en el [tutorial de MVUX](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Mvux/Overview.html?tabs=viewmodel%2Cmodel) si no existiera el fichero podria indicar algun error gracias al codigo establecido en el xaml:
  ```
  <mvux:FeedView.ValueTemplate>
        <DataTemplate>
          <TextBlock>
            <Run Text="Current temperature: " />
            <Run Text="{Binding Data.Temperature}" />
          </TextBlock>
        </DataTemplate>
      </mvux:FeedView.ValueTemplate>
      <mvux:FeedView.ProgressTemplate>
        <DataTemplate>
          <ProgressRing />
        </DataTemplate>
      </mvux:FeedView.ProgressTemplate>
      <mvux:FeedView.ErrorTemplate>
        <DataTemplate>
          <TextBlock Text="Error" />
        </DataTemplate>
      </mvux:FeedView.ErrorTemplate>
  ```
- Se podria establecer esperas para renderizar el canvas vinculando propiedades como se indica en el [tutorial de MVUX](https://platform.uno/docs/articles/external/uno.extensions/doc/Learn/Mvux/Overview.html?tabs=viewmodel%2Cmodel). Por ejemplo, si no se ha indicado el fichero .svg de la siguiente manera:
  ```cs
  public partial record MainModel(IWeatherService WeatherService)
  {
    public IState<string> City => State<string>.Empty(this);

    public IFeed<WeatherInfo> CurrentWeather => Feed.Async(async ct =>
    {
      var city = await City;
      if (city is not null)
      {
        return await this.WeatherService.GetCurrentWeather(city, ct);
      }
      return default;
    });
  }
  ```
  Y vinculando City a un TextBox:
  ```
  <TextBox Text="{Binding City, Mode=TwoWay}" />
  ```

## Dudas

- A la hora de usar SKXamlCanvas y SkiaSharp.Svg: ¿Habrán problemas a la hora de generar la solucion para android o algun sistema operativo?
- Para hacer zoom estoy usando la rueda del raton pero no esta soportado por Uno por obvias razones (en dispositivos mobiles no hay rueda). ¿Deberia usar otro control o no pensar en dispositivos mobiles?

## Exportar .svg desde QGIS.
  El proceso que he seguido para extraer los archivos .svg es el siguiente:
  - Descargarme los [datos](https://opendata.sitcan.es/dataset/base-topografica-5000-20042006) en formato SpatialLite.
  - Abrirlos en QGIS arrastrando el archivo 074_C05_TF.sqlite a las capas y abriendo solo la capa de las lineas.
  - Una vez haya cargado todo iremos a 'Proyecto > Nueva composicion de impresion...' o 'Control+P', añadiremos un nombre a nuestra impresion y se abrira la siguiente ventana:
https://github.com/user-attachments/assets/756eb974-c73d-40d9-8de4-5be9cc0ecacc
  - Ahora dentro de esta ventana le daremos a la herramienta 'Añadir Mapa' a la izquierda y acto seguido click a la esquina superior izquierda del lienzo. Esto pintara la capa de las lineas:
https://github.com/user-attachments/assets/fd80b5c0-d569-44b0-886f-98da4b3e4c5a
  - Ajustamos en las propiedades (panel de la derecha) la escala a la que se hara la impresion y le damos a 'Diseño > Exportar como SVG...' teniendo en cuenta las opciones marcadas en el video:
https://github.com/user-attachments/assets/c7e6b2bf-52ce-4106-a3b2-f10ac4820a16
  [Este](tenerife.svg) es el fichero extraido.
