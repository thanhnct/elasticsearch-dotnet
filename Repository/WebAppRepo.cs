using ElasticSearch.Controllers;
using ElasticSearch.Models;

namespace ElasticSearch.Repository
{
    public class WebAppRepo : IWebAppRepo
    {
        private static readonly string[] FirstNames = new[]
        {
            "John", "Mary", "Alex", "Cristiano", "Lionel", "Jr", "Gareth", "Karim", "Zidine", "Erik", 
            "Băng Băng","Lệ Băng","Tuyết Băng","Như Bảo","Gia Bảo","Xuân Bảo","Ngọc Bích","An Bình","Thái Bình","Sơn Ca","Ngọc Cầm","Nguyệt Cầm", "Thi Cầm","Bảo Châu","Bích Châu","Diễm Châu", "Hải Châu","Hoàn Châu","Hồng Châu", "Linh Châu","Loan Châu","Ly Châu","Mai Châu","Minh Châu","Trân Châu","Diệp Chi","Diễm Chi", "Hạnh Chi","Khánh Chi","Kim Chi","Lan Chi","Lệ Chi","Linh Chi","Mai Chi","Phương Chi","Quế Chi","Quỳnh Chi", "Bích Chiêu","Hoàng Cúc","Kim Cương","Diệu Ái","Khả Ái","Ngọc Ái","Hoài An","Huệ An", "Minh An","Phương An","Thanh An","Hải Ân","Huệ Ân","Mỹ Anh","Ngọc Anh","Nguyệt Anh","Như Anh","Phương Anh","Quế Anh","Quỳnh Anh","Thục Anh","Thúy Anh","Thùy Anh"
        };

        private static readonly string[] LastNames = new[]
     {
            "Doe", "Greenwood", "Ronaldo", "Neymar", "Messi", "Bale", "Benzema", "Zidane", "ten Hag",
        };

        public IList<WeatherForecastModel> GetData(int length)
        {
            return Enumerable.Range(1, 1000).Select(index => new WeatherForecastModel
            {
                Date = DateTime.Now.AddDays(index).ToString("dd/MM/yyyy HH:mm:ss"),
                TemperatureC = Random.Shared.Next(-20, 55),
                FirstName = FirstNames[Random.Shared.Next(FirstNames.Length)],
                LastName = LastNames[Random.Shared.Next(LastNames.Length)],
            }).ToList();
        }
    }
}
