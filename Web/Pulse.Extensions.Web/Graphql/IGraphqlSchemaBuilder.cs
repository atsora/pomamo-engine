// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Pulse.Extensions.Web.Graphql
{
  /// <summary>
  /// Builder of a GraphQL schema
  ///
  /// It mirrors the part of the underlying GraphQL library that the extensions need, so that
  /// a plugin declares its schema without referencing the library that serves it. The glue
  /// between this interface and the library lives in the web service, and the methods that
  /// are missing are to be added here and in the glue
  ///
  /// The types are passed as <see cref="Type"/> rather than as generic parameters because the
  /// constraints of the underlying library would leak here otherwise. Use the generic methods
  /// of <see cref="GraphqlSchemaBuilderExtensions"/> instead
  /// </summary>
  public interface IGraphqlSchemaBuilder
  {
    /// <summary>
    /// Set the type that exposes the queries of the schema
    /// </summary>
    /// <param name="type">not null</param>
    /// <returns>this, so that the calls may be chained</returns>
    IGraphqlSchemaBuilder AddQueryType (Type type);

    /// <summary>
    /// Declare an empty query type, whose fields all come from the type extensions
    ///
    /// It is the way a schema that spans several domains is built: one type extension per
    /// domain rather than one class that knows them all
    /// </summary>
    /// <returns>this, so that the calls may be chained</returns>
    IGraphqlSchemaBuilder AddQueryType ();

    /// <summary>
    /// Set the type that exposes the mutations of the schema
    /// </summary>
    /// <param name="type">not null</param>
    /// <returns>this, so that the calls may be chained</returns>
    IGraphqlSchemaBuilder AddMutationType (Type type);

    /// <summary>
    /// Declare an empty mutation type, whose fields all come from the type extensions
    ///
    /// <see cref="AddQueryType()"/>
    /// </summary>
    /// <returns>this, so that the calls may be chained</returns>
    IGraphqlSchemaBuilder AddMutationType ();

    /// <summary>
    /// Add the fields of a type extension to the type it extends, typically the query or
    /// the mutation type
    /// </summary>
    /// <param name="type">not null</param>
    /// <returns>this, so that the calls may be chained</returns>
    IGraphqlSchemaBuilder AddTypeExtension (Type type);

    /// <summary>
    /// Add a type to the schema: an object type, an interface, a union, an enum or a scalar
    ///
    /// It is required for the types that are not reachable from the query and mutation types,
    /// and for the ones whose default inference is not the expected one
    /// </summary>
    /// <param name="type">not null</param>
    /// <returns>this, so that the calls may be chained</returns>
    IGraphqlSchemaBuilder AddType (Type type);

    /// <summary>
    /// Associate a runtime type to the schema type that exposes it, so that every field of
    /// this runtime type is exposed with this schema type
    /// </summary>
    /// <param name="runtimeType">not null</param>
    /// <param name="schemaType">not null</param>
    /// <returns>this, so that the calls may be chained</returns>
    IGraphqlSchemaBuilder BindRuntimeType (Type runtimeType, Type schemaType);

    /// <summary>
    /// Should the details of an exception, its stack trace included, be returned to the
    /// client? They are internals of the plugin, so it is off by default
    /// </summary>
    /// <param name="includeExceptionDetails"></param>
    /// <returns>this, so that the calls may be chained</returns>
    IGraphqlSchemaBuilder IncludeExceptionDetails (bool includeExceptionDetails);
  }
}
