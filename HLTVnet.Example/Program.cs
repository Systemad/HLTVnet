using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HLTVnet.Models;
using HLTVnet.Parsing;

namespace HLTVnet.Example
{
    class Program
    {
        private static List<UpcomingMatch> _task = new List<UpcomingMatch>();
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
            //var task = HltvParser.GetMatch(2349612);
        }
        
        private static async Task MainAsync()
        {
            try
            {
                _task = await HLTVParser.GetUpcomingMatches();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                throw;
            }
        }
    }
}