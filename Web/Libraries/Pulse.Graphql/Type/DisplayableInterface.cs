// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// DisplayableInterface
  /// </summary>
  public class DisplayableInterface: InterfaceGraphType<IDisplayable>
  {
    readonly ILog log = LogManager.GetLogger (typeof (DisplayableInterface).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DisplayableInterface ()
    {
      Name = "IDisplayable";
      Field<string> ("display");
    }
  }
}
