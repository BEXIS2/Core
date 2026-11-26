using System;

namespace BEXIS2.DB.Entities.Common
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DomainSignatureAttribute : Attribute
    { }
}