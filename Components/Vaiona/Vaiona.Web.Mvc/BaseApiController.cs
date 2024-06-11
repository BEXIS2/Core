using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Vaiona.Web.Mvc
{
    public abstract class BaseApiController : ApiController
    {
        protected IList<IDisposable> Disposables;

        protected BaseApiController()
        {
            Disposables = new List<IDisposable>();
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var disposable in Disposables)
            {
                disposable.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}