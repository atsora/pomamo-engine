// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of CellResetGoal.
  /// </summary>
  public partial class CellResetGoal : UserControl
  {
    #region Getters / Setters
    /// <summary>
    /// Company
    /// </summary>
    public ICompany Company { get; private set; }
    
    /// <summary>
    /// Value
    /// </summary>
    public double Value {
      get { return (double)numericUpDown.Value; }
    }
    #endregion // Getters / Setters
    
    static readonly ILog log = LogManager.GetLogger(typeof (CellResetGoal).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CellResetGoal(ICompany company, double defaultValue)
    {
      InitializeComponent();
      
      Company = company;
      labelCompany.Text = company.Display;
      numericUpDown.Value = (decimal)defaultValue;
    }
    #endregion // Constructors
  }
}
