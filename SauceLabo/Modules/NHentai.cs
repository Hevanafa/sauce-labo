using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SauceLabo.Modules
{
    partial class DownloadManager
    {
        // ID format:
        // see entryId
        // nhentai_{nuclear_code}

        Tuple<string, string> getNHentaiTitles(string HtmlString) {
            // <span(.*?)>(.*?)</span>
            string[] getTitleAry(string rawTitleTag) =>
                Regex.Matches(
                    rawTitleTag,
                    @"(?<=<span class=""(.*?)"">)(.*?)(?=<\/span>)",
                    RegexOptions.Singleline
                ).Cast<Match>()
                .Select(x => x.Value)
                .ToArray();

            // Get the English title
            // <h1 class=""title"">(.*?)</h1>
            var enTitleRaw = Regex.Match(
                HtmlString,
                @"(?<=<h1 class=""title"">)(.*?)(?=<\/h1>)",
                RegexOptions.Singleline
            ).Value;

            var enTitleAry = getTitleAry(enTitleRaw);
            var enTitle = string.Join("", enTitleAry);

            appendOutput("EN title: " + enTitle);


            // Get the Japanese title
            // <h2 class=""title"">(.*?)</h1>
            var jpTitleRaw = Regex.Match(
                HtmlString,
                @"(?<=<h2 class=""title"">)(.*?)(?=<\/h2>)",
                RegexOptions.Singleline
            ).Value;

            var jpTitleAry = getTitleAry(jpTitleRaw);
            var jpTitle = string.Join("", jpTitleAry);

            appendOutput("JP title: " + jpTitle);

            return Tuple.Create(
                enTitle, jpTitle
            );
        }

        string getNHentaiCoverPage(string HtmlString) {
            // Obtain the cover page
            // (<?=<div id=""cover"">)(.*?)(?=</div>)
            var coverRaw = Regex.Match(
                HtmlString,
                @"(?<=<div id=""cover"">)(.*?)(?=<\/div>)",
                RegexOptions.Singleline
            ).Value;

            var coverURL = Regex.Match(
                coverRaw,
                @"(?<=data-src="")(.*?)(?="")",
                RegexOptions.Singleline
            ).Value;

            return coverURL;
        }

        string[] getNHentaiTags(string HtmlString) {
            // Obtain the tags
            // (?<=<section id=""tags"">)(.*?)(?=</section>)
            var tagSection = Regex.Match(
                HtmlString,
                @"(?<=<section id=""tags"">)(.*?)(?=<\/section>)",
                RegexOptions.Singleline
            ).Value;

            var tagsField = Regex.Matches(
                tagSection,
                @"(?<=<div(.*?)tag-container(.*?)>)(.*?)(?=<\/div>)",
                RegexOptions.Singleline
            ).Cast<Match>()
            .Select(x => x.Value)
            .ToArray();

            var tagsRaw = tagsField
            .Where(tag => tag.Contains("Tags:"))
            .First();

            // <span class=""name"">(.*?)</span>
            var tags = Regex.Matches(
                tagsRaw,
                @"(?<=<span class=""name"">)(.*?)(?=<\/span>)",
                RegexOptions.Singleline
            ).Cast<Match>()
            .Select(x => x.Value)
            .ToArray();

            return tags;
        }

        async void fetchNHentai()
        {
            preventNavigation = true;

            try
            {
                appendOutput("Fetching data...");
                appendOutput(pageUri);

                var HtmlString = await downloadHtmlString(uri);

                // Done: get the EN title
                // Done: get the JP title
                // Done: get the cover page
                // Done: get the tags

                var titles = getNHentaiTitles(HtmlString);

                string enTitle = titles.Item1,
                    jpTitle = titles.Item2;

                appendOutput("EN title: " + enTitle);
                appendOutput("JP title: " + jpTitle);


                var coverURL = getNHentaiCoverPage(HtmlString);
                appendOutput("Cover page: " + coverURL);


                var tags = getNHentaiTags(HtmlString);
                appendOutput("Tags: " + string.Join(", ", tags));


                // https://nhentai.net/g/368280/
                var nuclearCode = Regex.Match(
                    pageUri,
                    @"(?<=\/g\/)\d+"
                );
                var entryId = $"nhentai_{nuclearCode}";

                saveSauce(
                    pageUri,
                    enTitle,
                    jpTitle,
                    entryId,
                    coverURL,
                    string.Join(",", tags)
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
