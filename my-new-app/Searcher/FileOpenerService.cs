using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Models;
using HR.Searcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace my_new_app.Searcher
{
    public class FileOpenerService : IHostedService
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly SearchService _searchService;
        private readonly List<City> _cities = new List<City>();

        public FileOpenerService(IHostEnvironment hostEnvironment, IConfiguration configuration,
            SearchService searchService)
        {
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
            _searchService = searchService;
            try
            {
                _cities = JsonConvert.DeserializeObject<List<City>>(
                    File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "cities.json")));
            }
            catch (Exception e)
            {
                _cities = new List<City>();
                Console.WriteLine(e);
            }
        }

        protected Task Execute()
        {
            var path = _hostEnvironment.IsDevelopment()
                ? Path.Combine(Environment.CurrentDirectory, "hr")
                : _configuration["Folder"];

            Console.WriteLine($"Reading files from {path}...");
            Console.WriteLine("Updating...");
            var list = Directory.GetFiles(path);
            Console.WriteLine($"from {list.Length}");

            list = list.Where(x => !FilesRepo.Files.ContainsKey(x)).ToArray();

            var index = 0;
            foreach (var file in list)
            {
                try
                {
                    index++;
                    var openedFile = CreateFileModel(file);
                    FilesRepo.Files[file] = openedFile;
                    Console.Write($"\r{index} of {list.Length}  ");
                }
                catch (Exception e)
                {
                    FilesRepo.Files[file] = new OpenedFile() {IsCorrupt = true};
                    Console.WriteLine($"File: {file} cannot open. Err: {e.Message}");
                }
            }

            Console.WriteLine("Finished reading");
            return Task.CompletedTask;
        }

        private OpenedFile CreateFileModel(string file)
        {
            var text = _searchService.GetText(file);

            var creationTime = File.GetCreationTime(file);
            return new OpenedFile()
                {Words = text.SplitFormat().ToList(), Text = text, CreationTime = creationTime, FilePath = file};
        }

        private string CityFromFile(string text)
        {
            var cityFromFile = "";
            var c = StringComparison.CurrentCultureIgnoreCase;
            foreach (var city in _cities.Where(x =>
                !(string.IsNullOrEmpty(x.Name) || string.IsNullOrEmpty(x.EnglishName))))
            {
                if (text.Contains(city.Name, c) || text.Contains(city.EnglishName, c) ||
                    text.Contains(city.Name.Reverse()))
                {
                    cityFromFile = city.Name;
                    break;
                }
            }

            return cityFromFile;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Execute();
                    await Task.Delay(5 * 60 * 1000);
                    Console.WriteLine("File Opener sleeps");
                }
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}