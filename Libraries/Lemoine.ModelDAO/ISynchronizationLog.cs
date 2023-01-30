// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table SynchronizationLog
  /// </summary>
  public interface ISynchronizationLog: IBaseLog
  {
    /// <summary>
    /// XML Element containing the node where the log was recorded
    /// </summary>
    string XmlElement { get; set; }
  }
}
