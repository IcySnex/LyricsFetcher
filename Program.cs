using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

HttpClient Client = new();
string Lyrics = "";

//Console.Write("Enter genius API key (get free one at 'https://docs.genius.com/'): ");
string? ApiKey = "ucRn7Z0zUGzwSe-qh3ulzELWMFhKEfiy61xpTJN81MbBiI75K4dB3eqTrsgON0yK";//Console.ReadLine();
if (string.IsNullOrEmpty(ApiKey))
    Cancel("ApiKey is empty");

Console.Write("Enter Song Title: ");
string? SongTitle = Console.ReadLine();
if (string.IsNullOrEmpty(SongTitle))
    Cancel("SongTitle is empty"); ;
Console.Write("Enter Song Artist: ");

string? SongArtist = Console.ReadLine();
if (string.IsNullOrEmpty(SongArtist))
    Cancel("SongArtist is empty");


Console.WriteLine("Searchig for Lyrics...");

lyrics_search? SearchResult = JsonConvert.DeserializeObject<lyrics_search>(await Client.GetStringAsync($"https://api.genius.com/search?q={SongTitle} {SongArtist}&access_token={ApiKey}"));
if (SearchResult is null)
    Cancel("SearchResult is null");

if (SearchResult!.response.hits.Count == 0)
    Cancel("SearchResult has no hits");


Console.WriteLine("Found Hit.");

var Html = new HtmlDocument();
Html.LoadHtml((await Client.GetStringAsync(SearchResult.response.hits[0].result.url)).Replace("https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js", ""));

var Nodes = Html.DocumentNode.SelectNodes("//div[@data-lyrics-container]");

string GetChildrenDirectText(HtmlNodeCollection nodes)
{
    StringBuilder builder = new();
    foreach (HtmlNode child in nodes)
    {
        if (child.HasChildNodes)
        {
            builder.Append(GetChildrenDirectText(child.ChildNodes));
            continue;
        }

        string innerText = child.GetDirectInnerText();
        if (!string.IsNullOrWhiteSpace(innerText))
            builder.AppendLine(innerText);
    }

    return builder.ToString();
}

Lyrics = WebUtility.HtmlDecode(Regex.Replace(GetChildrenDirectText(Nodes), @"\[(.*?)\]", "\n$&").Trim());
Console.WriteLine("Lyrics fetched!\n");

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(Lyrics);
Console.ForegroundColor = ConsoleColor.White;

Console.WriteLine("\nDo you want to restart the application? Y/N");
if (Console.ReadKey().Key == ConsoleKey.Y)
{
    Console.Clear();
    Process.Start(AppDomain.CurrentDomain.FriendlyName);
    Environment.Exit(0);
}


void Cancel(string Reason)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Program failed: {Reason}");
    Environment.Exit(0);
}


public partial class lyrics_search
{
    public lyrics_search_response response { get; set; }
}
public partial class lyrics_search_response
{
    public List<lyrics_search_response_hits> hits { get; set; }
}
public partial class lyrics_search_response_hits
{
    public lyrics_search_response_hits_result result { get; set; }
}
public partial class lyrics_search_response_hits_result
{
    public string url { get; set; }
}