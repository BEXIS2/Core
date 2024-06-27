using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace BExIS.UI.Helpers
{
    public static class BexisUIHelper
    {
        public static BexisUI BexisUI(this HtmlHelper html)
        {
            return new BexisUI();
        }
    }

    public class BexisUI
    {
        public List List()
        {
            return new List();
        }

        public TextBox TextBox()
        {
            return new TextBox();
        }

        public Slider Slider()
        {
            return new Slider();
        }
    }

    public class TextBox : BexisTag
    {
        public TextBox()
        {
            // div, input, span

            //div
            TagDictionary.Add(BexisUITagType.div, new Dictionary<string, object>());

            mergeAttribute(BexisUITagType.div, "class", "bx-input-container");

            //input
            TagDictionary.Add(BexisUITagType.input, new Dictionary<string, object>());
            mergeAttribute(BexisUITagType.input, "type", "text");
            mergeAttribute(BexisUITagType.input, "class", "bx-input");

            //span
            TagDictionary.Add(BexisUITagType.span, new Dictionary<string, object>());
            mergeAttribute(BexisUITagType.span, "class", "bx-input-icon");

            //setAttribute(_inputTagBuilder, "data-val", "true");
            //setAttribute(_inputTagBuilder, "data-val-required", "The Name field is required");
        }

        public TextBox Name(string value)
        {
            addAttribute(BexisUITagType.input, "name", value);
            addAttribute(BexisUITagType.input, "id", convertToValidId(value));
            return this;
        }

        public TextBox AddCssClass(string value)
        {
            mergeAttribute(BexisUITagType.div, "class", value);
            return this;
        }

        public TextBox AddHtmlAttributes(Dictionary<string, object> attributes)
        {
            mergeAttributes(BexisUITagType.div, attributes);
            return this;
        }

        public TextBox Value(string value)
        {
            mergeAttribute(BexisUITagType.input, "value", value);
            return this;
        }

        public TextBox Icon(string value)
        {
            mergeAttribute(BexisUITagType.span, "class", value);
            mergeAttribute(BexisUITagType.span, "class", "bx");

            return this;
        }

        public TextBox ClientEvent(string eventName, string functionName)
        {
            addAttribute(BexisUITagType.input, eventName.ToLower(), functionName + "(event)");
            return this;
        }

        public MvcHtmlString Render()
        {
            string temp = "";

            //create input container

            _tagBuilder = new TagBuilder(BexisUITagType.input.ToString());
            _tagBuilder.MergeAttributes(_tagDictionary[BexisUITagType.input]);

            temp += _tagBuilder.ToString(TagRenderMode.Normal);

            //create icon container

            _tagBuilder = new TagBuilder(BexisUITagType.span.ToString());
            _tagBuilder.MergeAttributes(_tagDictionary[BexisUITagType.span]);

            temp += _tagBuilder.ToString(TagRenderMode.Normal);

            //create Container
            _tagBuilder = new TagBuilder(BexisUITagType.div.ToString());
            _tagBuilder.MergeAttributes(_tagDictionary[BexisUITagType.div]);
            _tagBuilder.InnerHtml += temp;

            return MvcHtmlString.Create(_tagBuilder.ToString(TagRenderMode.Normal));
        }
    }

    public class Slider : BexisTag
    {
        public Slider()
        {
            TagDictionary.Add(BexisUITagType.input, new Dictionary<string, object>());
            addAttribute(BexisUITagType.input, "type", "text");
        }

        public Slider Name(string value)
        {
            addAttribute(BexisUITagType.input, "name", value);
            addAttribute(BexisUITagType.input, "id", convertToValidId(value));
            return this;
        }

        public Slider AddCssClass(string value)
        {
            addAttribute(BexisUITagType.input, "class", value);
            return this;
        }

        public Slider AddHtmlAttributes(Dictionary<string, object> attributes)
        {
            addAttributes(BexisUITagType.input, attributes);
            return this;
        }

        public Slider Min(object value)
        {
            addAttribute(BexisUITagType.input, "data-slider-min", value);

            return this;
        }

        public Slider Max(object value)
        {
            addAttribute(BexisUITagType.input, "data-slider-max", value);
            return this;
        }

        public Slider Steps(object value)
        {
            addAttribute(BexisUITagType.input, "data-slider-step", value);
            return this;
        }

        public Slider ClientEvent(string eventName, string functionName)
        {
            addAttribute(BexisUITagType.input, eventName.ToLower(), functionName + "(event)");
            return this;
        }

        public Slider Value(object value)
        {
            addAttribute(BexisUITagType.input, "data-slider-value", value.ToString());

            return this;
        }

        public Slider Value(object min, object max)
        {
            addAttribute(BexisUITagType.input, "data-slider-value", "[" + min.ToString() + "," + max.ToString() + "]");
            return this;
        }

        public MvcHtmlString Render()
        {
            if (TagDictionary.Count > 0)
            {
                string result = "";

                // Input
                _tagBuilder = new TagBuilder(BexisUITagType.input.ToString());
                _tagBuilder.AddCssClass("bx-slider");
                _tagBuilder.MergeAttributes<string, object>(TagDictionary[BexisUITagType.input]);

                result += _tagBuilder.ToString(TagRenderMode.Normal);
                _tagBuilder = new TagBuilder(BexisUITagType.script.ToString());
                _tagBuilder.InnerHtml = " var slider = new Slider('#" + TagDictionary[BexisUITagType.input]["id"] + "', {}); ";

                result += _tagBuilder.ToString(TagRenderMode.Normal);

                return MvcHtmlString.Create(result);
            }
            else
                return MvcHtmlString.Create("not able to render.");
        }
    }

    public class List : BexisTag
    {
        private SelectList _dataSource;

        public List()
        {
            _dataSource = null;

            TagDictionary.Add(BexisUITagType.a, new Dictionary<string, object>());

            TagDictionary.Add(BexisUITagType.ul, new Dictionary<string, object>());
            addAttribute(BexisUITagType.ul, "class", "bx-list");
        }

        public List Name(string value)
        {
            addAttribute(BexisUITagType.ul, "name", value);
            addAttribute(BexisUITagType.ul, "id", convertToValidId(value));
            return this;
        }

        public List AddCssClass(string value)
        {
            addAttribute(BexisUITagType.ul, "class", value);
            return this;
        }

        public List AddHtmlAttributes(Dictionary<string, object> attributes)
        {
            addAttributes(BexisUITagType.ul, attributes);
            return this;
        }

        public List Data(SelectList list)
        {
            _dataSource = list;
            return this;
        }

        public List Action(string actionName, string controllerName, RouteValueDictionary routeValues, string parameterName)
        {
            addAttribute(BexisUITagType.a, "actionName", actionName);
            addAttribute(BexisUITagType.a, "controllerName", controllerName);
            addAttribute(BexisUITagType.a, "routeValues", routeValues);
            addAttribute(BexisUITagType.a, "parameterName", parameterName);

            return this;
        }

        public MvcHtmlString Render()
        {
            string temp = "";

            foreach (var kvp in _dataSource)
            {
                _tagBuilder = new TagBuilder(BexisUITagType.li.ToString());
                _tagBuilder.MergeAttribute("name", kvp.Text);
                _tagBuilder.MergeAttribute("id", kvp.Text);
                _tagBuilder.MergeAttribute("text", kvp.Text);
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    _tagBuilder.MergeAttribute("value", kvp.Text);
                    _tagBuilder.InnerHtml = kvp.Text;

                    _tagBuilder.InnerHtml += generateAction(
                        TagDictionary[BexisUITagType.a]["actionName"].ToString(),
                        TagDictionary[BexisUITagType.a]["controllerName"].ToString(),
                        TagDictionary[BexisUITagType.a]["parameterName"].ToString(),
                        kvp.Text,
                        (RouteValueDictionary)TagDictionary[BexisUITagType.a]["routeValues"]
                        );
                }
                else
                {
                    _tagBuilder.MergeAttribute("value", kvp.Value);
                    _tagBuilder.InnerHtml = kvp.Value;
                    _tagBuilder.InnerHtml += generateAction(
                       TagDictionary[BexisUITagType.a]["actionName"].ToString(),
                       TagDictionary[BexisUITagType.a]["controllerName"].ToString(),
                       TagDictionary[BexisUITagType.a]["parameterName"].ToString(),
                       kvp.Value,
                       (RouteValueDictionary)TagDictionary[BexisUITagType.a]["routeValues"]
                       );
                }

                if (kvp.Selected)
                {
                    _tagBuilder.MergeAttribute("selected", "selected");
                }

                temp += _tagBuilder.ToString(TagRenderMode.Normal);
            }

            _tagBuilder = new TagBuilder(BexisUITagType.ul.ToString());
            _tagBuilder.MergeAttributes(TagDictionary[BexisUITagType.ul]);
            _tagBuilder.InnerHtml += temp;

            return MvcHtmlString.Create(_tagBuilder.ToString(TagRenderMode.Normal));
        }

        private string generateAction(string actionName, string controllername, string parameter, object value, RouteValueDictionary routeValues)
        {
            UrlHelper urlHelper = new UrlHelper();

            routeValues.Add(parameter, value);
            routeValues.Add("Value", value);
            return urlHelper.Action(actionName, controllername, routeValues);
        }
    }

    public abstract class BexisTag : IBexistag
    {
        public Dictionary<BexisUITagType, Dictionary<string, object>> _tagDictionary;

        protected TagBuilder _tagBuilder;

        protected void addAttribute(BexisUITagType tagType, string key, object value)
        {
            //tag exist
            if (TagDictionary.ContainsKey(tagType))
            {
                // get HtmlAttributeDic
                Dictionary<string, object> currentHtmlAttributeDic = TagDictionary[tagType];
                // htmlAttribute exist
                if (currentHtmlAttributeDic.ContainsKey(key))
                    currentHtmlAttributeDic[key] = value;
                // htmlAttribute not exist
                else
                    currentHtmlAttributeDic.Add(key, value);
            }
            //create tag dic add htmlattribute to the HtmlAttributeDic
            else
            {
                Dictionary<string, object> currentHtmlAttributeDic = new Dictionary<string, object>();
                currentHtmlAttributeDic.Add(key, value);
                TagDictionary.Add(tagType, currentHtmlAttributeDic);
            }
        }

        protected void addAttributes(BexisUITagType tagType, Dictionary<string, object> htmlAttributes)
        {
            //tag exist
            if (TagDictionary.ContainsKey(tagType))
            {
                // get HtmlAttributeDic
                Dictionary<string, object> currentHtmlAttributeDic = TagDictionary[tagType];

                foreach (KeyValuePair<string, object> kvp in htmlAttributes)
                {
                    // htmlAttribute exist
                    if (currentHtmlAttributeDic.ContainsKey(kvp.Key))
                        currentHtmlAttributeDic[kvp.Key] = kvp.Value;
                    // htmlAttribute not exist
                    else
                        currentHtmlAttributeDic.Add(kvp.Key, kvp.Value);
                }
            }
            //create tag dic add htmlattribute to the HtmlAttributeDic
            else
            {
                Dictionary<string, object> currentHtmlAttributeDic = new Dictionary<string, object>();
                TagDictionary.Add(tagType, htmlAttributes);
            }
        }

        protected void mergeAttribute(BexisUITagType tagType, string key, object value)
        {
            //tag exist
            if (TagDictionary.ContainsKey(tagType))
            {
                // get HtmlAttributeDic
                Dictionary<string, object> currentHtmlAttributeDic = TagDictionary[tagType];
                // htmlAttribute exist
                if (currentHtmlAttributeDic.ContainsKey(key))
                    currentHtmlAttributeDic[key] += " " + value;
                // htmlAttribute not exist
                else
                    currentHtmlAttributeDic.Add(key, value);
            }
            //create tag dic add htmlattribute to the HtmlAttributeDic
            else
            {
                Dictionary<string, object> currentHtmlAttributeDic = new Dictionary<string, object>();
                currentHtmlAttributeDic.Add(key, value);
                TagDictionary.Add(tagType, currentHtmlAttributeDic);
            }
        }

        protected void mergeAttributes(BexisUITagType tagType, Dictionary<string, object> htmlAttributes)
        {
            //tag exist
            if (TagDictionary.ContainsKey(tagType))
            {
                // get HtmlAttributeDic
                Dictionary<string, object> currentHtmlAttributeDic = TagDictionary[tagType];

                foreach (KeyValuePair<string, object> kvp in htmlAttributes)
                {
                    // htmlAttribute exist
                    if (currentHtmlAttributeDic.ContainsKey(kvp.Key))
                        currentHtmlAttributeDic[kvp.Key] = " " + kvp.Value;
                    // htmlAttribute not exist
                    else
                        currentHtmlAttributeDic.Add(kvp.Key, kvp.Value);
                }
            }
            //create tag dic add htmlattribute to the HtmlAttributeDic
            else
            {
                Dictionary<string, object> currentHtmlAttributeDic = new Dictionary<string, object>();
                TagDictionary.Add(tagType, htmlAttributes);
            }
        }

        public Dictionary<BexisUITagType, Dictionary<string, object>> TagDictionary
        {
            get
            {
                if (_tagDictionary == null)
                {
                    _tagDictionary = new Dictionary<BexisUITagType, Dictionary<string, object>>();
                    return _tagDictionary;
                }
                else return _tagDictionary;
            }
        }

        #region Helper Functions

        protected string convertToValidId(string name)
        {
            name = name.Replace('\\', '_');
            name = name.Replace(',', '_');
            name = name.Replace(';', '_');
            name = name.Replace('/', '_');

            return name;
        }

        #endregion Helper Functions

        public MvcHtmlString Render()
        {
            return MvcHtmlString.Create("");
        }
    }

    public enum BexisUITagType
    {
        a,
        button,
        div,
        input,
        li,
        option,
        select,
        script,
        span,
        ul
    }

    public interface IBexistag
    {
        Dictionary<BexisUITagType, Dictionary<string, object>> TagDictionary { get; }

        MvcHtmlString Render();
    }
}