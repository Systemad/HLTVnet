using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HLTVnet.Models;
using HtmlAgilityPack;

namespace HLTVnet.Parsing
{
    public static partial class HLTVParser
    {
        public static Task<FullMatch> GetMatch(int id, WebProxy proxy = null)
        {
            return FetchPage($"matches/{id}/-", (response) => ParseMatchPage(response, id), proxy);
        }

        private static FullMatch ParseMatchPage(Task<HttpResponseMessage> response, int id = 0)
        {
            var content = response.Result.Content;
            string htmlContent = content.ReadAsStringAsync().Result;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlContent);

            HtmlNode document = html.DocumentNode;

            var model = new FullMatch();

            model.Id = id;

            //Match date
            long date = long.Parse(document.QuerySelector(".timeAndEvent .date").Attributes["data-unix"].Value);
            model.Date = HLTVParser.DateTimeFromUnixTimestampMillis(date);
            //Match format
            try
            {
                string preformattedText = document.QuerySelector(".preformatted-text").InnerText;
                model.Format = preformattedText.Split('\n').First();
                //model.AdditionalInfo = preformattedText.Substring(preformattedText.IndexOf('\n') + 1);
            }
            catch (Exception)
            {
                model.Format = "Best of 1";
            }

            //Team 1
            Team team1 = new Team();
            Console.WriteLine(team1.Name);
            
            //team1.Id = int.Parse(document.QuerySelectorAll(".logo team1Logo").First().Attributes["src"].Value.Split('/').Last());
            team1.Name = document.QuerySelectorAll(".team1Logo").First().Attributes["title"].Value;
            model.Team1 = team1;

            //Team 2
            Team team2 = new Team();
            //team2.Id = int.Parse(document.QuerySelectorAll(".team img.logo").Last().Attributes["src"].Value.Split('/').Last());
            team2.Name = document.QuerySelectorAll(".team1Logo").Last().Attributes["title"].Value;
            model.Team2 = team2;

            //Winning team
            if (document.QuerySelector(".team1-gradient > div").HasClass("won"))
                model.WinningTeam = team1;
            if (document.QuerySelector(".team2-gradient > div").HasClass("won"))
                model.WinningTeam = team2;

            //Event
            Event matchEvent = new Event();
            matchEvent.Name = document.QuerySelector(".timeAndEvent .event a").Attributes["title"].Value;
            //matchEvent.Id = int.Parse(document.QuerySelector(".timeAndEvent .event a").Attributes["href"].Value.Split('/', StringSplitOptions.RemoveEmptyEntries)[1]);
            model.Event = matchEvent;

            //Maps
            var mapHolderNodes = document.QuerySelectorAll(".mapholder");

            List<MapResult> mapResults = new List<MapResult>();
            foreach (var mapHolderNode in mapHolderNodes)
            {
                MapResult mapResult = new MapResult();
                mapResult.Name = mapHolderNode.QuerySelector(".mapname").InnerText;
                var resultsNode = mapHolderNode.QuerySelector(".results");
                var scoreNodes = resultsNode.QuerySelectorAll(".results-team-score").ToList();
                if (scoreNodes.Count > 0 && scoreNodes[0].InnerText != "-")
                {
                    mapResult.Team1Score = int.Parse(scoreNodes[0].InnerText);
                    mapResult.Team2Score = int.Parse(scoreNodes[1].InnerText);
                    if (mapHolderNode.QuerySelector(".results-stats") != null)
                        mapResult.StatsId = int.Parse(mapHolderNode.QuerySelector(".results-stats").Attributes["href"].Value.Split('/', StringSplitOptions.RemoveEmptyEntries)[3]);
                }
                
                mapResults.Add(mapResult);
            }
            model.Maps = mapResults.ToArray();

