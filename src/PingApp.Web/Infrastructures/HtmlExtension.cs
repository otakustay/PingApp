using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PingApp.Entity;

namespace PingApp.Web.Infrastructures {
    public static class HtmlExtension {
        private static readonly DateTime unitChangeDate = new DateTime(2011, 11, 18);

        #region 表单

        public static IHtmlString InputTextBox(this HtmlHelper helper,
            string name, string value = null, int maxLength = 0, bool autoFocus = false, int tabIndex = 0) {
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "text");
            input.MergeAttribute("id", name);
            input.MergeAttribute("name", name);
            input.MergeAttribute("class", "text");
            if (!String.IsNullOrEmpty(value)) {
                input.MergeAttribute("value", value);
            }
            if (maxLength > 0) {
                input.MergeAttribute("maxlength", maxLength.ToString());
            }
            if (autoFocus) {
                input.MergeAttribute("autofocus", "autofocus");
            }
            if (tabIndex > 0) {
                input.MergeAttribute("tabindex", tabIndex.ToString());
            }

            return helper.Raw(input.ToString());
        }

        public static IHtmlString InputPassword(this HtmlHelper helper, string name, bool submittable = true, int tabIndex = 0) {
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "password");
            input.MergeAttribute("id", name);
            if (submittable) {
                input.MergeAttribute("name", name);
            }
            input.MergeAttribute("class", "text");
            if (tabIndex > 0) {
                input.MergeAttribute("tabindex", tabIndex.ToString());
            }

