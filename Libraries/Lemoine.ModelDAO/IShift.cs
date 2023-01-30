// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Shift
  /// </summary>
  public interface IShift
    : IDisplayable, ISelectionable, IDataWithVersion, ISerializableModel
    , IComparable
  {
    // Note: IShift does not inherit from IVersionable and IDataWithTranslation
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
    /// Code
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// External Code
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Color that is associated to the machine mode
    /// 
    /// Not null
    /// </summary>
    string Color { get; set; }
    
    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    int? DisplayPriority { get; set; }
  }

  /// <summary>
  /// IMachineModuleExtensions
  /// </summary>
  public static class IShiftExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IShift data)
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
