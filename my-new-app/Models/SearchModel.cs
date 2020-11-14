﻿using System.Collections;
using System.Collections.Generic;

namespace HR.Models
{
    public class SearchModel
    {
        public string Text { get; set; }
        public List<FileModel> Files { get; set; }
        public string Error { get; set; }
        public List<FileModel> CorruptFiles { get; set; }
        public SearchModel()
        {
            Files = new List<FileModel>();
            CorruptFiles = new List<FileModel>();
            
        }
    }
}