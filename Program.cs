using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace WebScraper
{
    class Program
    {
        public string tempFile = "../../temp.txt";
        static void Main(string[] args)
        {
            Program program = new Program();
            string urlPlayers = "https://www.futwiz.com/en/fifa21/career-mode/players?minrating=1&maxrating=99&teams%5B%5D=246&leagues[]=10&order=rating&s=desc";
            string destinationPlayers = "../../players.txt";
            string playerIdentifier = "<a href=" + '\u0022' + "/en/fifa21/career-mode/player/";

            program.ReadURLInTemp("https://www.futwiz.com/en/fifa21/career-mode/players?minrating=1&maxrating=99&teams%5B%5D=21&leagues[]=19&order=rating&s=desc");
            program.FilterOnString("<a href=\u0022/en/fifa21/career-mode/player/", "../../temp.txt");
            // get all budgets
            string[] leagues = File.ReadAllLines("../../leagues.txt");
            //for (int i = 0; i < leagues.Length; i++)
            //{
            ////    /*get all budgets */
            //    string[] values = program.GetAllOccurences("https://www.futwiz.com" + leagues[i], "<p>Budget <strong>", '>', 2);
            //    //    /* access all budgets*/
            //    for (int j = 0; j < values.Length; j++) { if (values[j] != null) { Console.WriteLine(i + ". " + values[j].Split('<')[0].Remove(0, 1)); } }/*.Split('<')[0].Remove(0, 1)*/ 
            //}
            //program.ParseAllLeagues();
            //program.GetPages(urlPlayers, destinationPlayers, playerIdentifier, 2);
            //league name Console.WriteLine(program.GetString("https://www.futwiz.com/en/fifa21/career-mode/teams?l=330", "<a href=" + '\u0022' + "/en/fifa21/career-mode/teams?l=330", '>').Split('<')[0]);
            //player name Console.WriteLine(program.GetString("https://www.futwiz.com/en/fifa21/career-mode/player/steven-berghuis/3796", "<h1>", '>').Split('<')[0]);
            //player nation Console.WriteLine(program.GetString("https://www.futwiz.com/en/fifa21/career-mode/player/steven-berghuis/3796", "<div style=" + '\u0022' + "font-size:14px;", '>', 1).Split('|')[0]);
            Console.WriteLine("done");
            Console.Read();
        }


        public void GetPages(string url, string destination, string identifier, int recurrance)
        {
            StringBuilder pages = new StringBuilder();
            WebClient client = new WebClient();

            client.Headers.Add("User-Agent", "C# console program");
            string content = client.DownloadString(url);
            File.WriteAllText(tempFile, content);
            string[] lines = File.ReadAllLines(tempFile);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(identifier) && i % recurrance == 0)
                {
                    pages.Append(lines[i].Split('\u0022')[1] + Environment.NewLine);
                }
            }
            File.AppendAllText(destination, pages.ToString());
        }

        public string GetString(string url, string identifier, char split)
        {
            string result = null;
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "C# console program");

            string content = client.DownloadString(url);
            File.WriteAllText(tempFile, content);
            string[] lines = File.ReadAllLines(tempFile);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(identifier))
                {
                    return lines[i].Split(split)[1];
                }
            }
            return result;
        }

        public void GetPlayers()
        {
            string destination = "../../players.txt";
            string identifier = "<td width=+ '\u0022' + 5% + '\u0022' + class=+ '\u0022' + face+ '\u0022' + >";
            string[] teams = File.ReadAllLines("../../teams.txt");
            for (int i = 0; i < teams.Length; i++)
            {
                GetPages("https://futwiz.com" + teams[i], destination, identifier, 2);
            }
        }

        public string GetLeagueName(string url, string localUrl)
        {
            return GetString(url, "<a href=" + '\u0022' + localUrl, '>').Split('<')[0];

        }

        public void GetLeagues()
        {
            string url = "https://www.futwiz.com/en/fifa21/career-mode/teams?l=19";
            string destination = "../../leagues.txt";
            string identifier = "<a href=" + '\u0022' + "/en/fifa21/career-mode/teams";
            GetPages(url, destination, identifier, 1);
        }

        public void GetTeams()
        {
            string destination = "../../teams.txt";
            string identifier = "<h5>";
            string[] leagues = File.ReadAllLines("../../leagues.txt");
            for(int i = 0; i < leagues.Length; i++)
            {
                GetPages("https://futwiz.com" + leagues[i], destination, identifier, 1);
            }
        }

        public string ParseLeague(string url, string localUrl)
        {
            string leagueName = GetLeagueName(url, localUrl);
            byte[] bytes = Encoding.Default.GetBytes(leagueName);
            leagueName = Encoding.UTF8.GetString(bytes);
            int teamAmount = CheckNumberOfLineStarts(url, "<h5>");
            return leagueName + " #teams " + teamAmount;
        }

        public void ParseAllLeagues()
        {
            StringBuilder output = new StringBuilder();
            string[] leagues = File.ReadAllLines("../../leagues.txt");
            for(int i = 0; i < leagues.Length; i++)
            {
                output.Append(i + "," + ParseLeague("http://futwiz.com" + leagues[i], leagues[i]) + Environment.NewLine);
            }
            Console.WriteLine(output);
        }

        public int CheckNumberOfLineStarts(string url, string lineStart)
        {
            int number = 0;
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "C# console program");

            string content = client.DownloadString(url);
            File.WriteAllText(tempFile, content);
            string[] lines = File.ReadAllLines(tempFile);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(lineStart))
                {
                    number++;
                }
            }
            return number;
        }

        public string[] GetAllOccurences(string url, string identifier, char split, int splitNumber)
        {
            string[] values = new string[CheckNumberOfLineStarts(url, identifier)];
            int index = 0;
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "C# console program");

            string content = client.DownloadString(url);
            File.WriteAllText(tempFile, content);
            string[] lines = File.ReadAllLines(tempFile);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(identifier))
                {
                    if(lines[i].Split(split).Length <= splitNumber)
                    {
                        break;
                    }
                    if(values.Length > index)
                    {
                        values[index] = lines[i].Split(split)[splitNumber];
                        index++;
                    }                   
                }
            }

            return values;
        }

        public void ReadURLInTemp(string url)
        {
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "C# console program");
            string fileName = "../../temp.txt";

            string content = client.DownloadString(url);
            File.WriteAllText(fileName, content);
        }

        public void FilterOnString(string filter, string file)
        {
            string[] strings = File.ReadAllLines(file);
            StringBuilder filteredStrings = new StringBuilder();
            for(int i = 0; i < strings.Length; i++)
            {
                if (strings[i].StartsWith(filter))
                {
                    filteredStrings.Append(strings[i].Split('\u0022')[1] + Environment.NewLine);
                }
            }
            File.WriteAllText(file, filteredStrings.ToString());
        }
        //public string GetBudgets()
        //{
            
        //}
    }
}