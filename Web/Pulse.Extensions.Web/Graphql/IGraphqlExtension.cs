// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Pulse.Extensions.Web.Graphql
{
  /// <summary>
  /// Extension point to add a GraphQL schema to a web service
  ///
  /// Each extension owns a named schema, served on its own endpoint: the type names of a
  /// plugin never collide with the ones of another plugin, and a plugin that is not installed
  /// removes its endpoint rather than a part of a shared schema
  /// </summary>
  public interface IGraphqlExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Name of the schema. It must be unique and it is not exposed to the client
    /// </summary>
    string SchemaName { get; }

    /// <summary>
    /// Path of the endpoint the schema is served on. It must start with /
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Configure the schema: query type, mutation type, additional types, ...
    /// </summary>
    /// <param name="builder">builder of the schema named <see cref="SchemaName"/>, not null</param>
    void ConfigureSchema (IGraphqlSchemaBuilder builder);
  }
}