            //Demos
            var demoNodes = document.QuerySelectorAll(".stream-box").Where(node => node.Attributes["data-stream-embed"] == null);
            List<Demo> demos = new List<Demo>();
            foreach (var demoNode in demoNodes)
            {
                if (demoNode.QuerySelector("a") == null || !demoNode.QuerySelector("a").Attributes.Contains("href"))
                    continue;

                Demo demo = new Demo();
                string demoDownloadUrl = "https://www.hltv.org" + demoNode.QuerySelector("a").Attributes["href"].Value;
                demo.Name = demoNode.QuerySelector("a").InnerText;
                demo.Url = GetDemoDirectDownloadUrl(demoDownloadUrl);
                demos.Add(demo);
            }
            // TODO: ADD TWITCH LINKS LINKS
            model.Demos = demos.ToArray();

            //Veto
            var vetoNodes = document.QuerySelectorAll(".veto-box .padding > div");
            List<Veto> vetos = new List<Veto>();
            foreach (var vetoNode in vetoNodes)
            {
                Veto veto = new Veto();
                string cleanVeto = Regex.Replace(vetoNode.InnerText.Trim(), @"^\d.", "").Trim();

                bool containsPicked = vetoNode.InnerText.ToLower().Contains("picked");
                bool containsRemoved = vetoNode.InnerText.ToLower().Contains("removed");

                string teamName = "";
                string mapName = "";
                string action = containsPicked ? "picked" : "removed";

                if (containsPicked)
                {
                    teamName = cleanVeto.Split(" picked ")[0];
                    mapName = cleanVeto.Split(" picked ")[1];
                }
                if (containsRemoved)
                {
                    teamName = cleanVeto.Split(" removed ")[0];
                    mapName = cleanVeto.Split(" removed ")[1];
                }

                if (mapName == "" || teamName == "")
                {
                    teamName = "";
                    mapName = cleanVeto.Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];
                    action = "was left over";
                }

                if (teamName != "")
                    veto.Team = team1.Name == teamName ? team1 : team2;
                veto.Map = mapName;
                veto.Action = action;
                vetos.Add(veto);
            }
            model.Vetos = vetos.ToArray();

            //Team 1 players
            var team1PlayersHolderNode = document.QuerySelectorAll("div.players").First();
            var team1PlayersFlagAllignNodes = team1PlayersHolderNode.QuerySelectorAll(".flagAlign");
            List<Player> team1Players = new List<Player>();
            foreach (var flagAllignNode in team1PlayersFlagAllignNodes)
            {
                Player player = new Player();
                player.Name = flagAllignNode.QuerySelector(".text-ellipsis").InnerText;
                if (flagAllignNode.ParentNode.Attributes["href"] != null && flagAllignNode.ParentNode.Attributes["href"].Value != null)
                    player.Id = int.Parse(flagAllignNode.ParentNode.Attributes["href"].Value.Split('/', StringSplitOptions.RemoveEmptyEntries)[1]);
                team1Players.Add(player);
            }
            model.Team1Players = team1Players.ToArray();

            //Team 2 players
            var team2PlayersHolderNode = document.QuerySelectorAll("div.players").Last();
            var team2PlayersFlagAllignNodes = team2PlayersHolderNode.QuerySelectorAll(".flagAlign");
            List<Player> team2Players = new List<Player>();
            foreach (var flagAllignNode in team2PlayersFlagAllignNodes)
            {
                Player player = new Player();
                player.Name = flagAllignNode.QuerySelector(".text-ellipsis").InnerText;
                if (flagAllignNode.ParentNode.Attributes["href"] != null && flagAllignNode.ParentNode.Attributes["href"].Value != null)
                    player.Id = int.Parse(flagAllignNode.ParentNode.Attributes["href"].Value.Split('/', StringSplitOptions.RemoveEmptyEntries)[1]);
                team2Players.Add(player);
            }
            model.Team2Players = team2Players.ToArray();

            //Console.WriteLine(model.Team1.Name);
            return model;
        }

        //https://stackoverflow.com/a/47806360
        static string GetDemoDirectDownloadUrl(string demoDownloadUrl)
        {
            var request = (HttpWebRequest)WebRequest.Create(demoDownloadUrl);
            request.Method = "HEAD";
            request.AllowAutoRedirect = false;

            string location = "";

            try
            {
                using (request.GetResponse() as HttpWebResponse)
                {

                }
            }
            catch (WebException e)
            {
                location = e.Response.Headers["Location"];
            }

            return location;
        }
    }
}