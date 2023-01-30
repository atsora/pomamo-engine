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
  /// Description of CncParameterInt.
  /// </summary>
  public class CncParameterInt: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterInt).FullName);

    #region Getters / Setters
    private int Min { get; set; }
    private int Max { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterInt(XmlNode node) : base(node) {}
    #endregion // Constructors

    #region Methods
    protected override bool Parse(XmlAttributeCollection attributes)
    {
      bool ok = true;
      
      if (attributes["min"] == null) {
        Min = -2000000000;
      }
      else {
        try {
          Min = Int32.Parse(attributes["min"].Value);
        } catch (Exception) {
          log.Error("Couldn't read the attribute \"min\" of the cnc parameter \"" + Name + "\"");
          ok = false;
        }
      }
      
      if (attributes["max"] == null) {
        Max = 2000000000;
      }
      else {
        try {
          Max = Int32.Parse(attributes["max"].Value);
        } catch (Exception) {
          log.Error("Couldn't read the attribute \"max\" of the cnc parameter \"" + Name + "\"");
          ok = false;
        }
      }
      
      return ok;
    }
    
    protected override Control CreateControl(string defaultValue)
    {
      var c = new NumericUpDown();
      c.Minimum = Min;
      c.Maximum = Max;
      
      int val = Min;
      if (defaultValue != "") {
        try {
          val = Int32.Parse(defaultValue);
        } catch (Exception) {
          log.Error("The default value of the parameter \"" + Name + "\" couldn't be parsed as an int.");
        }
        if (val < Min) {
          val = Min;
        }
        else if (val > Max) {
          val = Max;
        }
      } else {
        if (Min <= 0 && Max >= 0) {
          val = 0;
        }
      }
      c.Value = val;
      c.Text = val.ToString();
      
      return c;
    }
    
    protected override string GetValue(Control specializedControl)
    {
      var c = specializedControl as NumericUpDown;
      return c.Value.ToString();
    }
    
    protected override void SetValue(Control specializedControl, string value)
    {
      var c = specializedControl as NumericUpDown;
      c.Value = (decimal)int.Parse(value);
    }
    
    protected override string ValidateInput(Control specializedControl)
    {
      var c = specializedControl as NumericUpDown;
      return String.IsNullOrEmpty(c.Text) ? "a value cannot be empty when a number is required" : "";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      // No warnings
      return "";
    }
    #endregion // Methods
  }
}
