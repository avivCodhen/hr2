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
            var text = ConvertToTextWithIText(fileName);

            if (text.IsHebrew())
                text = text.ReverseText();
                return text;
        }
        

        private static string ConvertToTextWithTextSharper(byte[] bytes)
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
        
        
        private static string ConvertToTextWithIText(string fileName)
        {
            var sb = new StringBuilder();

            
            PdfReader pdfReader = new PdfReader(fileName);
            for (int page = 1; page  < pdfReader.NumberOfPages; page++) {
                LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(
                    Encoding.Default, Encoding.GetEncoding("windows-1255"), Encoding.Default.GetBytes(currentText)));
                sb.Append(currentText);
            }
            return sb.ToString();
            
        }

        
    }
}