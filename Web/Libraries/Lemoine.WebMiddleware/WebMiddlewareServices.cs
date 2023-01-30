// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Cache;
using Lemoine.Core.Extensions.Cache;
using Lemoine.Core.Extensions.Logging;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Contracts;
using Lemoine.WebMiddleware.Contracts.DataTypeConverters;
using Lemoine.WebMiddleware.Handlers;
using Lemoine.WebMiddleware.Response;
using Lemoine.WebMiddleware.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Lemoine.WebMiddleware
{
  public static class WebMiddlewareServices
  {
    static readonly string CACHE_IMPLEMENTATION_KEY = "WebService.Cache.Implementation";
    static readonly string CACHE_IMPLEMENTATION_DEFAULT = "default"; // "lru" or "memory" or "default"

    static readonly string LRU_SIZE_KEY = "WebService.Cache.LRU.Size";
    static readonly int LRU_SIZE_DEFAULT = 100000;

    static readonly string JWT_AUDIENCE_KEY = "Jwt.Audience";
    static readonly string JWT_AUDIENCE_DEFAULT = "urn:pulse";

    static readonly string AUTHENTICATION_REQUIRED_KEY = "Authentication.Required";
    static readonly bool AUTHENTICATION_REQUIRED_DEFAULT = false;

    static readonly ILog log = LogManager.GetLogger (typeof (WebMiddlewareServices).FullName);

    /// <summary>
    /// Note: IServiceAssembliesResolver must be already set in the services
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureServices (IServiceCollection services)
    {
      services.AddSingleton<Lemoine.Core.Log.ILogFactory> ((IServiceProvider sp) => Lemoine.Core.Log.LogManager.LoggerFactory);

      services.AddSingleton<ICacheClientWithCleanExtension> ((IServiceProvider sp) => CreateCacheClient (sp));
      services.AddSingleton<ICacheClient> ((IServiceProvider sp) => GetCacheClient (sp));

      services.AddSingleton<IContractRouteInitializer, ContractRouteInitializerAssemblies> ();
      services.AddSingleton<HandlerFactory> ();
      services.AddSingleton<HandlerMapperInitializer> ();
      services.AddSingleton<JsonContractInitializer> ();
      services.AddSingleton<ConverterRegistrar> ();
      services.AddSingleton<IHandlersResolver, HandlersResolver> ();
      services.AddSingleton<HandlersRegister> ();

      services.AddTransient<ResponseWriter> ();
    }

    public static void AddAuthentication (IServiceCollection services)
    {
      var audience = Lemoine.Info.ConfigSet.LoadAndGet (JWT_AUDIENCE_KEY, JWT_AUDIENCE_DEFAULT);
      services
        .AddAuthentication (JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearerConfiguration (audience: audience);
    }

    public static void AddAuthorization (IServiceCollection services)
    {
      services.AddAuthorization (options => {
        options.AddPolicy ("default", SetDefaultPolicy);
        options.AddPolicy ("anonymous", SetAnonynousPolicy);
        options.AddPolicy ("authorize", SetAuthorizePolicy);
        options.DefaultPolicy = options.GetPolicy ("default") ?? throw new NullReferenceException ();
      }
      );
    }

    static void SetDefaultPolicy (AuthorizationPolicyBuilder policyBuilder)
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (AUTHENTICATION_REQUIRED_KEY, AUTHENTICATION_REQUIRED_DEFAULT)) {
        SetAuthorizePolicy (policyBuilder);
      }
      else {
        SetAnonynousPolicy (policyBuilder);
      }
    }

    static void SetAnonynousPolicy (AuthorizationPolicyBuilder policyBuilder)
    {
      policyBuilder.RequireAssertion (context => true);
    }

    static void SetAuthorizePolicy (AuthorizationPolicyBuilder policyBuilder)
    {
      policyBuilder
        .RequireAuthenticatedUser ()
        .RequireAssertion (context => !IsAccessTokenExpired (context));
    }

    static bool IsAccessTokenExpired (AuthorizationHandlerContext context)
    {
      try {
        var validToString = context.User.FindFirst ("valid_to")?.Value;
        if (!string.IsNullOrEmpty (validToString)) {
          var validTo = DateTime.Parse (validToString).ToUniversalTime ();
          if (validTo < DateTime.UtcNow) {
            log.Error ($"IsAccessTokenExpired: validTo={validTo} too old => return true");
            return true;
          }
        }
        return false;
      }
      catch (Exception ex) {
        log.Fatal ($"IsAccessTokenExpired: exception => return true", ex);
        return true;
      }
    }

    static ICacheClient GetCacheClient (IServiceProvider sp)
    {
      var result = sp.GetService<ICacheClientWithCleanExtension> ();
      if (result is null) {
        log.Fatal ($"GetCacheClient: no ICacheClientWithCleanExtension service was defined, which is unexpected");
        throw new Exception ("An ICacheClientWithCleanExtension service must be defined first");
      }
      return result;
    }

    public static void RegisterServices (IServiceCollection services, IServiceProvider serviceProvider)
    {
      var handlersRegister = serviceProvider.GetService<HandlersRegister> ();
      if (handlersRegister is null) {
        log.Fatal ("RegisterServices: no HandlersRegister service was defined, which is unexpected");
        throw new Exception ("Define a HandlersRegister service first");
      }
      handlersRegister.Register (services);
    }

    static ICacheClientWithCleanExtension CreateCacheClient (IServiceProvider sp)
    {
      string cacheImplementation = Lemoine.Info.ConfigSet
        .LoadAndGet<string> (CACHE_IMPLEMENTATION_KEY, CACHE_IMPLEMENTATION_DEFAULT);
      switch (cacheImplementation.ToLowerInvariant ()) {
      case "lru": {
        int lruSize = Lemoine.Info.ConfigSet.LoadAndGet<int> (LRU_SIZE_KEY,
                                                              LRU_SIZE_DEFAULT);
        if (log.IsInfoEnabled) {
          log.Info ($"InitializeCache: use a LRU cache of size {lruSize}");
        }
        return new CacheClientWithExpiresAtData (new LruCacheClient (lruSize));
      }
      case "memory":
        if (log.IsInfoEnabled) {
          log.Info ($"InitializeCache: use the Microsoft Memory Cache");
        }
        var memoryCache = sp.GetService<IMemoryCache> ();
        if (memoryCache is null) {
          log.Fatal ($"CreateCacheClient: no IMemoryCache service was defined, which is unexpected");
          throw new Exception ("An IMemoryCache service must be defined first");
        }
        return new CacheClientWithExpiresAtData (new CacheClientWithCleanExtensionCore (new MemoryCacheClient (memoryCache)));
      default: {
        int lruSize = Lemoine.Info.ConfigSet.LoadAndGet<int> (LRU_SIZE_KEY,
                                                              LRU_SIZE_DEFAULT);
        if (log.IsInfoEnabled) {
          log.InfoFormat ($"InitializeCache: use a (default) LRU cache of size {lruSize} with clean extension");
        }
        return new CacheClientWithExpiresAtData (new CacheClientWithCleanExtensionCore (new LruCacheClient (lruSize)));
      }
      }
    }
  }
}
