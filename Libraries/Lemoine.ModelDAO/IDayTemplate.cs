// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table daytemplate
  /// </summary>
  public interface IDayTemplate: IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
  {
    // Note: it does not inherit from IVersionable and IDataWithTranslation
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
    /// List of items that are part of the day template
    /// </summary>
    ISet<IDayTemplateItem> Items { get; }
    
    /// <summary>
    /// Add an item
    /// </summary>
    /// <param name="cutOff">cut-off time</param>
    /// <param name="weekDays">Applicable week days</param>
    /// <returns></returns>
    IDayTemplateItem AddItem (TimeSpan cutOff, WeekDay weekDays);
  }


  /// <summary>
  /// IMachineModuleExtensions
  /// </summary>
  public static class IDayTemplateExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IDayTemplate data)
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
