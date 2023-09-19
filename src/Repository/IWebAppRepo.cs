using ElasticSearch.Models;

namespace ElasticSearch.Repository
{
    public interface IWebAppRepo
    {
        IList<WeatherForecastModel> GetData(int length);
    }
}
