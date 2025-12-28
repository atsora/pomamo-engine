// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// List of some base default Ids for Reasons
  /// </summary>
  public enum ReasonId
  {
    /// <summary>
    /// Default / Undefined value
    /// </summary>
    Undefined = 1,
    /// <summary>
    /// Motion
    /// </summary>
    Motion = 2,
    /// <summary>
    /// Short
    /// </summary>
    Short = 3,
    /// <summary>
    /// Unanswered
    /// </summary>
    Unanswered = 4,
    /// <summary>
    /// Unattended
    /// </summary>
    Unattended = 5,
    /// <summary>
    /// Off
    /// </summary>
    Off = 6,
    /// <summary>
    /// Unknown status
    /// </summary>
    Unknown = 7,
    /// <summary>
    /// Processing (an auto or default reason will be set automatically in a close future)
    /// </summary>
    Processing = 8,
    /// <summary>
    /// Break
    /// </summary>
    Break = 9,
  }

  /// <summary>
  /// Model of table Reason
  /// 
  /// Reason that is associated to a period.
  /// The reason is very close to the machine mode.
  /// For each machine mode, a default reason is defined.
  /// </summary>
  public interface IReason
    : IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
    , IComparable
  {
    // Note: IReason does not inherit from IVersionable and IDataWithTranslation
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Long display
    /// 
    /// Reason with its description
    /// </summary>
    string LongDisplay { get; }

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
    /// Code of the reason
    /// </summary>
    string Code { get; set; }

    /// <summary>
    /// Description of the reason
    /// </summary>
    string Description { get; set; }
    
    /// <summary>
    /// Reference to a translation key for the description
    /// </summary>
    string DescriptionTranslationKey { get; set; }
    
    /// <summary>
    /// Description display that is deduced from the translation table
    /// </summary>
    string DescriptionOrTranslation { get; }
    
    /// <summary>
    /// Color to use when the reason is displayed in an application
    /// </summary>
    string Color { get; }
    
    /// <summary>
    /// Color to use when the reason is displayed in a report
    /// </summary>
    string ReportColor { get; }

    /// <summary>
    /// Custom color to override the reason group color
    /// </summary>
    string CustomColor { get; set; }
    
    /// <summary>
    /// Custom report color to override the reason group report color
    /// </summary>
    string CustomReportColor { get; set; }
    
    /// <summary>
    /// Does this reason imply an operation should be automatically set before or after it
    /// </summary>
    LinkDirection LinkOperationDirection { get; set; }
    
    /// <summary>
    /// Reference to its group of reason
    /// </summary>
    IReasonGroup ReasonGroup { get; set; }

    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    int? DisplayPriority { get; set; }

    /// <summary>
    /// Associated production state (nullable)
    /// </summary>
    IProductionState ProductionState { get; set; }
  }


  /// <summary>
  /// IMachineModuleExtensions
  /// </summary>
  public static class IReasonExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IReason data)
    {
      if (ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (data)) {
        return data.ToString ();
      }
      else {
        return $"[{data.GetType ().Name} {data.Id}]";
      }
    }
  }
}
