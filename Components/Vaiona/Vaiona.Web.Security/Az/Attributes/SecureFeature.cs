using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Vaiona.Entities.Security;
using Vaiona.IoC;
using Vaiona.Persistence.Api;

namespace Vaiona.Web.Security.Az.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class SecureFeatureInfoAttribute : AuthorizeAttribute
    {
        public SecureFeatureInfoAttribute(string key = "", string parentKey = "", string dispalyName = "")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class SecureFeatureAttribute : SecureFeatureInfoAttribute
    {
        private IAuthorizationService azService; // to be injected by DI

        public string AccessRule { get; set; }
        public string FeatureKey { get; set; }
        public string ParentFeatureKey { get; set; }
        public bool FetchAccessRuleFromDB { get; set; }

        public string FullKey
        { get { return (string.Format("{0}.{1}", ParentFeatureKey, FeatureKey)); } }

        //private static MemoryCache ruleCache = new MemoryCache("RuleCache");

        public SecureFeatureAttribute()
            : this("", "", "", true)
        {
        }

        public SecureFeatureAttribute(string parentKey)
            : this("", "", parentKey, true)
        {
        }

        public SecureFeatureAttribute(string accessRule, bool fetchAccessRuleFromDB)
            : this(accessRule, "", "", fetchAccessRuleFromDB)
        {
        }

        public SecureFeatureAttribute(string accessRule, string featureKey = "", string parentActionKey = "", bool fetchAccessRuleFromDB = false)
        {
            azService = IoCFactory.Container.Resolve<IAuthorizationService>() as IAuthorizationService;
            AccessRule = accessRule;
            FeatureKey = featureKey;
            ParentFeatureKey = parentActionKey;
            FetchAccessRuleFromDB = fetchAccessRuleFromDB;
        }

        /// <summary>
        /// This is an authorization system for actions of an MVC application. in the security system all parts of action identification e.g. Area, Controller and Action
        /// are considered.
        /// If a controller does not belong to an area, then "Shell" is used as default.
        /// Although the default behavior of the system is to use Area, Controller and Action names to form the security hierarchy,
        /// developer can introduce specific names for actions (as security key) and their parents to establish a custom hierarchy with custom names.
        /// and also hierarchies with more than 3 levels are allowed
        /// Every level of features can have their own acces rules. i.e. Area of "Members" can be authorized just for "Admins".
        /// and Controller "Credit" under that area is secured for "Accountants" an so on.
        /// Security admin can set the access grant policy to Maximun, Minimum or Normal using the config key "AccessRuleMergeOption".
        /// Maximum access, fetchs all the access rules of the action and all its parents and join them ba an "OR", the same for
        /// Minimum but with "AND". in case of normal, only the direct access rule that is defined for the action applies.
        /// If there is no rule defined then the final Grant| Deny is defined by a configuration key named "GrantAccessOnNoRule".
        /// Finally the security admin is able to turn off or on the whole authorization system by setting the config key "SkipAuthorization".
        /// Currently this system uses RuleBasedAuthorization class that implements IAuthorizationService, so other developers can develop theirs if this class does not satisfy their needs.
        /// When an authorization request from a specific user for an action arrives
        /// 1: the access rule is fetched. a) from the SecureFeature attribute. b) from the database
        /// 2: the access rule itself is cached for later user. as it is user independent it gets cached in the application level with specific config based sliding invalidation policy "AutorizationRuleCacheTime".
        /// 3: access rule is checked against current user and its roles (access rule can contain roles and usernames + operators. usernames come with a leading "@" without quotes), and the result that is the access permission for that specific user
        /// is cached for later use and returned to the caller (Telerik menu here)
        /// 4: The permission cache is per user, so it is persisted in a session level variable and gets invalidated using a config based sliding policy "AutorizationResultCacheTime".
        /// Also all user level permissions get invalidated upon log-off and log-in.
        ///
        /// ps: On first run, in order to create database table(s), set the config key CreateDatabase to true, set the db connection info in the Vaiona.Model.Persistence.NH project's MsSql2008Dialect.hibernate.cfg.xml file
        /// also you need to config the app to use the ASP.NET Membership and Role providers
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            bool skipAuthorization = false;
            bool.TryParse(ConfigurationManager.AppSettings["SkipAuthorization"], out skipAuthorization);
            if (skipAuthorization) // no security check
                return;
            if (checkAllowAnonymous(filterContext))
                return;
            base.OnAuthorization(filterContext);
            if (filterContext.Result is HttpUnauthorizedResult) // base Az checks
                return;
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated) // see whether user is authenticated? normally this is done in the base, but here is double checked as it is possible to override the base behavior!
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            bool? result = null;
            init(filterContext);
            try
            {
                result = filterContext.HttpContext.Session.GetActionCache().Get(FullKey) as bool?;
            }
            catch { }
            if (result == null)
            {
                try
                {
                    fetchRule(filterContext);
                    var temp = FullKey;
                    result = azService.IsAuthorized(filterContext, AccessRule, FeatureKey, ParentFeatureKey);

                    int cacheTime = 600;
                    int.TryParse(ConfigurationManager.AppSettings["AutorizationResultCacheTime"], out cacheTime);
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.SlidingExpiration = new TimeSpan(0, 0, cacheTime);
                    filterContext.HttpContext.Session.GetActionCache().Add(FullKey, result, policy, null);
                }
                catch
                {
                    result = bool.Parse(ConfigurationManager.AppSettings["GrantAccessOnNoRule"]);
                }
            }
            if (!result.HasValue || result.Value == false)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        private bool checkAllowAnonymous(AuthorizationContext filterContext)
        {
            var methods = filterContext.ActionDescriptor.ControllerDescriptor.ControllerType.GetMethods().ToList()
                .Where(p => p.Name.Equals(filterContext.ActionDescriptor.ActionName)); //((filterContext.ActionDescriptor.ActionName);
            foreach (var method in methods) // method may have overloads
            {
                //if there is any secured feature overload, anonymous acces is denied on all overloads!
                if (method.GetCustomAttributes(typeof(SecureFeatureAttribute), true).Count() > 0)
                    return (false);
            }
            foreach (var method in methods) // method may have overloads
            {
                //if there is no any secured feature overload, then check for explicit AllowAnonymous
                if (method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Count() > 0)
                    return (true);
            }
            return (false);
        }

        //protected override bool AuthorizeCore(HttpContextBase httpContext)
        //{
        //    if (httpContext == null)
        //    {
        //        throw new ArgumentNullException("httpContext");
        //    }

        //    IPrincipal user = httpContext.User;
        //    if (!user.Identity.IsAuthenticated)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        private void init(AuthorizationContext filterContext)
        {
            FeatureKey = !string.IsNullOrWhiteSpace(FeatureKey) ? FeatureKey : filterContext.ActionDescriptor.ActionName;
            string areaName = "Shell";
            try
            {
                //areaName = filterContext.RouteData.Values["area"].ToString();
                areaName = filterContext.RouteData.DataTokens["area"].ToString();
                //areaName = !string.IsNullOrWhiteSpace(areaName) ? areaName : "Shell";
            }
            catch { }
            ParentFeatureKey = !string.IsNullOrWhiteSpace(ParentFeatureKey)
                                ? ParentFeatureKey
                                : string.Format("{0}.{1}", areaName, filterContext.ActionDescriptor.ControllerDescriptor.ControllerName);
        }

        private void fetchRule(AuthorizationContext filterContext)
        {
            if (!string.IsNullOrWhiteSpace(AccessRule) || FetchAccessRuleFromDB == false)
                return;
            string temp = FullKey;
            try
            {
                AccessRule = filterContext.HttpContext.Application.GetAccessRuleCache().Get(temp) as string;
            }
            catch { AccessRule = string.Empty; }
            if (!string.IsNullOrWhiteSpace(AccessRule))
                return;

            // extract all sub keys to fetch them from db
            List<string> subKeys = new List<string>();

            subKeys.Add(temp);
            do
            {
                if (temp.Contains("."))
                    temp = temp.Substring(0, temp.LastIndexOf("."));
                subKeys.Add(temp);
            }
            while (temp.Contains("."));
            subKeys.RemoveAll(p => string.IsNullOrWhiteSpace(p));

            //fetch access rules coreponsing to all subkeys
            IPersistenceManager persistenceManager = PersistenceFactory.GetPersistenceManager();

            List<AccessRuleEntity> rules;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IReadOnlyRepository<AccessRuleEntity> accessRuleRepo = uow.GetReadOnlyRepository<AccessRuleEntity>();
                rules = accessRuleRepo.Get(p => subKeys.Contains(p.SecurityKey) && p.SecurityObjectType == SecurityObjectType.Feature)
                                .OrderBy(p => p.SecurityKey.Length).ToList();
            }

            // compile the final access rules based on action hierarchy and rule merging policy
            AccessRule = string.Empty;
            AccessRuleMergeOption mergeOption = (AccessRuleMergeOption)Enum.Parse(typeof(AccessRuleMergeOption), ConfigurationManager.AppSettings["AccessRuleMergeOption"]);
            switch (mergeOption)
            {
                case AccessRuleMergeOption.MaximumRight:
                    foreach (var rule in rules.Where(p => !string.IsNullOrWhiteSpace(p.RuleBody)))
                    {
                        AccessRule = AccessRule + " | (" + rule.RuleBody + ")";
                    }
                    AccessRule = AccessRule.Trim(" | ".ToCharArray());

                    // creates nested parentheses
                    //AccessRule = rules.Where(p=> !string.IsNullOrWhiteSpace(p.RuleBody))
                    //                .Select(x => x.RuleBody)
                    //                .Aggregate((current, next) => '(' + current + ") | (" + next + ')');

                    break;

                case AccessRuleMergeOption.MinimumRight:
                    foreach (var rule in rules.Where(p => !string.IsNullOrWhiteSpace(p.RuleBody)))
                    {
                        AccessRule = AccessRule + " & (" + rule.RuleBody + ")";
                    }
                    AccessRule = AccessRule.Trim(" & ".ToCharArray());
                    break;

                case AccessRuleMergeOption.Normal:
                    AccessRule = rules.Last().RuleBody; // fetches more than required, but is the NH.Linq issue
                    break;

                default:
                    break;
            }
            if (!string.IsNullOrWhiteSpace(AccessRule))
            {
                int cacheTime = 600;
                int.TryParse(ConfigurationManager.AppSettings["AutorizationRuleCacheTime"], out cacheTime);
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.SlidingExpiration = new TimeSpan(0, 0, cacheTime);
                filterContext.HttpContext.Application.GetAccessRuleCache().Add(FullKey, AccessRule, policy, null);
            }
        }
    }

    //public class AllowAnonymousAttribute : Attribute
    //{
    //}
}