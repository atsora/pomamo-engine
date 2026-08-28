// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for <see cref="DurationSource">
  /// 
  /// The values are explicitly added so that they keep the same case as in the C# enum
  /// </summary>
  public class DurationSourceGraphType : EnumerationGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (DurationSourceGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DurationSourceGraphType ()
    {
      Name = "DurationSource";
      Add ("Manual", DurationSource.Manual, "The duration was set manually");
      Add ("Auto", DurationSource.Auto, "The duration was determined automatically");
      Add ("Extern", DurationSource.Extern, "The duration comes from an external system");
    }
  }
}
