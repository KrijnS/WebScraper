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
            string playerUrl = "https://www.futwiz.com/en/fifa21/career-mode/player/joshua-kimmich/111";
            string[] playerUrls = File.ReadAllLines("../../players.txt");
            string[] players = new string[playerUrls.Length];

            for(int i = 0; i < playerUrls.Length; i++)
            {
                players[i] = program.PlayerToString("https://futwiz.com" + playerUrls[i]);
                Console.WriteLine(players[i]);
            }

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
        public string GetString(string url, string identifier, char split, int stringSplit, bool download)
        {
            string result = null;
            if (download)
            {
                WebClient client = new WebClient();
                client.Headers.Add("User-Agent", "C# console program");

                string content = client.DownloadString(url);
                File.WriteAllText(tempFile, content);
            }
            
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
            return GetString(url, "<a href=" + '\u0022' + localUrl, '>', 1, true).Split('<')[0];

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
            int teamAmount = CheckNumberOfLineStarts(url, "<h5>", true);
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
        public int CheckNumberOfLineStarts(string url, string lineStart, bool download)
        {
            int number = 0;

            if (download)
            {
                WebClient client = new WebClient();
                client.Headers.Add("User-Agent", "C# console program");

                string content = client.DownloadString(url);
                File.WriteAllText(tempFile, content);
            }
            
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
            string[] values = new string[CheckNumberOfLineStarts(url, identifier, true)];
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
            return CheckNumberOfLineStarts(url, "<a href=\u0022/en/fifa21/career-mode/player/", true) / 2;
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
                budgets[i] = ConvertBudget(values[i], true);
            }
            return budgets;
        }

        //Convert strings of budgets to doubles
        public double ConvertBudget(string value, bool club)
        {
            //initially budget is 0
            double budget = 0;
            //remove excess from string
            if (club)
            {
                value = value.Split('<')[0].Remove(0, 1).Split('\u00A3')[1];
            }
            //convert from K to 1.000 and M to 1.000.000
            if (value.Contains("M")) { value = value.Split('M')[0]; budget = Convert.ToDouble(value) * 1000000; }
            else if (value.Contains("K")) { value = value.Split('K')[0]; budget = Convert.ToDouble(value) * 1000; }
            //fix for ToDouble() ignoring point symbol
            if (value.Contains(".")) { budget /= 10; }
            return budget;
        }

        //return player name from player url
        public string GetPlayerName()
        {
            return GetString(tempFile, "<h1>", '>', 1, false).Split('<')[0];
        }

        //return player overall from player url
        public int GetPlayerOvr()
        {
            return Int32.Parse(GetString(tempFile, "<div class=\u0022cplayerprofile-ovr\u0022><p class=\u0022cprofstat\u0022>", '>', 2, false).Split('<')[0]);
        }

        //return player potential from player url
        public int GetPlayerPot()
        {
            return Int32.Parse(GetString(tempFile, "<div class=\u0022cplayerprofile-pot\u0022><p class=\u0022cprofstat\u0022>", '>', 2, false).Split('<')[0]);
        }

        //parse all player stats from player url
        public int[] GetPlayerStats()
        {
            int[] stats = new int[6];
            string[] statStrings = GetStatStrings();

            for(int i = 0; i < statStrings.Length; i++)
            {
                stats[i] = ParseStat(statStrings[i]);
            }
            return stats;
        }

        //get all strings containing a stat for players
        public string[] GetStatStrings()
        {
            string[] stats = new string[6];
            int counter = 0;
           
            string[] lines = File.ReadAllLines(tempFile);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("colour att"))
                {
                    stats[counter] = lines[i];
                    counter++;
                }
            }
            return stats;
        }

        //parse stat from string
        public int ParseStat(string s)
        {
            return Int32.Parse(s.Split('>')[1].Split('<')[0]);
        }
        
        //get contract length of player
        public int GetContractLength()
        {
            //check if player has real face, this would change the identifier of the contract string
            if (CheckNumberOfLineStarts(tempFile, "<div class=\u0022realfacebutton\u0022>Real Face</div>", false) == 0)
            {
                return Int32.Parse(GetString(tempFile, "<div class=\u0022cprofile-inforbar-label ml-20", '>', 3, false).Split('<')[0]);
            }
            else
            {
                return Int32.Parse(GetString(tempFile, "<div class=\u0022realfacebutton\u0022>Real Face</div> <div class=\u0022cprofile-inforbar-label ml-20\u0022>", '>', 5, false).Split('<')[0]);
            }          
        }

        //get player age
        public int GetPlayerAge()
        {
            return Int32.Parse(GetString(tempFile, "<p class=\u0022ppdb-d\u0022>", '>', 1, false).Split('<')[0]);
        }

        //get player position and transform in desired string
        public string GetPlayerPos()
        {
            return GetString(tempFile, "<div class=\u0022cplayerprofile-mobinfo\u0022>", '>', 2, false).Split('<')[0].Replace(", ", "/");
        }

        public double GetPlayerWorth()
        {
            string value = GetString(tempFile, "<div class=\u0022cprofile-inforbar-label\u0022 style=\u0022margin-left:10px;", '>', 3, false).Split('<')[0].Split(';')[1];
            return ConvertBudget(value, false);
        }

        public double GetPlayerWage()
        {
            string value = GetString(tempFile, "<div class=\u0022cprofile-inforbar-label\u0022>Wage", '>', 3, false).Split('<')[0].Split(';')[1];
            return ConvertBudget(value, false);
        }

        public string PlayerToString(string url)
        {
            ReadURLInTemp(url);
            StringBuilder toString = new StringBuilder();
            toString.Append(GetPlayerName() + ",");
            toString.Append(GetPlayerPos() + ",");
            toString.Append(GetPlayerOvr() + ",");
            toString.Append(GetPlayerPot() + ",");
            toString.Append(GetPlayerAge() + ",");
            toString.Append(GetPlayerWorth() + ",");
            toString.Append(GetPlayerWage() + ",");
            toString.Append(GetContractLength() + ",");

            int[] stats = GetPlayerStats();
            for (int i = 0; i < stats.Length-1; i++)
            {
                toString.Append(stats[i] + ",");
            }
            toString.Append(stats[stats.Length-1]);
            return toString.ToString();
        }
    }
}
