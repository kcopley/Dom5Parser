using Dom5Edit.Commands;

namespace Dom5Edit.Props
{
    public class FloatFloatFloatProperty : Property
    {
        public static Property Create()
        {
            return new FloatFloatFloatProperty();
        }

        public float Value1 { get; set; }
        public float Value2 { get; set; }
        public float Value3 { get; set; }
        public bool HasValue { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this.Command = c;
            this.Comment = comment;
            s = s.Trim();
            // the values, however they're spaced (the game reads the numbers it needs; more are kept as text)
            var split = s.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 3)
            {
                comment = (comment + " " + string.Join(" ", split.Skip(3))).Trim();
                this.Comment = comment;
                split = split.Take(3).ToArray();
            }
            if (split.Length == 3)
            {
                HasValue = split[0].TryRetrieveFloatFromString(out float val1, out string remainder1);
                if (HasValue) Value1 = val1;
                if (string.IsNullOrEmpty(remainder1))
                {
                    HasValue = split[1].TryRetrieveFloatFromString(out float val2, out string remainder2);
                    if (HasValue) Value2 = val2;
                    if (string.IsNullOrEmpty(remainder2))
                    {
                        HasValue = split[2].TryRetrieveFloatFromString(out float val3, out string remainder3);
                        if (HasValue) Value3 = val3;
                        if (remainder3.Length > 0)
                        {
                            Comment += remainder3;
                        }
                    }
                }
            }
            else
            {
                HasValue = false;
            }
            if (!HasValue && !string.IsNullOrWhiteSpace(s))
                Unparsed = s.Trim(); // (kept as written: the game reads the numbers that are there)
        }

        //Preliminary Example only for now, not optimal
        public override string ToExportString()
        {
            if (!HasValue && Unparsed != null && CommandsMap.TryGetString(Command, out var asRead))
                return UnparsedExport(asRead);
            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    if (HasValue)
                    {
                        return s + " " + Value1 + " " + Value2 + " " + Value3 + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    if (HasValue)
                    {
                        return s + " " + Value1 + " " + Value2 + " " + Value3;
                    }
                    else
                    {
                        return s;
                    }
                }
            }
            else return "";
        }

        internal override Property GetDefault()
        {
            return new FloatFloatFloatProperty() { Value1 = 10, Value2 = 0, Value3 = 0 };
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is FloatFloatFloatProperty)
            {
                var compare = copyFrom as FloatFloatFloatProperty;
                if (this.Command == compare.Command && this.Value1 == compare.Value1 && this.Value2 == compare.Value2 && this.Value3 == compare.Value3) return true;
            }
            return false;
        }
    }
}
