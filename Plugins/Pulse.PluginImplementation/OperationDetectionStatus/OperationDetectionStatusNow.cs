// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Database;

namespace Pulse.PluginImplementation.OperationDetectionStatus
{
  /// <summary>
  /// Return now for  the operation detection status
  /// because the operation never changes
  /// </summary>
  public class OperationDetectionStatusNow
    : IOperationDetectionStatusExtension
  {
    ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusNow).FullName);

    IMachine m_machine;
    int m_priority;

    /// <summary>
    /// Default constructor
    /// </summary>
    public OperationDetectionStatusNow ()
      : this (1)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="priority"></param>
    public OperationDetectionStatusNow (int priority)
    {
      m_priority = priority;
    }

    /// <summary>
    /// IOperationDetectionStatusExtension implementation
    /// </summary>
    public int OperationDetectionStatusPriority
    {
      get { return m_priority; }
    }

    /// <summary>
    /// IExtension implementation
    /// </summary>
    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// IOperationDetectionStatusExtension implementation
    /// </summary>
    /// <returns></returns>
    public DateTime? GetOperationDetectionDateTime ()
    {
      return DateTime.UtcNow;
    }

    /// <summary>
    /// ICycleDetectionStatusExtension implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;

      log = LogManager.GetLogger (typeof (OperationDetectionStatusNow).FullName + "." + machine.Id);

      return true;
    }
  }
}
