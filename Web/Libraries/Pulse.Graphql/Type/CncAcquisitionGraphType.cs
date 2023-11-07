// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL.Types;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Model;
using Pulse.Graphql.InputType;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for <see cref="ICncAcquisition"/>
  /// </summary>
  public class CncAcquisitionGraphType : ObjectGraphType<ICncAcquisition>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncAcquisitionGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncAcquisitionGraphType ()
    {
      Name = "CncAcquisition";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<NonNullGraphType<CncConfigGraphType>> ("cncConfig")
        .Resolve (ctx => new CncConfig (ctx.Source.ConfigFile));
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<CncConfigParamValueGraphType>>>> ("parameters")
        .Resolve (ctx => ConvertConfigParameters (ctx.Source.ConfigParameters).ToList ());
    }

    IEnumerable<CncConfigParamValue> ConvertConfigParameters (string parameters)
    {
      var parray = EnumerableString.ParseListString (parameters);
      for (int i = 0; i < parray.Length; ++i) { 
        yield return new CncConfigParamValue ($"Param{i+1}", parray[i]);
      }
    }
  }
}
