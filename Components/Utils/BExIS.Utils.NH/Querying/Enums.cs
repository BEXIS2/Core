using System;
using System.Collections.Generic;

namespace BExIS.Utils.NH.Querying
{
    public enum GenericOperation
    {
        Equals = 1,
        LessThan = 2,
        LessThanOrEqual = 3,
        GreaterThan = 4,
        GreaterThanOrEqual = 5,

        StartsWith = 6,
        Contains = 7,
        EndsWith = 8,

        Not = 9,
        Negate = 10,

        And = 11,
        Or = 12,
    }

    public enum SortDirection
    {
        Ascending,
        Descending,
    }

    public static class UnaryOperator
    {
        // the string value in the dictionary may need to be a tuple<enum, sql, sqlPattern>
        public static Dictionary<Operation, Tuple<string, string>> Translation = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Not,          Tuple.Create("NOT", "NOT ({0})") },
            { Operation.Negate,       Tuple.Create("-", "-({0})") },
        };

        public static Dictionary<Operation, Tuple<string, string>> TranslationToLINQ = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Not,          Tuple.Create("NOT", "!({0})") },
            { Operation.Negate,       Tuple.Create("-", "-({0})") },
        };

        public enum Operation
        {
            Not = GenericOperation.Not,
            Negate = GenericOperation.Negate,
        }
    }

    public static class BinaryOperator
    {
        public static Dictionary<Operation, Tuple<string, string>> Translation = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.And,                Tuple.Create("AND", "({0}) AND ({1})") },
            { Operation.Or,                 Tuple.Create("OR", "({0}) OR ({1})") },
        };

        public static Dictionary<Operation, Tuple<string, string>> TranslationToLINQ = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.And,                Tuple.Create("AND", "({0}) && ({1})") },
            { Operation.Or,                 Tuple.Create("OR", "({0}) || ({1})") },
        };

        public enum Operation
        {
            And = GenericOperation.And,
            Or = GenericOperation.Or,
        }
    }

    public static class NumberOperator
    {
        public static Dictionary<Operation, Tuple<string, string>> Translation = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Equals,             Tuple.Create("=", "({0}) = ({1})") },
            { Operation.LessThan,           Tuple.Create("<", "({0}) < ({1})") },
            { Operation.LessThanOrEqual,    Tuple.Create("<=", "({0}) <= ({1})") },
            { Operation.GreaterThan,        Tuple.Create(">", "({0}) > ({1})") },
            { Operation.GreaterThanOrEqual, Tuple.Create(">=", "({0}) >= ({1})") },
        };

        public static Dictionary<Operation, Tuple<string, string>> TranslationToLINQ = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Equals,             Tuple.Create("=", "{0} = {1}") },
            { Operation.LessThan,           Tuple.Create("<", "{0} < {1}") },
            { Operation.LessThanOrEqual,    Tuple.Create("<=", "{0} <= {1}") },
            { Operation.GreaterThan,        Tuple.Create(">", "{0} > {1}") },
            { Operation.GreaterThanOrEqual, Tuple.Create(">=", "{0} >= {1}") },
        };

        public enum Operation
        {
            Equals = GenericOperation.Equals,
            LessThan = GenericOperation.LessThan,
            LessThanOrEqual = GenericOperation.LessThanOrEqual,
            GreaterThan = GenericOperation.GreaterThan,
            GreaterThanOrEqual = GenericOperation.GreaterThanOrEqual,
        }
    }

    public static class StringOperator
    {
        public static Dictionary<Operation, Tuple<string, string>> Translation = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Equals,             Tuple.Create("=",  "LOWER({0}) = LOWER('{1}')") },
            { Operation.StartsWith,         Tuple.Create("ST", "({0}) ILIKE ('{1}%')") },
            { Operation.Contains,           Tuple.Create("ST", "({0}) ILIKE ('%{1}%')") },
            { Operation.EndsWith,           Tuple.Create("EW", "({0}) ILIKE ('%{1}')") },
        };

        public static Dictionary<Operation, Tuple<string, string>> TranslationToLINQ = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Equals,             Tuple.Create("=",  "{0}.ToString().ToLower() = \"{1}\"") },
            { Operation.StartsWith,         Tuple.Create("ST", "{0}.ToLower().Contains(\"{1}\")") },
            { Operation.Contains,           Tuple.Create("ST", "{0}.ToLower().Contains(\"{1}\")") },
            { Operation.EndsWith,           Tuple.Create("EW", "{0}.ToLower().Contains(\"{1}\")") }
        };

        public enum Operation
        {
            Equals = GenericOperation.Equals,
            StartsWith = GenericOperation.StartsWith,
            Contains = GenericOperation.Contains,
            EndsWith = GenericOperation.EndsWith,
        }
    }

    public static class DateTimeOperator
    {
        public static Dictionary<Operation, Tuple<string, string>> Translation = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Equals,             Tuple.Create("=", "({0}) = ('{1}')") },
            { Operation.LessThan,           Tuple.Create("<", "({0}) < ('{1}')") },
            { Operation.LessThanOrEqual,    Tuple.Create("<=", "({0}) <= ('{1}')") },
            { Operation.GreaterThan,        Tuple.Create(">", "({0}) > ('{1}')") },
            { Operation.GreaterThanOrEqual, Tuple.Create(">=", "({0}) >= ('{1}')") },
            { Operation.NotEquals, Tuple.Create("<>", "{0} <> {1}") }
        };

        public static Dictionary<Operation, Tuple<string, string>> TranslationToLINQ = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Equals,             Tuple.Create("=", "{0} = {1}") },
            { Operation.LessThan,           Tuple.Create("<", "{0} < {1} ") },
            { Operation.LessThanOrEqual,    Tuple.Create("<=", "{0} <= {1}") },
            { Operation.GreaterThan,        Tuple.Create(">", "{0} > {1}") },
            { Operation.GreaterThanOrEqual, Tuple.Create(">=", "{0} >= {1}") },
            { Operation.NotEquals, Tuple.Create("<>", "{0} <> {1}") }
        };

        public enum Operation
        {
            Equals = GenericOperation.Equals,
            LessThan = GenericOperation.LessThan,
            LessThanOrEqual = GenericOperation.LessThanOrEqual,
            GreaterThan = GenericOperation.GreaterThan,
            GreaterThanOrEqual = GenericOperation.GreaterThanOrEqual,
            NotEquals = GenericOperation.Not,
        }
    }

    public static class BooleanOperator
    {
        public static Dictionary<Operation, Tuple<string, string>> Translation = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Equals,             Tuple.Create("=", "({0}) = ({1})") },
        };

        public static Dictionary<Operation, Tuple<string, string>> TranslationLINQ = new Dictionary<Operation, Tuple<string, string>>()
        {
            { Operation.Equals,             Tuple.Create("=", "{0} = {1}") },
        };

        public enum Operation
        {
            Equals = GenericOperation.Equals,
        }
    }

    public enum DataType
    {
        String,
        Ineteger,
        DateTime,
        Double,
        Boolean,
    }
}