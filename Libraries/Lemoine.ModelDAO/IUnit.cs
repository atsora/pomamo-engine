// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// List of some base default Ids for Unit
  /// </summary>
  public enum UnitId
  {
    /// <summary>
    /// Feed rate
    /// </summary>
    FeedRate = 1,
    /// <summary>
    /// Feedrate US (IPM)
    /// </summary>
    FeedRateUS = 2,
    /// <summary>
    /// Rotation speed (RPM)
    /// </summary>
    RotationSpeed = 3,
    /// <summary>
    /// Percent (%)
    /// </summary>
    Percent = 4,
    /// <summary>
    /// Number of parts
    /// </summary>
    NumberOfParts = 5,
    /// <summary>
    /// No unit
    /// </summary>
    None = 6,
    /// <summary>
    /// Distance (mm)
    /// </summary>
    DistanceMillimeter = 7,
    /// <summary>
    /// Distance (inch)
    /// </summary>
    DistanceInch =8,
    /// <summary>
    /// Distance (m)
    /// </summary>
    DistanceMeter = 9,
    /// <summary>
    /// Distance (feet)
    /// </summary>
    DistanceFeet = 10,
    /// <summary>
    /// Duration (s)
    /// </summary>
    DurationSeconds = 11,
    /// <summary>
    /// Duration (min)
    /// </summary>
    DurationMinutes = 12,
    /// <summary>
    /// Duration (h)
    /// </summary>
    DurationHours = 13,
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = 14,
    /// <summary>
    /// Number of times a tool is used
    /// </summary>
    ToolNumberOfTimes = 15,
    /// <summary>
    /// ToolWear
    /// </summary>
    ToolWear = 16,
    /// <summary>
    /// Number of cycles
    /// </summary>
    NumberOfCycles = 17,
    /// <summary>
    /// Flow rate (L/s)
    /// </summary>
    FlowRate = 18,
  }
  
  /// <summary>
  /// Model of table Unit
  /// 
  /// This new table lists the used units and their translation. This is mainly referenced by the Field table.
  /// 
  /// You can use either the Unit Name column or the Unit Translation Key column.
  /// The other column is set to null.
  /// </summary>
  public interface IUnit: IDataWithIdentifiers, ISerializableModel, ISelectionable, IDataWithVersion
  {
    // Note: IUnit does not inherit from IVersionable and IDataWithTranslation
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name (or used abbrevation)
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// TranslationKey
    /// </summary>
    string TranslationKey { get; set; }
    
    /// <summary>
    /// Display name that is deduced from the translation table
    /// </summary>
    string Display { get; }

    /// <summary>
    /// Description of the unit
    /// </summary>
    string Description { get; set; }
  }


  /// <summary>
  /// IMachineModuleExtensions
  /// </summary>
  public static class IUnitExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IUnit data)
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
