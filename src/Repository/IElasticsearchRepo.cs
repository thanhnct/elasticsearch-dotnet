using ElasticSearch.Models;

namespace ElasticSearch.Repository
{
    public interface IElasticsearchRepo
    {
        ResponseModel CreateIndex();
        ResponseModel DeleteIndex();
        ResponseModel SeedData(int length);
        ResponseModel GetAllData();
        ResponseModel SearchQuery(string key);
    }
}
