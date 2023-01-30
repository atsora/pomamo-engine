// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Contracts;
using Lemoine.WebMiddleware.HttpContext;
using Lemoine.WebMiddleware.Handlers;
using Lemoine.WebMiddleware.Response;
using Lemoine.WebMiddleware.Routing;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.WebMiddleware
{
  public class CustomRoutingMiddleware
  {
    readonly ILog log = LogManager.GetLogger<CustomRoutingMiddleware> ();

    readonly RequestDelegate m_next; //We won't use this.

    readonly RouteMapper m_routingMapper;
    readonly HandlerFactory m_handlerFactory;
    readonly JsonContractInitializer m_contractInitializer;
    readonly ResponseWriter m_responseWriter;

    //public delegate object RunHandlerDelegate()

    public CustomRoutingMiddleware (RequestDelegate next, IContractRouteInitializer routingMapper, HandlerFactory handlerFactory, JsonContractInitializer contractInitializer, ResponseWriter responseWriter)
    {
      m_next = next;

      m_routingMapper = routingMapper.Mapper;
      m_handlerFactory = handlerFactory;
      m_contractInitializer = contractInitializer;
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context, IAuthorizationService authorizationService)
    {
      string path = context.Request.Path;
      string verb = context.Request.Method;
      var (contractType, pParams, policies) = m_routingMapper.GetMatchingContractType (path, verb);

      if ((contractType == null) || (null == pParams)) {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
      }
      var (handler, mInfo) = m_handlerFactory.ResolveHandler (contractType);
      if ((handler == null) || (null == mInfo)) {
        context.Response.StatusCode = StatusCodes.Status501NotImplemented;
        return;
      }

      var contentType = context.GetResponseContentType ();

      if (!policies.Any ()) {
        policies.Add ("default");
      }
      foreach (var policy in policies) {
        var authorizationResult =
            await authorizationService.AuthorizeAsync (context.User, null, policy);
        if (!authorizationResult.Succeeded) {
          log.Error ($"InvokeAsync: policy={policy} not authorized");
          var errorDto = new ErrorDTO ($"Not authorized, policy={policy}", ErrorStatus.AuthorizationError);
          await m_responseWriter.WriteToBodyAsync (context, errorDto, contentType);
          return;
        }
      }

      if (handler is IHttpContextSupport httpContextSupport) {
        if (httpContextSupport is null) {
          Debug.Assert (false);
          log.Fatal ("InvokeAsync: handler was expected to have the IHttpContextSupport interface");
        }
        else {
          httpContextSupport.HttpContext = context;
        }
      }

      if (handler is IRemoteIpSupport remoteIpSupport) {
        if (null == remoteIpSupport) {
          Debug.Assert (false);
          log.Fatal ("InvokeAsync: handler was expected to have the IRemoteIpSupport interface");
        }
        else {
          remoteIpSupport.SetRemoteIp (context.Connection.RemoteIpAddress?.ToString () ?? "");
        }
      }

      if (handler is IBodySupport bodySupport) {
        if (null == bodySupport) {
          Debug.Assert (false);
          log.Fatal ("InvokeAsync: handler was expected to have the IBodySupport interface");
        }
        else {
          bodySupport.SetBody (context.Request.Body);
        }
      }

      //connemt this out and it will just return without deserialize and reserialize.
      var contract = await m_contractInitializer.InstantiateAsync (contractType, pParams, context);
      Task runHandler () => (Task)(mInfo?.Invoke (handler, new[] { contract }) ?? throw new NullReferenceException ());

      var TResult = GetTaskResultType (mInfo);
      if (TResult == null) {
        await AwaitNoResponse (runHandler);
        return;
      }
      var outputDTO = await AwaitUnserializedResponse (runHandler, TResult);

      // Store in cache if GET
      if (context.Request.Method.Equals ("GET")) { // Cache is only valid with GET
        var customData = context.GetCustomData ();
        customData.Policies = policies;
        customData.OutputDTO = outputDTO;
        if (handler is ICachedHandler cachedHandler) {
          var timeOut = cachedHandler.GetCacheTimeOut (path + context.Request.QueryString.ToString (), contract, outputDTO);
          customData.CacheTimeOut = timeOut;
        }
        if (handler is IUserSpecificResponseSupport userSpecificResponseSupport) {
          if (userSpecificResponseSupport is null) {
            Debug.Assert (false);
            log.Fatal ("InvokeAsync: handler was expected to have the IUserSpecificResponseSupport interface");
          }
          else if (userSpecificResponseSupport.SpecificUser is not null) {
            context.GetCustomData ().SpecificUser = userSpecificResponseSupport.SpecificUser;
          }
        }
      }

      // TODO: Cache with POST

      if (handler is IResponseContentTypeSupport responseContentTypeSupport) {
        var responseContentType = responseContentTypeSupport.GetResponseContentType ();
        if (log.IsDebugEnabled) {
          log.Debug ($"InvokeAsync: response content type from handler is {responseContentType}");
        }
        context.GetCustomData ().ResponseContentType = responseContentType;
      }

      if (!string.IsNullOrEmpty (contentType)) {
        context.GetCustomData ().ResponseContentType = contentType;
      }
      await m_responseWriter.WriteToBodyAsync (context, outputDTO, contentType);
    }

    private async Task<object> AwaitUnserializedResponse (Func<Task> task, Type expectedResultType)
    {
      var awaitable = task ();
      await awaitable;

      object result = (object)((dynamic)awaitable).Result;

      return result;
    }

    private async Task AwaitNoResponse (Func<Task> task)
    {
      var awaitable = task ();
      await awaitable;
    }

    private Type? GetTaskResultType (MethodInfo method)
    {
      var taskType = method.ReturnType;
      if (!taskType.IsGenericType) {
        return null;
      }
      var args = taskType.GetGenericArguments ();
      return args[0];
    }
  }
}
