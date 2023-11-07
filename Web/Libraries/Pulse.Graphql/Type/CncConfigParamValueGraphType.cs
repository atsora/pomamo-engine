// Copyright (c) 2023 Atsora Solutions

using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pulse.Graphql.InputType
{
  /// <summary>
  /// Graphql type for <see cref="CncConfigParamValue"/>
  /// </summary>
  public class CncConfigParamValueGraphType : ObjectGraphType<CncConfigParamValue>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncConfigParamValueGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncConfigParamValueGraphType ()
    {
      Name = "CncConfigParamValue";
      Field<NonNullGraphType<StringGraphType>> ("name");
      Field<string> ("value", nullable: true);
    }
  }
}
