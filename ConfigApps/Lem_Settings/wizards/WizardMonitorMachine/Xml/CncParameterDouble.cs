// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncParameterDouble.
  /// </summary>
  public class CncParameterDouble: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterDouble).FullName);

    #region Getters / Setters
    Decimal Min { get; set; }
    Decimal Max { get; set; }
    int DecimalPlace { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterDouble(XmlNode node) : base(node) {}
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
          Min = Decimal.Parse(attributes["min"].Value);
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
          Max = Decimal.Parse(attributes["max"].Value, CultureInfo.InvariantCulture);
        } catch (Exception) {
          log.Error("Couldn't read the attribute \"max\" of the cnc parameter \"" + Name + "\"");
          ok = false;
        }
      }
      
      if (attributes["decimal"] == null) {
        DecimalPlace = 3;
      }
      else {
        try {
          DecimalPlace = Int32.Parse(attributes["decimal"].Value, CultureInfo.InvariantCulture);
        } catch (Exception) {
          log.Error("Couldn't read the attribute \"decimal\" of the cnc parameter \"" + Name + "\"");
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
      c.DecimalPlaces = DecimalPlace;
      Decimal val = Min;
      if (defaultValue != "") {
        try {
          val = Decimal.Parse(defaultValue, CultureInfo.InvariantCulture);
        } catch (Exception) {
          log.Error("The default value of the parameter \"" + Name + "\" couldn't be parsed as a decimal.");
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
      return c.Value.ToString().Replace(",", ".");
    }
    
    protected override void SetValue(Control specializedControl, string value)
    {
      var c = specializedControl as NumericUpDown;
      c.Value = (decimal)double.Parse(value.Replace(",", "."));
    }
    
    protected override string ValidateInput(Control specializedControl)
    {
      var c = specializedControl as NumericUpDown;
      return String.IsNullOrEmpty(c.Text) ? "a value cannot be empty when a decimal number is required" : "";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      // No warnings
      return "";
    }
    #endregion // Methods
  }
}
