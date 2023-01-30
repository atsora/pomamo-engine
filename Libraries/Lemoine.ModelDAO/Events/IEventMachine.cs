// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// IEventMachine: IEvent for a specific machine
  /// </summary>
  public interface IEventMachine: IEvent, IPartitionedByMachine
  {
  }
}
