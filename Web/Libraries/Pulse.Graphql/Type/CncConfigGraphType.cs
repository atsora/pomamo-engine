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
  /// Graphql type for <see cref="CncConfigMachine"/>
  /// </summary>
  public class CncConfigMachineGraphType : ObjectGraphType<CncConfigMachine>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncConfigMachineGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncConfigMachineGraphType ()
    {
      Name = "CncConfigMachine";
      Field<NonNullGraphType<StringGraphType>> ("text");
      Field<string> ("model", nullable: true);
    }
  }

  /// <summary>
  /// Graphql type for <see cref="CncConfigControl"/>
  /// </summary>
  public class CncConfigControlGraphType : ObjectGraphType<CncConfigControl>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncConfigControlGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncConfigControlGraphType ()
    {
      Name = "CncConfigControl";
      Field<NonNullGraphType<StringGraphType>> ("text");
      Field<string> ("version", nullable: true);
    }
  }

  /// <summary>
  /// Graphql type for <see cref="CncConfigProtocol"/>
  /// </summary>
  public class CncConfigProtocolGraphType : ObjectGraphType<CncConfigProtocol>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncConfigProtocolGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncConfigProtocolGraphType ()
    {
      Name = "CncConfigProtocol";
      Field<NonNullGraphType<StringGraphType>> ("text");
      Field<string> ("version", nullable: true);
    }
  }

  /// <summary>
  /// Graphql type for <see cref="IComponent"/>
  /// </summary>
  public class CncConfigGraphType : ObjectGraphType<CncConfig>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncConfigGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncConfigGraphType ()
    {
      Name = "CncConfig";
      Field<NonNullGraphType<StringGraphType>> ("name");
      Field<string> ("description", nullable: true);
      Field<ListGraphType<NonNullGraphType<CncConfigMachineGraphType>>, IEnumerable<CncConfigMachine>> ("supportedMachines");
      Field<ListGraphType<NonNullGraphType<CncConfigControlGraphType>>, IEnumerable<CncConfigControl>> ("supportedControls");
      Field<ListGraphType<NonNullGraphType<CncConfigProtocolGraphType>>, IEnumerable<CncConfigProtocol>> ("supportedProtocols");
      Field<string> ("unit", nullable: true);
      Field<ListGraphType<NonNullGraphType<CncConfigParamGraphType>>, IEnumerable<CncConfigParam>> ("parameters")
        .Resolve (ctx => ctx.Source.Parameters.Where (x => !x.Hidden && !x.Type.Equals ("null", StringComparison.InvariantCultureIgnoreCase)));
    }
  }
}
