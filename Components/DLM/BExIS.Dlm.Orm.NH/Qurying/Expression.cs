using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dlm.Orm.NH.Qurying
{
    public abstract class ModelExpression
    {
        public abstract string ToSQL();
    }

    public class Field
    {
        public DataType DataType { get; set; }
        public string Name { get; set; }

    }

    public abstract class FilterExpression : ModelExpression
    {
        // nothing to do
    }

    public class FilterNumberItemExpression : FilterExpression
    {
        public Field Field { get; set; }
        public object Value { get; set; }
        public NumberOperator.Operation Operator { get; set; } 

        public override string ToSQL()
        {
            // a set of checks against the type and value should be done
            return string.Format(NumberOperator.Translation[this.Operator].Item2, this.Field.Name, this.Value);
        }

    }

    public class FilterStringItemExpression : FilterExpression
    {
        public Field Field { get; set; }
        public object Value { get; set; }
        public StringOperator.Operation Operator { get; set; }

        public override string ToSQL()
        {
            return string.Format(StringOperator.Translation[this.Operator].Item2, this.Field.Name, this.Value);
        }

    }

    public class FilterDateTimeItemExpression : FilterExpression
    {
        public Field Field { get; set; }
        public object Value { get; set; }
        public DateTimeOperator.Operation Operator { get; set; }

        public override string ToSQL()
        {
            // a set of checks against the type and value should be done
            return string.Format(DateTimeOperator.Translation[this.Operator].Item2, this.Field.Name, this.Value);
        }
    }

    public class FilterBooleanItemExpression : FilterExpression
    {
        public Field Field { get; set; }
        public object Value { get; set; }
        public BooleanOperator.Operation Operator { get; set; }

        public override string ToSQL()
        {
            // a set of checks against the type and value should be done
            return string.Format(BooleanOperator.Translation[this.Operator].Item2, this.Field.Name, this.Value);
        }

    }

    public class UnaryFilterExpression: FilterExpression
    {
        public FilterExpression Operand { get; set; }
        public UnaryOperator.Operation Operator { get; set; }

        public static UnaryFilterExpression Not(FilterExpression operand)
        {
            return new UnaryFilterExpression() { Operand = operand, Operator = UnaryOperator.Operation.Not };
        }

        public static UnaryFilterExpression Negate(FilterExpression operand)
        {
            return new UnaryFilterExpression() { Operand = operand, Operator = UnaryOperator.Operation.Negate };
        }

        public override string ToSQL()
        {
            return string.Format(UnaryOperator.Translation[this.Operator].Item2, this.Operand.ToSQL());
        }
    }

    public class BinaryFilterExpression : FilterExpression
    {
        public ModelExpression Left { get; set; }
        public ModelExpression Right { get; set; }
        public BinaryOperator.Operation Operator { get; set; }

        public static BinaryFilterExpression And(FilterExpression left, FilterExpression right)
        {
            return new BinaryFilterExpression() { Left = left, Right = right, Operator = BinaryOperator.Operation.And };
        }

        public static BinaryFilterExpression Or(ModelExpression left, ModelExpression right)
        {
            return new BinaryFilterExpression() { Left = left, Right = right, Operator = BinaryOperator.Operation.Or };
        }

        public override string ToSQL()
        {
            return string.Format(BinaryOperator.Translation[this.Operator].Item2, this.Left.ToSQL(), this.Right.ToSQL());
        }
    }

    public class OrderItemExpression : ModelExpression
    {
        public string FieldName { get; set; }
        public SortDirection Direction { get; set; }

        public OrderItemExpression(string fiedName, SortDirection direction = SortDirection.Ascending)
        {
            this.FieldName = fiedName;
            this.Direction = direction;
        }
        public override string ToSQL()
        {
            return string.Format("{0} {1}", this.FieldName, this.Direction == SortDirection.Ascending ? "ASC" : "DESC");
        }
    }

    public class OrderByExpression : ModelExpression
    {
        public OrderItemExpression this[int index] { get { return this.Items[index]; } }

        public List<OrderItemExpression> Items { get; set; }

        public OrderByExpression()
        {
            this.Items = new List<OrderItemExpression>();
        }
        public OrderByExpression(List<OrderItemExpression> items) : this()
        {
            this.Items = items;
        }

        public override string ToSQL()
        {
            return string.Join(", ", this.Items.Select(p=>p.ToSQL()));
        }

    }

    public class ProjectionItemExpression : ModelExpression
    {
        public string FieldName { get; set; }
        public string Computation { get; set; }
        public string Alias { get; set; }

        public override string ToSQL()
        {
            return string.Format("{0} AS {1}", this.FieldName, string.IsNullOrWhiteSpace(this.Alias) ? FieldName : Alias);
        }
    }

    public class ProjectionExpression : ModelExpression
    {
        public ProjectionItemExpression this[int index] { get { return this.Items[index]; } }

        public List<ProjectionItemExpression> Items { get; set; }

        public ProjectionExpression()
        {
            this.Items = new List<ProjectionItemExpression>();
        }

        public ProjectionExpression(List<ProjectionItemExpression> items): this()
        {
            this.Items = items;
        }
        public override string ToSQL()
        {
            return string.Join(", ", this.Items.Select(p => p.ToSQL()));
        }
    }
}
