using System.Collections.Generic;
using HR.Models;

namespace HR.Searcher
{
    public class FilesRepo
    {
        public static Dictionary<string, OpenedFile> Files { get; set; } = new Dictionary<string, OpenedFile>();

    }
}