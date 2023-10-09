using Parser;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ValeraTheMarginal
{
    public class ActionException : Exception
    {
        public ActionException() { }
        public ActionException(string message) : base(message) { }
        public ActionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ActionNotFoundException : ActionException
    {
        public ActionNotFoundException() { }
        public ActionNotFoundException(string message) : base(message) { }
        public ActionNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ExtraValue
    {
        [JsonPropertyName("value"), JsonRequired]
        public int Value { get; set; }
        [JsonPropertyName("condition"), JsonRequired]
        public Dictionary<string, string> Condition { get; set; }

        public bool CheckConditions(Dictionary<string, int> stats)
        {
            foreach (var cond in Condition)
            {
                if (!ConditionParser.Parse(cond.Value, stats[cond.Key])) {
                    return false;
                }
            }
            return true;
        }
    }

    public class ValeraParam
    {
        [JsonPropertyName("default"), JsonRequired]
        public int Default { get; set; }
        [JsonPropertyName("extra"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ExtraValue>? Extra { get; set; }
    }

    public class Action
    {
        [JsonPropertyName("name"), JsonRequired]
        public string Name { get; set; }
        [JsonPropertyName("modifiable_params"), JsonRequired]
        public Dictionary<string, ValeraParam> ModifiableParams { get; set; }
        [JsonPropertyName("condition"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Condition { get; set; }

        private readonly JsonSerializerOptions _options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, _options);
        }
    }
}
