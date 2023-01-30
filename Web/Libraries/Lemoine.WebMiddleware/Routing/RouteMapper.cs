// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.WebMiddleware.Routing
{
  public class RouteMapper
  {
    static readonly string PARAMETER_DELIMITER = "^path_parameter_" + Guid.NewGuid ().ToString ();

    readonly RouteKey m_root;
    readonly IDictionary<Type, IList<string>> m_defaultPolicies = new Dictionary<Type, IList<string>> ();

    internal RouteMapper ()
    {
      m_root = new RouteKey ();
    }

    internal void AddPolicy (Type t, string policy)
    {
      if (m_defaultPolicies.TryGetValue (t, out var tPolicies)) {
        tPolicies.Add (policy);
      }
      else {
        m_defaultPolicies[t] = new List<string> { policy };
      }
    }

    internal void AddRoute (Type t, string path, string verb)
    {
      verb = verb.ToUpper ();
      var tokenizedPath = path.Split ('/', StringSplitOptions.RemoveEmptyEntries);
      var currentParent = m_root;
      List<PathParameter> pathParameters = new List<PathParameter> ();
      for (int i = 0; i < tokenizedPath.Length; i++) {
        var component = tokenizedPath[i];
        string componentKey = component;
        if (IsPathParameter (t, component)) {
          componentKey = PARAMETER_DELIMITER;
          pathParameters.Add (new PathParameter (i, component.Substring (1, component.Length - 2)));
        }

        //TODO allow configurable case sensitive endpoints.
        componentKey = componentKey.ToLower ();
        if (!currentParent.Children.ContainsKey (componentKey)) {
          currentParent.Children[componentKey] = new RouteKey ();
        }
        currentParent = currentParent.Children[componentKey];
      }
      //Set the type
      var conflictingRoute = currentParent.Contracts.FirstOrDefault (c => c.Verb == verb);
      if (conflictingRoute != null) {
        throw new RouteValidationException ($"Type {t.Name} and {conflictingRoute.ContractType.Name} conflict for the same endpoint and verb ({verb}). This API does not support branching on types.");
      }
      if (!m_defaultPolicies.TryGetValue (t, out var policies)) {
        policies = new List<string> ();
      }
      currentParent.Contracts.Add (new RouteLeaf (t, verb, pathParameters, policies));
    }

    private bool IsPathParameter (Type t, string component)
    {
      if (component.StartsWith ('{') || component.EndsWith ('}')) {
        if (!component.EndsWith ('}') || !component.StartsWith ('{')) { throw new RouteValidationException ($"Type {t.Name} has an unclosed PathParameter, {component}"); }

        string param = component.Substring (1, component.Length - 2);
        var property = t.GetProperty (param);
        if (property == null) {
          throw new RouteValidationException ($"Type {t.Name} is missing a mapable Path Parameter, for {param}");
        }
        return true;
      }
      return false;
    }

    internal (Type?, IEnumerable<PathParameter>?, IList<string> policies) GetMatchingContractType (string path, string verb)
    {
      var tokenizedPath = path.Split ('/', StringSplitOptions.RemoveEmptyEntries);
      verb = verb.ToUpper ();
      var (routeDescrip, pathParams) = MatchingContract (tokenizedPath, verb, m_root, new List<PathParameter> ());
      if ((routeDescrip == null) || (null == pathParams)) {
        return (null, null, new List<string> ());
      }
      MapPropertyNames (routeDescrip, pathParams);

      return (routeDescrip.ContractType, pathParams, routeDescrip.Policies);
    }

    (RouteLeaf? route, List<PathParameter>? pathParams) MatchingContract (string[] pathTokens, string verb, RouteKey root, List<PathParameter> @params, int startIndex = 0)
    {
      if (startIndex == pathTokens.Length) {
        var routeMatch = root.Contracts.FirstOrDefault (leaf => leaf.Verb == verb);
        if (routeMatch == null) {
          return (null, null);
        }
        return (routeMatch, @params);
      }
      var component = pathTokens[startIndex];
      //TODO allow configurable case sensitive mapping;
      var componentLower = component.ToLower ();
      if (root.Children.ContainsKey (componentLower)) {
        var (match, pathParams) = MatchingContract (pathTokens, verb, root.Children[componentLower], @params, startIndex + 1);
        if ((match != null) && (null != pathParams)) {
          return (match, pathParams);
        }
      }
      if (root.Children.ContainsKey (PARAMETER_DELIMITER)) {
        @params.Add (new PathParameter (startIndex, null, component));
        var (match, pathParams) = MatchingContract (pathTokens, verb, root.Children[PARAMETER_DELIMITER], @params, startIndex + 1);
        if ((match != null) && (null != pathParams)) {
          return (match, pathParams);
        }
      }
      return (null, null);
    }


    private void MapPropertyNames (RouteLeaf route, IList<PathParameter> @params)
    {
      for (int i = 0; i < @params.Count (); i++) {
        var param = @params[i];
        param.MappedPropertyName = route.OrderedParams[i].MappedPropertyName;
      }
    }

    private class RouteKey
    {
      public RouteKey ()
      {
        Contracts = new List<RouteLeaf> ();
        Children = new Dictionary<string, RouteKey> ();
        WildcardChildren = new Dictionary<string, RouteKey> ();
      }

      internal readonly List<RouteLeaf> Contracts;

      internal readonly Dictionary<string, RouteKey> Children;

      internal readonly Dictionary<string, RouteKey> WildcardChildren;
    }

    internal class RouteLeaf
    {
      internal RouteLeaf (Type contractType, string verb, IList<PathParameter> orderedParameters, IList<string> policies)
      {
        this.ContractType = contractType;
        this.Verb = verb;
        this.OrderedParams = orderedParameters;
        this.Policies = policies;
      }

      internal Type ContractType { get; }

      internal string Verb { get; }

      internal IList<PathParameter> OrderedParams { get; }

      internal IList<string> Policies { get; }
    }
  }
}
