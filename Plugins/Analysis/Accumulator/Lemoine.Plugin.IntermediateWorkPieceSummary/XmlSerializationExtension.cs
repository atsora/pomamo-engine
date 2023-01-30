// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Database;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  public class XmlSerializationExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IXmlSerializationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (XmlSerializationExtension).FullName);

    public Type[] GetExtraTypes ()
    {
      return new Type[] { typeof (IntermediateWorkPieceSummary), typeof (IntermediateWorkPieceByMachineSummary) };
    }
  }
}
