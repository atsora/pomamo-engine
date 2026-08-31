// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Pulse.Extensions.Web.Graphql
{
  /// <summary>
  /// Generic methods of <see cref="IGraphqlSchemaBuilder"/>
  ///
  /// They are extension methods rather than methods of the interface so that the interface
  /// carries no generic constraint of the underlying GraphQL library
  /// </summary>
  public static class GraphqlSchemaBuilderExtensions
  {
    /// <summary>
    /// <see cref="IGraphqlSchemaBuilder.AddQueryType(Type)"/>
    /// </summary>
    public static IGraphqlSchemaBuilder AddQueryType<TQuery> (this IGraphqlSchemaBuilder builder)
      => builder.AddQueryType (typeof (TQuery));

    /// <summary>
    /// <see cref="IGraphqlSchemaBuilder.AddMutationType(Type)"/>
    /// </summary>
    public static IGraphqlSchemaBuilder AddMutationType<TMutation> (this IGraphqlSchemaBuilder builder)
      => builder.AddMutationType (typeof (TMutation));

    /// <summary>
    /// <see cref="IGraphqlSchemaBuilder.AddType(Type)"/>
    /// </summary>
    public static IGraphqlSchemaBuilder AddType<T> (this IGraphqlSchemaBuilder builder)
      => builder.AddType (typeof (T));

    /// <summary>
    /// <see cref="IGraphqlSchemaBuilder.AddTypeExtension(Type)"/>
    /// </summary>
    public static IGraphqlSchemaBuilder AddTypeExtension<T> (this IGraphqlSchemaBuilder builder)
      => builder.AddTypeExtension (typeof (T));

    /// <summary>
    /// <see cref="IGraphqlSchemaBuilder.BindRuntimeType(Type, Type)"/>
    /// </summary>
    public static IGraphqlSchemaBuilder BindRuntimeType<TRuntimeType, TSchemaType> (this IGraphqlSchemaBuilder builder)
      => builder.BindRuntimeType (typeof (TRuntimeType), typeof (TSchemaType));
  }
}
