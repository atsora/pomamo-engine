// Copyright (C) 2025 Atsora Solutions
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
  /// Additional parameter in database that is not in the XML configuration file
  /// </summary>
  public class CncParameterAdditional : AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncParameterAdditional).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterAdditional (string key) : base (key) { }

    protected override bool Parse (XmlAttributeCollection attributes)
    {
      return true;
    }

    protected override Control CreateControl (string defaultValue)
    {
      var c = new TextBox ();
      c.Text = defaultValue;
      return c;
    }

    protected override string GetValue (Control specializedControl)
    {
      var c = specializedControl as TextBox;
      return c.Text;
    }

    protected override void SetValue (Control specializedControl, string value)
    {
      var c = specializedControl as TextBox;
      c.Text = value;
    }

    protected override string ValidateInput (Control specializedControl)
    {
      return "";
    }

    protected override string GetWarning (Control specializedControl)
    {
      return "";
    }
  }
}
