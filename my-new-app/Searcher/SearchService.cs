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
            var text = file.Text;
            var model = new FileModel() {Name = Path.GetFileName(filePath), Path = filePath};

            var containedWords = searchWords.Where(
                s => text.Contains(s, StringComparison.OrdinalIgnoreCase)
                     || (s.IsHebrew() && text.Contains(s.Reverse())
                         || (s.IsHebrew() && s.HebrewSexContained(text))
                         || (s.IsHebrew() && s.HebrewConnectionContained(text)))
            );

            var wordsFromFileName = searchWords.Where(x => model.Name.Contains(x));

            model.CreationTime = File.GetCreationTime(filePath);
            model.Words = containedWords.Union(wordsFromFileName).Distinct();
            model.Score =
                model.Words.Count();
            return !model.Words.Any() ? null : model;
        }
    }
}