using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Transport;
using ElasticSearch.Controllers;
using ElasticSearch.Models;
using System;
using System.Text.Json;

namespace ElasticSearch.Repository
{
    public class ElasticsearchRepo : IElasticsearchRepo
    {
        private readonly string INDEX_NAME = "weather";
        private readonly IWebAppRepo _repoWebApp;
        private readonly ILogger<ElasticsearchRepo> _logger;
        private readonly ElasticsearchClient _client;

        public class settinaags
        {
            public settings settings { get; set; }
        }
        public class settings
        {
            public index index { get; set; }
        }

        public class index
        {
            public int number_of_shards { get; set; }
        }
        public ElasticsearchRepo(IWebAppRepo repoWebApp, ILogger<ElasticsearchRepo> logger)
        {
            _repoWebApp = repoWebApp;
            _logger = logger;


            //var setting = new ElasticsearchClientSettings(new Uri("http://localhost:9200")).CertificateFingerprint("")
            //    .Authentication(new BasicAuthentication("elastic", "changeme"));

            var setting = new ElasticsearchClientSettings(new Uri("http://localhost:9200"));

            _client = new ElasticsearchClient(setting);

        }

        //create an index and seed data for elasticsearch 
        public ResponseModel CreateIndex()
        {
            //declare my filter
            ResponseModel result = new();

            var absolutepath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(absolutepath + "/PostDataJson/elasticsearch_index_data.json");
            PostData postData;
            using (StreamReader r = new StreamReader(filePath))
            {
                postData = r.ReadToEnd();
            }

            CreateIndexResponse response = _client.Transport.Put<CreateIndexResponse>("/weather", postData);

            //var response = _client.Indices.Create(INDEX_NAME, c => c.Settings(s =>
            //    s.NumberOfShards(1) //The number of primary shards that an index should have. Default 1.
            //    .NumberOfReplicas(1) //The number of replicas each primary shard has. Defaults to 1.
            //    .MaxNgramDiff(7) //The maximum allowed difference between min_gram and max_gram for NGramTokenizer and NGramTokenFilter. Defaults to 1
            //    .Analysis(a =>
            //        a.TokenFilters(f => f.cus(""))
            //));

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
        public ResponseModel SearchQueryMatchField(string key)
        {
            ResponseModel result = new();
            result.Data = new();
            if (!string.IsNullOrEmpty(key))
            {
                var response = _client.Search<WeatherForecastModel>(s => s.Index(INDEX_NAME).From(0).Size(10000).Query(
                q => q.Match(m => m.Field(f => f.Name).Query(key).Analyzer("vietnamese_analyzer"))));

                var dataResult = response.Documents.ToList();

                // If nothing is found, search for the character
                if (!dataResult.Any())
                {
                    response = _client.Search<WeatherForecastModel>(s => s.Index(INDEX_NAME).From(0).Size(10000).Query(
                    q => q.Match(m => m.Field(f => f.OriginName).Query(key).Analyzer("standard"))));
                    dataResult = response.Documents.ToList();
                }

                if (response.IsValidResponse)
                {
                    result.Data = dataResult;
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
            }
            return result;
        }
        public ResponseModel SearchQueryMatchFields(string key)
        {
            ResponseModel result = new();
            result.Data = new();
            if (!string.IsNullOrEmpty(key))
            {
                var response = _client.Search<WeatherForecastModel>(s => s.Index(INDEX_NAME).From(0).Size(10000).Query(
                q => q.QueryString(m => m.Fields(new string[] { "code", "name"}).Query(key))));

                var dataResult = response.Documents.ToList();

                if (response.IsValidResponse)
                {
                    result.Data = dataResult;
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
            }
            return result;
        }
        public ResponseModel SearchQueryStringFields(string key)
        {
            ResponseModel result = new();
            result.Data = new();
            var absolutepath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(absolutepath + "/PostDataJson/elasticsearch_search_data.json");

            PostData postData;
            using (StreamReader r = new StreamReader(filePath))
            {
                postData = r.ReadToEnd();
            }

            SearchResponse<WeatherForecastModel> response = _client.Transport.Post<SearchResponse<WeatherForecastModel>>("/weather/_search", postData);

            var dataResult = response.Documents.ToList();

            if (response.IsValidResponse)
            {
                result.Data = dataResult;
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
