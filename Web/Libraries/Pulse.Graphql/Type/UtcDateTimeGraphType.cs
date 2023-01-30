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

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for DateTime in UTC
  /// </summary>
  public class UtcDateTimeGraphType : ScalarGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (UtcDateTimeGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UtcDateTimeGraphType ()
    {
      Name = "UtcDateTime";
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
        string s => ParseDateTime (s),
        _ => ThrowValueConversionError (value)
      };

    DateTime? ParseDateTime (string s)
    {
      if (string.IsNullOrEmpty (s)) {
        return null;
      }
      if (s.EndsWith ("Z")) {
        try {
          return DateTime.ParseExact (s, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime ();
        }
        catch (Exception) { }
      }
      try {
        return DateTime.ParseExact (s, "o", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
      }
      catch (Exception) { }
      return DateTime.Parse (s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }

    /// <summary>
    /// <see cref="ScalarGraphType"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override object? Serialize (object? value) =>
      value switch {
        null => null,
        DateTime d => d.ToUniversalTime ().ToString ("yyyy-MM-ddTHH:mm:ssZ"),
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
