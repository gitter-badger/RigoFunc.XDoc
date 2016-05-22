// Copyright (c) RigoFunc (xuyingting). All rights reserved.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace RigoFunc.XDoc.ApiDoc.Filters {
    /// <summary>
    /// Represents a filter to handle Api exception.
    /// </summary>
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute {
        /// <summary>
        /// Calls when an exception occurs.
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(ExceptionContext context) {
            try {
                var loggerFactory = context.HttpContext.RequestServices.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ApiExceptionFilterAttribute>();
                var env = context.HttpContext.RequestServices.GetService<IHostingEnvironment>();

                var exception = context.Exception;

                var json = new JObject();

                // Error message.
                json.Add("ErrorMessage", exception.Message);

                // Api documentation.
                if (exception is ArgumentNullException && env.IsDevelopment()) {
                    var parameters = context.ActionDescriptor.Parameters;
                    if (parameters.Count > 0) {
                        if (parameters.Count == 1) {
                            // Api doc
                            json.Add("ApiDoc", parameters[0].ParameterType.GetXDoc());
                        }
                        else {
                            var apiDocJson = new JObject();
                            foreach (var item in parameters) {
                                apiDocJson.Add(item.Name, item.ParameterType.GetXDoc());
                            }

                            json.Add("ApiDoc", apiDocJson);
                        }
                    }
                }

                // log error
                logger.LogError(exception.ToString());

                // Bad Request
                context.HttpContext.Response.StatusCode = 400;

                // Json result.
                context.Result = new JsonResult(json);
            }
            catch {
                base.OnException(context);
            }
        }
    }
}
