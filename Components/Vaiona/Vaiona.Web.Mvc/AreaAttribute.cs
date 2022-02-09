using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Vaiona.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AreaAttribute: Attribute
    {
        public Type Registration { get; set; }

        public AreaAttribute(Type registration)
        {
            Registration = registration;
        }
    }
}
