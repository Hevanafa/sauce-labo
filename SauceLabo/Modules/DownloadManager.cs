using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SauceLabo.Modules
{
    /// <summary>
    /// Main download manager class
    /// </summary>
    partial class DownloadManager
    {
        const string coverDirName = "Covers";
        public const string libraryFilename = "library.txt";


        /// <summary>
        /// The TextBox where the output will be redirected to.
        /// </summary>
        public TextBox txbOutput;

        void appendOutput(string text)
        {
            txbOutput.Text += text + "\n";
        }

        private string pageUri;
        private Uri uri { get {
                return new Uri(pageUri);
            }
        }
        public void setUri(string uriStr) {
            pageUri = uriStr;
        }


        // Done: refactor this class (last time: 07-08-2021)
        // Todo: create the "How to Use" page
        // Done: create the library browser
        // Todo: make Dream Features list
        // Todo: make the version info class
        // Done: Download from NHentai
        //appendOutput("To be added");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalLink"></param>
        /// <param name="enTitle"></param>
        /// <param name="jpTitle"></param>
        /// <param name="entryId"></param>
        /// <param name="coverPageSource">Any Uri string, the query options are trimmed in this method.</param>
        /// <param name="tags"></param>
        async void saveSauce(
            string originalLink,
            string enTitle,
            string jpTitle,
            string entryId,
            string coverPageSource,
            string tags
        )
        {
            try
            {
                var coverPageUri = new Uri(coverPageSource)
                    .GetLeftPart(UriPartial.Path);
                var ext = Path.GetExtension(coverPageUri);
                var filename = Path.Combine(
                        coverDirName,
                        entryId + ext
                    );

                bool skipWriting = false;

                if (!File.Exists(libraryFilename))
                    File.Create(libraryFilename);
                else
                {
                    // Pending test: check for duplicates
                    var sr = new StreamReader(libraryFilename);

                    try
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = await sr.ReadLineAsync();
                            if (line.Split('\t').First() == entryId)
                            {
                                appendOutput("This sauce already exists.");
                                skipWriting = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        appendOutput("Error: " + ex.Message);
                    }
                    finally
                    {
                        sr.Close();
                        sr.Dispose();
                    }
                }

                if (skipWriting)
                    return;

                var sw = new StreamWriter(libraryFilename, true);

                //var coverFilename = Path.Combine(
                //    "Covers",
                //    Path.GetFileName(coverPageSource)
                //);

                await sw.WriteLineAsync(
                    string.Join(
                        "\t",
                        entryId,
                        originalLink,
                        enTitle,
                        jpTitle,
                        filename,
                        tags
                    )
                );

                sw.Close();
                sw.Dispose();
            }
            catch (Exception ex)
            {
                appendOutput("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Download the cover page/image from image boards.
        /// </summary>
        /// <param name="preferableFilename">Filename without extension. Example: nhentai_177013</param>
        /// <param name="coverPageSource">File URL (query options are automatically trimmed)</param>
        async void saveCoverPage(
            string preferableFilename,
            string coverPageSource
        )
        {
            try
            {
                var coverPageUri = new Uri(coverPageSource).GetLeftPart(UriPartial.Path);
                var ext = Path.GetExtension(coverPageUri);
                var filename = Path.Combine(
                        coverDirName,
                        preferableFilename + ext
                    );

                if (!Directory.Exists(coverDirName))
                    Directory.CreateDirectory(coverDirName);

                // Skip download
                if (File.Exists(filename))
                    throw new Exception("Cover page already exists.");

                appendOutput("Downloading cover page...");
                appendOutput($"Saving as \"{filename}\"");

                using (var wc = new WebClient())
                    await wc.DownloadFileTaskAsync(new Uri(coverPageSource), "temp");

                File.Move("temp", filename);

                appendOutput("Download is completed.");
            }
            catch (Exception ex)
            {
                appendOutput("Error: " + ex.Message);
            }
        }

        const string ChromeUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36";

        bool preventNavigation = false;

        async Task<string> downloadHtmlString(Uri uri)
        {
            var req = (HttpWebRequest)WebRequest.Create(uri);
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.UserAgent = ChromeUserAgent;

            var res = await req.GetResponseAsync();
            var sr = new StreamReader(res.GetResponseStream());

            var HtmlString = await sr.ReadToEndAsync();

            appendOutput($"Page size: {HtmlString.Length} chars");

            req = null;
            res.Dispose();
            sr.Dispose();

            return HtmlString;
        }

        public void doFetch(string pageUri)
        {
            if (preventNavigation)
                return;

            this.pageUri = pageUri;

            if (pageUri.Contains("pururin.to"))
            {
                appendOutput("Source: Pururin");
                fetchPururin();
            }

            if (pageUri.Contains("nhentai.net"))
            {
                appendOutput("Source: NHentai");
                fetchNHentai();
            }

            if (pageUri.Contains("rule34.xxx")) {
                appendOutput("Source: Rule34.xxx");
                fetchRule34();
            }
        }
    }
}
