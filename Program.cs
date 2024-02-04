using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

HttpClient client = new();
string lyrics = "";

Console.Write("Enter genius API key (get free one at 'https://docs.genius.com/'): ");
string? apiKey = Console.ReadLine();
if (string.IsNullOrEmpty(apiKey))
    Cancel("ApiKey is empty");

Console.Write("Enter Song Title: ");
string? songTitle = Console.ReadLine();
if (string.IsNullOrEmpty(songTitle))
    Cancel("SongTitle is empty"); ;
Console.Write("Enter Song Artist: ");

string? songArtist = Console.ReadLine();
if (string.IsNullOrEmpty(songArtist))
    Cancel("SongArtist is empty");


Console.WriteLine("Searchig for Lyrics...");

string url = $"https://api.genius.com/search?q={WebUtility.UrlEncode($"{songTitle} {songArtist}")}&access_token={apiKey}";
lyrics_search? searchResult = JsonConvert.DeserializeObject<lyrics_search>(await client.GetStringAsync(url));
if (searchResult is null)
    Cancel("SearchResult is null");

if (searchResult!.response.hits.Count == 0)
    Cancel("SearchResult has no hits");


Console.WriteLine("Found Hit.");

HtmlDocument html = new();
html.LoadHtml((await client.GetStringAsync(searchResult.response.hits[0].result.url)).Replace("https://ajax.googleapis.com/ajax/libs/jquery/2.1.4/jquery.min.js", ""));

HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//div[@data-lyrics-container]");
if (nodes is null)
    return;

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
        if (string.IsNullOrWhiteSpace(innerText))
            continue;

        if (innerText.Length < 2 && innerText[0] == '(' || innerText[^1] == '(')
        {
            builder.Append(innerText);
            continue;
        }

        if (innerText[0] == ')')
            builder.Remove(builder.Length - 2, 2);
        builder.AppendLine(innerText);
    }

    return builder.ToString();
}

lyrics = WebUtility.HtmlDecode(Regex.Replace(GetChildrenDirectText(nodes), @"\[(.*?)\]", "\n$&").Replace("\r\n (", " (").Trim());
Console.WriteLine("Lyrics fetched!\n");

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(lyrics);
Console.ForegroundColor = ConsoleColor.White;

Console.WriteLine("\nDo you want to restart the application? Y/N");
if (Console.ReadKey().Key == ConsoleKey.Y)
{
    Console.Clear();
    Process.Start(AppDomain.CurrentDomain.FriendlyName);
    Environment.Exit(0);
}


void Cancel(string reason)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Program failed: {reason}");
    Environment.Exit(0);
}


#nullable disable
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