using Expected.Services;

namespace Expected.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        DataContext = new MainViewModel(new WeatherService());
    }
}
