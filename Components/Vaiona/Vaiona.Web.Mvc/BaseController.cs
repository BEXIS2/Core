using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Vaiona.Web.Mvc
{
    public abstract class BaseController : Controller
    {
        private IList<IDisposable> disposables;

        protected BaseController()
        {
            disposables = new List<IDisposable>();
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            base.OnActionExecuted(filterContext);
        }

        [Obsolete("Use Try Finally pattern to dispose the disposables in the finally block.", true)]
        public IList<IDisposable> Disposables
        { get { return this.disposables; } }
    }
}