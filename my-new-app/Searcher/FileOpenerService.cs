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

        public FileOpenerService(IHostEnvironment hostEnvironment, IConfiguration configuration, SearchService searchService)
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

            var list = Directory.GetFiles(path);

            foreach (var file in list)
            {
                try
                {
                    var text = _searchService.GetText(file);
                    FilesRepo.Files[file] = CreateFileModel(text);
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

        private OpenedFile CreateFileModel(string text)
        {
            var cityFromFile = "";
            var c = StringComparison.CurrentCultureIgnoreCase;
            foreach (var city in _cities.Where(x =>
                !(string.IsNullOrEmpty(x.Name) || string.IsNullOrEmpty(x.EnglishName))))
            {
                if (text.Contains(city.Name, c) || text.Contains(city.EnglishName, c) ||
                    text.Contains(city.Name.ReverseSearchText()))
                {
                    cityFromFile = city.Name;
                    break;
                }
            }

            return new OpenedFile() {Words = text.SplitFormat(), Text = text, City = cityFromFile};
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