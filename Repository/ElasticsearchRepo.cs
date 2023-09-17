using Elastic.Clients.Elasticsearch;
using ElasticSearch.Controllers;
using ElasticSearch.Models;

namespace ElasticSearch.Repository
{
    public class ElasticsearchRepo : IElasticsearchRepo
    {
        private readonly string INDEX_NAME = "weather";
        private readonly IWebAppRepo _repoWebApp;
        private readonly ILogger<ElasticsearchRepo> _logger;
        private readonly ElasticsearchClient _client;

        public ElasticsearchRepo(IWebAppRepo repoWebApp, ILogger<ElasticsearchRepo> logger)
        {
            _repoWebApp = repoWebApp;
            _logger = logger;


            //_setting = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
            //    .CertificateFingerprint("[Your_Certificate]")
            //    .Authentication(new BasicAuthentication("[Your_User]", "[Your_Password]"));

            var setting = new ElasticsearchClientSettings(new Uri("http://localhost:9200"));

            _client = new ElasticsearchClient(setting);

        }

        //create an index and seed data for elasticsearch 
        public ResponseModel CreateIndex()
        {
            ResponseModel result = new();
            var response = _client.Indices.Create(INDEX_NAME, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .CharFilters(cf => cf
                            .Mapping("programming_language", mca => mca
                                .Mappings(new[]
                                {
                                    "c# => csharp",
                                    "C# => Csharp"
                                })
                            )
                        )
                        .Analyzers(an => an
                            .Custom("question", ca => ca
                                .CharFilter(new[] { "programming_language", "html_strip" })
                                .Tokenizer("standard")
                                .Filter(new[] { "lowercase", "stop" })
                            )
                        )
                    )
                ).Mappings(mm => mm.().Properties(p => p.Text(t => t.Name(n => n.Body).Analyzer("index_question").SearchAnalyzer("search_question"))))));

            result.IsSuccess = response.IsValidResponse;
            var objError = response.ApiCallDetails.OriginalException;
            if (objError is not null)
            {
                result.Message = objError.Message;
            }
            if (response.ElasticsearchServerError is not null)
            {
                result.Message = response.ElasticsearchServerError.Error.Type;
            }
            return result;
        }
        public ResponseModel DeleteIndex()
        {
            ResponseModel result = new();
            var response = _client.Indices.Delete(INDEX_NAME);
            result.IsSuccess = response.IsValidResponse;
            var objError = response.ApiCallDetails.OriginalException;
            if (objError is not null)
            {
                result.Message = objError.Message;
            }

            if (response.ElasticsearchServerError is not null)
            {
                result.Message = response.ElasticsearchServerError.Error.Type;
            }
            return result;
        }
        public ResponseModel SeedData(int length)
        {
            ResponseModel result = new();
            var data = _repoWebApp.GetData(length);
            foreach (var item in data)
            {
                var response = _client.Index(item, INDEX_NAME);
                if (!response.IsValidResponse)
                {
                    result.IsSuccess = response.IsValidResponse;
                    var objError = response.ApiCallDetails.OriginalException;
                    if (objError is not null)
                    {
                        result.Message = objError.Message;
                    }
                    if (response.ElasticsearchServerError is not null)
                    {
                        result.Message = response.ElasticsearchServerError.Error.Type;
                    }
                    break;
                }
            }
            return result;
        }
        public ResponseModel GetAllData()
        {
            ResponseModel result = new();
            result.Data = new();
            var response = _client.Search<WeatherForecastModel>(s => s.Index(INDEX_NAME).From(0).Size(10000).Query(q => q.MatchAll()));
            if (response.IsValidResponse)
            {
                result.Data = response.Documents.ToList();
            }
            else
            {
                result.IsSuccess = response.IsValidResponse;
                var objError = response.ApiCallDetails.OriginalException;
                if (objError is not null)
                {
                    result.Message = objError.Message;
                }
                if (response.ElasticsearchServerError is not null)
                {
                    result.Message = response.ElasticsearchServerError.Error.Type;
                }
            }
            return result;
        }
        //function search
    }
}
