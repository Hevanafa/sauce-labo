using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

// Todo: save this project on a Git

// Example link:
// https://rule34.xxx/index.php?page=post&s=view&id=4960670
namespace SauceLabo.Modules
{
    partial class DownloadManager
    {
        // Todo: move this to the main class
        string[] singlelineMatches(string HtmlString, string regex) =>
            Regex.Matches(
                    HtmlString,
                    regex,
                    RegexOptions.Singleline
                ).Cast<Match>()
                .Select(x => x.Value)
                .ToArray();

        // Todo: refactor this
        async void fetchRule34() {
            preventNavigation = true;

            try {
                appendOutput("Fetching data...");
                appendOutput(pageUri);

                var HtmlString = await downloadHtmlString(uri);

                // ID format:
                // artists(sparated with comma)_{post_id}
                // EN Title:
                // artists - characters (website - post_id)
                // Skip JP Title
                // Tags: Characters & General tags combined

                // Example URL:
                // https://rule34.xxx/index.php?page=post&s=view&id=4743663

                var query = HttpUtility.ParseQueryString(pageUri);
                var postId = int.Parse(query["id"]);

                string[] cleanTags(string[] rawTags) =>
                    rawTags
                    .Select(x => Regex.Match(
                        x,
                        @"(?<=<a(.*?)>)(.*?)(?=<\/a>)",
                        RegexOptions.Singleline
                    ).Value)
                    .ToArray();

                // Pending test: obtain the artists
                // (?<=<li class=""tag-type-artist tag"">)(.*?)(?=<\/li>)
                // (?<=<a(.*?)>)(.*?)(?=<\/a>)
                var artistsRaw = singlelineMatches(
                        HtmlString,
                        @"(?<=<li class=""tag-type-artist tag"">)(.*?)(?=<\/li>)"
                    );
                    
                //    Regex.Matches(
                //    HtmlString,
                //    @"(?<=<li class=""tag-type-artist tag"">)(.*?)(?=<\/li>)",
                //    RegexOptions.Singleline
                //).Cast<Match>()
                //.Select(x => x.Value)
                //.ToArray();

                var artists = cleanTags(artistsRaw);

                appendOutput("Found artists: " + string.Join(",", artists));


                // tag-type-character tag
                var charactersRaw = singlelineMatches(
                    HtmlString,
                    @"(?<=<li class=""tag-type-character tag"">)(.*?)(?=<\/li>)"
                );

                //Regex.Matches(
                //    HtmlString,
                //    @"(?<=<li class=""tag-type-character tag"">)(.*?)(?=<\/li>)",
                //    RegexOptions.Singleline
                //).Cast<Match>()
                //.Select(x => x.Value)
                //.ToArray();

                var characters = cleanTags(charactersRaw);
                appendOutput("Found characters: " + string.Join(",", characters));



                // Pending test: obtain the tags
                // (?<=<li class=""tag-type-general tag"">)(.*?)(?=<\/li>)
                var generalTagsRaw = singlelineMatches(
                    HtmlString,
                    @"(?<=<li class=""tag-type-general tag"">)(.*?)(?=<\/li>)"
                    );

                    
                //Regex.Matches(
                //    HtmlString,
                //    @"(?<=<li class=""tag-type-general tag"">)(.*?)(?=<\/li>)",
                //    RegexOptions.Singleline
                //).Cast<Match>()
                //.Select(x => x.Value)
                //.ToArray();

                var generalTags = cleanTags(generalTagsRaw);

                appendOutput("Found tags: " + string.Join(",", generalTags));


                var enTitle = string.Join(", ", artists) + " - " +
                    string.Join(", ", characters) + " " +
                    $"(Rule34.xxx - {postId})";

                // Done: save to library
                // Sample tag:
                // <img alt="1girls :d aged_up armpits belly belly_button bikini bikini_top biting_lip blonde_hair breasts cleavage female female_only genshin_impact klee_(genshin_impact) looking_at_viewer medium_breasts micro_shorts pointy_ears red_eyes shorts skindentation skyhood smile solo thong underboob" src="https://wimg.rule34.xxx//images/4174/899c3f36c58df50b3bd781faa51279ae.jpeg?4743663" id="image" onclick="Note.toggle();" width="1030" height="1413">

                var coverImgTag = Regex.Match(
                    HtmlString,
                    "<img (.*?) id=\"image\"(.*?)>",
                    RegexOptions.Multiline
                ).Value;

                var coverImgUri = Regex.Match(
                    coverImgTag,
                    @"(?<=src="")(.*?)(?="")"
                ).Value;

                var preferableFilename = $"rule34xxx_{postId}";

                //coverImg = new Uri(coverImg).GetLeftPart(UriPartial.Path);

                saveSauce(
                    pageUri,
                    enTitle,
                    "",
                    preferableFilename,
                    coverImgUri,

                    string.Join(",", characters.Concat(generalTags))
                );

                // Pending test: obtain the preview image
                saveCoverPage(
                    preferableFilename,
                    coverImgUri
                );
            }
            catch (Exception ex)
            {
                appendOutput("Error: " + ex.Message);
            }
            finally {
                preventNavigation = false;
            }
        }
    }
}
