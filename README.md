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