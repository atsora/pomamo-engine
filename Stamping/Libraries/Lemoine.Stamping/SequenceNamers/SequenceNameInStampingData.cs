// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Lemoine.Core.Log;

namespace Lemoine.Stamping.SequenceNamers
{
  /// <summary
  /// Use the SequenceName property of StampingData as the sequence name
  /// 
  /// Use as fallback the sequence order
  /// </summary>
  public class SequenceNameInStampingData : ISequenceNamer
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceNameInStampingData).FullName);

    readonly StampingData m_stampingData;

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceNameInStampingData (StampingData stampingData)
    {
      m_stampingData = stampingData;
    }

    /// <summary>
    /// <see cref="ISequenceNamer"/>
    /// </summary>
    /// <param name="sequenceOrder"></param>
    /// <returns></returns>
    public string GetSequenceName (int sequenceOrder)
    {
      if (m_stampingData.TryGet ("SequenceName", out string? sequenceName)) {
        return sequenceName??$"Seq{sequenceOrder}";
      }
      else {
        return $"Seq{sequenceOrder}";
      }
    }
  }
}
