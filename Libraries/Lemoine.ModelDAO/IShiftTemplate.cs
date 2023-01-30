// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Shifttemplate
  /// </summary>
  public interface IShiftTemplate: IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
  {
    // Note: IShiftTemplate does not inherit from IVersionable 
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name
    /// 
    /// Not null
    /// </summary>
    string Name { get; set; }
     
    /// <summary>
    /// List of items that are part of the shift template
    /// </summary>
    ISet<IShiftTemplateItem> Items { get; }
    
    /// <summary>
    /// Set of breaks
    /// </summary>
    ISet<IShiftTemplateBreak> Breaks { get; }
    
    /// <summary>
    /// Append an item with the specified shift
    /// </summary>
    /// <param name="shift">Can be null because the it is not known yet when a new line is added in the configuration DataGridView</param>
    /// <returns></returns>
    IShiftTemplateItem AddItem (IShift shift);
    
    /// <summary>
    /// Add a break
    /// </summary>
    /// <returns></returns>
    IShiftTemplateBreak AddBreak ();
  }


  /// <summary>
  /// IMachineModuleExtensions
  /// </summary>
  public static class IShiftTemplateExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IShiftTemplate data)
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
