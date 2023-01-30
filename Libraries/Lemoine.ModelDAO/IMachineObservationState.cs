// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Default MachineObservationState Ids
  /// </summary>
  public enum MachineObservationStateId
  {
    /// <summary>
    /// Machine ON with operator (attended)
    /// </summary>
    Attended = 1,
    /// <summary>
    /// Machine ON without operator
    /// </summary>
    Unattended = 2,
    /// <summary>
    /// Machine ON with operator (on-site)
    /// </summary>
    OnSite = 3,
    /// <summary>
    /// Machine ON with on call operator (off-site)
    /// </summary>
    OnCall = 4,
    /// <summary>
    /// Machine OFF
    /// </summary>
    Off = 5,
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = 6,
    /// <summary>
    /// Set-up
    /// </summary>
    SetUp = 7,
    /// <summary>
    /// Quality check
    /// </summary>
    QualityCheck = 8,
    /// <summary>
    /// Production
    /// </summary>
    Production = 9,
    /// <summary>
    /// Maintenance
    /// </summary>
    Maintenance = 10,
    /// <summary>
    /// Break
    /// </summary>
    Break = 11,
    /// <summary>
    /// Clean-up
    /// </summary>
    Cleanup = 12,
    /// <summary>
    /// Weekend
    /// </summary>
    Weekend = 13,
    /// <summary>
    /// Night
    /// </summary>
    Night = 14,
  }

  /// <summary>
  /// Model of table MachineObservationState
  /// 
  /// This table lists the different machine observation states:
  /// <item>attended</item>
  /// <item>unattended</item>
  /// <item>on-site</item>
  /// <item>on-call</item>
  /// <item>machine OFF</item>
  /// </summary>
  public interface IMachineObservationState : IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
  {
    // Note: IReason does not inherit from IVersionable and IDataWithTranslation
    //       else the corresponding properties can't be used in a DataGridView binding

    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Translation key
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string TranslationKey { get; set; }

    /// <summary>
    /// Translated name of the object (if applicable, else the name of the object)
    /// </summary>
    string NameOrTranslation { get; }

    /// <summary>
    /// Is a user required for this observation state ?
    /// </summary>
    bool UserRequired { get; set; }

    /// <summary>
    /// Is a shift required for this observation state ?
    /// </summary>
    bool ShiftRequired { get; set; }

    /// <summary>
    /// Does this Machine Observation State mean the associated user
    /// is on site ?
    /// 
    /// null in case of not applicable
    /// </summary>
    bool? OnSite { get; set; }

    /// <summary>
    /// In which new MachineObservationState should this MachineObservationState
    /// be changed in case the site attendance change of the associated user
    /// changes ?
    /// </summary>
    IMachineObservationState SiteAttendanceChange { get; set; }

    /// <summary>
    /// Does this machine observation state imply an operation should be automatically set before or after it
    /// </summary>
    LinkDirection LinkOperationDirection { get; set; }

    /// <summary>
    /// Does it correspond to a production time ?
    /// </summary>
    bool IsProduction { get; set; }
  }
}
