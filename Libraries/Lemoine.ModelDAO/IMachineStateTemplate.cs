// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Machine state template category
  /// </summary>
  public enum MachineStateTemplateCategory {
    /// <summary>
    /// Production
    /// </summary>
    Production = 1,
    /// <summary>
    /// Set-up
    /// </summary>
    SetUp = 2,
  }
  
  /// <summary>
  /// Model of table machinestatetemplate
  /// </summary>
  public interface IMachineStateTemplate: IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
  {
    // Note: IMachineStateTemplate does not inherit from IVersionable 
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
    /// Machine state template category
    /// </summary>
    MachineStateTemplateCategory? Category { get; set; }
    
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
    /// In which new MachineStateTemplate should this MachineStateTemplate
    /// be changed in case the site attendance change of the associated user
    /// changes ?
    /// </summary>
    IMachineStateTemplate SiteAttendanceChange { get; set; }

    /// <summary>
    /// List of items that are part of the machine state template
    /// </summary>
    IList<IMachineStateTemplateItem> Items { get; }
    
    /// <summary>
    /// Set of stop conditions
    /// </summary>
    ISet<IMachineStateTemplateStop> Stops { get; }
    
    /// <summary>
    /// Append an item with the specified machine observation state
    /// </summary>
    /// <param name="machineObservationState">Can be null because the it is not known yet when a new line is added in the configuration DataGridView</param>
    /// <returns></returns>
    IMachineStateTemplateItem AddItem (IMachineObservationState machineObservationState);
    
    /// <summary>
    /// Insert an item at the specified position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="machineObservationState">Can be null because the it is not known yet when a new line is added in the configuration DataGridView</param>
    /// <returns></returns>
    IMachineStateTemplateItem InsertItem (int position, IMachineObservationState machineObservationState);
    
    /// <summary>
    /// Add a stop condition
    /// </summary>
    /// <returns></returns>
    IMachineStateTemplateStop AddStop ();
    
    /// <summary>
    /// Does this machine state template imply an operation should be automatically set before or after it
    /// </summary>
    LinkDirection LinkOperationDirection { get; set; }

    /// <summary>
    /// Optional color
    /// </summary>
    string Color { get; set; }

    /// <summary>
    /// Dynamic end to be applied when there is no upper bound in range in MachineStateTemplateAssociation
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string DynamicEnd { get; set; }

    /// <summary>
    /// Next machine state template to use when dynamic end is analyzed
    /// 
    /// Nullable foreign key to another machine state template
    /// </summary>
    IMachineStateTemplate NextMachineStateTemplate { get; set; }
  }

  /// <summary>
  /// IMachineStateTemplateExtensions
  /// </summary>
  public static class IMachineStateTemplateExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IMachineStateTemplate data)
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
