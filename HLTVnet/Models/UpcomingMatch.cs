using System;

namespace HLTVnet.Models
{
    public class UpcomingMatch
    {
        public int Id { get; set; }
        public Team Team1 { get; set; }
        public Team Team2 { get; set; }
        public DateTime Date { get; set; }
        public string Format { get; set; }
        public Event Event { get; set; }
        public string Map { get; set; } = null;
        public string Title { get; set; }
        public int Stars { get; set; }
    }
}
