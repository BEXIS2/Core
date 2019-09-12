using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.IO.Transform.Validation.ValueCheck
{
    public class MissingValueCheck : IValueCheck
    {
        #region private

        private ValueType appliedTo = new ValueType();
        private string name = "";
        private string dataType = "";

        #endregion private

        #region get

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public ValueType AppliedTo
        {
            get
            {
                return appliedTo;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DataType
        {
            get { return dataType; }
        }

        #endregion get

        public IEnumerable<MissingValue> MissingValues { get; set; }

        public object Execute(string value, int row)
        {
            if (MissingValues.Any(m => m.DisplayName.Equals(value)))
            {
                return MissingValues.Where(m => m.DisplayName.Equals(value)).FirstOrDefault().Placeholder;
            }

            return null;
        }

        public MissingValueCheck(string name, string dataType, IEnumerable<MissingValue> missingValues)
        {
            this.appliedTo = ValueType.All;
            this.name = name;
            this.dataType = dataType;
            this.MissingValues = missingValues;
        }
    }
}