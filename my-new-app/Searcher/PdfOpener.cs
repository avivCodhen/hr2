﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace HR.Searcher
{
    public class PdfOpener : IOpener
    {
        public string OpenFile(string fileName)
        {
            var bytes = File.ReadAllBytes(fileName);
            var text = ConvertToText(bytes);

            if (text.IsHebrew())
                text = text.ReverseText();
                return text;
        }
        

        private static string ConvertToText(byte[] bytes)
        {
            var sb = new StringBuilder();


            var reader = new PdfReader(bytes);
            var numberOfPages = reader.NumberOfPages;

            for (var currentPageIndex = 1; currentPageIndex <= numberOfPages; currentPageIndex++)
            {
                var text = PdfTextExtractor.GetTextFromPage(reader, currentPageIndex,
                    new LocationTextExtractionStrategy());
                sb.Append(text);
            }

            return sb.ToString();
        }
    }
}