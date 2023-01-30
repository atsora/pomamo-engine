// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using Lemoine.Model;

namespace Lemoine.ConfigControls
{
  /// <summary>Wrapper containing a list of ReasonSelection
  /// Used for representation as/in row in ReasonSelectionConfig
  /// </summary>
  internal class ReasonSelectionWrapper
  {
    public ReasonSelectionWrapper()
    {
      this.Reason = null;
      this.Selectable = false;
      this.DetailsRequired = false;
      this.MachineFilter = null;
      this.reasonSelections = new List<IReasonSelection>();
    }
    
    #region Members
    //Not with m_ convention for Binding reason
    public String Id { get; set; }
    public IReason Reason { get; set; }
    public bool Selectable { get; set; }
    public bool DetailsRequired { get; set; }
    public IMachineFilter MachineFilter { get; set; }

    public IList<IReasonSelection> reasonSelections {get; set;}
    
    public Color BackColor {get; set;}
    public Color FontColor {get; set;}
    #endregion
    
    #region Functions
    /// <summary>
    /// Update all object in List if !null &amp; !empty
    /// </summary>
    public void pushModification(){
      if(null != reasonSelections & 0 != reasonSelections.Count){
        foreach(IReasonSelection reasonSelection in this.reasonSelections){
          reasonSelection.Reason = this.Reason;
          reasonSelection.Selectable = this.Selectable;
          reasonSelection.DetailsRequired = this.DetailsRequired;
          reasonSelection.MachineFilter = this.MachineFilter;
        }
      }
    }
    
    /// <summary>
    /// Check if List contain a ReasonSelection with same attributes without ID.
    /// <param name='reasonSelectionToCmp'>ReasonSelection to compare</param>      
    /// <returns>True if founded False if nothing</returns>
    /// </summary>
    public Boolean checkIfContain(IReasonSelection reasonSelectionToCmp){
      foreach(IReasonSelection reasonSelection in this.reasonSelections){
        if(Object.Equals(reasonSelection.DetailsRequired, reasonSelectionToCmp.DetailsRequired) &&
           Object.Equals(reasonSelection.Selectable, reasonSelectionToCmp.Selectable) &&
           reasonSelection.Reason.Equals(reasonSelectionToCmp.Reason) &&
           Object.Equals(reasonSelection.MachineFilter, reasonSelectionToCmp.MachineFilter)){
          return true;
        }
      }
      return false;
    }
    
    /// <summary>
    /// Check if List contain a ReasonSelection with same Reason
    /// <param name='reasonSelectionToCmp'>ReasonSelection to compare</param>      
    /// <returns>True if founded False if nothing</returns>
    /// </summary>
    public bool checkIfContainSimilareReasonSelection(IReasonSelection reasonSelectionToCmp){
      foreach(IReasonSelection reasonSelection in this.reasonSelections){
        if(reasonSelection.Reason.Equals(reasonSelectionToCmp.Reason)){
          return true;
        }
      }
      return false;      
    }
    
    /// <summary>
    /// Return with ReasonSelection with same Reason
    /// </summary>
    /// <param name="reasonSelectionToCmp"></param>
    /// <returns></returns>
    public IReasonSelection findSimilareReasonSelection(IReasonSelection reasonSelectionToCmp){
      foreach(IReasonSelection reasonSelection in this.reasonSelections){
        if(reasonSelection.Reason.Equals(reasonSelectionToCmp.Reason)){
          return reasonSelection;
        }
      }
      return null;
    }
    
    #region Equals and GetHashCode implementation
    public override bool Equals(object obj)
    {
      ReasonSelectionWrapper other = obj as ReasonSelectionWrapper;
      if (other == null) {
        return false;
      }

      return this.Id == other.Id && object.Equals(this.Reason, other.Reason) && this.Selectable == other.Selectable && this.DetailsRequired == other.DetailsRequired && object.Equals(this.MachineFilter, other.MachineFilter) && object.Equals(this.reasonSelections, other.reasonSelections) && this.BackColor == other.BackColor && this.FontColor == other.FontColor;
    }
    
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (Id != null) {
          hashCode += 1000000007 * Id.GetHashCode();
        }

        if (Reason != null) {
          hashCode += 1000000009 * Reason.GetHashCode();
        }

        hashCode += 1000000021 * Selectable.GetHashCode();
        hashCode += 1000000033 * DetailsRequired.GetHashCode();
        if (MachineFilter != null) {
          hashCode += 1000000087 * MachineFilter.GetHashCode();
        }

        if (reasonSelections != null) {
          hashCode += 1000000093 * reasonSelections.GetHashCode();
        }

        hashCode += 1000000097 * BackColor.GetHashCode();
        hashCode += 1000000103 * FontColor.GetHashCode();
      }
      return hashCode;
    }
    
    #endregion
    
    #endregion
  }
}
