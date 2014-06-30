using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BExIS.Security.Entities.Objects
{
    public class DataPermission : Permission
    {
        // Entity
        public virtual string EntityName { get; set; }
        
        // Rights
        public virtual byte Rights { get; set; }

        // Statement
        public virtual string Property { get; set; }
        public virtual Comparator Comparator { get; set; }
        public virtual string Value { get; set; }

        public virtual string Equation
        {
            get { return Property + " " + GetEnumDescription(Comparator) + " " + Value.ToString(); }
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

    }

    public enum Comparator
    {
        [Description("=")]
        Equal,
        [Description("!=")]
        Unequal,
    }
}