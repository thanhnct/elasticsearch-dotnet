using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using ElasticSearch.Models;
using ElasticSearch.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ElasticSearch.Controllers
{

    public class HomeController : Controller
    {
 
        private readonly ILogger<HomeController> _logger;

        private readonly IElasticsearchRepo _repo;

        public HomeController(ILogger<HomeController> logger, IElasticsearchRepo repo)
        {
            _logger = logger;

            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Config()
        {
            return View();
        }



        public JsonResult Search(string key, string options)
        {
            ResponseModel data = new();
            switch(options)
            {
                default:
                    data = _repo.GetAllData();
                    break;
            }
            return Json(data);
        }


        public JsonResult CreateIndex()
        {
            return Json(_repo.CreateIndex());
        }

        public JsonResult DeleteIndex()
        {
            return Json(_repo.DeleteIndex());
        }

        public JsonResult SeedData(int length)
        {
            return Json(_repo.SeedData(length));
        }

        public JsonResult GetAllData()
        {
            return Json(_repo.GetAllData());
        }
    }
}