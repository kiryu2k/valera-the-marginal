using System.Text.Json.Serialization;

namespace ValeraTheMarginal
{
    public class StatusException : Exception
    {
        public StatusException() { }
        public StatusException(string message) : base(message) { }
        public StatusException(string message, Exception innerException) : base(message, innerException) { }
    }


    public class ValeraStatus
    {
        public readonly Dictionary<string, Limit> Limits = new()
        {
            {"health", new (0, 100)},
            {"alcohol", new (0, 100)},
            {"cheerfulness", new (-10, 10)},
            {"tiredness", new (0, 100)},
            {"money", new (0, int.MaxValue)},
        };

        private readonly HashSet<string> statsSet = new()
        {
            "health", "alcohol", "cheerfulness", "tiredness", "money"
        };

        public ValeraStatus()
        {
            Day = 1;
            LastAction = "Be born";
            Stats = new Dictionary<string, int>
            {
                {"health", 100},
                {"alcohol", 0},
                {"cheerfulness", 0},
                {"tiredness", 0},
                {"money", 50},
            };
        }

        [JsonPropertyName("day"), JsonRequired]
        public uint Day { get; set; }
        [JsonPropertyName("last_action"), JsonRequired]
        public string LastAction { get; set; }
        [JsonPropertyName("stats"), JsonRequired]
        public Dictionary<string, int> Stats { get; set; }

        public void Validate()
        {
            if (LastAction == null)
            {
                throw new StatusException("last action must not be null");
            }
            if (Stats == null || Stats.Count != statsSet.Count)
            {
                throw new StatusException("not enough stats for valera");
            }
            foreach (var stat in Stats)
            {
                if (!statsSet.Contains(stat.Key))
                {
                    throw new StatusException($"invalid valera's stat '{stat.Key}'");
                }
                if (Limits[stat.Key].Min > stat.Value || Limits[stat.Key].Max < stat.Value)
                {
                    throw new StatusException($"invalid value '{stat.Value}' of valera's stat '{stat.Key}'" +
                        $"\nit should be in the range [{Limits[stat.Key].Min}, {Limits[stat.Key].Max}]");
                }
            }
        }
    }
}
