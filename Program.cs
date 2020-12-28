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

            //program.GetPlayers("https://www.futwiz.com/en/fifa21/career-mode/players?minrating=1&maxrating=99&teams%5B%5D=21&leagues[]=19&order=rating&s=desc");
            //program.ReadURLInTemp("https://www.futwiz.com/en/fifa21/career-mode/player/robert-lewandowski/12");
            Console.WriteLine(program.GetPlayerOvr("https://www.futwiz.com/en/fifa21/career-mode/player/robert-lewandowski/12"));
            //Console.WriteLine(program.GetString("https://www.futwiz.com/en/fifa21/career-mode/player/steven-berghuis/3796", "<div style='\u0022'font-size:14px;", ">", 1).Split('|')[0]);
            Console.WriteLine("done");
            Console.Read();
        }

        //get all pages related with url and identifier
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

        //get string split at specific char
        public string GetString(string url, string identifier, char split, int stringSplit)
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
                    return lines[i].Split(split)[stringSplit];
                }
            }
            return result;
        }
        
        //parse league name from league url
        public string GetLeagueName(string url, string localUrl)
        {
            return GetString(url, "<a href=" + '\u0022' + localUrl, '>', 1).Split('<')[0];

        }

        //read leagues and write them to leagues.txt file
        public void GetLeagues()
        {
            string url = "https://www.futwiz.com/en/fifa21/career-mode/teams?l=19";
            string destination = "../../leagues.txt";
            string identifier = "<a href=" + '\u0022' + "/en/fifa21/career-mode/teams";
            GetPages(url, destination, identifier, 1);
        }

        //read team urls and write them to teams.txt file
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

        //get output for a league in string form
        public string ParseLeague(string url, string localUrl)
        {
            string leagueName = GetLeagueName(url, localUrl);
            byte[] bytes = Encoding.Default.GetBytes(leagueName);
            leagueName = Encoding.UTF8.GetString(bytes);
            int teamAmount = CheckNumberOfLineStarts(url, "<h5>");
            return leagueName + " #teams " + teamAmount;
        }

        //get output sequence of all leagues to console
        public void ParseAllLeagues()
        {
            StringBuilder output = new StringBuilder();
            string[] leagues = File.ReadAllLines("../../leagues.txt");
            for(int i = 0; i < leagues.Length; i++)
            {
                output.Append(i + ". " + ParseLeague("http://futwiz.com" + leagues[i], leagues[i]) + Environment.NewLine);
            }
            Console.WriteLine(output);
        }

        //check how many lines start with specific starting sequence
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

        //get all occurences of specific start sequence and split on specific char
        public string[] GetAllOccurences(string url, string identifier, char split, int splitNumber)
        {
            //initialise string array on number of occurences
            string[] values = new string[CheckNumberOfLineStarts(url, identifier)];
            int index = 0;
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "C# console program");

            string content = client.DownloadString(url);
            File.WriteAllText(tempFile, content);
            string[] lines = File.ReadAllLines(tempFile);
            for (int i = 0; i < lines.Length; i++)
            {
                //add line if starts with starting sequence
                if (lines[i].StartsWith(identifier))
                {
                    //check if splitNumber is smaller or equal to split string
                    if(lines[i].Split(split).Length <= splitNumber)
                    {
                        break;
                    }
                    //add to initial array and increase counter of array
                    if(values.Length > index)
                    {
                        values[index] = lines[i].Split(split)[splitNumber];
                        index++;
                    }                   
                }
            }

            return values;
        }

        //parse HTML code in temp file
        public void ReadURLInTemp(string url)
        {
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "C# console program");

            string content = client.DownloadString(url);
            File.WriteAllText(tempFile, content);
        }

        //filter file on specific start sequence of string
        public void FilterOnString(string filter, string file)
        {
            string[] strings = File.ReadAllLines(file);
            StringBuilder filteredStrings = new StringBuilder();
            for(int i = 0; i < strings.Length; i++)
            {
                //add string to stringbuilder if given start sequence
                if (strings[i].StartsWith(filter))
                {
                    filteredStrings.Append(strings[i].Split('\u0022')[1] + Environment.NewLine);
                }
            }
            File.WriteAllText(file, filteredStrings.ToString());
        }

        //reads file where player links are double and only keeps 1st entries
        public StringBuilder DeleteDuplicatePlayerLinks(string file)
        {
            string[] strings = File.ReadAllLines(file);
            StringBuilder filteredStrings = new StringBuilder();
            for (int i = 0; i < strings.Length; i++)
            {
                if (i%2 == 0)
                {
                    filteredStrings.Append(strings[i] + Environment.NewLine);
                }
            }
            return filteredStrings;
        }

        //read all player urls from club url
        public void GetPlayers(string url)
        {
            //first read team page in temp file
            ReadURLInTemp(url);
            //filter on all player specific lines
            FilterOnString("<a href=\u0022/en/fifa21/career-mode/player/", tempFile);
            //parse players and replace file
            File.AppendAllText("../../players.txt" , DeleteDuplicatePlayerLinks(tempFile).ToString());
        }

        //get number of players of specific team from team url
        public int GetNumberOfPlayers(string url)
        {
            return CheckNumberOfLineStarts(url, "<a href=\u0022/en/fifa21/career-mode/player/") / 2;
        }

        //return array of budgets in double form from league url
        public double[] GetBudgets(string url)
        {
            //get all budget strings of page
            string[] values = GetAllOccurences("https://www.futwiz.com" + url, "<p>Budget <strong>", '>', 2);
            double[] budgets = new double[values.Length];
            //convert strings via ConvertBudgets method
            for (int i = 0; i < values.Length; i++)
            {
                budgets[i] = ConvertBudget(values[i]);
            }
            return budgets;
        }

        //Convert strings of budgets to doubles
        public double ConvertBudget(string value)
        {
            //initially budget is 0
            double budget = 0;
            //remove excess from string
            value = value.Split('<')[0].Remove(0, 1).Split('\u00A3')[1];
            //convert from K to 1.000 and M to 1.000.000
            if (value.Contains("M")) { value = value.Split('M')[0]; budget = Convert.ToDouble(value) * 1000000; }
            else if (value.Contains("K")) { value = value.Split('K')[0]; budget = Convert.ToDouble(value) * 1000; }
            //fix for ToDouble() ignoring point symbol
            if (value.Contains(".")) { budget /= 10; }
            return budget;
        }

        //return player name from player url
        public string GetPlayerName(string url)
        {
            return GetString("https://www.futwiz.com/en/fifa21/career-mode/player/steven-berghuis/3796", "<h1>", '>', 1).Split('<')[0];
        }

        public int GetPlayerOvr(string url)
        {
            return Int32.Parse(GetString(url, "<div class=\u0022cplayerprofile-ovr\u0022><p class=\u0022cprofstat\u0022>", '>', 2).Split('<')[0]);
        }

    }
}