using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HR.Models;
using Microsoft.Extensions.Hosting;

namespace HR.Searcher
{
    public class SearchService
    {
        private readonly IHostEnvironment _environment;

        public SearchService(IHostEnvironment environment)
        {
            _environment = environment;
        }
        public IOpener GetOpenerByExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (_environment.IsDevelopment() && extension.Contains("docx", StringComparison.InvariantCultureIgnoreCase)) return new WordOpener();
            if (extension.Contains("doc", StringComparison.InvariantCultureIgnoreCase)) return new InteropOpener();
            if (extension.Contains("odt", StringComparison.InvariantCultureIgnoreCase)) return new WordOpener();
            if (extension.Contains("pdf", StringComparison.InvariantCultureIgnoreCase)) return new PdfOpener();
            throw new Exception($"קובץ לא קריא: {fileName}");
        }

        public string GetText(string fileName)
        {
            var opener = GetOpenerByExtension(fileName);
            var text = opener.OpenFile(fileName);
            return text;
        }

        public FileModel GetResults(IEnumerable<string> words, string fileName, IEnumerable<string> searchWords, int precision = 0)
        {
            var model = new FileModel() {Name = Path.GetFileName(fileName), Path = fileName};
            var score = 0;
            var matchedWords = new HashSet<string>();
            foreach (var searchWord in searchWords)
            {
                foreach (var word in words)
                {
                    var distance = searchWord.Distance(word);
                    if (distance == precision)
                    {
                        score++;
                        matchedWords.Add(searchWord);
                    }
                    else if (searchWord.IsHebrew() && searchWord.ReverseSearchText().Distance(word) == precision)
                    {
                        score++;
                        matchedWords.Add(searchWord);
                    }
                }
            }

            model.Score = score;
            model.Words = matchedWords;
            return score > 0 ? model : null;
        }
    }
}