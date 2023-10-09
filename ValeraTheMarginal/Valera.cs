using Parser;
using System.Text.Json;

namespace ValeraTheMarginal
{
    public class Valera
    {
        public readonly List<Action> Actions;

        private ValeraStatus _status;
        private readonly JsonSerializerOptions _options;

        public Valera(List<Action> actions)
        {
            _status = new ValeraStatus();
            Actions = actions;
            _options = new JsonSerializerOptions { WriteIndented = true };
        }

        public void DoAction(int actionNumber)
        {
            if (actionNumber < 0 || actionNumber >= Actions.Count)
            {
                throw new ActionNotFoundException("unacceptable action number");
            }
            var action = Actions[actionNumber];
            if (action.Condition != null)
            {
                foreach (var condition in action.Condition)
                {
                    if (!ConditionParser.Parse(condition.Value, _status.Stats[condition.Key]))
                    {
                        throw new ActionException($"Valera can't perfrom the action '{action.Name}' 'cause " +
                            $"the corresponding conditions for it aren't met: '{condition.Key}{condition.Value}'");
                    }
                }
            }
            _status.LastAction = action.Name;
            ++_status.Day;
            var stats = new Dictionary<string, int>();
            foreach (var stat in _status.Stats)
            {
                var param = action.ModifiableParams.GetValueOrDefault(stat.Key);
                if (param == null)
                {
                    stats.Add(stat.Key, stat.Value);
                    continue;
                }
                stats.Add(stat.Key, stat.Value + param.Default);
                stats[stat.Key] = AddExtraValues(stats[stat.Key], param.Extra);
                if (_status.Limits[stat.Key].Min > stats[stat.Key])
                {
                    stats[stat.Key] = _status.Limits[stat.Key].Min;
                }
                else if (_status.Limits[stat.Key].Max < stats[stat.Key])
                {
                    stats[stat.Key] = _status.Limits[stat.Key].Max;
                }
            }
            _status.Stats = stats;
        }

        private int AddExtraValues(int curValue, List<ExtraValue>? extra)
        {
            if (extra == null)
            {
                return curValue;
            }
            foreach (var ex in extra)
            {
                if (ex.CheckConditions(_status.Stats))
                {
                    curValue += ex.Value;
                }
            }
            return curValue;
        }

        public void LoadStatus(ref ValeraStatus status)
        {
            status.Validate();
            _status = status;
        }

        public bool IsDead()
        {
            return _status.Stats["health"] == 0;
        }

        public bool IsBroke()
        {
            return _status.Stats["money"] == 0;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(_status, _options);
        }
    }
}
