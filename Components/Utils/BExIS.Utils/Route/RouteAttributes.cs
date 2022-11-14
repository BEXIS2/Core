using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;

namespace BExIS.Utils.Route
{
    public class GetRouteAttribute : MethodConstraintedRouteAttribute
    {
        public GetRouteAttribute(string template) : base(template ?? "", HttpMethod.Get)
        {
        }
    }

    public class PostRouteAttribute : MethodConstraintedRouteAttribute
    {
        public PostRouteAttribute(string template) : base(template ?? "", HttpMethod.Post)
        {
        }
    }

    public class PutRouteAttribute : MethodConstraintedRouteAttribute
    {
        public PutRouteAttribute(string template) : base(template ?? "", HttpMethod.Put)
        {
        }
    }

    public class DeleteRouteAttribute : MethodConstraintedRouteAttribute
    {
        public DeleteRouteAttribute(string template) : base(template ?? "", HttpMethod.Delete)
        {
        }
    }

    public class MethodConstraintedRouteAttribute : RouteFactoryAttribute
    {
        public MethodConstraintedRouteAttribute(string template, HttpMethod method)
            : base(template)
        {
            Method = method;
        }

        public HttpMethod Method
        {
            get;
            private set;
        }

        public override IDictionary<string, object> Constraints
        {
            get
            {
                var constraints = new HttpRouteValueDictionary();
                constraints.Add("method", new MethodConstraint(Method));
                return constraints;
            }
        }
    }

    public class MethodConstraint : IHttpRouteConstraint
    {
        public HttpMethod Method { get; private set; }

        public MethodConstraint(HttpMethod method)
        {
            Method = method;
        }

        public bool Match(HttpRequestMessage request,
                          IHttpRoute route,
                          string parameterName,
                          IDictionary<string, object> values,
                          HttpRouteDirection routeDirection)
        {
            return request.Method == Method;
        }
    }
}