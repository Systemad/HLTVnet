using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HLTVnet.Models;
using HtmlAgilityPack;

namespace HLTVnet.Parsing
{
    public static partial class HLTVParser
    {
        public static Task<List<UpcomingMatch>> GetUpcomingMatches(WebProxy proxy = null)
        {
            return FetchPage("matches", ParseMatchesPage, proxy);
        }

        private static List<UpcomingMatch> ParseMatchesPage(Task<HttpResponseMessage> response)
        {
            var content = response.Result.Content;
            string htmlContent = content.ReadAsStringAsync().Result;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlContent);

            HtmlNode document = html.DocumentNode;

            var upcomingMatchNodes = document.QuerySelectorAll(".upcomingMatch");

            List<UpcomingMatch> upcomingMatches = new List<UpcomingMatch>();

            foreach (HtmlNode upcomingMatchNode in upcomingMatchNodes)
            {
                try
                {
                    UpcomingMatch model = new UpcomingMatch();

                    //Match ID
                    //string matchPageUrl = upcomingMatchNode.InnerHtml; //Attributes["href"].Value;
                    string matchPageUrl = upcomingMatchNode.QuerySelector("a").Attributes["href"].Value;
                    model.Id = int.Parse(matchPageUrl.Split('/', StringSplitOptions.RemoveEmptyEntries)[1]);
                    
                    //Match date
                    long unixDateMilliseconds = long.Parse(upcomingMatchNode.Attributes["data-zonedgrouping-entry-unix"].Value);
                    string dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixDateMilliseconds).ToString();
                    model.Date = DateTime.Parse(dateTimeOffset);
                    
                    //Event ID and name
                    // TODO: Properly fix event ID (if even needed)
                    Event eventModel = new Event();
                    //string eventImageUrl = upcomingMatchNode.QuerySelector(".matchEventLogo").Attributes["src"].Value;
                    //eventModel.Id = int.Parse(matchPageUrl.Split("/").First());
                    //eventModel.Id = int.Parse(eventImageUrl.Split("/").Last().Split(".").First());
                    //eventModel.Name = document.QuerySelector(".matchEventName").Attributes["title"].Value;
                    eventModel.Name = upcomingMatchNode.QuerySelector(".matchEventName").InnerHtml;
                    eventModel.Type = Convert.ToBoolean(upcomingMatchNode.Attributes["lan"].Value) ? "Lan" : "Online";
                    model.Event = eventModel;

                    //Number of stars
                    model.Stars = int.Parse(upcomingMatchNode.Attributes["stars"].Value);
                    
                    var team1Node = upcomingMatchNode.QuerySelector(".team1");
                    var team1 = team1Node.QuerySelector(".matchTeamName").InnerHtml; // Attributes["matchTeamName"].Value;
                    
                    var team2Node = upcomingMatchNode.QuerySelector(".team2");
                    var team2 = team2Node.QuerySelector(".matchTeamName").InnerHtml;
                    
                    // Team 1
                    Team team1Model = new Team();
                    string team1LogoUrl = team1Node.QuerySelector(".matchTeamLogo").Attributes["src"].Value;
                    team1Model.TeamLogoUrl = team1LogoUrl;
                    team1Model.Name = team1;
                    model.Team1 = team1Model;
                    
                    // Team 2
                    Team team2Model = new Team();
                    string team2LogoUrl = team2Node.QuerySelector(".matchTeamLogo").Attributes["src"].Value;
                    team2Model.TeamLogoUrl = team2LogoUrl;
                    team2Model.Name = team2;
                    model.Team2 = team2Model;

                    //Map and format
                    string mapText = upcomingMatchNode.QuerySelector(".matchMeta").InnerText;
                    model.Format = mapText;

                    upcomingMatches.Add(model);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return upcomingMatches;
        }
    }
}