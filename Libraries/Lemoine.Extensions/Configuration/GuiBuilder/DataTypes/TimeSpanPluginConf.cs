// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder.DataTypes
{
  /// <summary>
  /// Plugin conf to configure TimeSpan values
  /// </summary>
  public class TimeSpanPluginConf
    : TimeSpanAsTextPluginConf
    , IPluginConfDataType
  {
  }
}
