using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace my_new_app.Searcher
{
    public class PathService
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;

        public PathService(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        public string GetPath()
        {
            return _hostEnvironment.IsDevelopment()
                ? Path.Combine(Environment.CurrentDirectory, "hr")
                : _configuration["Folder"];
        }
    }
}