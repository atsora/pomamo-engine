// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lemoine.WebMiddleware.Assemblies;
using Lemoine.WebMiddleware.Routing;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Lemoine.WebMiddleware.Handlers
{
  public class HandlerMapperInitializer
  {
    public Dictionary<Type, (Type, MethodInfo)> ContractToHandler { get; }

    public HandlerMapperInitializer (IContractRouteInitializer routes, IServiceAssembliesResolver serviceAssembliesResolver)
    {
      var allContracts = routes.ContractsType;
      var assemblies = serviceAssembliesResolver.GetServiceAssemblies ();
      var allHandlers = assemblies
        .SelectMany (a => FindHandlers (a));
      ContractToHandler = BuilderMapper (allContracts, allHandlers);
    }

    public IEnumerable<Type> FindHandlers (Assembly endpointAssembly)
    {
      //type extension here?
      return endpointAssembly.GetTypes ()
        .Where (t => t.GetInterfaces ().Contains (typeof (IHandler)))
        .Where (t => !t.IsAbstract);
    }

    public Dictionary<Type, (Type, MethodInfo)> BuilderMapper (IEnumerable<Type> contracts, IEnumerable<Type> handlers)
    {
      var unmatchedContracts = new HashSet<Type> (contracts);
      Dictionary<Type, (Type, MethodInfo)> handlerMap = new Dictionary<Type, (Type, MethodInfo)> ();
      foreach (var handler in handlers) {
        var methods = handler.GetMethods (BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
        foreach (var m in methods) {
          var mParams = m.GetParameters ();
          var returnType = m.ReturnType;
          var verb = GetVerbFromMethod (m.Name);
          if (!IsValidHttpVerb (verb) || mParams.Count () != 1 || !IsTaskType (returnType)) {
            continue;
          }
          Type? matchedContract = null;
          foreach (var c in unmatchedContracts) {
            var route = c.GetCustomAttributes<RouteAttribute> ().First ();
            if (verb.Equals (route.Method, StringComparison.CurrentCultureIgnoreCase)) {
              if (mParams[0].ParameterType == c) {
                matchedContract = c;
                break;
              }
            }
          }
          if (matchedContract != null) {
            handlerMap[matchedContract] = (handler, m);
            unmatchedContracts.Remove (matchedContract);
          }
        }
      }
      //TODO Log a warning for unmatched routes.
      return handlerMap;
    }

    string GetVerbFromMethod (string name)
    {
      var verb = name.ToUpper ();
      return verb;
    }

    bool IsValidHttpVerb (string verb)
    {
      verb = verb.ToUpper ();
      return new[] { "GET", "HEAD", "POST", "PUT", "DELETE", "CONNECT", "OPTIONS", "TRACE", "PATCH" }.Contains (verb); //Move to constants & interfaces
    }

    private bool IsTaskType (Type T)
    {
      if (T.IsGenericType) {
        return T.GetGenericTypeDefinition () == typeof (Task<>);
      }

      return T == typeof (Task);
    }
  }
}
