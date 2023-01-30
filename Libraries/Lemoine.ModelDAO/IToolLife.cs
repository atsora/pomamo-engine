// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.SharedData;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.Model
{
  /// <summary>
  /// Extension methods to the LifeType enum
  /// </summary>
  public static class ToolLifeExtensions
  {
    /// <summary>
    /// Return the LifeType unit
    /// </summary>
    /// <param name="lifeType"></param>
    /// <returns></returns>
    public static string Name(this ToolUnit lifeType)
    {
      string txt = "";
      
      switch (lifeType) {
        case ToolUnit.Unknown:
          txt = I18N.PulseCatalog.GetString("UnitUnknown");
          break;
        case ToolUnit.TimeSeconds:
          txt = I18N.PulseCatalog.GetString("UnitSeconds");
          break;
        case ToolUnit.Parts:
          txt = I18N.PulseCatalog.GetString("UnitParts");
          break;
        case ToolUnit.NumberOfTimes:
          txt = I18N.PulseCatalog.GetString("UnitTimes");
          break;
        case ToolUnit.Wear:
          txt = I18N.PulseCatalog.GetString("UnitWear");
          break;
        case ToolUnit.DistanceMillimeters:
          txt = I18N.PulseCatalog.GetString("UnitMillimeters");
          break;
        case ToolUnit.DistanceInch:
          txt = I18N.PulseCatalog.GetString("UnitInches");
          break;
      }
      
      return txt;
    }
  }
  
  /// <summary>
  /// Description of IToolLife.
  /// </summary>
  public interface IToolLife: IDataWithVersion, IDataWithId, IPartitionedByMachineModule
  {
    /// <summary>
    /// Machine
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; }
    
    /// <summary>
    /// Position
    /// 
    /// not null
    /// </summary>
    IToolPosition Position { get; }
    
    /// <summary>
    /// Different way to count the life of a tool
    /// If down, the initial state is Limit, the final state is 0
    /// If up, the initial state is 0, the final state is Limit
    /// </summary>
    ToolLifeDirection Direction { get; }
    
    /// <summary>
    /// Current value of the tool life
    /// Normally increasing if UP
    /// Normally decreasing if DOWN
    /// </summary>
    double Value { get; set; }
    
    /// <summary>
    /// Absolute value defining the warning
    /// 
    /// May be null if the warning is not configured
    /// </summary>
    double? Warning { get; set; }
    
    /// <summary>
    /// Total value of the life
    /// Can be the initial value that is decreased if the direction is down
    /// Can be the final value defining the expiration point of a increasing life (up)
    /// May be null if the limit is not configured.
    /// </summary>
    double? Limit { get; set; }
    
    /// <summary>
    /// Unit of the values (type of life)
    /// </summary>
    IUnit Unit { get; set; }
  }
  
  /// <summary>
  /// Extension methods to IToolLife
  /// </summary>
  public static class IToolLifeExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IToolLifeExtensions).FullName);

    /// <summary>
    /// Test if the limit value was reached
    /// </summary>
    /// <returns></returns>
    public static bool IsLimitReached (this IToolLife toolLife)
    {
      return toolLife.Limit.HasValue
        && toolLife.Limit.Value > 0
        && ((toolLife.Direction == Lemoine.Core.SharedData.ToolLifeDirection.Up
             && toolLife.Value >= toolLife.Limit.Value)
            || (toolLife.Direction == Lemoine.Core.SharedData.ToolLifeDirection.Down
                && toolLife.Value <= 0));
    }
    
    /// <summary>
    /// Test if the warning value was reached
    /// </summary>
    /// <returns></returns>
    public static bool IsWarningReached (this IToolLife toolLife)
    {
      return toolLife.Warning.HasValue
        && toolLife.Warning.Value > 0
        && ( (toolLife.Direction == Lemoine.Core.SharedData.ToolLifeDirection.Up
              && toolLife.Value >= toolLife.Warning.Value)
            || (toolLife.Direction == Lemoine.Core.SharedData.ToolLifeDirection.Down
                && toolLife.Value <= toolLife.Warning.Value));
    }
    
    /// <summary>
    /// Remaining life to reach the limit
    /// </summary>
    /// <param name="toolLife"></param>
    /// <returns></returns>
    public static double? GetRemainingLifeToLimit (this IToolLife toolLife)
    {
      switch (toolLife.Direction) {
        case ToolLifeDirection.Up:
          if (toolLife.Limit.HasValue && (0 < toolLife.Limit.Value)) {
            return toolLife.Limit.Value - toolLife.Value;
          }
          else {
            return null;
          }
        case ToolLifeDirection.Down:
          return toolLife.Value;
        case ToolLifeDirection.Unknown:
        default:
          return null;
      }
    }

    /// <summary>
    /// Remaining life duration to reach the limit in case the unit is a duration limit.
    /// 
    /// If the unit is not a duration limit, raise an exception
    /// </summary>
    /// <param name="toolLife"></param>
    /// <returns>The returned time may be negative</returns>
    public static TimeSpan? GetRemainingLifeDurationToLimit (this IToolLife toolLife)
    {
      double? remainingLifeToLimit = toolLife.GetRemainingLifeToLimit ();
      if (!remainingLifeToLimit.HasValue) {
        return null;
      }
      switch (toolLife.Unit.Id) {
        case (int)UnitId.DurationHours:
          return TimeSpan.FromHours (remainingLifeToLimit.Value);
        case (int)UnitId.DurationMinutes:
          return TimeSpan.FromMinutes (remainingLifeToLimit.Value);
        case (int)UnitId.DurationSeconds:
          return TimeSpan.FromSeconds (remainingLifeToLimit.Value);
        default:
          log.ErrorFormat ("GetRemainingLifeDurationToLimit: invalid unit {0}", toolLife.Unit.Id);
          throw new InvalidOperationException ("Invalid tool life unit");
      }
    }

    /// <summary>
    /// Remaining life to reach the warning level
    /// </summary>
    /// <param name="toolLife"></param>
    /// <returns></returns>
    public static double? GetRemainingLifeToWarning (this IToolLife toolLife)
    {
      switch (toolLife.Direction) {
        case ToolLifeDirection.Up:
          if (toolLife.Warning.HasValue) {
            return toolLife.Warning.Value - toolLife.Value;
          }
          else {
            return null;
          }
        case ToolLifeDirection.Down:
          if (toolLife.Warning.HasValue) {
            return toolLife.Value - toolLife.Warning.Value;
          }
          else {
            return null;
          }
        case ToolLifeDirection.Unknown:
        default:
          return null;
      }
    }
  }
  
  /// <summary>
  /// Tool id comparer
  /// </summary>
  public class ToolLifeToolIdComparer: IEqualityComparer<IToolLife>
  {
    #region IEqualityComparer implementation
    /// <summary>
    /// IEqualityComparer implementation
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool Equals(IToolLife x, IToolLife y)
    {
      if (object.Equals (x, y)) {
        return true;
      }
      if ( (null == x) || (null == y)) {
        return false;
      }
      return object.Equals (x.Position.ToolId, y.Position.ToolId);
    }
    
    /// <summary>
    /// IEqualityComparer implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode(IToolLife obj)
    {
      if (null == obj) {
        return 0;
      }
      else {
        return obj.Position.ToolId.GetHashCode ();
      }
    }
    #endregion
  }
}
