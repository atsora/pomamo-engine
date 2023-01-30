// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of AbstractParameter.
  /// </summary>
  public abstract class AbstractParameter
  {
    #region Members
    Label m_label = null;
    TextBox m_oldLabel = null;
    CheckBox m_checkbox = null;
    Control m_specializedControl = null;
    TableLayoutPanel m_panel = null;
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (AbstractParameter).FullName);
    
    #region Getters / Setters
    /// <summary>
    /// Return true if the parameter is valid
    /// </summary>
    public bool IsValid { get; private set; }
    
    /// <summary>
    /// If true, only advanced user should have access to this parameter
    /// </summary>
    public bool AdvancedUsage { get; private set; }
    
    /// <summary>
    /// Get the default value if specified
    /// </summary>
    public string DefaultValue { get; private set; }
    
    /// <summary>
    /// True if a default value is specified, the value is not optional
    /// and the value is not set in the database yet
    /// </summary>
    public bool DefaultIsUsed { get; private set; }
    
    /// <summary>
    /// True if this parameter is optional
    /// </summary>
    public bool Optional { get; private set; }
    
    /// <summary>
    /// If true, the control is never displayed
    /// (useful in particular situations with deprecated files)
    /// </summary>
    public bool Hidden { get; private set; }
    
    /// <summary>
    /// Description of the parameter
    /// </summary>
    public string Description { get; private set; }
    
    /// <summary>
    /// True if "Parameter" is used instead of "ParamX"
    /// </summary>
    public bool OldConfiguration { get; private set; }
    
    /// <summary>
    /// Control to configure the parameter
    /// </summary>
    /// <param name="readOnly"></param>
    /// <returns></returns>
    public Control GetControl(bool readOnly)
    {
      if (m_panel == null) {
        CreateControl ();
      }

      if (m_specializedControl == null) {
        return null;
      }

      if (readOnly) {
        EnableControl(m_specializedControl, false);
        m_checkbox.Enabled = false;
      }
      
      return m_panel;
    }
    
    /// <summary>
    /// Show the old value just below the field to enter the new value
    /// </summary>
    /// <param name="value"></param>
    public void ShowOldValue(string value)
    {
      if (m_specializedControl != null) {
        if (string.IsNullOrEmpty(value)) {
          m_panel.Height = 48;
          m_panel.RowStyles[2].Height = 0;
          m_oldLabel.Text = "";
          m_oldLabel.Hide();
        } else {
          m_panel.Height = 72;
          m_panel.RowStyles[2].Height = 24;
          m_oldLabel.Text = "Old: " + value;
          m_oldLabel.Show();
        }
      }
    }
    
    protected string Name { get; private set; }
    protected int CncAcquisitionId { get; private set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="node"></param>
    protected AbstractParameter(XmlNode node)
    {
      XmlAttributeCollection attributes = node.Attributes;
      Name = attributes["name"].Value;
      OldConfiguration = string.Equals(Name, "Parameter", StringComparison.CurrentCultureIgnoreCase);
      IsValid = Parse(attributes);
      
      if (IsValid) {
        // Fill common properties
        Description = node.InnerText;
        DefaultValue = attributes["default"] == null ? "" : attributes["default"].Value;
        Optional = (attributes["optional"] != null && attributes["optional"].Value.ToLower() == "true");
        if (attributes["advanced"] == null) {
          AdvancedUsage = false;
        }
        else {
          AdvancedUsage = (attributes["advanced"].Value.ToLower() == "true");
          
          // A default value must be present if the parameter is not optional
          if (attributes["optional"] == null && attributes["default"] == null) {
            log.Error("The parameter \"" + Name + "\" is not optional and has no " +
                      "default value so we must fill its value. But this is only for advanced users.");
            IsValid = false;
          }
        }
        if (attributes["hidden"] == null) {
          Hidden = false;
        }
        else {
          Hidden = (attributes["hidden"].Value.ToLower() == "true");
          
          // A default value must be present if the parameter is not optional
          if (attributes["optional"] == null && attributes["default"] == null) {
            log.Error("The parameter \"" + Name + "\" is not optional and has no " +
                      "default value so we must fill its value. But this is never displayed.");
            IsValid = false;
          }
        }
      }
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Get the value configured by the user
    /// </summary>
    /// <returns></returns>
    public string GetValue()
    {
      if (m_specializedControl == null) {
        // It's possible that the control has not been shown
        // (if the user is not superAdmin for example)
        // But a default value may be present
        return DefaultValue;
      }
      
      return m_checkbox.Checked ? GetValue(m_specializedControl) : "";
    }
    
    /// <summary>
    /// Set a value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public void SetValue(string value)
    {
      if (m_panel == null) {
        CreateControl ();
      }

      DefaultIsUsed = false;
      if (m_specializedControl != null) {
        if (string.IsNullOrEmpty(value)) {
          if (Optional) {
            m_checkbox.Checked = false;
            EnableControl(m_specializedControl, false);
          } else {
            // A default value must be specified
            if (string.IsNullOrEmpty(DefaultValue)) {
              throw new Exception("Parameter '" + Name + "' is mandatory and has no default value");
            }

            // The default value is used
            DefaultIsUsed = true;
          }
        } else {
          m_checkbox.Checked = true;
          EnableControl(m_specializedControl, true);
          SetValue(m_specializedControl, value);
          if (!string.IsNullOrEmpty(ValidateInput(m_specializedControl))) {
            throw new Exception("Value '" + value + "' for parameter '" + Name + "' is not valid");
          }
        }
      }
    }
    
    /// <summary>
    /// Set the acquisition id
    /// </summary>
    /// <param name="cncAcquisitionId"></param>
    public void SetCncAcquisitionId(int cncAcquisitionId)
    {
      CncAcquisitionId = cncAcquisitionId;
    }
    
    /// <summary>
    /// Return a message if the parameter is badly set
    /// </summary>
    /// <returns></returns>
    public string GetError()
    {
      return (m_specializedControl == null || !m_checkbox.Checked) ?
        "" : ValidateInput(m_specializedControl);
    }
    
    /// <summary>
    /// Return a message if the parameter has a warning
    /// </summary>
    /// <returns></returns>
    public string GetWarning()
    {
      return m_specializedControl == null ? "" : GetWarning(m_specializedControl);
    }
    
    /// <summary>
    /// Dispose all controls previously created
    /// </summary>
    public void DisposeAllControls()
    {
      if (m_specializedControl != null) {
        m_specializedControl.Dispose();
        m_specializedControl = null;
      }
      if (m_checkbox != null) {
        m_checkbox.Dispose();
        m_checkbox = null;
      }
      if (m_label != null) {
        m_label.Dispose();
        m_label = null;
      }
      if (m_panel != null) {
        m_panel.Dispose();
        m_panel = null;
      }
    }
    
    void CreateControl()
    {
      m_panel = new TableLayoutPanel();
      
      // Get the specialized control
      m_specializedControl = CreateControl(DefaultValue);
      if (m_specializedControl == null) {
        return;
      }

      m_specializedControl.Dock = DockStyle.Fill;
      
      // Prepare the description
      m_label = new Label();
      m_label.Dock = DockStyle.Fill;
      m_label.Text = Description;
      m_label.TextAlign = ContentAlignment.MiddleLeft;
      m_label.AutoEllipsis = true;
      
      // Prepare the label for the old configuration
      m_oldLabel = new TextBox();
      m_oldLabel.ReadOnly = true;
      m_oldLabel.Dock = DockStyle.Fill;
      m_oldLabel.Font = new Font(m_oldLabel.Font, FontStyle.Italic);
      m_oldLabel.BorderStyle = BorderStyle.None;
      m_oldLabel.BackColor = SystemColors.Control;
      m_oldLabel.ForeColor = SystemColors.ControlDarkDark;
      
      // Prepare the checkbox
      m_checkbox = new CheckBox();
      m_checkbox.Dock = DockStyle.Fill;
      m_checkbox.CheckedChanged += OnCheckboxChecked;
      m_checkbox.CheckAlign = ContentAlignment.MiddleLeft;
      
      // Join the whole
      m_panel.Dock = DockStyle.Fill;
      m_panel.Margin = new Padding(0, 3, 0, 3);
      m_panel.Height = 48;
      m_panel.RowCount = 3;
      m_panel.ColumnCount = 2;
      m_panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute));
      m_panel.RowStyles.Add(new RowStyle(SizeType.Absolute));
      m_panel.RowStyles.Add(new RowStyle(SizeType.Absolute));
      m_panel.RowStyles.Add(new RowStyle(SizeType.Absolute));
      m_panel.Controls.Add(m_label, 0, 0);
      m_panel.SetColumnSpan(m_label, 2);
      m_panel.Controls.Add(m_checkbox, 0, 1);
      m_panel.Controls.Add(m_specializedControl, 1, 1);
      m_panel.Controls.Add(m_oldLabel, 1, 2);
      
      // Optional or not?
      if (Optional) {
        m_panel.ColumnStyles[0].Width = 25;
        EnableControl(m_specializedControl, false);
        m_checkbox.Checked = false;
      } else {
        m_panel.ColumnStyles[0].Width = 1;
        m_checkbox.Checked = true;
        m_checkbox.Hide();
      }
      
      // By default, old label is not shown
      m_panel.RowStyles[0].Height = 24;
      m_panel.RowStyles[1].Height = 24;
      m_panel.RowStyles[2].Height = 0;
      m_oldLabel.Hide();
    }
    
    static public void EnableControl(Control c, bool enabled)
    {
      var textBox = c as TextBox;
      var numeric = c as NumericUpDown;
      if (textBox != null) {
        textBox.ReadOnly = !enabled;
        textBox.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
        textBox.ForeColor = enabled ? SystemColors.ControlText : SystemColors.ControlDarkDark;
      } else if (numeric != null) {
        numeric.ReadOnly = !enabled;
        numeric.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
        numeric.ForeColor = enabled ? SystemColors.ControlText : SystemColors.ControlDarkDark;
        numeric.Increment = enabled ? 1 : 0;
      } else {
        c.Enabled = enabled;
      }
    }
    
    protected abstract bool Parse(XmlAttributeCollection attributes);
    protected abstract Control CreateControl(string defaultValue);
    protected abstract string GetValue(Control specializedControl);
    protected abstract void SetValue(Control specializedControl, string value);
    protected abstract string ValidateInput(Control specializedControl);
    protected abstract string GetWarning(Control specializedControl);
    #endregion // Methods
    
    #region Event reactions
    void OnCheckboxChecked(Object sender, EventArgs e)
    {
      EnableControl(m_specializedControl, m_checkbox.Checked);
    }
    #endregion // Event reactions
  }
}
