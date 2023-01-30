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
  /// Description of CncParameterHost.
  /// Can be a host or an ip
  /// </summary>
  public class CncParameterHost: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterUrl).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterHost(XmlNode node) : base(node) {}
    #endregion // Constructors

    #region Methods
    protected override bool Parse(XmlAttributeCollection attributes)
    {
      return true;
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
      string hostOrIP = GetValue(specializedControl);
      if (string.IsNullOrEmpty(hostOrIP)) {
        return "the IP address or host name is empty";
      }

      // Check that there are no special characters
      var regexItem = new Regex("^[-a-zA-Z0-9.]*$");
      return regexItem.IsMatch(hostOrIP) ? "" : "the host name or IP address should comprise no special characters";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      // Try to ping the host or IP
      try {
        return Util.Ping(GetValue(specializedControl));
      } catch (Exception e) {
        return e.Message;
      }
    }
    #endregion // Methods
  }
}
