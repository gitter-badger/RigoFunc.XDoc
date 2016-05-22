# RigoFunc.Xdoc
This repo contains the library that enables an application to read XML comments at run time. This feature will be very useful for developers those's mother language isn't
English, such as Chinese for China, Japanese for Japan.

# How to use
The following codes assume be developed by Chinese developer.

## ViewModels
```C#
public class LoginViewModel {
    /// <summary>
    /// 用户名称
    /// </summary>
    public string UserName { get; set; }
    /// <summary>
    /// 用户密码
    /// </summary>
    public string Password { get; set; }
}  
```

## POST Api
```C#
[HttpPost]
public void Post([FromBody]LoginViewModel model) {
    if (model == null) {
        throw new ArgumentNullException(nameof(model));
    }
    // ...
}
```

## Customized filter
```C#
// Copyright (c) RigoFunc (xuyingting). All rights reserved.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace RigoFunc.XDoc.ApiDoc.Filters {
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute {
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
```

## Inject filter to MVC filters collection
```C#
public void ConfigureServices(IServiceCollection services) {
    // Add framework services.
    services.AddApplicationInsightsTelemetry(Configuration);

    services.AddMvc(setup => {
        setup.Filters.Add(new ApiExceptionFilterAttribute());
    });
}
```

## Test using *POSTMAN*
```
{
  "ErrorMessage": "Value cannot be null.\r\nParameter name: model",
  "ApiDoc": {
    "UserName": "用户名称",
    "Password": "用户密码"
  }
}
```

# Use in other scenario
There are many cases can using this labrary, such as prompt user some validation error. The following code uses this library to resolve the property name in 
*FluentValidation*

```C#
ValidatorOptions.PropertyNameResolver = (type, memberInfo, expression) => {
                if (memberInfo != null) {
                    var xml = memberInfo.XmlDoc();
                    if (!string.IsNullOrEmpty(xml)) {
                        return xml;
                    }
                }

                if (expression != null) {
                    var chain = PropertyChain.FromExpression(expression);
                    if (chain.Count > 0)
                        return chain.ToString();
                }

                if (memberInfo != null) {
                    return memberInfo.Name;
                }

                return null;
            };
```
