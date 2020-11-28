using System.Collections.Concurrent;
using System.Collections.Generic;
using HR.Models;

namespace HR.Searcher
{
    public class FilesRepo
    {
        public static ConcurrentDictionary<string, OpenedFile> Files { get; set; } = new ConcurrentDictionary<string, OpenedFile>();

    }
}