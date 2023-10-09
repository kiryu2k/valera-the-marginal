using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Configs
{
    public class ConfigException : Exception
    {
        public ConfigException() { }
        public ConfigException(string message) : base(message) { }
        public ConfigException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class Config
    {
        public readonly List<ValeraTheMarginal.Action>? Actions;

        private readonly HashSet<string> _paramSet = new()
        {
            "health", "alcohol", "cheerfulness", "tiredness", "money"
        };

        private readonly string _opPattern = "^(<|>|=|!=)[0-9]+$";

        public Config(ref string configPath)
        {
            var actionsText = File.ReadAllText(configPath, System.Text.Encoding.UTF8);
            Actions = JsonSerializer.Deserialize<List<ValeraTheMarginal.Action>>(actionsText);
            ValidateActions();
        }

        private void ValidateActions()
        {
            if (Actions == null || Actions.Count == 0)
            {
                throw new ConfigException("empty config");
            }
            var nameSet = new HashSet<string>();
            foreach (var action in Actions)
            {
                if (action.Name == null)
                {
                    throw new ConfigException("invalid null value in action name");
                }
                if (nameSet.Contains(action.Name))
                {
                    throw new ConfigException($"duplicate action name '{action.Name}'");
                }
                nameSet.Add(action.Name);
                if (action.Condition != null)
                {
                    ValidateCondition(action.Condition);
                }
                ValidateModifiableParams(action);
            }
        }

        private void ValidateModifiableParams(ValeraTheMarginal.Action action)
        {
            if (action.ModifiableParams == null || action.ModifiableParams.Count > _paramSet.Count)
            {
                throw new ConfigException("invalid count of modifiable parameters");
            }
            foreach (var param in action.ModifiableParams)
            {
                if (!_paramSet.Contains(param.Key))
                {
                    throw new ConfigException($"unexpected valera's parameter '{param.Key}' in action '{action.Name}'");
                }
                if (param.Value.Extra == null)
                {
                    continue;
                }
                foreach (var extra in param.Value.Extra)
                {
                    if (extra.Condition == null)
                    {
                        throw new ConfigException($"valera's parameter '{param.Key}' expected some condition but there's empty");
                    }
                    ValidateCondition(extra.Condition);
                }
            }
        }

        private void ValidateCondition(Dictionary<string, string> condition)
        {
            var hasValidParam = false;
            foreach (var cond in condition)
            {
                if (!_paramSet.Contains(cond.Key))
                {
                    throw new ConfigException($"invalid valera's parameter '{cond.Key}' for condition");
                }
                if (!Regex.IsMatch(cond.Value, _opPattern))
                {
                    /* maybe it's condition like >40&<80 ...? */
                    foreach (var splittedCond in cond.Value.Split('&'))
                    {
                        if (!Regex.IsMatch(splittedCond, _opPattern))
                        {
                            throw new ConfigException($"invalid condition '{cond.Value}' for valera's parameter '{cond.Key}'");
                        }
                    }
                }
                hasValidParam = true;
            }
            if (!hasValidParam)
            {
                throw new ConfigException($"zero valid valera's parameter in condition");
            }
        }
    }
}
