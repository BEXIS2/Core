namespace BExIS.Dcm.CreateDatasetWizard
{
    //ToDO goto bexis.utils.data.metadatastructure

    //public class UsageHelper
    //{
    //    public static BaseUsage GetMetadataAttributeUsageById(long Id)
    //    {
    //        BaseUsage usage = new BaseUsage();

    //        MetadataPackageManager mpm = new MetadataPackageManager();

    //        var q = from p in mpm.MetadataPackageRepo.Get()
    //                from u in p.MetadataAttributeUsages
    //                where u.Id == Id // && p.Id.Equals(parentId)
    //                select u;

    //        return q.FirstOrDefault();

    //    }

    //    public static BaseUsage GetMetadataCompoundAttributeUsageById(long Id)
    //    {
    //        BaseUsage usage = new BaseUsage();

    //        MetadataAttributeManager mam = new MetadataAttributeManager();

    //        var x = from c in mam.MetadataCompoundAttributeRepo.Get()
    //                from u in c.Self.MetadataNestedAttributeUsages
    //                where u.Id == Id //&& c.Id.Equals(parentId)
    //                select u;

    //        return x.FirstOrDefault();

    //    }

    //    public static BaseUsage GetSimpleUsageById(BaseUsage parent, long Id)
    //    {
    //        BaseUsage usage = new BaseUsage();

    //        if (parent is MetadataPackageUsage)
    //        {
    //            MetadataPackageManager mpm = new MetadataPackageManager();

    //            var q = from p in mpm.MetadataPackageRepo.Get()
    //                    from u in p.MetadataAttributeUsages
    //                    where p.Id.Equals(parent.Id) && u.Id == Id && u.MetadataAttribute.Self is MetadataSimpleAttribute
    //                    select u;

    //            if (q != null && q.ToList().Count > 0)
    //            {
    //                return q.FirstOrDefault();
    //            }
    //            else return null;
    //        }

    //        else
    //        if (parent is MetadataNestedAttributeUsage)
    //        {
    //            MetadataAttributeManager mam = new MetadataAttributeManager();

    //            MetadataNestedAttributeUsage pUsage = (MetadataNestedAttributeUsage)parent;

    //            MetadataCompoundAttribute mca = mam.MetadataCompoundAttributeRepo.Get(pUsage.Member.Self.Id);

    //            var x = from nestedUsage in mca.MetadataNestedAttributeUsages
    //                    where nestedUsage.Id == Id && nestedUsage.Member.Self is MetadataSimpleAttribute
    //                    select nestedUsage;

    //            //var x = from c in mam.MetadataCompoundAttributeRepo.Get()
    //            //        from u in c.Self.MetadataNestedAttributeUsages
    //            //        where u.Id.Equals(parent.Id) && u.Member.Self.Id == Id && u.Member.Self is MetadataSimpleAttribute
    //            //        select u;

    //            return x.FirstOrDefault();
    //        }
    //        else if (parent is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)parent;
    //            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
    //            {
    //                MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mau.MetadataAttribute.Self;
    //                return mca.MetadataNestedAttributeUsages.Where(m => m.Id.Equals(Id)).FirstOrDefault();
    //            }

    //        }

    //        return null;
    //    }

    //    /// <summary>
    //    /// Search in the packageusages
    //    /// </summary>
    //    /// <param name="Id"></param>
    //    /// <returns></returns>
    //    public static BaseUsage GetUsageById(long Id)
    //    {
    //        BaseUsage usage = new BaseUsage();

    //        MetadataStructureManager msm = new MetadataStructureManager();

    //        var q = from p in msm.PackageUsageRepo.Get()
    //                where p.Id == Id
    //                select p;

    //        if (q != null && q.ToList().Count > 0)
    //        {
    //            return q.FirstOrDefault();
    //        }

    //        return null;

    //        //else
    //        //{
    //        //    MetadataAttributeManager mam = new MetadataAttributeManager();

    //        //    var x = from c in mam.MetadataCompoundAttributeRepo.Get()
    //        //            from u in c.Self.MetadataNestedAttributeUsages
    //        //            where u.Id == Id
    //        //            select u;

    //        //    return x.FirstOrDefault();
    //        //}
    //    }

    //    public static List<BaseUsage> GetChildren(BaseUsage usage)
    //    {
    //        List<BaseUsage> temp = new List<BaseUsage>();

    //        if (usage is MetadataPackageUsage)
    //        {
    //            MetadataPackageUsage mpu = (MetadataPackageUsage)usage;

    //            foreach (BaseUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
    //            {
    //                temp.Add(childUsage);
    //            }
    //        }

    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
    //                {
    //                    temp.Add(childUsage);
    //                }
    //            }
    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            if (mnau.Member.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
    //                {
    //                    temp.Add(childUsage);
    //                }
    //            }
    //        }

    //        return temp;
    //    }

    //    public static List<BaseUsage> GetCompoundChildrens(BaseUsage usage)
    //    {
    //        List<BaseUsage> temp = new List<BaseUsage>();

    //        if (usage is MetadataPackageUsage)
    //        {
    //            MetadataPackageUsage mpu = (MetadataPackageUsage)usage;

    //            foreach (BaseUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
    //            {
    //                if (IsCompound(childUsage))
    //                    temp.Add(childUsage);
    //            }
    //        }

    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
    //                {
    //                    if (IsCompound(childUsage))
    //                        temp.Add(childUsage);
    //                }
    //            }
    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            if (mnau.Member.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
    //                {
    //                    if (IsCompound(childUsage))
    //                        temp.Add(childUsage);
    //                }
    //            }
    //        }

    //        return temp;
    //    }

    //    public static List<BaseUsage> GetSimpleChildrens(BaseUsage usage)
    //    {
    //        List<BaseUsage> temp = new List<BaseUsage>();

    //        if (usage is MetadataPackageUsage)
    //        {
    //            MetadataPackageUsage mpu = (MetadataPackageUsage)usage;

    //            foreach (BaseUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
    //            {
    //                if (IsSimple(childUsage))
    //                    temp.Add(childUsage);
    //            }
    //        }

    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
    //                {
    //                    if (IsSimple(childUsage))
    //                        temp.Add(childUsage);
    //                }
    //            }
    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            if (mnau.Member.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
    //                {
    //                    if (IsSimple(childUsage))
    //                        temp.Add(childUsage);
    //                }
    //            }
    //        }

    //        return temp;
    //    }

    //    private static bool IsCompound(BaseUsage usage)
    //    {
    //        MetadataAttributeManager mam = new MetadataAttributeManager();

    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            MetadataAttribute ma = mam.MetadataAttributeRepo.Get(mau.MetadataAttribute.Id);

    //            if (ma.Self is MetadataCompoundAttribute) return true;

    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            MetadataAttribute ma = mam.MetadataAttributeRepo.Get(mnau.Member.Id);
    //            if (ma.Self is MetadataCompoundAttribute) return true;
    //        }

    //        return false;
    //    }

    //    public static bool IsSimple(BaseUsage usage)
    //    {
    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            if (mau.MetadataAttribute.Self is MetadataSimpleAttribute) return true;

    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            if (mnau.Member.Self is MetadataSimpleAttribute) return true;
    //        }

    //        return false;
    //    }

    //    public static string GetNameOfType(BaseUsage usage)
    //    {
    //        if (usage is MetadataPackageUsage)
    //        {
    //            MetadataPackageUsage mpu = (MetadataPackageUsage)usage;
    //            return mpu.MetadataPackage.Name;
    //        }

    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            return mau.MetadataAttribute.Name;
    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            return mnau.Member.Name;
    //        }

    //        return "";
    //    }

    //    public static long GetIdOfType(BaseUsage usage)
    //    {
    //        if (usage is MetadataPackageUsage)
    //        {
    //            MetadataPackageUsage mpu = (MetadataPackageUsage)usage;
    //            return mpu.MetadataPackage.Id;
    //        }

    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            return mau.MetadataAttribute.Id;
    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            return mnau.Member.Id;
    //        }

    //        return 0;
    //    }

    //    public static bool HasUsagesWithSimpleType(BaseUsage usage)
    //    {
    //        if (usage is MetadataPackageUsage)
    //        {
    //            MetadataPackageUsage mpu = (MetadataPackageUsage)usage;

    //            foreach (BaseUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
    //            {
    //                if (IsSimple(childUsage)) return true;
    //            }
    //        }

    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
    //                {
    //                    if (IsSimple(childUsage)) return true;
    //                }
    //            }
    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            if (mnau.Member.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
    //                {
    //                    if (IsSimple(childUsage)) return true;
    //                }
    //            }
    //        }

    //        return false;
    //    }

    //    public static bool IsRequired(BaseUsage usage)
    //    {
    //        if (usage.MinCardinality > 0)
    //            return true;
    //        else
    //            return false;
    //    }

    //    public static bool HasRequiredSimpleTypes(BaseUsage usage)
    //    {
    //        if (usage is MetadataPackageUsage)
    //        {
    //            MetadataPackageUsage mpu = (MetadataPackageUsage)usage;

    //            foreach (BaseUsage childUsage in mpu.MetadataPackage.MetadataAttributeUsages)
    //            {
    //                if (IsSimple(childUsage) && childUsage.MinCardinality > 0) return true;
    //            }
    //        }

    //        if (usage is MetadataAttributeUsage)
    //        {
    //            MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
    //            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mau.MetadataAttribute.Self).MetadataNestedAttributeUsages)
    //                {
    //                    if (IsSimple(childUsage) && childUsage.MinCardinality > 0) return true;
    //                }
    //            }
    //        }

    //        if (usage is MetadataNestedAttributeUsage)
    //        {
    //            MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
    //            if (mnau.Member.Self is MetadataCompoundAttribute)
    //            {
    //                foreach (BaseUsage childUsage in ((MetadataCompoundAttribute)mnau.Member.Self).MetadataNestedAttributeUsages)
    //                {
    //                    if (IsSimple(childUsage) && childUsage.MinCardinality > 0) return true;
    //                }
    //            }
    //        }

    //        return false;
    //    }
    //}
}