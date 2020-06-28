﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace WebScraper
{
    class Program
    {
        public string tempFile = @"C:\Users\Krijn\Documents\temp.txt";
        static void Main(string[] args)
        {
            Program program = new Program();
            string urlPlayers = "https://www.futwiz.com/en/fifa20/career-mode/players?minrating=1&maxrating=99&teams%5B%5D=246&leagues[]=10&order=rating&s=desc";
            string destinationPlayers = @"C:\Users\Krijn\Documents\players.txt";
            string playerIdentifier = "<a href=" + '\u0022' + "/en/fifa20/career-mode/player/";

            program.ParseAllLeagues();
            //program.GetPages(urlPlayers, destinationPlayers, playerIdentifier, 2);
            //league name Console.WriteLine(program.GetString("https://www.futwiz.com/en/fifa20/career-mode/teams?l=330", "<a href=" + '\u0022' + "/en/fifa20/career-mode/teams?l=330", '>').Split('<')[0]);
            //player name Console.WriteLine(program.GetString("https://www.futwiz.com/en/fifa20/career-mode/player/steven-berghuis/3796", "<h1>", '>').Split('<')[0]);
            //player nation Console.WriteLine(program.GetString("https://www.futwiz.com/en/fifa20/career-mode/player/steven-berghuis/3796", "<div style=" + '\u0022' + "font-size:14px;", '>', 1).Split('|')[0]);
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
                    result = lines[i].Split(split)[1];
                }
            }
            return result;
        }

        public string GetLeagueName(string url, string localUrl)
        {
            return GetString(url, "<a href=" + '\u0022' + localUrl, '>').Split('<')[0];

        }

        public void GetLeagues()
        {
            string url = "https://www.futwiz.com/en/fifa20/career-mode/teams?l=19";
            string destination = @"C:\Users\Krijn\Documents\leagues.txt";
            string identifier = "<a href=" + '\u0022' + "/en/fifa20/career-mode/teams";
            GetPages(url, destination, identifier, 1);
        }

        public void GetTeams()
        {
            string destination = @"C:\Users\Krijn\Documents\teams.txt";
            string identifier = "<h5>";
            string[] leagues = File.ReadAllLines(@"C:\Users\Krijn\Documents\leagues.txt");
            for(int i = 0; i < leagues.Length; i++)
            {
                GetPages("https://futwiz.com" + leagues[i], destination, identifier, 1);
            }
        }

        public void ParseLeague(string url, string localUrl)
        {
            string leagueName = GetLeagueName(url, localUrl);
            byte[] bytes = Encoding.Default.GetBytes(leagueName);
            leagueName = Encoding.UTF8.GetString(bytes);
            int teamAmount = CheckNumberOfLineStarts(url, "<h5>");
            Console.WriteLine(leagueName + " " + teamAmount);
        }

        public void ParseAllLeagues()
        {
            string[] leagues = File.ReadAllLines(@"C:\Users\Krijn\Documents\leagues.txt");
            for(int i = 0; i < leagues.Length; i++)
            {
                ParseLeague("http://futwiz.com" + leagues[i], leagues[i]);
            }
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
    }
}