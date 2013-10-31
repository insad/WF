using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace WF.Sample.Helpers
{
    /// <summary>
    /// web帮助类
    /// </summary>
    public static class WebPageHelpers
    {
        public static void PropagateSection(this WebPageBase page, string sectionName)
        {
            if (page.IsSectionDefined(sectionName))
            {
                page.DefineSection(sectionName, delegate { page.Write(page.RenderSection(sectionName)); });
            }
        }

        public static void PropagateSections(this WebPageBase page, params string[] sections)
        {
            foreach (string s in sections)
                PropagateSection(page, s);
        }
    }

    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString LabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                                                                Expression<Func<TModel, TProperty>> ex,
                                                                Func<object, HelperResult> template)
        {
            string memberName = ExpressionHelper.GetExpressionText(ex);
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(ex, new ViewDataDictionary<TModel>());

            var label = new TagBuilder("label");
            label.Attributes["for"] =
                TagBuilder.CreateSanitizedId(htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(memberName));
            label.InnerHtml = string.Format(
                "{0} {1}",
                (metadata.DisplayName ?? metadata.PropertyName ?? memberName),
                template(null).ToHtmlString()
                );
            return MvcHtmlString.Create(label.ToString());
        }
    }
}