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
  /// Use the already set tool name for the sequence
  /// </summary>
  public class ToolNameIsSequenceName: ISequenceNamer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ToolNameIsSequenceName).FullName);

    readonly StampingData m_stampingData;

    /// <summary>
    /// Constructor
    /// </summary>
    public ToolNameIsSequenceName (StampingData stampingData)
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
      var toolName = m_stampingData.GetString ("ToolName");
      if (toolName is null) {
        var toolDiameter = m_stampingData.Get<double?> ("ToolDiameter");
        var toolRadius = m_stampingData.Get<double?> ("ToolRadius");
        if (toolDiameter.HasValue) {
          if (toolRadius.HasValue) {
            return $"({toolDiameter.Value} {toolRadius.Value})";
          }
          else {
            return $"({toolDiameter.Value} )";
          }
        }
        else if (toolRadius.HasValue) {
          return $"( {toolRadius.Value})";
        }
        else if (!string.IsNullOrEmpty (m_stampingData.ToolNumber)) {
          return $"T{m_stampingData.ToolNumber}";
        }
        else { // No tool information
          log.Warn ($"GetSequenceName: no tool information, consider sequence order {sequenceOrder}");
          return $"Seq{sequenceOrder}";
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetSequenceName: toolName={toolName}");
        }
        return toolName;
      }
    }
  }
}
