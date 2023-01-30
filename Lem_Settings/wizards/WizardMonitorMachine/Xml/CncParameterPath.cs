// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncParameterPath.
  /// </summary>
  public class CncParameterPath: AbstractParameter
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterPath).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterPath(XmlNode node) : base(node) {}
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
      // Davy's note: I don't want to be responsible for the maintenance of this regular expression ;-)
      // https://regex101.com/r/T8vZNl/8/tests
      // Simpler version but that excludes spaces: ^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$
      // https://stackoverflow.com/questions/6416065/c-sharp-regex-for-file-paths-e-g-c-test-test-exe
      // Maybe we can just keep the warning below and remove this error
      return Regex.IsMatch(
        GetValue(specializedControl),
        @"(^([a-z]|[A-Z]):(?=\\(?![\0-\37<>:""/\\|?*])|\/(?![\0-\37<>:""/\\|?*])|$)|^\\(?=[\\\/][^\0-\37<>:""/\\|?*]+)|^(?=(\\|\/)$)|^\.(?=(\\|\/)$)|^\.\.(?=(\\|\/)$)|^(?=(\\|\/)[^\0-\37<>:""/\\|?*]+)|^\.(?=(\\|\/)[^\0-\37<>:""/\\|?*]+)|^\.\.(?=(\\|\/)[^\0-\37<>:""/\\|?*]+))((\\|\/)[^\0-\37<>:""/\\|?*]+|(\\|\/)$)*()$") ?
        "" : "the path is not correct";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      // Try the path
      try {
        return File.Exists(GetValue(specializedControl)) ?
          "" : "the path refers to a file that doesn't exist or is not accessible";
      } catch (Exception e) {
        return e.Message;
      }
    }
    #endregion // Methods
  }
}
