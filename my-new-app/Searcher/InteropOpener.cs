using Microsoft.Office.Interop.Word;

namespace HR.Searcher
{
    public class InteropOpener : IOpener
    {
        public string OpenFile(string fileName)
        {
            Application app = new Application();
            app.DisplayAlerts = WdAlertLevel.wdAlertsNone;
            Document doc = app.Documents.Open(fileName, ReadOnly:true);

            //Get all words
            string allWords = doc.Content.Text;
            
            doc.Close();
            app.Quit();

            return allWords;
        }
    }
}