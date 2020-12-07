﻿using System;
 using System.Collections;
 using System.Collections.Generic;

 namespace HR.Models
{
    public class FileModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Score { get; set; }
        public IEnumerable<string> Words { get; set; } = new List<string>();
        public DateTime CreationTime { get; set; }
        public bool IsCorrupt { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}