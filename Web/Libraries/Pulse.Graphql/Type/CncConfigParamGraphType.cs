// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for <see cref="CncConfigParam"/>
  /// </summary>
  public class CncConfigParamGraphType : ObjectGraphType<CncConfigParam>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncConfigParamGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncConfigParamGraphType ()
    {
      Name = "CncConfigParam";
      Field<NonNullGraphType<StringGraphType>> ("name");
      Field<string> ("label", nullable: true);
      Field<string> ("description", nullable: true);
      Field<string> ("type", nullable: true);
      Field<bool> ("optional", nullable: true);
      Field<int> ("min", nullable: true);
      Field<int> ("max", nullable: true);
      Field<string> ("default", nullable: true);
      Field<bool> ("advanced", nullable: true);
      Field<ListGraphType<NonNullGraphType<StringGraphType>>, IEnumerable<string>> ("values")
        .Resolve (ctx => ctx.Source.Values); // TODO: subdirectory
      Field<int> ("decimal", nullable: true);
      Field<string> ("regex", nullable: true);
    }
  }
}
