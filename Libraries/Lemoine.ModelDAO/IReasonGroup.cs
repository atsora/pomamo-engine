// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// List of some base default Ids for Reasons
  /// </summary>
  public enum ReasonGroupId
  {
    /// <summary>
    /// Default
    /// </summary>
    Default = 1,
    /// <summary>
    /// Motion
    /// </summary>
    Motion = 2,
    /// <summary>
    /// Short
    /// </summary>
    Short = 3,
    // 4: Unanswered
    /// <summary>
    /// Auto-reasons
    /// </summary>
    Auto = 5,
    /// <summary>
    /// Idle
    /// </summary>
    Idle = 6,
    /// <summary>
    /// Unknown status
    /// </summary>
    Unknown = 7,
  }

  /// <summary>
  /// Model of table ReasonGroup
  /// 
  /// Group of reasons.
  /// This allows to classify the different reasons
  /// and to make some more compact charts in some reports.
  /// </summary>
  public interface IReasonGroup
    : IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
    , IComparable
  {
    // Note: IReason does not inherit from IVersionable and IDataWithTranslation
    //       else the corresponding properties can't be used in a DataGridView binding

    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Long display: reason group with its description
    /// </summary>
    string LongDisplay { get; }

    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Translated name of the object (if applicable, else the name of the object)
    /// </summary>
    string NameOrTranslation { get; }
    
    /// <summary>
    /// Translation key
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string TranslationKey { get; set; }
    
    /// <summary>
    /// Description
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string Description { get; set; }
    
    /// <summary>
    /// Reference to a translation key for the description
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string DescriptionTranslationKey { get; set; }

    /// <summary>
    /// Description display that is deduced from the translation table
    /// </summary>
    string DescriptionOrTranslation { get; }

    /// <summary>
    /// Color to use when the reason group is displayed in an application
    /// </summary>
    string Color { get; set; }
    
    /// <summary>
    /// Set of reasons that are part of this reason group
    /// </summary>
    ICollection<IReason> Reasons { get; }

    /// <summary>
    /// Color to use when the reason group is displayed in a report
    /// </summary>
    string ReportColor { get; set; }

    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    int? DisplayPriority { get; set; }
  }
}