            return helper.Raw(input.ToString());
        }

        public static IHtmlString InputCheckBox(this HtmlHelper helper, string name, string label, bool selected, int tabIndex = 0) {
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "checkbox");
            input.MergeAttribute("id", name);
            input.MergeAttribute("name", name);
            input.MergeAttribute("value", "true");
            if (tabIndex > 0) {
                input.MergeAttribute("tabindex", tabIndex.ToString());
            }
            if (selected) {
                input.MergeAttribute("checked", "checked");
            }
            string labelHtml = String.Format("<label for=\"{0}\">{1}</label>", name, label);

            return helper.Raw(input.ToString() + labelHtml);
        }

        public static IHtmlString InputRadioBox(this HtmlHelper helper,
            string id, string name, object value, string label, bool selected, int tabIndex = 0) {
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "radio");
            input.MergeAttribute("id", id);
            input.MergeAttribute("name", name);
            input.MergeAttribute("value", value.ToString());
            if (tabIndex > 0) {
                input.MergeAttribute("tabindex", tabIndex.ToString());
            }
            if (selected) {
                input.MergeAttribute("checked", "checked");
            }
            string labelHtml = String.Format("<label for=\"{0}\">{1}</label>", id, label);

            return helper.Raw(input.ToString() + labelHtml);
        }

        public static IHtmlString InputSubmit(this HtmlHelper helper, string text, int tabIndex = 0) {
            TagBuilder input = new TagBuilder("input");
            input.MergeAttribute("type", "submit");
            input.MergeAttribute("id", "submit");
            input.MergeAttribute("value", text);
            input.MergeAttribute("class", "form-submit");
            if (tabIndex > 0) {
                input.MergeAttribute("tabindex", tabIndex.ToString());
            }

            return helper.Raw(input.ToString());
        }

        public static IHtmlString TitleLabel(this HtmlHelper helper, string target, string title) {
            string html = String.Format(
                "<label for=\"{0}\" class=\"title\">{1}</label>", target, title + "：");
            return helper.Raw(html);
        }

        public static IHtmlString ValidationLabel(this HtmlHelper helper, string target, string message) {
            string html = String.Format(
                "<label for=\"{0}\" class=\"validation-ready\">{1}</label>", target, message);
            return helper.Raw(html);
        }

        public static IHtmlString Option(this HtmlHelper helper, string value, string text, bool selected) {
            TagBuilder option = new TagBuilder("option");
            option.MergeAttribute("value", value);
            if (selected) {
                option.MergeAttribute("selected", "selected");
            }
            option.SetInnerText(text);

            return helper.Raw(option.ToString());
        }

        #endregion

        #region 条件过滤

        public static IHtmlString FilterLink(this HtmlHelper helper, string url, bool on, string text) {
            TagBuilder anchor = new TagBuilder("a");
            anchor.MergeAttribute("href", url);
            if (on) {
                anchor.MergeAttribute("class", "on");
            }
            anchor.SetInnerText(text);

            return helper.Raw(anchor.ToString());
        }

        public static IHtmlString FilterSorter(this HtmlHelper helper, string url, bool on, string text, string order) {
            TagBuilder anchor = new TagBuilder("a");
            anchor.MergeAttribute("href", url);
            anchor.InnerHtml = text;
            if (on) {
                anchor.MergeAttribute("class", "on");
                anchor.InnerHtml += String.Format(
                    "<span class=\"order {0}\">{1}</span>",
                    order == "desc" ? "desc" : "asc",
                    order == "desc" ? "降序" : "升序"
                );
            }

            return helper.Raw(anchor.ToString());
        }

        #endregion

        #region 文字相关

        public static IHtmlString PrettyDate(this HtmlHelper helper, DateTime time) {
            DateTime now = DateTime.Now;
            TimeSpan span = now - time;
            string output;
            if (span.Days == 0) {
                if (span.Hours == 0) {
                    output = Math.Max(span.Minutes, 1) + "分钟前";
                }
                else {
                    output = span.Hours + "小时前";
                }
            }
            else if (span.Days < 30) {
                output = span.Days + "天前";
            }
            else {
                output = "<span class=\"trival\">很久以前</span>";
            }

            return helper.Raw(output);
        }

        public static IHtmlString AppRatingCount(this HtmlHelper helper, AppBrief app) {
            string output;
            if (app.AverageUserRatingForCurrentVersion.HasValue) {
                output = app.AverageUserRatingForCurrentVersion + "(" + app.UserRatingCountForCurrentVersion + ")";
            }
            else {
                output = "<span class=\"trival\">未评分</span>"; ;
            }

            return helper.Raw(output);
        }

        public static IHtmlString AppDeviceType(this HtmlHelper helper, DeviceType type) {
            switch (type) {
                case DeviceType.None:
                    return helper.Raw("<span class=\"trival\">无</span>");
                case DeviceType.IPhone:
                    return helper.Raw("iPhone");
                case DeviceType.IPad:
                    return helper.Raw("iPad");
                case DeviceType.Universal:
                    return helper.Raw("<span class=\"trival\">iPhone+iPad</span>通用");
                default:
                    return helper.Raw("<span class=\"trival\">未知</span>");
            }
        }

        public static IHtmlString UpdateType(this HtmlHelper helper, AppUpdateType type) {
            string text;
            string className;
            switch (type) {
                case AppUpdateType.New:
                    text = "新近上架";
                    className = "new-added";
                    break;
                case AppUpdateType.AddToPing:
                    text = "加入系统";
                    className = "new-added";
                    break;
                case AppUpdateType.PriceIncrease:
                    text = "价格上涨";
                    className = "price-increase";
                    break;
                case AppUpdateType.PriceDecrease:
                    text = "特价销售";
                    className = "price-decrease";
                    break;
                case AppUpdateType.PriceFree:
                    text = "限时免费";
                    className = "price-free";
                    break;
                case AppUpdateType.NewRelease:
                    text = "版本更新";
                    className = "new-release";
                    break;
                case AppUpdateType.Revoke:
                    text = "应用下架";
                    className = "off-sale";
                    break;
                default:
                    text = "未知状态";
                    className = "unknown";
                    break;
            }
            return helper.Raw("<em class=\"app-update-entry " + className + "\">" + text + "</em>");
        }

        public static IHtmlString UpdateDescrioption(this HtmlHelper helper, AppUpdate update) {
            if (update.Type == AppUpdateType.New || update.Type == AppUpdateType.AddToPing || update.Type == AppUpdateType.Revoke) {
                return helper.Raw(update.OldValue);
            }
            else if (update.Type == AppUpdateType.NewRelease) {
                return helper.Raw(update.OldValue + " -> " + update.NewValue);
            }
            else {
                char unit = update.Time < unitChangeDate ? '$' : '￥';
                return helper.Raw(unit + update.OldValue + " -> " + unit + update.NewValue);
            }
        }

        public static string FileSize(this HtmlHelper helper, int bytes) {
            string[] units = { "B", "KB", "MB", "GB" };
            float value = bytes;
            int index = 0;
            while (value > 1024) {
                value = value / 1024;
                index++;
            }
            return String.Format("{0:F2}{1}", value, units[index]);
        }

        #endregion
    }
}