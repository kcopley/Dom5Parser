using Dom5Edit.Commands;

namespace Dom5Edit.Props
{
    public class BitmaskProperty : Property
    {
        public static Property Create()
        {
            return new BitmaskProperty();
        }

        public ulong Value { get; set; }
        public bool HasValue { get; set; }

        /// <summary>A negative argument (#nextingeo -1), kept as written; Value holds its bits.</summary>
        public long? Negative { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this.Command = c;
            this.Comment = comment;
            HasValue = s.TryRetrieveUlongFromString(out ulong val, out string remainder);
            if (HasValue) Value = val;
            else if (s.TrimStart().StartsWith("-") && s.Trim().TryRetrieveNumericFromString(out int neg, out string negRemainder))
            {
                HasValue = true;
                Negative = neg;
                Value = unchecked((ulong)(long)neg);
                remainder = negRemainder;
            }
            if (remainder.Length > 0) Comment += remainder;
        }

        private string ValueText => Negative.HasValue && unchecked((ulong)Negative.Value) == Value ? Negative.Value.ToString() : Value.ToString();

        //Preliminary Example only for now, not optimal
        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    if (HasValue)
                    {
                        return s + " " + ValueText + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return HasValue ? s + " " + ValueText : s;
                }
            }
            else return "";
        }
        internal override Property GetDefault()
        {
            return new BitmaskProperty() { Value = 0 };
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is BitmaskProperty)
            {
                var compare = copyFrom as BitmaskProperty;
                if (this.Command == compare.Command && this.Value == compare.Value) return true;
            }
            return false;
        }
    }
}
