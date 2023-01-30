// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lemoine.Hosting.AsyncInitialization
{
  /// <summary>
  /// Provides extension methods to register async initializers.
  /// </summary>
  public static class AsyncInitializationServiceCollectionExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (AsyncInitializationHostExtensions).FullName);

    /// <summary>
    /// Registers necessary services for async initialization support.
    /// </summary>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddAsyncInitialization (this IServiceCollection services)
    {
      if (services is null) {
        log.Error ("AddAsyncInitialization: services null");
        Debug.Assert (false);
        throw new ArgumentNullException (nameof (services));
      }

      services.TryAddTransient<RootInitializer> ();
      return services;
    }

    /// <summary>
    /// Adds an async initializer of the specified type.
    /// </summary>
    /// <typeparam name="TInitializer">The type of the async initializer to add.</typeparam>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddAsyncInitializer<TInitializer> (this IServiceCollection services)
        where TInitializer : class, IAsyncInitializer
    {
      return services
        .AddAsyncInitialization ()
        .AddTransient<IAsyncInitializer, TInitializer> ();
    }

    /// <summary>
    /// Adds the specified async initializer instance.
    /// </summary>
    /// <typeparam name="TInitializer">The type of the async initializer to add.</typeparam>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
    /// <param name="initializer">The service initializer</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddAsyncInitializer<TInitializer> (this IServiceCollection services, TInitializer initializer)
        where TInitializer : class, IAsyncInitializer
    {
      if (initializer is null) {
        log.Error ("AddAsyncInitialization: services null");
        Debug.Assert (false);
        throw new ArgumentNullException (nameof (initializer));
      }

      return services
        .AddAsyncInitialization ()
        .AddSingleton<IAsyncInitializer> (initializer);
    }

    /// <summary>
    /// Adds an async initializer with a factory specified in <paramref name="implementationFactory" />.
    /// </summary>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
    /// <param name="implementationFactory">The factory that creates the async initializer.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddAsyncInitializer (this IServiceCollection services, Func<IServiceProvider, IAsyncInitializer> implementationFactory)
    {
      if (implementationFactory is null) {
        log.Error ("AddAsyncInitialization: services null");
        Debug.Assert (false);
        throw new ArgumentNullException (nameof (implementationFactory));
      }

      return services
        .AddAsyncInitialization ()
        .AddTransient (implementationFactory);
    }

    /// <summary>
    /// Adds an async initializer of the specified type
    /// </summary>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
    /// <param name="initializerType">The type of the async initializer to add.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddAsyncInitializer (this IServiceCollection services, Type initializerType)
    {
      if (initializerType is null) {
        log.Error ("AddAsyncInitialization: services null");
        Debug.Assert (false);
        throw new ArgumentNullException (nameof (initializerType));
      }

      return services
        .AddAsyncInitialization ()
        .AddTransient (typeof (IAsyncInitializer), initializerType);
    }

    /// <summary>
    /// Adds an async initializer whose implementation is the specified delegate.
    /// </summary>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the service to.</param>
    /// <param name="initializer">The delegate that performs async initialization.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddAsyncInitializer (this IServiceCollection services, Func<Task> initializer)
    {
      if (initializer is null) {
        log.Error ("AddAsyncInitialization: services null");
        Debug.Assert (false);
        throw new ArgumentNullException (nameof (initializer));
      }

      return services
        .AddAsyncInitialization ()
        .AddSingleton<IAsyncInitializer> (new DelegateAsyncInitializer (initializer));
    }

    private class DelegateAsyncInitializer : IAsyncInitializer
    {
      private readonly Func<Task> _initializer;

      public DelegateAsyncInitializer (Func<Task> initializer)
      {
        _initializer = initializer;
      }

      public Task InitializeAsync ()
      {
        return _initializer ();
      }
    }
  }
}
