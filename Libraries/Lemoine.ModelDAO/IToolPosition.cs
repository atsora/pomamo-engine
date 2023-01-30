// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Core.SharedData;
using Lemoine.Model;

namespace Lemoine.Model
{
  /// <summary>
  /// Extension methods to the ToolState enum
  /// </summary>
  public static class ToolStateExtensions
  {
    static ToolState s_minAvailable = ToolState.Available;
    static ToolState s_minTempUnavailable = ToolState.TemporaryUnavailable;
    static ToolState s_minDefUnavailable = ToolState.DefinitelyUnavailable;
    static ToolState s_minOther = ToolState.Other;
    
    /// <summary>
    /// Return true is the state is available
    /// </summary>
    /// <param name="toolState"></param>
    /// <returns></returns>
    public static bool IsAvailable(this ToolState toolState)
    {
      return toolState >= s_minAvailable && toolState < s_minTempUnavailable;
    }
    
    /// <summary>
    /// Return true is the state is temporary unavailable
    /// </summary>
    /// <param name="toolState"></param>
    /// <returns></returns>
    public static bool IsTemporaryUnavailable(this ToolState toolState)
    {
      return toolState >= s_minTempUnavailable && toolState < s_minDefUnavailable;
    }
    
    /// <summary>
    /// Return true is the state is definitely unavailable
    /// </summary>
    /// <param name="toolState"></param>
    /// <returns></returns>
    public static bool IsDefinitelyUnavailable(this ToolState toolState)
    {
      return toolState >= s_minDefUnavailable && toolState < s_minOther;
    }
    
    /// <summary>
    /// Return the name of the state
    /// </summary>
    /// <param name="toolState"></param>
    /// <param name="shortName"></param>
    /// <returns></returns>
    public static string Name(this ToolState toolState, bool shortName)
    {
      string txt = "";
      
      switch (toolState) {
        case ToolState.Unknown:
          txt = I18N.PulseCatalog.GetString("ToolStateUnknown");
          break;
        case ToolState.Available:
          txt = I18N.PulseCatalog.GetString("ToolStateAvailable");
          break;
        case ToolState.New:
          txt = I18N.PulseCatalog.GetString("ToolStateNew");
          break;
        case ToolState.NotUsedNotNew:
          txt = I18N.PulseCatalog.GetString("ToolStateAlreadyUsed");
          break;
        case ToolState.TemporaryUnavailable:
          txt = I18N.PulseCatalog.GetString("ToolStateTempUnavailable");
          break;
        case ToolState.Used:
          txt = I18N.PulseCatalog.GetString("ToolStateUsedByCurrentMachine");
          break;
        case ToolState.Reserved:
          txt = I18N.PulseCatalog.GetString("ToolStateReservedByAnotherMachine");
          break;
        case ToolState.Busy:
          txt = I18N.PulseCatalog.GetString("ToolStateUsedByAnotherMachine");
          break;
        case ToolState.Measurement:
          txt = I18N.PulseCatalog.GetString("ToolStateMeasurement");
          break;
        case ToolState.Reconditioning:
          txt = I18N.PulseCatalog.GetString("ToolStateReconditioned");
          break;
        case ToolState.NotAppropriate:
          txt = I18N.PulseCatalog.GetString("ToolStateNotAppropriate");
          break;
        case ToolState.DefinitelyUnavailable:
          txt = I18N.PulseCatalog.GetString("ToolStateDefUnavailable");
          break;
        case ToolState.Expired:
          txt = I18N.PulseCatalog.GetString("ToolStateExpired");
          break;
        case ToolState.Broken:
          txt = I18N.PulseCatalog.GetString("ToolStateBroken");
          break;
        case ToolState.NotRegistered:
          txt = I18N.PulseCatalog.GetString("ToolStateNotRegistered");
          break;
        case ToolState.Other:
          txt = I18N.PulseCatalog.GetString("ToolStateOther");
          break;
        case ToolState.Unused:
          txt = I18N.PulseCatalog.GetString("ToolStateUnused");
          break;
      }
      
      if (shortName) {
        string[] txts = txt.Split('(', ')');
        if (txts.Length == 3) {
          txt = txts[1][0].ToString().ToUpper() + txts[1].Substring(1);
        }
      }
      
      return txt;
    }
  }
  
  /// <summary>
  /// Description of IToolPosition.
  /// </summary>
  public interface IToolPosition: IDisplayable, IVersionable, IDataWithId, IPartitionedByMachineModule
  {
    /// <summary>
    /// Monitored machine
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; }
    
    /// <summary>
    /// Magazine (first level)
    /// </summary>
    int? Magazine { get; set; }
    
    /// <summary>
    /// Pot (second level)
    /// </summary>
    int? Pot { get; set; }
    
    /// <summary>
    /// Tool number (T)
    /// </summary>
    string ToolNumber { get; set; }
    
    /// <summary>
    /// Tool id (unique per machine module)
    /// </summary>
    string ToolId { get; }
    
    /// <summary>
    /// Current tool state
    /// </summary>
    ToolState ToolState { get; set; }
    
    /// <summary>
    /// Datetime when the tool disappeared (if it is not considered as a removal)
    /// </summary>
    DateTime? LeftDateTime { get; set; }

    /// <summary>
    /// Datetime when the tool life changed for the last time
    /// </summary>
    DateTime? LifeChangedDateTime { get; set; }

    /* Properties from the json column in the database */

    /// <summary>
    /// Get a property from the json column in the database
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns>can be null</returns>
    object GetProperty(string propertyName);
    
    /// <summary>
    /// Set a property into the json column in the database
    /// List of known properties
    /// * LengthCompensation (double)
    /// * CutterCompensation (double)
    /// * GeometryUnit (IUnit)
    /// * ATCSpeed (int: 0=normal, 1=slow, 2=middle)
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    void SetProperty(string propertyName, object value);
    
    /// <summary>
    /// Get all properties
    /// </summary>
    IDictionary<string, object> Properties { get; }
  }
}
