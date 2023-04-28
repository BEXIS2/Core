using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Utils.Data.MetadataStructure
{
    public class MetadataStructureUsageHelper
    {
        public BaseUsage GetMetadataAttributeUsageById(long Id)
        {

            using (MetadataPackageManager mpm = new MetadataPackageManager())
            {
                var q = from p in mpm.MetadataPackageRepo.Get()
                        from u in p.MetadataAttributeUsages
                        where u.Id == Id // && p.Id.Equals(parentId)
                        select u;

                return q.FirstOrDefault();
            }
        }

        public List<BaseUsage> GetChildren(long usageId, Type type)
        {
            using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
            {

                List<BaseUsage> temp = new List<BaseUsage>();

                if (type.Equals(typeof(MetadataPackageUsage)))
                {
                    MetadataPackageUsage mpu = unitOfWork.GetReadOnlyRepository<MetadataPackageUsage>().Get(usageId);
                    var mauRepo = unitOfWork.GetReadOnlyRepository<MetadataAttributeUsage>();

                    foreach (MetadataAttributeUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
                    {
                        mauRepo.LoadIfNot((childUsage).MetadataAttribute);
                        temp.Add(childUsage);
                    }
                }

                if (type.Equals(typeof(MetadataAttributeUsage)))
                {
                    MetadataAttributeUsage mau = unitOfWork.GetReadOnlyRepository<MetadataAttributeUsage>().Get(usageId);
                    if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                    {
                        var mnauRepo = unitOfWork.GetReadOnlyRepository<MetadataNestedAttributeUsage>();

                        foreach (MetadataNestedAttributeUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
                        {
                            mnauRepo.LoadIfNot(childUsage.Member);
                            temp.Add(childUsage);
                        }
                    }
                }

                if (type.Equals(typeof(MetadataNestedAttributeUsage)))
                {
                    MetadataNestedAttributeUsage mnau = unitOfWork.GetReadOnlyRepository<MetadataNestedAttributeUsage>().Get(usageId);

                    if (mnau.Member.Self is MetadataCompoundAttribute)
                    {
                        var mnauRepo = unitOfWork.GetReadOnlyRepository<MetadataNestedAttributeUsage>();

                        foreach (MetadataNestedAttributeUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
                        {
                            temp.Add(childUsage);
                        }
                    }
                }

                return temp;
            }
        }

        public ICollection<MetadataParameterUsage> GetParameters(long usageId, Type type)
        {
            using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
            {

                if (type.Equals(typeof(MetadataPackageUsage)))
                {
                    MetadataPackageUsage mpu = unitOfWork.GetReadOnlyRepository<MetadataPackageUsage>().Get(usageId);
                }

                if (type.Equals(typeof(MetadataAttributeUsage)))
                {
                    MetadataAttributeUsage mau = unitOfWork.GetReadOnlyRepository<MetadataAttributeUsage>().Get(usageId);
                    return mau.MetadataAttribute.MetadataParameterUsages;
                    
                }

                if (type.Equals(typeof(MetadataNestedAttributeUsage)))
                {
                    MetadataNestedAttributeUsage mnau = unitOfWork.GetReadOnlyRepository<MetadataNestedAttributeUsage>().Get(usageId);
                    return mnau.Member.MetadataParameterUsages;

                }

                return null;
            }
        }

        public List<BaseUsage> GetCompoundChildrens(long usageId, Type type)
        {
            using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
            {
                List<BaseUsage> temp = new List<BaseUsage>();
                BaseUsage usage = loadUsage(usageId, type);

                if (type.Equals(typeof(MetadataPackageUsage)))
                {
                    MetadataPackageUsage mpu = (MetadataPackageUsage)usage;
                    var mauRepo = unitOfWork.GetReadOnlyRepository<MetadataAttributeUsage>();

                    foreach (MetadataAttributeUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
                    {
                        if (IsCompound(childUsage))
                        {
                            mauRepo.LoadIfNot(childUsage.MetadataAttribute);
                            temp.Add(childUsage);
                        }
                    }
                }

                if (type.Equals(typeof(MetadataAttributeUsage)))
                {
                    MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
                    if (mau.MetadataAttribute.Self.GetType().Equals(typeof(MetadataCompoundAttribute)))
                    {
                        var mnauRepo = unitOfWork.GetReadOnlyRepository<MetadataNestedAttributeUsage>();

                        foreach (MetadataNestedAttributeUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
                        {
                            if (IsCompound(childUsage))
                            {
                                mnauRepo.LoadIfNot(childUsage.Member);
                                temp.Add(childUsage);
                            }
                        }
                    }
                }

                if (type.Equals(typeof(MetadataNestedAttributeUsage)))
                {
                    MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;

                    if (mnau.Member.Self.GetType().Equals(typeof(MetadataCompoundAttribute)))
                    {
                        var mnauRepo = unitOfWork.GetReadOnlyRepository<MetadataNestedAttributeUsage>();

                        foreach (MetadataNestedAttributeUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
                        {
                            if (IsCompound(childUsage))
                            {
                                mnauRepo.LoadIfNot(childUsage.Member);
                                temp.Add(childUsage);
                            }
                        }
                    }
                }

                return temp;
            }
        }

        public List<BaseUsage> GetSimpleChildrens(BaseUsage usage)
        {
            using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
            {
                List<BaseUsage> temp = new List<BaseUsage>();

                if (usage is MetadataPackageUsage)
                {
                    MetadataPackageUsage mpu = (MetadataPackageUsage)usage;
                    var mauRepo = unitOfWork.GetReadOnlyRepository<MetadataAttributeUsage>();

                    foreach (MetadataAttributeUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
                    {
                        if (IsSimple(childUsage))
                        {
                            mauRepo.LoadIfNot(childUsage.MetadataAttribute);
                            temp.Add(childUsage);
                        }
                    }
                }

                if (usage is MetadataAttributeUsage)
                {
                    MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
                    if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                    {
                        var mnauRepo = unitOfWork.GetReadOnlyRepository<MetadataNestedAttributeUsage>();

                        foreach (MetadataNestedAttributeUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
                        {
                            if (IsSimple(childUsage))
                            {
                                mnauRepo.LoadIfNot(childUsage.Member);
                                temp.Add(childUsage);

                            }
                        }
                    }
                }

                if (usage is MetadataNestedAttributeUsage)
                {
                    MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                    if (mnau.Member.Self is MetadataCompoundAttribute)
                    {
                        var mnauRepo = unitOfWork.GetReadOnlyRepository<MetadataNestedAttributeUsage>();

                        foreach (MetadataNestedAttributeUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
                        {
                            if (IsSimple(childUsage))
                            {
                                mnauRepo.LoadIfNot(childUsage.Member);
                                temp.Add(childUsage);
                            }
                        }
                    }
                }

                return temp;
            }
        }

        private bool IsCompound(BaseUsage usage)
        {
            //using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
            //{
                MetadataAttribute ma = null;

                if (usage is MetadataAttributeUsage)
                {
                    MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
                    ma = mau.MetadataAttribute;//unitOfWork.GetReadOnlyRepository<MetadataAttribute>().Get(mau.MetadataAttribute.Id);
                }

                if (usage is MetadataNestedAttributeUsage)
                {
                    MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                ma = mnau.Member; //unitOfWork.GetReadOnlyRepository<MetadataAttribute>().Get(mnau.Member.Id);
                }

                if (ma != null && ma.Self is MetadataCompoundAttribute) return true;

                return false;
            //}
        }

        public bool IsSimple(BaseUsage usage)
        {
            //using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
            //{
                MetadataAttribute ma = null;

                if (usage is MetadataAttributeUsage)
                {
                    MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
                    ma = mau.MetadataAttribute; //unitOfWork.GetReadOnlyRepository<MetadataAttribute>().Get(mau.MetadataAttribute.Id);
                }

                if (usage is MetadataNestedAttributeUsage)
                {
                    MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                    ma = mnau.Member;//unitOfWork.GetReadOnlyRepository<MetadataAttribute>().Get(mnau.Member.Id);
                }

                if (ma != null && ma.Self is MetadataSimpleAttribute) return true;

                return false;
            //}
        }

        public string GetNameOfType(BaseUsage usage)
        {

            if (usage is MetadataPackageUsage)
            {
                MetadataPackageUsage mpu = (MetadataPackageUsage)usage;
                return mpu.MetadataPackage.Name;
            }

            if (usage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
                return mau.MetadataAttribute.Name;
            }

            if (usage is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                return mnau.Member.Name;
            }

            return "";
        }

        public long GetIdOfType(BaseUsage usage)
        {

            if (usage is MetadataPackageUsage)
            {
                MetadataPackageUsage mpu = (MetadataPackageUsage)usage;
                return mpu.MetadataPackage.Id;
            }

            if (usage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
                return mau.MetadataAttribute.Id;
            }

            if (usage is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                return mnau.Member.Id;
            }

            return 0;
        }

        public bool HasUsagesWithSimpleType(long usageId, Type type)
        {
            BaseUsage usage = loadUsage(usageId, type);

            if (type.Equals(typeof(MetadataPackageUsage)))
            {

                MetadataPackageUsage mpu = this.GetUnitOfWork().GetReadOnlyRepository<MetadataPackageUsage>().Get(usageId);

                foreach (BaseUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
                {
                    if (IsSimple(childUsage)) return true;
                }
            }

            if (type.Equals(typeof(MetadataAttributeUsage)))
            {
                MetadataAttributeUsage mau = this.GetUnitOfWork().GetReadOnlyRepository<MetadataAttributeUsage>().Get(usageId); ;

                if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                {
                    foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
                    {
                        if (IsSimple(childUsage)) return true;
                    }
                }
            }

            if (type.Equals(typeof(MetadataNestedAttributeUsage)))
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                if (mnau.Member.Self is MetadataCompoundAttribute)
                {
                    foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
                    {
                        if (IsSimple(childUsage)) return true;
                    }
                }
            }

            return false;
        }

        public bool IsRequired(BaseUsage usage)
        {
            if (usage.MinCardinality > 0)
                return true;
            else
                return false;
        }

        public bool HasRequiredSimpleTypes(BaseUsage usage)
        {
            if (usage is MetadataPackageUsage)
            {
                MetadataPackageUsage mpu = (MetadataPackageUsage)usage;

                foreach (BaseUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
                {
                    if (IsSimple(childUsage) && childUsage.MinCardinality > 0) return true;
                }
            }

            if (usage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
                if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                {
                    foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
                    {
                        if (IsSimple(childUsage) && childUsage.MinCardinality > 0) return true;
                    }
                }
            }

            if (usage is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                if (mnau.Member.Self is MetadataCompoundAttribute)
                {
                    foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
                    {
                        if (IsSimple(childUsage) && childUsage.MinCardinality > 0) return true;
                    }
                }
            }

            return false;
        }

        private BaseUsage loadUsage(long Id, Type type)
        {
            if (type.Equals(typeof(MetadataAttributeUsage)))
                return this.GetUnitOfWork().GetReadOnlyRepository<MetadataAttributeUsage>().Get(Id);
            if (type.Equals(typeof(MetadataNestedAttributeUsage)))
                return this.GetUnitOfWork().GetReadOnlyRepository<MetadataNestedAttributeUsage>().Get(Id);
            if (type.Equals(typeof(MetadataPackageUsage)))
                return this.GetUnitOfWork().GetReadOnlyRepository<MetadataPackageUsage>().Get(Id);

            return null;
        }
    }
}
