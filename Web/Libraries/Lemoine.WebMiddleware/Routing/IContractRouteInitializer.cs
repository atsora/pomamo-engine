// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lemoine.Extensions.Web.Attributes;

namespace Lemoine.WebMiddleware.Routing
{
  /// <summary>
  /// 
  /// </summary>
  public interface IContractRouteInitializer
  {
    RouteMapper Mapper { get; }

    IEnumerable<Type> ContractsType { get; }
  }

  /// <summary>
  /// Extensions to <see cref="IContractRouteInitializer">
  /// </summary>
  public static class ContractRouteInitializerExtensions
  {
    public static void AddContract (this IContractRouteInitializer contractRouteInitializer, Type contract)
    {
      // Policies first
      if (contract.GetCustomAttribute<AllowAnonymousAttribute> () is not null) {
        contractRouteInitializer.Mapper.AddPolicy (contract, "anonymous");
      }
      else {
        var authorizeAttributes = contract.GetCustomAttributes<AuthorizeAttribute> ();
        foreach (var authorizeAttribute in authorizeAttributes) {
          if (!string.IsNullOrEmpty (authorizeAttribute.Policy)) {
            contractRouteInitializer.Mapper.AddPolicy (contract, authorizeAttribute.Policy);
          }
          else {
            contractRouteInitializer.Mapper.AddPolicy (contract, "authorize");
          }
        }
      }

      var routeAttr = contract.GetCustomAttributes<RouteAttribute> ();
      ValidateRoutesOnType (contract, routeAttr);

      foreach (var attr in routeAttr) {
        contractRouteInitializer.Mapper.AddRoute (contract, attr.Endpoint, attr.Method);
      }
    }

    static void ValidateRoutesOnType (Type t, IEnumerable<RouteAttribute> routeAttributes)
    {
      if (routeAttributes.GroupBy (a => a.Method).Count () > 1) {
        throw new RouteValidationException ($"Type {t.Name} has multiple routes with different Verbs");
      }
    }
  }
}
