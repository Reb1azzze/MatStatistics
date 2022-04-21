using System.Globalization;
using System.Security.Cryptography;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;

StreamWriter sw = new StreamWriter("ouweft.txt");
List<double> input = new List<double>();
double xmax = 0, xmin = double.MaxValue;
using (TextFieldParser tfp = new TextFieldParser("accelerometer.csv"))
{
    tfp.TextFieldType = FieldType.Delimited;
    tfp.SetDelimiters(",");
    while (!tfp.EndOfData)
    {
        string[] fields = tfp.ReadFields();
        var temp = double.Parse(fields[2], new CultureInfo("en-us")) * 100;
        input.Add(temp);
        xmax = Math.Max(xmax, temp);
        xmin = Math.Min(xmin, temp);
    }
}

xmax = Math.Round(xmax);
xmin = Math.Round(xmin);
int n = input.Count;
double k = Math.Round((xmax - xmin) / Math.Round(3.322 * Math.Log10(n), 3));
var parts = new List<List<double>>();
var xStart = new List<double>();
var xEnd = new List<double>();
var xAverage = new List<double>();

int partsCount = (int)Math.Ceiling((xmax - xmin) / k);
for (int i = 0; i <partsCount ; i++)
{
    var start = xmin + i * k;
    xStart.Add(start);
    xEnd.Add(start+k);
    xAverage.Add(start + k / 2);
    parts.Add(SearchValues(start, xmin + (i + 1) * k));
}

var wi = new List<double>();
foreach (var i in parts)
    wi.Add((double)i.Count/n);

var nakop = new List<double>();
nakop.Add(parts[0].Count);
for (int i = 1; i < partsCount; i++)
    nakop.Add(nakop[i - 1] + parts[i].Count);

double xWithWave = 0;
for (int i = 0; i < partsCount; i++)
    xWithWave += wi[i] * xAverage[i];
xWithWave = Math.Round(xWithWave, 3);

double sigmaSquare = 0;
for (int i = 0; i < partsCount; i++)
    sigmaSquare += Math.Pow(xAverage[i] - xWithWave, 2) * wi[i];
sigmaSquare = Math.Round(sigmaSquare, 3);
var sigma = Math.Round(Math.Sqrt(sigmaSquare),3);

var modalInterval = Math.Round(xStart[2] + (parts[2].Count - parts[1].Count)
    / (2*parts[2].Count - parts[1].Count - parts[3].Count)*k,3);

var medianInterval =
    Math.Round(xStart[partsCount / 2] + 
               (input.Count / 2 - nakop[partsCount / 2 - 1]) / parts[partsCount / 2].Count * k, 3);

double m3 = 0;
for (int i = 0; i < partsCount; i++)
    m3 += Math.Pow(xAverage[i] - xWithWave, 3) * wi[i];
double A3 = Math.Round(m3/(sigmaSquare * sigma),3);
string A3text = A3 > 0.25 ? (A3 > 0.5) ? "Существенная" : "Умеренная" : "Незначительная";

double m4 = 0;
for (int i = 0; i < partsCount; i++)
    m4 += Math.Pow(xAverage[i] - xWithWave, 4) * wi[i];
double Ek = Math.Round(m4 / (sigmaSquare * sigmaSquare) - 3,3);
string Ektext = Ek > 0 ? "Вытянутая" : "Низкая и пологая";

double V = Math.Round(sigma / xWithWave, 3)* 100;
string Vtext = V < 30 ? "Однородная" : "Неоднородная";

sw.WriteLine("     n[i] w[i] xAvg zi nakop");
for (int i = 0; i < 9; i++)
    sw.WriteLine($"{xStart[i]}-{xEnd[i]} {parts[i].Count} {wi[i]} " +
    $"{xAverage[i]} {Math.Round(xAverage[i]-xWithWave,3)} {nakop[i]}");
sw.WriteLine($"square: {sigmaSquare} , sigma: {sigma} , modal: {modalInterval} , median: {medianInterval}");
sw.WriteLine($"A3 = {A3} - {A3text} , Ek = {Ek} - {Ektext}");
sw.WriteLine($"XwithWave = {xWithWave} , V = {V}% - {Vtext}");
sw.Close();
List<double> SearchValues(double a, double b)
{
    var result = new List<double>();
    foreach (var i in input)
        if (a <= i && i < b)
            result.Add(i);
    return result;
}