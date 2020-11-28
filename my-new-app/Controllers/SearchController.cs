using System;
using System.Collections.Concurrent;
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

                var searchPrhases = textModel.Text.Split(" ").ToList();
                Console.WriteLine(Environment.CurrentDirectory);

                var model = new SearchModel();
                var fileModels = new ConcurrentDictionary<string, FileModel>();
                var files = FilesRepo.Files.ToList();

                foreach (var file in files)
                {
                    var fileModel = GetFileData(textModel.Precision, file, searchPrhases);
                    if (fileModel != null) fileModels[file.Key] = fileModel;
                }

                model.CorruptFiles = fileModels.Where(x => x.Value.IsCorrupt).Select(x => x.Value).ToList();
                model.Files = fileModels.Where(x => !x.Value.IsCorrupt).Select(x => x.Value).ToList();
                if (textModel.SortBy == "results")
                    model.Files = model.Files.OrderByDescending(x => x.Score).ToList();
                else
                    model.Files = model.Files.OrderByDescending(x => x.CreationTime).ToList();

                return Ok(model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest(e.Message);
            }
        }

        private FileModel GetFileData(int precision, KeyValuePair<string, OpenedFile> file, List<string> searchPrhases)
        {
            var fileModel = new FileModel()
                {Name = Path.GetFileName(file.Key), Path = file.Key, IsCorrupt = file.Value.IsCorrupt};
            if (fileModel.IsCorrupt)
            {
                Console.WriteLine($"File: {file} has error");
                return fileModel;
            }

            var results = _searchService.GetResults(file.Value,
                searchPrhases, precision);

            return results;
        }
    }
}