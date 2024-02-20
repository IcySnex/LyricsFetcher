using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;

HttpClient client = new();
StringBuilder builder = new();

Console.Write("Enter genius API key (get free one at 'https://docs.genius.com/'): ");
string? apiKey = Console.ReadLine();
if (string.IsNullOrEmpty(apiKey))
    ThrowError("ApiKey is empty");

Console.Write("Enter Song Title: ");
string? songTitle = Console.ReadLine();
if (string.IsNullOrEmpty(songTitle))
    ThrowError("SongTitle is empty"); ;
Console.Write("Enter Song Artist: ");

string? songArtist = Console.ReadLine();
if (string.IsNullOrEmpty(songArtist))
    ThrowError("SongArtist is empty");


Console.WriteLine("Searching for Lyrics...");

string url = $"https://api.genius.com/search?q={WebUtility.UrlEncode($"{songTitle} {songArtist}")}&access_token={apiKey}";
string urlResponse = await client.GetStringAsync(url);
LyricsSearch? searchResult = JsonConvert.DeserializeObject<LyricsSearch>(urlResponse);
if (searchResult is null)
    ThrowError("SearchResult is null");

if (searchResult!.Response.Hits.Count == 0)
    ThrowError("SearchResult has no hits");
if (searchResult!.Response.Hits[0].Type != "song")
    ThrowError("SearchResult hit is not a song");
if (searchResult!.Response.Hits[0].Type != "song")
    ThrowError("SearchResult hit is not a song");


Console.WriteLine("Found Hit.");

string website = (await client.GetStringAsync(searchResult.Response.Hits[0].Result.Url)).Replace("https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js", "");
HtmlDocument html = new();
html.LoadHtml(website);

HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//div[@data-lyrics-container]");
if (nodes is null)
    return;

void ExtractText(
    HtmlNode node,
    StringBuilder builder)
{
    foreach (HtmlNode childNode in node.ChildNodes)
        switch (childNode.NodeType)
        {
            case HtmlNodeType.Text:
                builder.Append(childNode.InnerText);
                break;
            case HtmlNodeType.Element:
                if (childNode.Name == "br")
                    builder.AppendLine();
                else
                    ExtractText(childNode, builder);
                break;
        }
}

foreach(HtmlNode node in nodes)
{
    ExtractText(node, builder);
    builder.AppendLine();
}

Console.WriteLine("Lyrics fetched!\n");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(WebUtility.HtmlDecode(builder.ToString()));
Console.ForegroundColor = ConsoleColor.White;

Console.WriteLine("\nDo you want to restart the application? Y/N");
if (Console.ReadKey().Key == ConsoleKey.Y)
{
    Console.Clear();
    Process.Start(AppDomain.CurrentDomain.FriendlyName);
    Environment.Exit(0);
}


void ThrowError(string reason)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Program failed: {reason}");
    Environment.Exit(0);
}


#nullable disable
public partial class LyricsSearch
{
    public LyricsSearchResponse Response { get; set; }
}
public partial class LyricsSearchResponse
{
    public List<LyricsSearchResponseHits> Hits { get; set; }
}
public partial class LyricsSearchResponseHits
{
    public string Type { get; set; }

    public LyricsSearchResponseHitsResult Result { get; set; }
}
public partial class LyricsSearchResponseHitsResult
{
    public string Title { get; set; }

    public string Artist_Names { get; set; }

    public string Url { get; set; }
}