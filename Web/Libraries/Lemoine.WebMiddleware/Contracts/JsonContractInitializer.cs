// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Contracts.DataTypeConverters;
using Lemoine.WebMiddleware.Routing;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;

namespace Lemoine.WebMiddleware.Contracts
{
  public class JsonContractInitializer
  {
    readonly ILog log = LogManager.GetLogger<JsonContractInitializer> ();

    private readonly ConverterRegistrar converters;

    public JsonContractInitializer (ConverterRegistrar converters)
    {
      this.converters = converters;
    }

    public async Task<object> InstantiateAsync (Type t, IEnumerable<PathParameter> pParams, Microsoft.AspNetCore.Http.HttpContext context)
    {
      object? deserialized;
      object contract;
      try {
        deserialized = await JsonSerializer.DeserializeAsync (context.Request.Body, t);
        if (deserialized is null) {
          log.Error ($"InstantiateAsync: body was deserialized to a null object {t}, which is unexpected");
        }
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"InstantiateAsync: nothing to deserialize => use the default constructor", ex);
        }
        deserialized = null;
      }
      if (deserialized is null) {
        object? nullableContract = Activator.CreateInstance (t);
        if (null != nullableContract) {
          contract = (object)nullableContract;
        }
        else {
          if (log.IsErrorEnabled) {
            log.Error ($"InstantiateAsync: object of type {t} could not be instantiated");
          }
          throw new Exception ("Type instantiation error");
        }
      }
      else {
        contract = deserialized;
      }

      try {
        contract = MapParameters (contract, t, pParams);
        contract = MapQuery (contract, t, context.Request.Query);
      }
      catch (Exception ex) {
        log.Error ($"InstantiateAsync: exception in MapParameters or MapQuery", ex);
        throw;
      }

      return contract;
    }

    object MapParameters (object contract, Type t, IEnumerable<PathParameter> pParams)
    {
      foreach (var pParam in pParams) {
        if (null == pParam.MappedPropertyName) {
          log.Fatal ($"MapParameters: property should be set for property index {pParam.PathIndex}");
          Debug.Assert (false);
          continue;
        }
        if (null == pParam.ParameterValue) {
          log.Fatal ($"MapParameters: property should be set for property name {pParam.MappedPropertyName}");
          Debug.Assert (false);
          continue;
        }
        System.Reflection.PropertyInfo? nullableProperty = t.GetProperty (pParam.MappedPropertyName);
        if (null == nullableProperty) {
          log.Fatal ($"MapParameters: property {pParam.MappedPropertyName} does not exist in type {t}");
          throw new Exception ("Missing property");
        }
        var property = nullableProperty ?? throw new NullReferenceException ();
        var converter = converters.GetConverter (property.PropertyType);

        var methodInfo = property.GetSetMethod () ?? throw new NullReferenceException ();
        methodInfo.Invoke (contract, new[] { converter.Invoke (pParam.ParameterValue) });
      }

      //i dont want to do more work so here is a null so i can test!
      return contract;
    }

    object MapQuery (object contract, Type t, IQueryCollection queries)
    {
      foreach (var query in queries) {
        if (query.Key is null) {
          log.Fatal ($"MapQuery: unexpected null query key");
          Debug.Assert (false);
          continue;
        }
        if (query.Key.Equals ("_", StringComparison.InvariantCultureIgnoreCase)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"MapQuery: skip property _");
          }
          continue;
        }
        System.Reflection.PropertyInfo? nullableProperty = t
          .GetProperty (query.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (nullableProperty is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"MapQuery: property {query.Key} does not exist in type {t} => skip it");
          }
          continue;
        }
        var property = nullableProperty ?? throw new NullReferenceException ();
        var converter = converters.GetConverter (property.PropertyType);

        var methodInfo = property.GetSetMethod () ?? throw new NullReferenceException ();
        methodInfo.Invoke (contract, new[] { converter.Invoke (query.Value) });
      }

      //i dont want to do more work so here is a null so i can test!
      return contract;
    }
  }
}
