using System.Collections.Generic;

namespace HLTVnet.Models.Enums
{
    public static class MapSlug
    {
        public static readonly Dictionary<string, string> MapSlugs = new Dictionary<string, string>()
        {
            { "tba", "TBA" },
            { "trn", "Train" },
            { "cbl", "Cobblestone" },
            { "inf", "Inferno" },
            { "cch", "Cache" },
            { "mrg", "Mirage" },
            { "ovp", "Overpass" },
            { "d2", "Dust2" },
            { "nuke", "Nuke" },
            { "tcn", "Tuscan" },
            { "vtg", "Vertigo" },
            { "-", "Default" },
        };
    }
}
