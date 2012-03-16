using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PingApp.Web.Infrastructures {
    public class NewtonJsonActionResult : ActionResult {
        public object Value { get; private set; }

        public NewtonJsonActionResult(object value) {
            Value = value;
        }

        public override void ExecuteResult(ControllerContext context) {
            context.HttpContext.Response.ContentEncoding = Encoding.UTF8;
            context.HttpContext.Response.ContentType = "application/json";

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string text = JsonConvert.SerializeObject(Value, Formatting.None, settings);

            context.HttpContext.Response.Write(text);
            context.HttpContext.Response.End();
        }
    }
}