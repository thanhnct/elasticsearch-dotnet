﻿namespace ElasticSearch.Models
{
    public class WeatherForecastModel
    {
        public string? Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string? Name { get; set; }
        public string? OriginName { get; set; }
        public string Code { get; set; }

    }
}
