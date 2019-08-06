using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Persistence.Api;
using BExIS.Dlm.Services.Helpers;
using System.Linq;
using Microsoft.ExtendedReflection.Metadata;
using BExIS.Dlm.Services.TypeSystem;

namespace BExIS.Dlm.Services.DataStructure
{  
    public class MissingValueManager: IDisposable
    {
        IUnitOfWork uow = null;
        public MissingValueManager()
        {
            uow = this.GetIsolatedUnitOfWork();
            this.Repo = uow.GetReadOnlyRepository<MissingValue>();        
        }

        private bool isDisposed = false;
        ~MissingValueManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (uow != null)
                        uow.Dispose();
                        isDisposed = true;
                }
            }
        }


        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<MissingValue> Repo { get; private set; }

        #endregion

        private string getPlaceholder(TypeCode typeCode, long variableId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MissingValue> repo = uow.GetRepository<MissingValue>();

                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return null;

                    case TypeCode.Char:
                        return null;

                    case TypeCode.String:
                        return null;

                    case TypeCode.Int16:
                        try
                        {
                            List<Int16> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToInt16(mv.Placeholder)).ToList();
                            Int16 temp = Int16.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp--;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Int32:
                        try
                        {
                            List<Int32> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToInt32(mv.Placeholder)).ToList();
                            Int32 temp = Int32.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp--;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Int64:
                        try
                        {
                            List<Int64> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToInt64(mv.Placeholder)).ToList();
                            Int64 temp = Int64.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp--;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt16:
                        try
                        {
                            List<UInt16> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToUInt16(mv.Placeholder)).ToList();
                            UInt16 temp = UInt16.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp--;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt32:
                        try
                        {
                            List<UInt32> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToUInt32(mv.Placeholder)).ToList();
                            UInt32 temp = UInt32.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp--;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt64:
                        try
                        {
                            List<UInt64> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToUInt64(mv.Placeholder)).ToList();
                            UInt64 temp = UInt64.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp--;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Double:
                        try
                        {
                            List<double> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToDouble(mv.Placeholder)).ToList();
                            double temp = Double.MaxValue - 0.1;
                            while (placeholders.Contains(temp))
                            {
                                temp -= (double)1.0;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Decimal:
                        try
                        {
                            List<decimal> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToDecimal(mv.Placeholder)).ToList();
                            decimal temp = Decimal.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp -= (decimal)1.0;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Single:
                        try
                        {
                            List<float> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToSingle(mv.Placeholder)).ToList();
                            float temp = Single.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp -= (float)1.0;
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.DateTime:
                        try
                        {
                            List<DateTime> placeholders = repo.Get().Where(mv => mv.Variable.Id.Equals(variableId)).Select(mv => Convert.ToDateTime(mv.Placeholder)).ToList();
                            DateTime temp = DateTime.MaxValue;
                            while (placeholders.Contains(temp))
                            {
                                temp = temp.AddHours(-1);
                            }
                            return temp.ToString();
                        }
                        catch
                        {
                            return null;
                        }
                }
            }
            return null;
        }

        public MissingValue Create(string displayName, string description, Variable variable, string placeholder = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(displayName));
            Contract.Requires(variable != null);

            Contract.Ensures(Contract.Result<MissingValue>() != null && Contract.Result<MissingValue>().Id >= 0);

            TypeCode typecode = new TypeCode();

            if (String.IsNullOrEmpty(placeholder))
            {
                foreach (DataTypeCode tc in Enum.GetValues(typeof(DataTypeCode)))
                {
                    if (tc.ToString() == variable.DataAttribute.DataType.SystemType)
                    {
                        typecode = (TypeCode)tc;
                        break;
                    }
                }

                placeholder = getPlaceholder(typecode, variable.Id);                
            }

            if (!String.IsNullOrEmpty(placeholder))
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<MissingValue> repo = uow.GetRepository<MissingValue>();

                    if (repo.Query(p => p.DisplayName.ToLower() == displayName.ToLower()).Count() <= 0)
                    {
                        MissingValue missingValue = new MissingValue()
                        {
                            DisplayName = displayName,
                            Placeholder = placeholder,
                            Description = description,
                            Variable = variable,
                        };
                        repo.Put(missingValue);
                        uow.Commit();
                        return (missingValue);
                    }
                    return null; // This should throw an exception instead.
                }
            }
            return null;
        }
    }
}
