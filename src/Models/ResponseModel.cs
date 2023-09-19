namespace ElasticSearch.Models
{
    public class ResponseModel
    {
        public List<WeatherForecastModel> Data { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "Success";
    }
}
