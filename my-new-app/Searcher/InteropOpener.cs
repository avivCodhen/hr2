using Microsoft.Office.Interop.Word;

namespace HR.Searcher
{
    public class InteropOpener : IOpener
    {
        public string OpenFile(string fileName)
        {
            Application app = new Application();
            Document doc = app.Documents.Open(fileName);

            //Get all words
            string allWords = doc.Content.Text;
            
            doc.Close();
            app.Quit();

            return allWords;
        }
    }
}