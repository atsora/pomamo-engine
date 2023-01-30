// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncConfiguratorControl.
  /// </summary>
  public partial class CncConfiguratorControl : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncConfiguratorControl).FullName);

    #region Getters / Setters
    private IList<AbstractParameter> Parameters { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncConfiguratorControl()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize with all field to configure a cnc document
    /// </summary>
    /// <param name="cncDocument"></param>
    /// <param name="advancedUsageAllowed"></param>
    /// <param name="readOnly"></param>
    public void Init(CncDocument cncDocument, bool advancedUsageAllowed, bool readOnly)
    {
      verticalScrollLayout.RemoveAll();
      
      Parameters = cncDocument.Parameters;
      foreach (AbstractParameter parameter in Parameters) {
        if (!parameter.Hidden && (advancedUsageAllowed || !parameter.AdvancedUsage)) {
          Control control = parameter.GetControl(readOnly);
          if (control != null) {
            verticalScrollLayout.AddControl(control);
          }
        }
      }
    }
    
    /// <summary>
    /// Display the old configuration for each parameter
    /// </summary>
    /// <param name="oldDocument"></param>
    /// <param name="advancedUsageAllowed"></param>
    public void ShowOld(CncDocument oldDocument, bool advancedUsageAllowed)
    {
      var oldParameters = oldDocument.Parameters;
      foreach (var parameter in Parameters) {
        if (!parameter.Hidden && (advancedUsageAllowed || !parameter.AdvancedUsage)) {
          // Find the corresponding old parameter
          AbstractParameter oldParameter = null;
          foreach (var tmp in oldParameters) {
            if (tmp.Description == parameter.Description) {
              oldParameter = tmp;
              break;
            }
          }
          
          if (oldParameter != null) {
            parameter.ShowOldValue(oldParameter.GetValue());
          }
        }
      }
    }
    #endregion // Methods
  }
}
