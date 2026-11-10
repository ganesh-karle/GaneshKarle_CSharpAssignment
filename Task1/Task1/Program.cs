using System.Text;
using System.Text.Json;

string url = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";

HttpClient client = new HttpClient();
string json = await client.GetStringAsync(url);

JsonSerializerOptions options = new JsonSerializerOptions();
options.PropertyNameCaseInsensitive = true;
List<TimeEntry> entries = JsonSerializer.Deserialize<List<TimeEntry>>(json, options)!;

Dictionary<string, double> totals = new Dictionary<string, double>();

foreach (TimeEntry entry in entries)
{
    if (entry.EmployeeName == null || entry.DeletedOn != null)
        continue;

    double hours = (entry.EndTimeUtc - entry.StarTimeUtc).TotalHours;

    if (hours < 0)
        continue;

    if (totals.ContainsKey(entry.EmployeeName))
        totals[entry.EmployeeName] += hours;
    else
        totals[entry.EmployeeName] = hours;
}

var sorted = totals.OrderByDescending(x => x.Value).ToList();

StringBuilder html = new StringBuilder();
html.AppendLine("<html><body>");
html.AppendLine("<h2>Employee Total Time Worked</h2>");
html.AppendLine("<table border='1' cellpadding='8' cellspacing='0'>");
html.AppendLine("<tr><th>Name</th><th>Total Time Worked (hours)</th></tr>");

foreach (var item in sorted)
{
    if (item.Value < 100)
        html.AppendLine("<tr style='background-color:lightcoral'>");
    else
        html.AppendLine("<tr>");

    html.AppendLine("<td>" + item.Key + "</td>");
    html.AppendLine("<td>" + item.Value.ToString("F2") + "</td>");
    html.AppendLine("</tr>");
}

html.AppendLine("</table>");
html.AppendLine("</body></html>");

File.WriteAllText("employees.html", html.ToString());
Console.WriteLine("Done! Open employees.html in your browser.");

class TimeEntry
{
    public string? EmployeeName { get; set; }
    public DateTime StarTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public DateTime? DeletedOn { get; set; }
}