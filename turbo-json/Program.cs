string server;
try
{
    if (Int32.TryParse(args[0], out int year))
    {
        if (args[1] == "local")
        {
            server = $"http://localhost:57812/api/TurboStats/{year}";
            Console.WriteLine("---Sending data to a local service, if one is not running, this script will not execute.");
        }
        else if (args[1] == "production")
        {
            server = $"https://api.thegrindsession.com/api/TurboStats/{year}";
            Console.WriteLine("---Sending data to https://api.thegrindsession.com");
        }
        else
        {
            throw new Exception("ERROR: You must specify either 'local' or 'production' for the environment.");
        }
    }
    else
    {
        throw new Exception("ERROR: You must specify the year and the environment. Ex. turbo-json 2021 local");
    }
}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
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
catch
{
    Console.WriteLine("---Something went wrong. Make sure a service is running locally if you specified local at the command line.");
}
#endregion

Console.WriteLine("---Finished.");