// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;

namespace WizardChangeProductionQuantities
{
  /// <summary>
  /// Description of RowQuantities.
  /// </summary>
  public class RowQuantities
  {
    #region Getters / Setters
    /// <summary>
    /// Shift
    /// </summary>
    public IShift Shift { get; private set; }
    
    /// <summary>
    /// Name of the shift
    /// </summary>
    public string ShiftName { get; private set; }
    
    /// <summary>
    /// Quantities to display in the row, per component iwp
    /// </summary>
    public IDictionary<IComponentIntermediateWorkPiece, int> Quantities { get; private set; }
    
    /// <summary>
    /// True if different targets are found for a component iwp
    /// </summary>
    public IDictionary<IComponentIntermediateWorkPiece, bool> DifferentValues { get; private set; }
    
    /// <summary>
    /// Modified status of a cell, per component iwp
    /// </summary>
    public IDictionary<IComponentIntermediateWorkPiece, bool> ModifiedStatus { get; private set; }
    
    /// <summary>
    /// True if another production has been found for the shift
    /// </summary>
    public int NumberOfExternalTargets { get; set; }
    
    /// <summary>
    /// Return true if the row comprises no values (or 0)
    /// </summary>
    public bool Empty {
      get {
        ICollection<IComponentIntermediateWorkPiece> ciwps = Quantities.Keys;
        foreach (IComponentIntermediateWorkPiece ciwp in ciwps) {
          if (Quantities[ciwp] != 0 || (DifferentValues[ciwp] && !ModifiedStatus[ciwp])) {
            return false;
          }
        }

        return true;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RowQuantities (IShift shift, string name, ICollection<IComponentIntermediateWorkPiece> ciwps)
    {
      Shift = shift;
      ShiftName = name;
      Quantities = new Dictionary<IComponentIntermediateWorkPiece, int>();
      DifferentValues = new Dictionary<IComponentIntermediateWorkPiece, bool>();
      ModifiedStatus = new Dictionary<IComponentIntermediateWorkPiece, bool>();
      NumberOfExternalTargets = 0;
      
      foreach (IComponentIntermediateWorkPiece ciwp in ciwps) {
        Quantities[ciwp] = 0;
        DifferentValues[ciwp] = false;
        ModifiedStatus[ciwp] = false;
      }
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Set all values as modified
    /// </summary>
    public void SetNotNullModified()
    {
      IList<IComponentIntermediateWorkPiece> ciwps = ModifiedStatus.Keys.ToList();
      foreach (IComponentIntermediateWorkPiece ciwp in ciwps) {
        if (Quantities.ContainsKey(ciwp) && Quantities[ciwp] != 0) {
          DifferentValues[ciwp] = true;
        }
      }
    }
    #endregion // Methods
  }
}
