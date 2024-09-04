// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Model;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of CellGoalType.
  /// </summary>
  public partial class CellGoalType : UserControl
  {
    #region Events
    /// <summary>
    /// Trigerred when the button "Edit" is clicked
    /// The first argument is the goal type
    /// </summary>
    public event Action<IGoalType> EditClicked;
    
    /// <summary>
    /// Trigerred when the button "Reset" is clicked
    /// The first argument is the goal type
    /// </summary>
    public event Action<IGoalType> ResetClicked;
    #endregion // Events
    
    #region Members
    readonly IGoalType m_goalType;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CellGoalType(IGoalType goalType, string display)
    {
      InitializeComponent();
      
      m_goalType = goalType;
      labelGoalType.Text = display;
    }
    #endregion // Constructors

    #region Event reactions
    void ButtonEditClick(object sender, EventArgs e)
    {
      if (EditClicked != null) {
        EditClicked (m_goalType);
      }
    }
    
    void ButtonResetClick(object sender, EventArgs e)
    {
      if (ResetClicked != null) {
        ResetClicked (m_goalType);
      }
    }
    #endregion // Event reactions
  }
}
