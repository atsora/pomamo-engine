// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Lemoine.Stamping.Config
{
  /// <summary>
  /// Extensions
  /// </summary>
  public static class StampingConfigExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (StampingConfigExtensions).FullName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="typeLoader"></param>
    /// <param name="stampingConfig"></param>
    /// <returns></returns>
    public static IServiceCollection AddStampingConfig (this IServiceCollection serviceCollection, TypeLoader typeLoader, StampingConfig stampingConfig)
    {
      return stampingConfig.Configure (typeLoader, serviceCollection);
    }

    /// <summary>
    /// Configure
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection Configure (this StampingConfig stampingConfig, TypeLoader typeLoader, IServiceCollection services)
    {
      stampingConfig.AddFileFlow (typeLoader, services);
      stampingConfig.AddStamper (typeLoader, services);
      stampingConfig.AddStampingParser (typeLoader, services);
      stampingConfig.AddConfigurators (typeLoader, services);
      stampingConfig.AddSingletons (typeLoader, services);
      stampingConfig.AddScopedServices (typeLoader, services);
      stampingConfig.AddEventHandlers (typeLoader, services);
      return services;
    }

    static void AddFileFlow (this StampingConfig stampingConfig, TypeLoader typeLoader, IServiceCollection services)
    {
      stampingConfig.TryAddSingleton ("Lemoine.Stamping.IStampingFileFlow, Lemoine.Stamping", stampingConfig.StampingFileFlow, "", null, typeLoader, services);
    }

    static void AddStamper (this StampingConfig stampingConfig, TypeLoader typeLoader, IServiceCollection services)
    {
      stampingConfig.TryAddSingleton ("Lemoine.Stamping.IStamper, Lemoine.Stamping", stampingConfig.Stamper, "", null, typeLoader, services);
    }

    static void AddStampingParser (this StampingConfig stampingConfig, TypeLoader typeLoader, IServiceCollection services)
    {
      stampingConfig.TryAddSingleton ("Lemoine.Stamping.IStampingParser, Lemoine.Stamping", stampingConfig.StampingParser, "", null, typeLoader, services);
    }

    static void AddConfigurators (this StampingConfig stampingConfig, TypeLoader typeLoader, IServiceCollection services)
    {
      foreach (var configurator in stampingConfig.Configurators) {
        stampingConfig.TryAddConfigurator (configurator, typeLoader, services);
      }
    }

    static void TryAddConfigurator (this StampingConfig stampingConfig, string configurator, TypeLoader typeLoader, IServiceCollection services)
    {
      try {
        var stampingServicesConfigurator = typeLoader.Load<IStampingServicesConfigurator> (configurator);
        if (stampingServicesConfigurator is null) {
          log.Fatal ($"TryAddConfigurator: unexpected null stampingServicesConfigurator");
        }
        else {
          stampingServicesConfigurator.Configure (services);
        }
      }
      catch (Exception ex) {
        log.Error ($"TryAddConfigurator: exception in {configurator} load", ex);
      }
    }

    static void AddSingletons (this StampingConfig stampingConfig, TypeLoader typeLoader, IServiceCollection services)
    {
      foreach (var singleton in stampingConfig.Singletons) {
        stampingConfig.TryAddSingleton (singleton.Name, singleton.ImplementationName, singleton.DelegationName, singleton.Json, typeLoader, services);
      }
    }

    static void AddScopedServices (this StampingConfig stampingConfig, TypeLoader typeLoader, IServiceCollection services)
    {
      foreach (var scoped in stampingConfig.ScopedServices) {
        stampingConfig.TryAddScoped (scoped.Name, scoped.ImplementationName, scoped.DelegationName, scoped.Json, typeLoader, services);
      }
    }

    static void AddEventHandlers (this StampingConfig stampingConfig, TypeLoader typeLoader, IServiceCollection services)
    {
      foreach (var eventHandler in stampingConfig.EventHandlers) {
        stampingConfig.TryAddSingleton (eventHandler, "", "", null, typeLoader, services);
      }
    }

    static void TryAddSingleton (this StampingConfig stampingConfig, string name, string implementationName, string delegationName, string? json, TypeLoader typeLoader, IServiceCollection services)
    {
      try {
        if (string.IsNullOrEmpty (name)) {
          log.Error ($"TryAddSingleton: empty name");
          return;
        }

        var type = typeLoader.GetType (name);
        if (type is null) {
          log.Error ($"TryAddSingleton: no type for {name}");
          return;
        }

        if (string.IsNullOrEmpty (implementationName)) {
          if (string.IsNullOrEmpty (delegationName)) {
            if (string.IsNullOrEmpty (json)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"TryAddSingleton: adding {name}");
              }
              services.AddSingleton (type);
            }
            else { // json not empty
              if (log.IsDebugEnabled) {
                log.Debug ($"TryAddSingleton: adding {name} with json");
              }
              services.AddSingleton (type, sp => DeserializeJson (json, type));
            }
          }
          else { // delegation name set
            if (log.IsDebugEnabled) {
              log.Debug ($"TryAddSingleton: adding {name} using delegation {delegationName}");
            }
            var delegationType = typeLoader.GetType (delegationName);
            if (delegationType is null) {
              log.Error ($"TryAddSingleton: no type for delegation {delegationName}");
              return;
            }
            services.AddSingleton (type, sp => sp.GetService (delegationType) ?? throw new Exception ($"TryAddSingleton: no delegate service {delegationName}"));
          }
        }
        else {
          var implementationType = typeLoader.GetType (implementationName);
          if (implementationType is null) {
            log.Error ($"TryAddSingleton: no implementation type for {implementationType}");
          }
          else {
            if (string.IsNullOrEmpty (json)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"TryAddSingleton: adding {name}/{implementationName}");
              }
              services.AddSingleton (type, implementationType);
            }
            else { // json not empty
              if (log.IsDebugEnabled) {
                log.Debug ($"TryAddSingleton: adding {name}/{implementationName} with json");
              }
              services.AddSingleton (type, sp => DeserializeJson (json, implementationType));
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"TryAddSingleton: exception adding {name}, {implementationName}", ex);
      }
    }

    static object DeserializeJson (string json, Type implementationType)
    {
      try {
        var result = JsonSerializer.Deserialize (json, implementationType, new JsonSerializerOptions {
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault
        });
        if (result is null) {
          log.Error ($"DeserializeJson: null is returned");
          throw new Exception ("DeserializeJson: null object returned");
        }
        return result;
      }
      catch (Exception ex) {
        log.Error ($"DeserializeJson: exception", ex);
        throw;
      }
    }

    static void TryAddScoped (this StampingConfig stampingConfig, string name, string implementationName, string delegationName, string? json, TypeLoader typeLoader, IServiceCollection services)
    {
      try {
        if (string.IsNullOrEmpty (name)) {
          log.Error ($"TryAddScoped: empty name");
          return;
        }

        var type = typeLoader.GetType (name);
        if (type is null) {
          log.Error ($"TryAddScoped: no type for {name}");
          return;
        }

        if (string.IsNullOrEmpty (implementationName)) {
          if (string.IsNullOrEmpty (delegationName)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"TryAddScoped: adding {name}");
            }
            services.AddScoped (type);
          }
          else { // delegation name set
            if (log.IsDebugEnabled) {
              log.Debug ($"TryAddScoped: adding {name} using delegation {delegationName}");
            }
            var delegationType = typeLoader.GetType (delegationName);
            if (delegationType is null) {
              log.Error ($"TryAddScoped: no type for delegation {delegationName}");
              return;
            }
            services.AddScoped (type, sp => sp.GetService (delegationType) ?? throw new Exception ($"TryAddScoped: no delegate service {delegationName}"));
          }
        }
        else {
          var implementationType = typeLoader.GetType (implementationName);
          if (implementationType is null) {
            log.Error ($"TryAddScoped: no implementation type for {implementationType}");
          }
          else {
            if (string.IsNullOrEmpty (json)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"TryAddScoped: adding {name}/{implementationName}");
              }
              services.AddScoped (type, implementationType);
            }
            else { // json not empty
              if (log.IsDebugEnabled) {
                log.Debug ($"TryAddScoped: adding {name}/{implementationName} with json");
              }
              services.AddScoped (type, sp => JsonSerializer.Deserialize (json, implementationType) ?? throw new Exception ("TryAddScoped: null object returned"));
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"TryAddScoped: exception adding {name}, {implementationName}", ex);
      }
    }

  }

}
