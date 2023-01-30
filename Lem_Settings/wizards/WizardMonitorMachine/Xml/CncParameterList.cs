// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncParameterList.
  /// </summary>
  public class CncParameterList: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterList).FullName);

    #region Getters / Setters
    private IList<string> Values { get; set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterList(XmlNode node) : base(node) {}
    #endregion // Constructors

    #region Methods
    protected override bool Parse(XmlAttributeCollection attributes)
    {
      bool ok = true;
      
      if (attributes["values"] == null) {
        ok = false;
      }
      else {
        Values = attributes["values"].Value.Split('|').ToList();
        if (Values.Count < 2) {
          ok = false;
        }
      }
      
      return ok;
    }
    
    protected override Control CreateControl(string defaultValue)
    {
      var c = new ComboBox();
      c.DropDownStyle = ComboBoxStyle.DropDownList;
      foreach (string str in Values) {
        c.Items.Add(str);
      }

      c.SelectedIndex = 0;
      if (defaultValue != "") {
        c.SelectedText = defaultValue;
      }

      return c;
    }
    
    protected override string GetValue(Control specializedControl)
    {
      var c = specializedControl as ComboBox;
      string str = c.GetItemText(c.SelectedItem);
      
      return str;
    }
    
    protected override void SetValue(Control specializedControl, string value)
    {
      var c = specializedControl as ComboBox;
      c.SelectedIndex = c.FindString(value);
    }
    
    protected override string ValidateInput(Control specializedControl)
    {
      return String.IsNullOrEmpty(GetValue(specializedControl)) ?
        "the selection in the list is empty" : "";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      // No warnings
      return "";
    }
    #endregion // Methods
  }
}
