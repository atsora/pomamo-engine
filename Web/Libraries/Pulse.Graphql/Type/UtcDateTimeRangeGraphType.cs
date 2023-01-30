// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using GraphQL.Types;
using GraphQLParser.AST;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for <see cref="UtcDateTimeRange">
  /// </summary>
  public class UtcDateTimeRangeGraphType : ScalarGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (UtcDateTimeRangeGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UtcDateTimeRangeGraphType ()
    {
      Name = "UtcDateTimeRange";
    }

    /// <summary>
    /// <see cref="ScalarGraphType"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override object? ParseLiteral (GraphQLValue value) =>
      value switch {
        GraphQLNullValue => null,
        GraphQLStringValue stringValue => ParseValue ((string)stringValue.Value),
        _ => ThrowLiteralConversionError (value)
      };

    /// <summary>
    /// <see cref="ScalarGraphType"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override object? ParseValue (object? value) =>
      value switch {
        null => null,
        string s => ParseRange (s),
        _ => ThrowValueConversionError (value)
      };

    UtcDateTimeRange? ParseRange (string s)
    {
      if (string.IsNullOrEmpty (s)) {
        return null;
      }
      else {
        return new UtcDateTimeRange (s);
      }
    }

    /// <summary>
    /// <see cref="ScalarGraphType"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override object? Serialize (object? value) =>
      value switch {
        null => null,
        UtcDateTimeRange r => r.ToString (x => x.ToUniversalTime ().ToString ("yyyy-MM-ddTHH:mm:ssZ")),
        _ => ThrowSerializationError (value)
      };

    /// <summary>
    /// <see cref="ScalarGraphType"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool CanParseLiteral (GraphQLValue value) =>
      value switch {
        GraphQLNullValue => true,
        GraphQLStringValue => true,
        _ => false
      };

    /// <summary>
    /// <see cref="ScalarGraphType"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool CanParseValue (object? value) =>
      value switch {
        null => true,
        string => true,
        _ => false
      };
  }
}
