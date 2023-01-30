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
  /// Graphql type for <see cref="SequenceKind">
  /// </summary>
  public class SequenceKindGraphType : ScalarGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceKindGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceKindGraphType ()
    {
      Name = "SequenceKind";
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
        GraphQLIntValue => ParseValue((int?)(new IntGraphType().ParseLiteral (value))),
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
        string s => string.IsNullOrEmpty (s) ? null: Enum.Parse<SequenceKind> (s, true),
        int x => (SequenceKind)x,
        _ => ThrowValueConversionError (value)
      };

    /// <summary>
    /// <see cref="ScalarGraphType"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override object? Serialize (object? value) =>
      value switch {
        null => null,
        SequenceKind x => x.ToString (),
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
        GraphQLIntValue => true,
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
        int => true,
        _ => false
      };
  }
}
