// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using System.Drawing;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Wrapper containing a list of MachineModeDefault
  /// Used for representation as/in row in MachineModeDefaultReasonConfig
  /// </summary>
  internal class MachineModeDefaultReasonWrapper
  {
    
    #region Getters / Setters
    public String Id {get; set;}
    public TimeSpan? MaximumDuration {get; set;}
    public IReason Reason {get; set;}
    public double Score { get; set; }
    public bool Auto { get; set; }
    public bool OverwriteRequired {get; set;}
    public IMachineFilter IncludeMachineFilter {get; set;}
    public IMachineFilter ExcludeMachineFilter {get; set;}
    public IList<IMachineModeDefaultReason> machineModeDefaultReasons {get; set;}
    
    public Color BackColor {get; set;}
    public Color FontColor {get; set;}
    #endregion // Getters / Setters
    
    #region Constructor
    public MachineModeDefaultReasonWrapper()
    {
      Id = null;
      MaximumDuration = null;
      Reason = null;
      Score = 0.0;
      Auto = false;
      OverwriteRequired = false;
      IncludeMachineFilter = null;
      ExcludeMachineFilter = null;
      machineModeDefaultReasons = new List<IMachineModeDefaultReason>();
      BackColor = Color.White;
      FontColor = Color.Black;
    }
    #endregion // Constructor
    
    #region Methods
    /// <summary>
    /// Update all object in List if !null &amp; !empty
    /// </summary>
    public void pushModification() {
      if (machineModeDefaultReasons != null && machineModeDefaultReasons.Count != 0) {
        foreach(IMachineModeDefaultReason machineModeDefaultReason in this.machineModeDefaultReasons) {
          machineModeDefaultReason.MaximumDuration = this.MaximumDuration;
          if (this.Reason != null) {
            machineModeDefaultReason.Reason = this.Reason;
          }

          machineModeDefaultReason.Score = this.Score;
          machineModeDefaultReason.Auto = this.Auto;
          machineModeDefaultReason.OverwriteRequired = this.OverwriteRequired;
          machineModeDefaultReason.IncludeMachineFilter = this.IncludeMachineFilter;
          machineModeDefaultReason.ExcludeMachineFilter = this.ExcludeMachineFilter;
        }
      }
    }
    
    /// <summary>
    /// Check if List contain a MachineModeDefaultReason with same attributes without ID.
    /// <param name='machineModeDefaultReasonToCmp'>MachineModeDefaultReason to compare</param>      
    /// <returns>True if founded False if nothing</returns>
    /// </summary>
    public Boolean checkIfContain(IMachineModeDefaultReason machineModeDefaultReasonToCmp) {
      foreach (IMachineModeDefaultReason machineModeDefaultReason in this.machineModeDefaultReasons){
        if (machineModeDefaultReason.MaximumDuration.Equals(machineModeDefaultReasonToCmp.MaximumDuration) &&
             machineModeDefaultReason.Reason.Equals(machineModeDefaultReasonToCmp.Reason ) &&
             (machineModeDefaultReason.Score == machineModeDefaultReasonToCmp.Score) &&
             (machineModeDefaultReason.Auto == machineModeDefaultReasonToCmp.Auto) &&
             Object.Equals(machineModeDefaultReason.OverwriteRequired, machineModeDefaultReasonToCmp.OverwriteRequired) &&
             Object.Equals(machineModeDefaultReason.IncludeMachineFilter, machineModeDefaultReasonToCmp.IncludeMachineFilter) &&
             Object.Equals(machineModeDefaultReason.ExcludeMachineFilter, machineModeDefaultReasonToCmp.ExcludeMachineFilter)){
          return true;
        }
      }      
      return false;
    }
    
    /// <summary>
    /// Check if contain a MachineModeDefaultReason with same Reason
    /// </summary>
    /// <returns></returns>
    public bool checkIfContainSimilaryMachineModeDefaultReason(IMachineModeDefaultReason machineModeDefaultReasonToCmp) {
      foreach(IMachineModeDefaultReason machineModeDefaultReason in this.machineModeDefaultReasons) {
        if (machineModeDefaultReason.Reason.Equals(machineModeDefaultReasonToCmp.Reason)) {
          return true;
        }
      }      
      return false;
    }
    
    /// <summary>
    /// Check if contain a MachineModeDefaultReason with same Reason
    /// </summary>
    /// <returns></returns>
    public IMachineModeDefaultReason findSimilaryMachineModeDefaultReason(IMachineModeDefaultReason machineModeDefaultReasonToCmp){
      foreach(IMachineModeDefaultReason machineModeDefaultReason in this.machineModeDefaultReasons){
        if(machineModeDefaultReason.Reason.Equals(machineModeDefaultReasonToCmp.Reason )){
          return machineModeDefaultReason;
        }
      }      
      return null;
    }
    #endregion // Methods
    
    #region Equals and GetHashCode implementation
    public override bool Equals(object obj)
    {
      MachineModeDefaultReasonWrapper other = obj as MachineModeDefaultReasonWrapper;
      if (other == null) {
        return false;
      }

      return this.Id == other.Id && this.MaximumDuration == other.MaximumDuration && object.Equals (this.Reason, other.Reason)
        && this.Score == other.Score
        && this.Auto == other.Auto
        && this.OverwriteRequired == other.OverwriteRequired
        && object.Equals(this.IncludeMachineFilter, other.IncludeMachineFilter)
        && object.Equals(this.ExcludeMachineFilter, other.ExcludeMachineFilter)
        && object.Equals(this.machineModeDefaultReasons, other.machineModeDefaultReasons)
        && this.BackColor == other.BackColor && this.FontColor == other.FontColor;
    }
    
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (Id != null) {
          hashCode += 1000000007 * Id.GetHashCode();
        }

        hashCode += 1000000009 * MaximumDuration.GetHashCode();
        if (Reason != null) {
          hashCode += 1000000021 * Reason.GetHashCode();
        }

        hashCode += 1000000023 * Score.GetHashCode ();
        hashCode += 1000000025 * Auto.GetHashCode ();
        hashCode += 1000000033 * OverwriteRequired.GetHashCode();
        if (IncludeMachineFilter != null) {
          hashCode += 1000000087 * IncludeMachineFilter.GetHashCode();
        }

        if (ExcludeMachineFilter != null) {
          hashCode += 1000000093 * ExcludeMachineFilter.GetHashCode();
        }

        if (machineModeDefaultReasons != null) {
          hashCode += 1000000097 * machineModeDefaultReasons.GetHashCode();
        }

        hashCode += 1000000103 * BackColor.GetHashCode();
        hashCode += 1000000123 * FontColor.GetHashCode();
      }
      return hashCode;
    }
    #endregion
  }
}
