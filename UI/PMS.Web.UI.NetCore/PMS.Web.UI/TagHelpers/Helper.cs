using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace PMS.Web.UI.TagHelpers
{
    public static class Helper
    {
        public static IHtmlContent WrapElementsWithDiv(List<IHtmlContent> elements, string classValue)
        {
            TagBuilder div = new TagBuilder("div");
            div.AddCssClass(classValue);
            foreach (IHtmlContent element in elements)
            {
                div.InnerHtml.AppendHtml(element);
            }

            return div;
        }


        public static IHtmlContent WrapElementsWithDiv(IHtmlContent element, string classValue)
        {
            return WrapElementsWithContainer("div", element, classValue);
        }

        public static IHtmlContent WrapElementsWithContainer(string containerTag, IHtmlContent element, string classValue)
        {
            TagBuilder container = new TagBuilder(containerTag);
            container.AddCssClass(classValue);
            container.InnerHtml.AppendHtml(element);            
            return container;
        }

        public static IHtmlContent CreateElement(string tagName, TagHelperAttributeList tagAttributes, string htmlContent)
        {
            TagBuilder element = new TagBuilder(tagName);
            
            if (tagAttributes.Any())
            {
                
                foreach (TagHelperAttribute attribute in tagAttributes)
                {
                    if (attribute.Value!=null)
                        element.Attributes[attribute.Name] = attribute.Value.ToString();
                }
            }
            if (!string.IsNullOrWhiteSpace(htmlContent))
                element.InnerHtml.AppendHtml(htmlContent);
            return element;
        }

        public static TagHelperOutput CreateTagHelperOutput(string tagName)
        {
            return CreateTagHelperOutput(tagName, null);
        }
        public static TagHelperOutput CreateTagHelperOutput(string tagName, TagHelperAttributeList tagAttributes)
        {

            if (tagAttributes == null)
                tagAttributes = new TagHelperAttributeList();
            return new TagHelperOutput(
                tagName: tagName,
                attributes: tagAttributes,
                getChildContentAsync: (s, t) =>
                {
                    return Task.Factory.StartNew<TagHelperContent>(
                            () => new DefaultTagHelperContent());
                }
            );
        }


        public static string GetVisiblityStyle(bool hidden)
        {
            return hidden ? "display:none" : string.Empty;
        }

        public static string GetSizeCssClass(int xs, int sm, int md, int lg, int xl)
        {
            string css = string.Empty;
            if (xs > 0 && xs <= 12)
                css += $"col-xs-{xs} ";
            if (sm > 0 && sm <= 12)
                css += $"col-sm-{sm} ";
            if (md > 0 && md <= 12)
                css += $"col-md-{md} ";
            if (lg > 0 && lg <= 12)
                css += $"col-lg-{lg} ";
            if (xl > 0 && xl <= 12)
                css += $"col-xl-{xl} ";

            return css.Trim();
        }

        public static int CssClassIndex(this string allClass, string findClass)
        {
            int lengthClass = 0;
            return CssClassIndex(allClass, findClass, out lengthClass);
        }
        public static int CssClassIndex(this string allClass,string findClass,out int lengthClass)
        {
            allClass = allClass.Trim();
            findClass = findClass.Trim();
            string findClassX = $"{findClass} ";
            lengthClass = findClass.Length;

            if (allClass.StartsWith(findClassX, StringComparison.OrdinalIgnoreCase))
                return 0;
            

            findClassX = $" {findClass}";
            if (allClass.EndsWith(findClassX, StringComparison.OrdinalIgnoreCase))            
                lengthClass = findClass.Length;
                
            findClassX = $" {findClass} ";
            int index = allClass.IndexOf(findClassX, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)            
                return index++;


            return -1;
        }

        public static string InsertClass(this string allClass, string newClass)
        {
            if (string.IsNullOrWhiteSpace(newClass))
                return allClass;
            return $"{newClass.Trim()} {allClass.Trim()}";
        }

        public static string InsertClassBefore(this string allClass, string findClass, string newClass)
        {
            if (string.IsNullOrWhiteSpace(newClass))
                return allClass;

            int findClassIndex = allClass.CssClassIndex(findClass);
            if (findClassIndex < 0)
                return allClass.InsertClass(newClass);

            return $"{allClass.Substring(0, findClassIndex)} {newClass} {allClass.Substring(findClassIndex, allClass.Length - findClassIndex + 1)}";
        }

        public static string AppendClassAfter(this string allClass, string findClass, string newClass)
        {
            if (string.IsNullOrWhiteSpace(newClass))
                return allClass;

            int findClassLengh = findClass.Length;
            int findClassIndex = allClass.CssClassIndex(findClass);
            if (findClassIndex < 0)
                return allClass.AppendClass(newClass);

            return $"{allClass.Substring(0, findClassIndex + findClassLengh)} {newClass} {allClass.Substring(findClassIndex+findClassLengh, allClass.Length - findClassIndex - findClassLengh + 1)}";
        }

        public static string AppendClass(this string allClass, string newClass)
        {
            if (string.IsNullOrWhiteSpace(newClass))
                return allClass;
            return $"{allClass.Trim()} {newClass.Trim()}";
        }

        public static string GetHtmlContent(this IHtmlContent htmlContent)
        {
            using (var writer = new System.IO.StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        public static string ToHtml(this bool value)
        {
            return value ? "true" : "false";
        }


    }
}
