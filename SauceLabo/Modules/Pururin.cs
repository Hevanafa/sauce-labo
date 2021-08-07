using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SauceLabo.Modules
{
    partial class DownloadManager
    {
        Tuple<string, string> getPururinTitles(string HtmlString)
        {
            var titleMix = Regex.Match(
                    HtmlString,
                    @"(?<=<h1>)(.*?)(?=<\/h1>)",
                    RegexOptions.Singleline
                ).Value;

            var splitTitle = titleMix
                .Split('/')
                .Select(x => x.Trim())
                .ToArray();

            return Tuple.Create(splitTitle[0], splitTitle[1]);
        }

        string getPururinCoverPage(string HtmlString)
        {
            // Get the cover image
            var coverWrapper = Regex.Match(
                HtmlString,
                @"(?<=<div class=""cover-wrapper"">)(.*?)(?=<\/div>)",
                RegexOptions.Singleline
            ).Value;

            // Format:
            // URL without the protocol:
            //  //cdn.~~~~
            // var protocol = uri.GetLeftPart(UriPartial.Scheme).Replace("//", "");
            var protocol = "https:";
            var coverURL = protocol +
            Regex.Match(
                coverWrapper,
                @"(?<=src="")(.*?)(?="")"
            ).Value;

            // <div class=""cover-wrapper"">(.*?)</div>
            // src=""(.*?)""

            return coverURL;
        }

        string[] getPururinContents(string HtmlString)
        {
            // Get the Contents tag
            // <table class=""table-gallery-info"">
            // </table>
            var tableGalleryInfoTag = Regex.Match(
                HtmlString,
                @"(?<=<table(.*?)table-gallery-info"">)(.*?)(?=<\/table>)",
                RegexOptions.Singleline
            ).Value;

            // <tr>(.*?)</tr>
            var tableRows = Regex.Matches(
                tableGalleryInfoTag,
                @"(?<=<tr>)(.*?)(?=<\/tr>)",
                RegexOptions.Singleline
            ).Cast<Match>()
            .Select(x => x.Value)
            .ToArray();

            var contentsTag = tableRows
                .Where(x => x.Contains("Contents"))
                .ToArray()[0];

            var contentsRaw = Regex.Matches(
                contentsTag,
                @"<td>(.*?)<\/td>",
                RegexOptions.Singleline
            ).Cast<Match>()
            .ToArray()[1]
            .Value;

            // <a(.*?)>(.*?)</a>
            var contents = Regex.Matches(
                contentsRaw,
                @"(?<=<a(.*?)>)(.*?)(?=<\/a>)"
            ).Cast<Match>()
            .Select(x => x.Value)
            .ToArray();

            return contents;
        }

        async void fetchPururin()
        {
            preventNavigation = true;

            try
            {
                appendOutput("Fetching data...");
                appendOutput(pageUri);

                var HtmlString = await downloadHtmlString(uri);

                // Obtain the title
                var titles = getPururinTitles(HtmlString);
                string enTitle = titles.Item1,
                    jpTitle = titles.Item2;

                appendOutput("EN title: " + enTitle);
                appendOutput("JP title: " + jpTitle);


                // Obtain the cover page
                var coverURL = getPururinCoverPage(HtmlString);
                appendOutput("Cover image: " + coverURL);


                // Obtain the tags (Contents in this case)
                var contents = getPururinContents(HtmlString);

                appendOutput("Contents: " + string.Join(", ", contents));


                // https://pururin.to/gallery/46892/randoseru-shotta-kemono-ga-shukudai-o-oshiete-morau-hon
                var nuclearCode = Regex.Match(
                    pageUri,
                    @"(?<=\/gallery\/)\d+"
                );
                var entryId = $"pururin_{nuclearCode}";

                saveSauce(
                    pageUri,
                    enTitle,
                    jpTitle,
                    entryId,
                    coverURL,
                    string.Join(",", contents)
                );

                saveCoverPage(entryId, coverURL);
            }
            catch (Exception ex)
            {
                appendOutput("Error: " + ex.Message);
            }
            finally
            {
                preventNavigation = false;
            }
        }
    }
}
