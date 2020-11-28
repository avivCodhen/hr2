using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (_environment.IsDevelopment() && extension.Contains("doc", StringComparison.CurrentCultureIgnoreCase))
                return new WordOpener();
            if (extension.Distance(".docx") == 0)
                return new WordOpener();
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

        public FileModel GetResults(OpenedFile file,
            IEnumerable<string> searchWords,
            int precision = 0)
        {
            var filePath = file.FilePath;
            var words = file.Words;
            var text = file.Text;
            var model = new FileModel() {Name = Path.GetFileName(filePath), Path = filePath};

            var matchedWords = words
                .Where(x => searchWords.Any(s =>
                    s.Distance(x) == precision || s.IsHebrew() && (s.Reverse().Distance(x) == precision) ||
                    s.HebrewSexDistance(x, precision)))
                .Distinct();

            var matchedPhrases = searchWords.Phrases()
                .Where(x => text.Contains(x, StringComparison.OrdinalIgnoreCase) || (x.IsHebrew() &&
                    text.Contains(x.Reverse(), StringComparison.OrdinalIgnoreCase)));

            var wordsFromFileName = searchWords.Where(x => model.Name.Contains(x));
            
            var finalWords = matchedWords.Union(matchedPhrases).Union(wordsFromFileName);

            var w = finalWords.Where(x => !finalWords.Where(s => s != x).Contains(x));
            model.CreationTime = File.GetCreationTime(filePath);
            model.Words = w;
            model.Score =
                matchedWords.Count() +
                (matchedPhrases.Count() * 10); // just to make sure words with matchedPhrases will be on top
            return !model.Words.Any() ? null : model;
        }
    }
}