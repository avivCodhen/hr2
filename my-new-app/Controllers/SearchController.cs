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
using my_new_app.Searcher;

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

                var searchPrhases = textModel.Text.Split("  ").ToList();
                Console.WriteLine(Environment.CurrentDirectory);

                var model = new SearchModel();
                var fileModels = new ConcurrentDictionary<string, FileModel>();
                var files = FilesRepo.Files.ToList();

                var tasks = new List<Task>();

                foreach (var file in files)
                {
                    var task = Task.Run(() =>
                    {
                        var fileModel = GetFileData(textModel.Precision, file, searchPrhases);
                        if (fileModel != null)
                        {
                            fileModel.Email = file.Value.Email;
                            fileModel.Phone = file.Value.Phone;
                            fileModels[file.Key] = fileModel;
                        }
                    });
                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
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
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return null;
        }

        [HttpPut("delete")]
        public IActionResult DeleteFile([FromBody] DeleteModel model)
        {
            try
            {
                var file = FilesRepo.Files[model.FileName];
                if (file == null) throw new Exception("file not found");
                System.IO.File.Delete(file.FilePath);
                FilesRepo.Files.Remove(file.FilePath, out file);
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }


        [HttpPut("rename")]
        public IActionResult RenameFile([FromBody] RenameFileModel model, [FromServices] PathService pathService)
        {
            try
            {
                var file = FilesRepo.Files[model.FileName];
                if (file == null) throw new Exception("file not found");
                var newPath = Path.Combine(pathService.GetPath(), model.NewName) +
                              Path.GetExtension(model.FileName);
                System.IO.File.Move(file.FilePath, newPath);
                FilesRepo.Files.Remove(file.FilePath, out file);
                file.FilePath = newPath;
                FilesRepo.Files[newPath] = file;
                return Ok(new {path = newPath});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }
    }
}