// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Extensions.Database;
using Lemoine.Model;
using Lemoine.Core.Log;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.AllCyclesFull
{
  /// <summary>
  /// Description of OperationCycleFullExtension.
  /// </summary>
  public class OperationCycleFullExtension: IOperationCycleFullExtension
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycleFullExtension).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OperationCycleFullExtension ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IOperationCycleFullExtension implementation

    public bool? IsFull(IOperationCycle operationCycle)
    {
      return true;
    }

    #endregion

    #region IExtension implementation

    public bool UniqueInstance {
      get {
        return true;
      }
    }

    #endregion
  }
}
