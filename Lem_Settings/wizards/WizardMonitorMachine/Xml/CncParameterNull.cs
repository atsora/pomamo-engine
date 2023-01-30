// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using System.Xml;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncParameterNull.
  /// </summary>
  public class CncParameterNull: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterNull).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterNull(XmlNode node) : base(node) {}
    #endregion // Constructors

    #region Methods
    protected override bool Parse(XmlAttributeCollection attributes)
    {
      return true;
    }
    
    protected override Control CreateControl(string defaultValue)
    {
      return null;
    }
    
    protected override string GetValue(Control specializedControl)
    {
      return "";
    }
    
    protected override void SetValue(Control specializedControl, string value)
    {
      
    }
    
    protected override string ValidateInput(Control specializedControl)
    {
      return "";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      return "";
    }
    #endregion // Methods
  }
}
