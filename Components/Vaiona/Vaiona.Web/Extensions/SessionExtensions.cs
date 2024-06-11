using System;
using System.Globalization;
using System.Web;
using System.Web.SessionState;
using Vaiona.Model.MTnt;
using Vaiona.Web.Helpers;

namespace Vaiona.Web.Extensions
{
    public static class SessionExtensions
    {
        public static void SetDomainUser(this HttpSessionState session)
        {
            throw new NotImplementedException();
        }

        public static void ApplyCulture(this HttpSessionState session, string cultureId)
        {
            session.ApplyCulture(new CultureInfo(cultureId, true));
        }

        public static void ApplyCulture(this HttpSessionState session, CultureInfo culture)
        {
            GlobalizationHelper.SetSessionCulture(culture);
        }

        public static CultureInfo GetCurrentCulture(this HttpSessionState session)
        {
            return (GlobalizationHelper.GetCurrentCulture());
        }

        public static void SetTenant(this HttpSessionState session, Tenant tenant)
        {
            session["CurrentTenant"] = tenant;
        }

        public static Tenant GetTenant(this HttpSessionState session)
        {
            object tenantObj = session["CurrentTenant"];
            if (tenantObj != null)
                return (Tenant)tenantObj;
            return null;
        }

        public static Tenant GetTenant(this HttpSessionStateBase session)
        {
            object tenantObj = session["CurrentTenant"];
            if (tenantObj != null)
                return (Tenant)tenantObj;
            return null;
        }
    }
}