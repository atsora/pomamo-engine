// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.SequenceNamers
{
  /// <summary>
  /// OrderIsSequenceName
  /// </summary>
  public class OrderIsSequenceName: ISequenceNamer
  {
    readonly ILog log = LogManager.GetLogger (typeof (OrderIsSequenceName).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OrderIsSequenceName ()
    {
    }

    /// <summary>
    /// <see cref="ISequenceNamer"/>
    /// </summary>
    /// <param name="sequenceOrder"></param>
    /// <returns></returns>
    public string GetSequenceName (int sequenceOrder)
    {
      return $"Seq{sequenceOrder}";
    }
  }
}
