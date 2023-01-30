// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncParameterIP.
  /// </summary>
  public class CncParameterIP: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterIP).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterIP(XmlNode node) : base(node) {}
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
      IPAddress ip;
      return IPAddress.TryParse(GetValue(specializedControl), out ip) ?
        "" : "the IP address is not correct";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      // Try to ping the IP
      try {
        return Util.Ping(GetValue(specializedControl));
      } catch (Exception e) {
        return e.Message;
      }
    }
    #endregion // Methods
  }
}
