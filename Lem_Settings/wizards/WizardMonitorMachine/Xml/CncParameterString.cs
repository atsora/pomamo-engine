// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncParameterString.
  /// </summary>
  public class CncParameterString: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterString).FullName);

    #region Getters / Setters
    Regex WellFormedRegex { get; set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterString(XmlNode node) : base(node) {}
    #endregion // Constructors

    #region Methods
    protected override bool Parse(XmlAttributeCollection attributes)
    {
      bool ok = true;
      
      if (attributes["regex"] == null) {
        WellFormedRegex = null;
      }
      else {
        try {
          WellFormedRegex = new Regex(attributes["regex"].Value);
        } catch (Exception) {
          log.Error("Couldn't create a regex with the attribute \"regex\" of the cnc parameter \"" + Name + "\"");
          ok = false;
        }
      }
      
      return ok;
    }
    
    protected override Control CreateControl(string defaultValue)
    {
      var c = new TextBox();
      c.Text = defaultValue;
      return c;
    }
    
    protected override string GetValue(Control specializedControl)
    {
      var c = specializedControl as TextBox;
      return c.Text;
    }
    
    protected override void SetValue(Control specializedControl, string value)
    {
      var c = specializedControl as TextBox;
      c.Text = value;
    }
    
    protected override string ValidateInput(Control specializedControl)
    {
      string error = "";
      
      string answer = GetValue(specializedControl);
      if (WellFormedRegex != null && !WellFormedRegex.IsMatch(answer)) {
        error = "a field doesn't match the requirements: \"" + Description + "\"";
      }

      return error;
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      return "";
    }
    #endregion // Methods
  }
}
