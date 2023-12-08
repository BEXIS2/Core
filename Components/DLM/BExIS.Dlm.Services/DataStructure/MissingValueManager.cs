using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public class MissingValueManager : IDisposable
    {
        private IUnitOfWork uow = null;

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

        #endregion Data Readers

        public string getPlaceholder(TypeCode typeCode, long variableId, string format = "")
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MissingValue> repo = uow.GetRepository<MissingValue>();
                IRepository<VariableInstance> varRepo = uow.GetRepository<VariableInstance>();
                var variable = varRepo.Get(variableId);

                switch (typeCode)
                {
                    case TypeCode.Int16:
                        try
                        {
                            List<short> placeholders = variable.MissingValues.Select(mv=> Convert.ToInt16(mv.Placeholder)).ToList();
                            short temp = short.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > short.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Int32:
                        try
                        {
                            List<Int32> placeholders = variable.MissingValues.Select(mv => Convert.ToInt32(mv.Placeholder)).ToList();

                            int temp = int.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > int.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Int64:
                        try
                        {
                            List<long> placeholders = variable.MissingValues.Select(mv => Convert.ToInt64(mv.Placeholder)).ToList();
                            long temp = long.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > long.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt16:
                        try
                        {
                            List<ushort> placeholders = variable.MissingValues.Select(mv => Convert.ToUInt16(mv.Placeholder)).ToList();
                            ushort temp = ushort.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > ushort.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt32:
                        try
                        {
                            List<uint> placeholders = variable.MissingValues.Select(mv => Convert.ToUInt32(mv.Placeholder)).ToList();

                            uint temp = uint.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > uint.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt64:
                        try
                        {
                            List<ulong> placeholders = variable.MissingValues.Select(mv => Convert.ToUInt64(mv.Placeholder)).ToList();

                            ulong temp = ulong.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > ulong.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Double:
                        try
                        {
                            List<string> placeholders = variable.MissingValues.Select(mv => mv.Placeholder).ToList();

                            double temp = double.MaxValue / 10 - 1.0;
                            while (placeholders.Contains(temp.ToString(format)) && temp > double.MinValue / 10 + 1.0)
                            {
                                temp -= temp / 1E+14;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Decimal:
                        try
                        {
                            List<string> placeholders = variable.MissingValues.Select(mv => mv.Placeholder).ToList();

                            decimal temp = decimal.MaxValue - (decimal)1.0;
                            while (placeholders.Contains(temp.ToString(format)) && temp > decimal.MinValue + (decimal)1.0)
                            {
                                temp -= (decimal)1.0;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Single:
                        try
                        {
                            List<float> placeholders = variable.MissingValues.Select(mv => Convert.ToSingle(mv.Placeholder)).ToList();

                            float temp = float.MaxValue - (float)1.0;
                            while (placeholders.Contains(temp) && temp > float.MinValue + (float)1.0)
                            {
                                temp -= (float)1.0;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.DateTime:
                        try
                        {
                            List<string> placeholders = variable.MissingValues.Select(mv => mv.Placeholder).ToList();

                            DateTime temp = DateTime.MaxValue.AddHours(-1);
                            while (placeholders.Contains(temp.ToString(format)))
                            {
                                temp = temp.AddHours(-1);
                                temp = temp.AddYears(-1); //Reduce also by 1 year to be able to distinguish placeholder also after application of display pattern e.g. YYYY-MM-DD
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.String:
                        try
                        {
                            int temp = DateTime.Now.GetHashCode();
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    default:
                        return null;
                }
            }
        }

        //with this funktion you can check if the Placeholder you want to use can be used
        public bool ValidatePlaceholder(TypeCode typeCode, string placeholder, long variableId, long missingvalueId = 0)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MissingValue> repo = uow.GetRepository<MissingValue>();
                IRepository<VariableInstance> varRepo = uow.GetRepository<VariableInstance>();
                var variable = varRepo.Get(variableId);

                List<MissingValue> missingValues = variable.MissingValues!=null? variable.MissingValues.ToList(): new List<MissingValue>();

                switch (typeCode)
                {
                    case TypeCode.Int16:
                        try
                        {
                            short temp = 0;
                            if (short.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToInt16(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Int32:
                        try
                        {
                            int temp = 0;
                            if (int.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToInt32(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Int64:
                        try
                        {
                            long temp = 0;
                            if (long.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToInt64(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.UInt16:
                        try
                        {
                            ushort temp = 0;
                            if (ushort.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToUInt16(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.UInt32:
                        try
                        {
                            uint temp = 0;
                            if (uint.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToUInt32(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.UInt64:
                        try
                        {
                            ulong temp = 0;
                            if (ulong.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToUInt64(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Double:
                        try
                        {
                            double temp = (double)0.0;
                            if (double.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToDouble(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Decimal:
                        try
                        {
                            decimal temp = (decimal)0.0;
                            if (decimal.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToDecimal(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Single:
                        try
                        {
                            float temp = (float)0.0;
                            if (float.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToSingle(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.DateTime:
                        try
                        {
                            DateTime temp = new DateTime();
                            if (DateTime.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && placeholder == mv.Placeholder)
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.String:
                        try
                        {
                            foreach (MissingValue mv in missingValues)
                            {
                                if (mv.Id != missingvalueId && placeholder == mv.Placeholder)
                                    return false;
                            }
                            return true;
                        }
                        catch
                        {
                            return false;
                        }

                    default:
                        return false;
                }
            }
        }

        public MissingValue Create(string displayName, string description, Variable variable, string placeholder = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(displayName));
            Contract.Requires(variable != null);

            Contract.Ensures(Contract.Result<MissingValue>() != null && Contract.Result<MissingValue>().Id >= 0);

            TypeCode typecode = new TypeCode();

            foreach (DataTypeCode tc in Enum.GetValues(typeof(DataTypeCode)))
            {
                if (tc.ToString() == variable.DataType.SystemType)
                {
                    typecode = (TypeCode)tc;
                    break;
                }
            }

            if (String.IsNullOrEmpty(placeholder))
            {
                placeholder = getPlaceholder(typecode, variable.Id);
            }

            if (!String.IsNullOrEmpty(placeholder) && ValidatePlaceholder(typecode, placeholder, variable.Id))
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<MissingValue> repo = uow.GetRepository<MissingValue>();

                    MissingValue missingValue = new MissingValue()
                    {
                        DisplayName = displayName,
                        Placeholder = placeholder
                    };
                    repo.Put(missingValue);
                    uow.Commit();
                    return (missingValue);
                }
            }
            return null;
        }

        public bool Delete(MissingValue entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MissingValue> repo = uow.GetRepository<MissingValue>();

                entity = repo.Reload(entity);

                //delete the unit
                repo.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public MissingValue Update(MissingValue entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<MissingValue>() != null && Contract.Result<MissingValue>().Id >= 0, "No entity is persisted!");

            

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> varRepo = uow.GetRepository<VariableInstance>();
                var variable = varRepo.Query(v=>v.MissingValues.Contains(entity)).FirstOrDefault();

                TypeCode typecode = new TypeCode();

                foreach (DataTypeCode tc in Enum.GetValues(typeof(DataTypeCode)))
                {
                    if (tc.ToString() == variable.DataType.SystemType)
                    {
                        typecode = (TypeCode)tc;
                        break;
                    }
                }

                if (String.IsNullOrEmpty(entity.Placeholder))
                {
                    entity.Placeholder = getPlaceholder(typecode, variable.Id);
                }

                if (!String.IsNullOrEmpty(entity.Placeholder) && ValidatePlaceholder(typecode, entity.Placeholder, variable.Id, entity.Id))
                {
                    IRepository<MissingValue> repo = uow.GetRepository<MissingValue>();
                    repo.Merge(entity);
                    var merged = repo.Get(entity.Id);
                    repo.Put(merged);
                    uow.Commit();
                    return (entity);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}