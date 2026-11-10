using System.Drawing;
using System.Drawing.Imaging;
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
double grandTotal = sorted.Sum(x => x.Value);

Color[] colors = {
    Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple,
    Color.Teal, Color.Gold, Color.Brown, Color.HotPink, Color.Gray
};

Bitmap image = new Bitmap(800, 500);
Graphics g = Graphics.FromImage(image);
g.Clear(Color.White);

Font titleFont = new Font("Arial", 16, FontStyle.Bold);
Font textFont = new Font("Arial", 11);

g.DrawString("Percentage of Total Time Worked", titleFont, Brushes.Black, 20, 10);

Rectangle pieArea = new Rectangle(20, 60, 400, 400);
float startAngle = 0;
int legendY = 80;
int i = 0;

foreach (var item in sorted)
{
    float sweepAngle = (float)(item.Value / grandTotal * 360);
    double percent = item.Value / grandTotal * 100;

    Brush brush = new SolidBrush(colors[i % colors.Length]);

    g.FillPie(brush, pieArea, startAngle, sweepAngle);
    startAngle += sweepAngle;

    g.FillRectangle(brush, 450, legendY, 15, 15);
    g.DrawString(item.Key + " - " + percent.ToString("F1") + "%", textFont, Brushes.Black, 475, legendY - 2);

    legendY += 30;
    i++;
}

image.Save("pie_chart.png", ImageFormat.Png);
Console.WriteLine("Done! Open pie_chart.png");

class TimeEntry
{
    public string? EmployeeName { get; set; }
    public DateTime StarTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public DateTime? DeletedOn { get; set; }
}