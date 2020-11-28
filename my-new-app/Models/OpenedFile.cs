using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HR.Models
{
    public class OpenedFile
    {
        public string FilePath { get; set; }
        public string City { get; set; }
        public bool IsCorrupt { get; set; }
        public List<string> Words { get; set; }
        public string Text { get; set; }
        public DateTime CreationTime { get; set; }
    }
}