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
  /// Description of CncParameterBool.
  /// </summary>
  public class CncParameterBool: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterBool).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterBool(XmlNode node) : base(node) {}
    #endregion // Constructors

    #region Methods
    protected override bool Parse(XmlAttributeCollection attributes)
    {
      return true;
    }
    
    protected override Control CreateControl(string defaultValue)
    {
      var c = new CheckBox();
      c.Checked = ParseBool(defaultValue, false);
      return c;
    }
    
    protected override string GetValue(Control specializedControl)
    {
      var c = specializedControl as CheckBox;
      return c.Checked ? "true" : "false";
    }
    
    protected override void SetValue(Control specializedControl, string value)
    {
      var c = specializedControl as CheckBox;
      c.Checked = !String.IsNullOrEmpty(value) && ParseBool(value, true);
    }
    
    protected override string ValidateInput(Control specializedControl)
    {
      // Always ok
      return "";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      // No warnings
      return "";
    }
    
    bool ParseBool(string txt, bool throwException)
    {
      txt = (txt ?? "").ToLower();
      if (txt == "true" || txt == "1") {
        return true;
      }

      if (txt == "" || txt == "false" || txt == "0") {
        return false;
      }

      var error = String.Format("value \"{0}\" of the parameter \"{1}\" couldn't be parsed as a bool.", txt, Name);
      log.Error(error);
      if (throwException) {
        throw new Exception(error);
      }

      return false;
    }
    #endregion // Methods
  }
}
