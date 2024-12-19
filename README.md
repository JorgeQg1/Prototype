# Using Uno Platform with MVUX and Skia

## Crear el proyecto de la siguiente manera quita los problemas con Hot Reload:

- Presets: Blank
- Framework: .NET 8.0
- Platforms: macOS, Windows, Desktop
- Presentation: MVUX (opcional)
- Markup: XAML
- Theme: Material
- Extensions: Dependency Injection, Configuration, Blank, Default
- Features: Toolkit, VSC Debugging

## Instalar MAPSUI en el proyecto

- Click derecho encima del proyecto > Administrar Paquetes NuGet... > Examinar > Mapsui.Uno.WinUI
- Instalamos el paquete
- Añadimos el namespace: xmlns:maps="using:Mapsui.UI.WinUI" al xaml y el mapa <maps:MapControl Grid.Row="1" x:Name="MyMap" VerticalAlignment="Stretch" HorizontalAlignment="Stretch" />
- Añadimos las librerias siguientes y en el constructor la linea --> MyMap.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
  - using System;
  - using System.ComponentModel;
  - using System.Threading.Tasks;
  - using Mapsui;
  - using Mapsui.Animations;
  - using Mapsui.Extensions;
  - using Mapsui.Projections;
  - using Mapsui.Tiling;
  - using Mapsui.UI;
  - using Mapsui.Widgets.Zoom;
  - using Microsoft.UI.Xaml.Controls;
  - using Microsoft.UI.Xaml.Controls.Primitives;
  - using Microsoft.UI.Xaml.Input;
- Ejecutamos y tenemos un programa que se conecta de forma online a OpenStreetMap. Falta investigar con los .mbtiles o primero con las carpetas indexadas con xyz.

## Hacemos prueba con contenido en local

- Agregamos una carpeta MBTiles a la carpeta Assets y metemos ahi nuestros archivos .mbtiles

- Añadimos el siguiente codigo:
```cs
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        MyMap.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Projects\\PruebaMapsui\\PruebaMapsui\\Assets\\MBTiles", "world.mbtiles");
        var map = new Map();
        map.Layers.Add(CreateMbTilesLayer(filePath, "world"));
        return map;
    }

    public static ILayer CreateMbTilesLayer(string path, string name)
    {
        // Crear la capa del mapa a partir del archivo .mbtiles
        var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
        var mbTilesLayer = new TileLayer(mbTilesTileSource) { Name = name };
        return mbTilesLayer;
    }
}
```
- Click derecho encima del proyecto > Administrar Paquetes NuGet... > Examinar > SQLite
- Instalamos el paquete
- Click derecho encima del proyecto > Administrar Paquetes NuGet... > Examinar > BruTile.MbTiles
- Instalamos el paquete
- QUIZAS TAMBIEN HACE FALTA INSTALAR: Mapsui