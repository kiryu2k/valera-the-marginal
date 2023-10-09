namespace Parser
{
    public class ParserException : Exception
    {
        public ParserException() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception innerException) : base(message, innerException) { }
    }

    internal class ConditionParser
    {
        public static bool Parse(string condition, int value)
        {
            if (condition == null)
            {
                throw new ParserException("invalid condition");
            }
            var splittedCond = condition.Split('&');
            if (splittedCond[0].Length == 0)
            {
                splittedCond[0] = condition;
            }
            var result = true;
            foreach (var cond in splittedCond)
            {
                if (!result)
                {
                    return false;
                }
                var idx = cond.IndexOfAny("0123456789".ToCharArray());
                if (!int.TryParse(cond[idx..], out int num))
                {
                    throw new ParserException("invalid condition");
                }
                var op = cond[..idx];
                if (op == ">")
                {
                    result &= (value > num);
                }
                else if (op == "<")
                {
                    result &= (value < num);
                }
                else if (op == "=")
                {
                    result &= (value == num);
                }
                else if (op == "!=")
                {
                    result &= (value != num);
                }
            }
            return result;
        }
    }
}
