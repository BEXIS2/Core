using BExIS.Dlm.Entities.DataStructure;
using System.Collections.Generic;

namespace BExIS.IO.Transform.Validation.ValueCheck
{
    public class MissingValueCheck : IValueCheck
    {
        #region private

        private ValueType appliedTo = new ValueType();
        private string name = "";
        private string dataType = "";
        private Dictionary<string, string> missingValues = new Dictionary<string, string>();

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

        public object Execute(string value, int row)
        {
            if (missingValues.ContainsKey(value))
            {
                return missingValues[value];
            }

            return null;
        }

        public MissingValueCheck(string name, string dataType, IEnumerable<MissingValue> missingValues)
        {
            this.appliedTo = ValueType.All;
            this.name = name;
            this.dataType = dataType;
            this.missingValues = new Dictionary<string, string>();

            if (missingValues != null)
            {
                foreach (var missingValue in missingValues)
                {
                    if(!string.IsNullOrEmpty(missingValue.DisplayName))
                    this.missingValues.Add(missingValue.DisplayName, missingValue.Placeholder);
                }
            }
        }
    }
}