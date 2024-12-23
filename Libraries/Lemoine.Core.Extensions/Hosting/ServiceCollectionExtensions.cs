// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Microsoft.Extensions.DependencyInjection;

namespace Lemoine.Core.Extensions.Hosting
{
  /// <summary>
  /// Extensions of <see cref="IServiceCollection"/>
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceCollectionExtensions).FullName);

    /// <summary>
    /// Add and register a single application initializer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializer<T> (this IServiceCollection services)
      where T : class, IApplicationInitializer
    {
      return services.AddSingleton<IApplicationInitializer, T> ();
    }

    /// <summary>
    /// Add and register two application initializers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializer<T, U> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
    {
      return services
        .AddSingleton<T> ()
        .AddSingleton<U> ()
        .SetApplicationInitializerCollection<T, U> ();
    }

    /// <summary>
    /// Set only the application initializer collection from two initializers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializerCollection<T, U> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
    {
      return services
        .AddSingleton<IApplicationInitializerCollection> ((IServiceProvider sp) => new ApplicationInitializerCollection (sp.GetRequiredService<T> (), sp.GetRequiredService<U> ()))
        .AddSingleton<IApplicationInitializer> ((IServiceProvider sp) => sp.GetRequiredService<IApplicationInitializerCollection> ());
    }

    /// <summary>
    /// Add and register three application initializers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializer<T, U, V> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
      where V : class, IApplicationInitializer
    {
      return services
        .AddSingleton<T> ()
        .AddSingleton<U> ()
        .AddSingleton<V> ()
        .SetApplicationInitializerCollection<T, U, V> ();
    }

    /// <summary>
    /// Set only the application initializer collection from three initializers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializerCollection<T, U, V> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
      where V : class, IApplicationInitializer
    {
      return services
        .AddSingleton<IApplicationInitializerCollection> ((IServiceProvider sp) => new ApplicationInitializerCollection (sp.GetRequiredService<T> (), sp.GetRequiredService<U> (), sp.GetRequiredService<V> ()))
        .AddSingleton<IApplicationInitializer> ((IServiceProvider sp) => sp.GetRequiredService<IApplicationInitializerCollection> ());
    }

    /// <summary>
    /// Add and register four application initializers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializer<T, U, V, W> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
      where V : class, IApplicationInitializer
      where W : class, IApplicationInitializer
    {
      return services
        .AddSingleton<T> ()
        .AddSingleton<U> ()
        .AddSingleton<V> ()
        .AddSingleton<W> ()
        .SetApplicationInitializerCollection<T, U, V, W> ();
    }

    /// <summary>
    /// Set only the application initializer collection from four initializers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializerCollection<T, U, V, W> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
      where V : class, IApplicationInitializer
      where W : class, IApplicationInitializer
    {
      return services
        .AddSingleton<IApplicationInitializerCollection> ((IServiceProvider sp) => new ApplicationInitializerCollection (sp.GetRequiredService<T> (), sp.GetRequiredService<U> (), sp.GetRequiredService<V> (), sp.GetRequiredService<W> ()))
        .AddSingleton<IApplicationInitializer> ((IServiceProvider sp) => sp.GetRequiredService<IApplicationInitializerCollection> ());
    }

    /// <summary>
    /// Add and register five application initializers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializer<T, U, V, W, X> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
      where V : class, IApplicationInitializer
      where W : class, IApplicationInitializer
      where X : class, IApplicationInitializer
    {
      return services
        .AddSingleton<T> ()
        .AddSingleton<U> ()
        .AddSingleton<V> ()
        .AddSingleton<W> ()
        .AddSingleton<X> ()
        .SetApplicationInitializerCollection<T, U, V, W, X> ();
    }

    /// <summary>
    /// Set only the application initializer collection from five initializers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializerCollection<T, U, V, W, X> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
      where V : class, IApplicationInitializer
      where W : class, IApplicationInitializer
      where X : class, IApplicationInitializer
    {
      return services
        .AddSingleton<IApplicationInitializerCollection> ((IServiceProvider sp) => new ApplicationInitializerCollection (sp.GetRequiredService<T> (), sp.GetRequiredService<U> (), sp.GetRequiredService<V> (), sp.GetRequiredService<W> (), sp.GetRequiredService<X> ()))
        .AddSingleton<IApplicationInitializer> ((IServiceProvider sp) => sp.GetRequiredService<IApplicationInitializerCollection> ());
    }

    /// <summary>
    /// Set only the application initializer collection from siz initializers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection SetApplicationInitializerCollection<T, U, V, W, X, Y> (this IServiceCollection services)
      where T : class, IApplicationInitializer
      where U : class, IApplicationInitializer
      where V : class, IApplicationInitializer
      where W : class, IApplicationInitializer
      where X : class, IApplicationInitializer
      where Y : class, IApplicationInitializer
    {
      return services
        .AddSingleton<IApplicationInitializerCollection> ((IServiceProvider sp) => new ApplicationInitializerCollection (sp.GetRequiredService<T> (), sp.GetRequiredService<U> (), sp.GetRequiredService<V> (), sp.GetRequiredService<W> (), sp.GetRequiredService<X> (), sp.GetRequiredService<Y> ()))
        .AddSingleton<IApplicationInitializer> ((IServiceProvider sp) => sp.GetRequiredService<IApplicationInitializerCollection> ());
    }
  }
}
