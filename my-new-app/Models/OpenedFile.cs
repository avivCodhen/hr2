using System.Collections;
using System.Collections.Generic;

namespace HR.Models
{
    public class OpenedFile
    {
        public string City { get; set; }
        public bool IsCorrupt { get; set; }
        public IEnumerable<string> Words { get; set; }
        public string Text { get; set; }
    }
}