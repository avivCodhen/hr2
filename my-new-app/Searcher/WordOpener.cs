﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
 using System.Text.RegularExpressions;
 using Microsoft.Office.Interop.Word;
 using SautinSoft.Document;

 namespace HR.Searcher
{
    public class WordOpener : IOpener
    {
        public string OpenFile(string fileName)
        {
           
            DocumentCore dc = DocumentCore.Load(fileName);

            return dc.Content.ToString();
            
        }

       
    }
}