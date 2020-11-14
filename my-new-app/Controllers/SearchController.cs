using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HR.Models;
using HR.Searcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HR.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly SearchService _searchService;

        public SearchController(ILogger<SearchController> logger, IConfiguration configuration,
            IHostEnvironment environment, SearchService searchService)
        {
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
            _searchService = searchService;
        }

        [HttpPost("openFile")]
        public IActionResult OpenFile([FromBody] TextModel textModel)
        {
            Process.Start(new ProcessStartInfo(textModel.Text) {UseShellExecute = true});

            return Ok();
        }

        [HttpPost]
        public IActionResult SearchText([FromBody] TextModel textModel)
        {
            try
            {
                if (string.IsNullOrEmpty(textModel.Text)) return BadRequest();

                var texts = textModel.Text.Split(" ");
                Console.WriteLine(Environment.CurrentDirectory);

                var model = new SearchModel();
                var files = FilesRepo.Files.ToList();
                foreach (var file in files)
                {
                    try
                    {
                        if (file.Value.IsCorrupt) throw new Exception();
                        var results = _searchService.GetResults(file.Value.Words, file.Key, texts, textModel.Precision);

                        if (results != null)
                        {
                            results.City = file.Value.City;
                            model.Files.Add(results);
                        }
                    }
                    catch (Exception e)
                    {
                        model.CorruptFiles.Add(new FileModel() {Name = Path.GetFileName(file.Key), Path = file.Key});
                        Console.WriteLine($"File: {file} has error: {e.Message}");
                    }
                }

                model.Files = model.Files.OrderByDescending(x => x.Words.Count()).ToList();
                return Ok(model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}