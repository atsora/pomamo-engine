// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// List of some base default Ids for MachineMonitoringType
  /// </summary>
  public enum MachineMonitoringTypeId
  {
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined = 1,
    /// <summary>
    /// Monitored
    /// </summary>
    Monitored = 2,
    /// <summary>
    /// Not monitored
    /// </summary>
    NotMonitored = 3,
    /// <summary>
    /// Outsource
    /// </summary>
    Outsource = 4,
    /// <summary>
    /// Obsolete
    /// </summary>
    Obsolete = 5
  }

  /// <summary>
  /// Model of the machinemonitoringtype table
  /// 
  /// This new table lists the different monitoring types:
  /// <item>Monitored machine</item>
  /// <item>Not monitored machine</item>
  /// <item>Outsource</item>
  /// <item>Obsolete</item>
  /// </summary>
  public interface IMachineMonitoringType
    : IVersionable, IDataWithIdentifiers, IDisplayable, ISelectionable, IReferenceData, IDataWithTranslation, ISerializableModel
  {
  }
}
