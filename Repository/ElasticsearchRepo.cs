using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
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


            var setting = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                .Authentication(new BasicAuthentication("elastic", "changeme"));

            //var setting = new ElasticsearchClientSettings(new Uri("http://localhost:9200"));

            _client = new ElasticsearchClient(setting);

        }

        //create an index and seed data for elasticsearch 
        public ResponseModel CreateIndex()
        {
            ResponseModel result = new();
            var response = _client.Indices.Create(INDEX_NAME);

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

        public ResponseModel SearchQuery(string key)
        {
            ResponseModel result = new();
            result.Data = new();
            var response = _client.Search<WeatherForecastModel>(s => s.Index(INDEX_NAME).From(0).Size(10000).Query(
                q => q.Match(m => m.Field(f => f.FirstName).Query(key).Analyzer("my_analyzer"))));

            if(!response.Documents.ToList().Any())
            {
                response = _client.Search<WeatherForecastModel>(s => s.Index(INDEX_NAME).From(0).Size(10000).Query(
                q => q.Match(m => m.Field(f => f.LastName).Query(key).Analyzer("autocomplete_analyzer"))).Collapse(c => c.Field("YS")));
            }

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
    }
}
