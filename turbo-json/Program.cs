string server;
try
{
    if (args[0] == "local")
    {
        server = "http://localhost:57812/api/TurboStats";
        Console.WriteLine("---Sending data to a local service, if one is not running, this script will not execute.");
    }
    else if (args[0] == "web")
    {
        server = "https://mobileapi.thegrindsession.com/api/TurboStats";
        Console.WriteLine("---Sending data to https://mobileapi.thegrindsession.com");
    }
    else
    {
        throw new Exception();
    }
}
catch (Exception ex)
{
    Console.WriteLine("ERROR: You must specify either 'local' or 'web' at the command line.");
    Console.WriteLine("       Use local if you are running the api on localhost.");
    Console.WriteLine("       Use web if you are using the official api at 'mobileapi.thegrindsession.com/api'.");
    return;
}

#region Begin Script
string currentDirectory = Directory.GetCurrentDirectory();
string[] files = Directory.GetFiles(currentDirectory, "*.json");
var client = new HttpClient();
Console.WriteLine("---Submitting all json files inside: " + currentDirectory);

try
{
    foreach (var file in files)
    {
        List<string> lines = File.ReadLines(file).ToList();
        if (lines.Count == 1)
        {
            await client.PostAsync(server, new StringContent(lines[0], System.Text.Encoding.UTF8, "application/json"));
        }
        else
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = lines[i].Replace("3fgm", "fgm3");
                lines[i] = lines[i].Replace("3fga", "fga3");
                lines[i] = lines[i].Replace("+-", "plusOrMinus");
                if (lines[i].Contains("channel") && !lines[i + 1].Contains("source"))
                {
                    lines[i] = "\"channel\": \"\",";
                    lines[i + 1] = "";
                }
                if (lines[i].StartsWith("{\"event\":\""))
                {
                    var playString = lines[i].Replace("{\"event\":\"", "");
                    playString = playString.Replace("\"},", "");
                    if (playString.Contains("\""))
                    {
                        var newPlayString = playString.Replace("\"", "\\\"");
                        lines[i].Replace(playString, newPlayString);
                    }
                }
            }
            lines.Remove("This database is the intellectual property of TurboStats Software and is protected by International Copyright Laws. (c) 2020 All Rights Reserved.  Use of this database must comply with the terms of your TurboStats License Agreement.");
            string result = string.Join("\n", lines);
            await client.PostAsync(server, new StringContent(result, System.Text.Encoding.UTF8, "application/json"));
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("---Something went wrong. Make sure a service is running locally if you specified local at the command line.");
}
#endregion

Console.WriteLine("---Finished.");