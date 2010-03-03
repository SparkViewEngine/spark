// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
namespace Castle.MonoRail.Views.Spark
{
    using Castle.MonoRail.Framework.Helpers;

    public static class HelperExtensions
    {
        public static string BuildQueryString(this AbstractHelper helper, System.Object parameters)
        {
            return helper.BuildQueryString(new ModelDictionary(parameters));
        }

        public static string LinkToFunction(this AjaxHelper helper, System.String innerContent, System.String functionCodeOrName, System.Object attributes)
        {
            return helper.LinkToFunction(innerContent, functionCodeOrName, new ModelDictionary(attributes));
        }
        public static string LinkToFunction(this AjaxHelper helper, System.String innerContent, System.String functionCodeOrName, System.String confirm, System.Object attributes)
        {
            return helper.LinkToFunction(innerContent, functionCodeOrName, confirm, new ModelDictionary(attributes));
        }
        public static string ButtonToFunction(this AjaxHelper helper, System.String innerContent, System.String functionCodeOrName, System.Object attributes)
        {
            return helper.ButtonToFunction(innerContent, functionCodeOrName, new ModelDictionary(attributes));
        }
        public static string ButtonToRemote(this AjaxHelper helper, System.String innerContent, System.String url, System.Object options)
        {
            return helper.ButtonToRemote(innerContent, url, new ModelDictionary(options));
        }
        public static string ButtonToRemote(this AjaxHelper helper, System.String innerContent, System.String url, System.Object options, System.Object htmloptions)
        {
            return helper.ButtonToRemote(innerContent, url, new ModelDictionary(options), new ModelDictionary(htmloptions));
        }
        public static string LinkToRemote(this AjaxHelper helper, System.String innerContent, System.String url, System.Object options)
        {
            return helper.LinkToRemote(innerContent, url, new ModelDictionary(options));
        }
        public static string LinkToRemote(this AjaxHelper helper, System.String innerContent, System.String confirm, System.String url, System.Object options)
        {
            return helper.LinkToRemote(innerContent, confirm, url, new ModelDictionary(options));
        }
        public static string LinkToRemote(this AjaxHelper helper, System.String innerContent, System.String url, System.Object options, System.Object htmloptions)
        {
            return helper.LinkToRemote(innerContent, url, new ModelDictionary(options), new ModelDictionary(htmloptions));
        }
        public static string BuildFormRemoteTag(this AjaxHelper helper, System.String url, System.Object options)
        {
            return helper.BuildFormRemoteTag(url, new ModelDictionary(options));
        }
        public static string BuildFormRemoteTag(this AjaxHelper helper, System.Object options)
        {
            return helper.BuildFormRemoteTag(new ModelDictionary(options));
        }
        public static string ObserveField(this AjaxHelper helper, System.String fieldId, System.Int32 frequency, System.String url, System.Object options)
        {
            return helper.ObserveField(fieldId, frequency, url, new ModelDictionary(options));
        }
        public static string ObserveField(this AjaxHelper helper, System.Object options)
        {
            return helper.ObserveField(new ModelDictionary(options));
        }
        public static string ObserveForm(this AjaxHelper helper, System.String formId, System.Object options)
        {
            return helper.ObserveForm(formId, new ModelDictionary(options));
        }
        public static string ObserveForm(this AjaxHelper helper, System.Object options)
        {
            return helper.ObserveForm(new ModelDictionary(options));
        }
        public static string PeriodicallyCallRemote(this AjaxHelper helper, System.Object options)
        {
            return helper.PeriodicallyCallRemote(new ModelDictionary(options));
        }
        public static string PeriodicallyCallRemote(this AjaxHelper helper, System.String url, System.Object options)
        {
            return helper.PeriodicallyCallRemote(url, new ModelDictionary(options));
        }
        public static string InputTextWithAutoCompletion(this AjaxHelper helper, System.Object options, System.Object tagAttributes)
        {
            return helper.InputTextWithAutoCompletion(new ModelDictionary(options), new ModelDictionary(tagAttributes));
        }
        public static string InputTextWithAutoCompletion(this AjaxHelper helper, System.String inputName, System.String url, System.Object tagAttributes, System.Object completionOptions)
        {
            return helper.InputTextWithAutoCompletion(inputName, url, new ModelDictionary(tagAttributes), new ModelDictionary(completionOptions));
        }
        public static string AutoCompleteInputText(this AjaxHelper helper, System.String elementId, System.String url, System.Object options)
        {
            return helper.AutoCompleteInputText(elementId, url, new ModelDictionary(options));
        }
        public static string BuildRemoteFunction(this AjaxHelper helper, System.String url, System.Object options)
        {
            return helper.BuildRemoteFunction(url, new ModelDictionary(options));
        }
        public static string RemoteFunction(this AjaxHelper helper, System.Object options)
        {
            return helper.RemoteFunction(new ModelDictionary(options));
        }
        public static string For(this UrlHelper helper, System.Object parameters)
        {
            return helper.For(new ModelDictionary(parameters));
        }
        public static string Link(this UrlHelper helper, System.String innerContent, System.Object parameters)
        {
            return helper.Link(innerContent, new ModelDictionary(parameters));
        }
        public static string Link(this UrlHelper helper, System.String innerContent, System.Object parameters, System.Object anchorAttributes)
        {
            return helper.Link(innerContent, new ModelDictionary(parameters), new ModelDictionary(anchorAttributes));
        }
        public static string ButtonLink(this UrlHelper helper, System.String innerContent, System.Object parameters)
        {
            return helper.ButtonLink(innerContent, new ModelDictionary(parameters));
        }
        public static string ButtonLink(this UrlHelper helper, System.String innerContent, System.Object parameters, System.Object buttonAttributes)
        {
            return helper.ButtonLink(innerContent, new ModelDictionary(parameters), new ModelDictionary(buttonAttributes));
        }
        public static string VisualEffect(this ScriptaculousHelper helper, System.String name, System.String elementId, System.Object options)
        {
            return helper.VisualEffect(name, elementId, new ModelDictionary(options));
        }
        public static string VisualEffectDropOut(this ScriptaculousHelper helper, System.String elementId, System.Object queue)
        {
            return helper.VisualEffectDropOut(elementId, new ModelDictionary(queue));
        }
        public static string CreatePageLink(this PaginationHelper helper, System.Int32 page, System.String text, System.Object htmlAttributes)
        {
            return helper.CreatePageLink(page, text, new ModelDictionary(htmlAttributes));
        }
        public static string CreatePageLink(this PaginationHelper helper, System.Int32 page, System.String text, System.Object htmlAttributes, System.Object queryStringParams)
        {
            return helper.CreatePageLink(page, text, new ModelDictionary(htmlAttributes), new ModelDictionary(queryStringParams));
        }
        public static string CreatePageLinkWithCurrentQueryString(this PaginationHelper helper, System.Int32 page, System.String text, System.Object htmlAttributes)
        {
            return helper.CreatePageLinkWithCurrentQueryString(page, text, new ModelDictionary(htmlAttributes));
        }
        public static string FormTag(this FormHelper helper, System.Object parameters)
        {
            return helper.FormTag(new ModelDictionary(parameters));
        }
        public static string FormTag(this FormHelper helper, System.String url, System.Object parameters)
        {
            return helper.FormTag(url, new ModelDictionary(parameters));
        }
        public static string AjaxFormTag(this FormHelper helper, System.Object parameters)
        {
            return helper.AjaxFormTag(new ModelDictionary(parameters));
        }
        public static string Submit(this FormHelper helper, System.String value, System.Object attributes)
        {
            return helper.Submit(value, new ModelDictionary(attributes));
        }
        public static string ImageSubmit(this FormHelper helper, System.String imgsrc, System.String alttext, System.Object attributes)
        {
            return helper.ImageSubmit(imgsrc, alttext, new ModelDictionary(attributes));
        }
        public static string Button(this FormHelper helper, System.String value, System.Object attributes)
        {
            return helper.Button(value, new ModelDictionary(attributes));
        }
        public static string ButtonElement(this FormHelper helper, System.String innerText, System.String type, System.Object attributes)
        {
            return helper.ButtonElement(innerText, type, new ModelDictionary(attributes));
        }
        public static string TextFieldValue(this FormHelper helper, System.String target, System.Object value, System.Object attributes)
        {
            return helper.TextFieldValue(target, value, new ModelDictionary(attributes));
        }
        public static string TextField(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.TextField(target, new ModelDictionary(attributes));
        }
        public static string TextFieldAutoComplete(this FormHelper helper, System.String target, System.String url, System.Object tagAttributes, System.Object completionOptions)
        {
            return helper.TextFieldAutoComplete(target, url, new ModelDictionary(tagAttributes), new ModelDictionary(completionOptions));
        }
        public static string FilteredTextField(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.FilteredTextField(target, new ModelDictionary(attributes));
        }
        public static string NumberField(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.NumberField(target, new ModelDictionary(attributes));
        }
        public static string NumberFieldValue(this FormHelper helper, System.String target, System.Object value, System.Object attributes)
        {
            return helper.NumberFieldValue(target, value, new ModelDictionary(attributes));
        }
        public static string TextArea(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.TextArea(target, new ModelDictionary(attributes));
        }
        public static string TextAreaValue(this FormHelper helper, System.String target, System.Object value, System.Object attributes)
        {
            return helper.TextAreaValue(target, value, new ModelDictionary(attributes));
        }
        public static string PasswordField(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.PasswordField(target, new ModelDictionary(attributes));
        }
        public static string PasswordNumberField(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.PasswordNumberField(target, new ModelDictionary(attributes));
        }
        public static string LabelFor(this FormHelper helper, System.String target, System.String label, System.Object attributes)
        {
            return helper.LabelFor(target, label, new ModelDictionary(attributes));
        }
        public static string HiddenField(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.HiddenField(target, new ModelDictionary(attributes));
        }
        public static string HiddenField(this FormHelper helper, System.String target, System.Object value, System.Object attributes)
        {
            return helper.HiddenField(target, value, new ModelDictionary(attributes));
        }
        public static string CheckboxField(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.CheckboxField(target, new ModelDictionary(attributes));
        }
        public static string RadioField(this FormHelper helper, System.String target, System.Object valueToSend, System.Object attributes)
        {
            return helper.RadioField(target, valueToSend, new ModelDictionary(attributes));
        }
        public static string FileField(this FormHelper helper, System.String target, System.Object attributes)
        {
            return helper.FileField(target, new ModelDictionary(attributes));
        }
        public static string Select(this FormHelper helper, System.String target, System.Collections.IEnumerable dataSource, System.Object attributes)
        {
            return helper.Select(target, dataSource, new ModelDictionary(attributes));
        }
        public static string Select(this FormHelper helper, System.String target, System.Object selectedValue, System.Collections.IEnumerable dataSource, System.Object attributes)
        {
            return helper.Select(target, selectedValue, dataSource, new ModelDictionary(attributes));
        }

        public static string PasswordField(this FormHelper helper, System.String target, System.Object attributes, ValueBehaviour valueBehaviour)
        {
            return helper.PasswordField(target, new ModelDictionary(attributes), valueBehaviour);
        }
        public static string PasswordNumberField(this FormHelper helper, System.String target, System.Object attributes, ValueBehaviour valueBehaviour)
        {
            return helper.PasswordNumberField(target, new ModelDictionary(attributes), valueBehaviour);
        }
    }
}
