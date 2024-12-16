using Expected.Services;

namespace Expected.Presentation;
public partial record MainModel(WeatherService WeatherService)
{
    public IFeed<WeatherInfo> CurrentWeather => Feed.Async(this.WeatherService.GetCurrentWeather);

    /*public IFeed<CanvasInfo> CurrentCanvas => Feed.Async(async ct =>
    {
        var filepath = await FilePath;
        if (filepath is not null)
        {
            return await this.WeatherService.GetCurrentWeather(city, ct);
    }
        return default;
    });

    public IState<string> FilePath => State<string>.Empty(this);*/
}
