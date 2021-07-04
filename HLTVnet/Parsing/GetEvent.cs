﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public static Task<FullEvent> GetEvent(int id, WebProxy proxy = null)
        {
            return HLTVParser.FetchPage($"events/{id}/-", ParseEventPage, proxy);
        }

        private static FullEvent ParseEventPage(Task<HttpResponseMessage> response)
        {
            var content = response.Result.Content;
            string htmlContent = content.ReadAsStringAsync().Result;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlContent);

            HtmlNode document = html.DocumentNode;

            FullEvent model = new FullEvent();

            //Event start and finish dates
            long dateStart = long.Parse(document.QuerySelectorAll(".eventdate span").First().Attributes["data-unix"].Value);
            long dateFinish = long.Parse(document.QuerySelectorAll(".eventdate span").Last().Attributes["data-unix"].Value);
            model.DateStart = HLTVParser.DateTimeFromUnixTimestampMillis(dateStart);
            model.DateEnd = HLTVParser.DateTimeFromUnixTimestampMillis(dateFinish);

            //Event id
            model.Id = int.Parse(document.QuerySelector(".event-header-component a").Attributes["href"].Value.Split('/', StringSplitOptions.RemoveEmptyEntries)[1]);

            //Event name
            model.Name = document.QuerySelector(".eventname").InnerText;

            //Event prizepool
            model.PrizePool = document.QuerySelector(".prizepool.text-ellipsis").InnerText;

            //Event location
            Country countryModel = new Country();
            countryModel.Name = document.QuerySelector(".flag-align span").InnerText;
            countryModel.Code = document.QuerySelector(".flag-align img").Attributes["src"].Value.Split('/').Last().Split(".").First();
            model.Location = countryModel;

            //Event IsOnline
            model.IsOnline = model.Location.Name.ToLower().Contains("online");

            //Related events
            List<Event> relatedEvents = new List<Event>();
            var relatedEventNodes = document.QuerySelectorAll(".related-event img");
            foreach (var relatedEventNode in relatedEventNodes)
            {
                try
                {
                    Event relatedEventModel = new Event();
                    relatedEventModel.Name = relatedEventNode.Attributes["title"].Value;
                    //relatedEventModel.Id = int.Parse(relatedEventNode.Attributes["src"].Value.Split('/').Last().Split(".").First());
                    relatedEvents.Add(relatedEventModel);
                }
                catch (Exception)  { }
            }
            model.RelatedEvents = relatedEvents.ToArray();

            //Attending teams
            List<Team> attendingTeams = new List<Team>();
            var attendingTeamNodes = document.QuerySelectorAll(".teams-attending .team-box .team-name a");
            foreach (var attendingTeamNode in attendingTeamNodes)
            {
                Team attendingTeamModel = new Team();
                attendingTeamModel.Name = attendingTeamNode.QuerySelector("div").InnerText;
                //attendingTeamModel.Id = int.Parse(attendingTeamNode.Attributes["href"].Value.Split('/', StringSplitOptions.RemoveEmptyEntries)[1]);
                attendingTeams.Add(attendingTeamModel);
            }
            model.Teams = attendingTeams.ToArray();

            return model;
        }
    }
}
