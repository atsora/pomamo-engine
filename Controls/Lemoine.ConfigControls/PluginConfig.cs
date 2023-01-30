// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Lemoine.Model;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// <see cref="Lemoine.Extensions.Configuration.GuiBuilder.IPluginConfig"/>
  /// </summary>
  public partial class PluginConfig
    : UserControl
    , Lemoine.Extensions.Configuration.GuiBuilder.IPluginConfig
  {
    bool m_isInitialized = false;
    bool m_initialLoad = false;
    IDictionary<string, Action> m_loaders = new Dictionary<string, Action> ();
    IDictionary<string, Func<object>> m_getters = new Dictionary<string, Func<object>> ();
    IDictionary<string, Action<object>> m_setters = new Dictionary<string, Action<object>> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public PluginConfig ()
    {
      InitializeComponent ();
    }

    /// <summary>
    /// Getter
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object Get (string name)
    {
      var getter = m_getters[name];
      return getter ();
    }

    /// <summary>
    /// Setter
    /// </summary>
    /// <param name="name"></param>
    /// <param name="v"></param>
    public void Set (string name, object v)
    {
      var setter = m_setters[name];
      setter (v);
    }

    private void PluginConfig_Load (object sender, EventArgs e)
    {
      InitialLoad ();
      return;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.GuiBuilder.IPluginConfig"/>
    /// </summary>
    public void BeforeAddingControls ()
    {
      tableLayoutPanel1.SuspendLayout ();
      this.SuspendLayout ();
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.GuiBuilder.IPluginConfig"/>
    /// </summary>
    public void AfterAddingControls ()
    {
      tableLayoutPanel1.ResumeLayout (false);
      tableLayoutPanel1.PerformLayout ();
      this.ResumeLayout (false);
      m_isInitialized = true;
    }

    void AddControl (string name, string labelText, Control control)
    {
      m_isInitialized = false; // You must call AfterAddingControls after that

      const int labelHeight = 13;
      var height = control.Height > labelHeight ? control.Height : labelHeight;

      var panel = new System.Windows.Forms.Panel ();
      panel.SuspendLayout ();
      panel.Dock = System.Windows.Forms.DockStyle.Fill;
      panel.Location = new System.Drawing.Point (3, 3);
      panel.Name = name + "Panel";
      panel.Size = new System.Drawing.Size (426, height+6);
      panel.TabIndex = 0;

      ++tableLayoutPanel1.RowCount;
      var rowNumber = tableLayoutPanel1.RowCount - 2;
      tableLayoutPanel1.RowStyles.Insert (rowNumber, new RowStyle (SizeType.Absolute, height+12));
      tableLayoutPanel1.Controls.Add (panel, 0, rowNumber);

      var label = new System.Windows.Forms.Label ();
      label.AutoSize = true;
      label.Location = new System.Drawing.Point (3, 6);
      label.Name = name + "Label";
      label.Size = new System.Drawing.Size (10, labelHeight);
      label.TabIndex = 0;
      label.Text = labelText;

      var labelSize = TextRenderer.MeasureText (labelText, label.Font);

      control.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      control.Location = new System.Drawing.Point (labelSize.Width + 9, 3);
      control.Name = name + "Control";
      control.Size = new System.Drawing.Size (426 - (labelSize.Width + 9) - 3, height);
      control.TabIndex = 1;

      panel.Controls.Add (label);
      panel.Controls.Add (control);

      panel.PerformLayout ();
      panel.ResumeLayout ();
    }

    CheckBox AddOptionalControl (string name, string labelText, Control control)
    {
      CheckBox checkBox;
      var optionalControl = CreateOptionalControl (name, control, out checkBox);
      AddControl (name, labelText, optionalControl);
      return checkBox;
    }

    Control CreateOptionalControl (string name, Control control, out CheckBox checkBox)
    {
      const int checkBoxHeight = 18;
      var height = checkBoxHeight < control.Height ? control.Height : checkBoxHeight;

      var panel = new System.Windows.Forms.Panel ();
      panel.SuspendLayout ();
      panel.Dock = System.Windows.Forms.DockStyle.Fill;
      panel.Location = new System.Drawing.Point (3, 3);
      panel.Name = name + "SubPanel";
      panel.Size = new System.Drawing.Size (426, height + 6);
      panel.TabIndex = 0;

      checkBox = new CheckBox ();
      checkBox.AutoSize = true;
      checkBox.Location = new System.Drawing.Point (3, 6);
      checkBox.Name = name + "CheckBox";
      checkBox.UseVisualStyleBackColor = true;
      checkBox.Size = new System.Drawing.Size (15, checkBoxHeight);
      checkBox.CheckedChanged += (sender, e) => control.Enabled = ((CheckBox)sender).Checked;

      control.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      control.Location = new System.Drawing.Point (checkBox.Size.Width + 9, 3);
      control.Name = name + "OptionalControl";
      control.Size = new System.Drawing.Size (426 - (checkBox.Size.Width + 9) - 3, height);
      control.TabIndex = 1;
      control.Enabled = false;

      panel.Controls.Add (checkBox);
      panel.Controls.Add (control);

      panel.PerformLayout ();
      panel.ResumeLayout ();

      return panel;
    }

    #region TextBox
    /// <summary>
    /// Add a text
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    public void AddText (string name, string labelText)
    {
      var control = new TextBox ();
      control.Height = 20;
      AddControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.Text;
      m_setters[name] = (v) => SetTextInTextBox (control, (string)v);
    }

    void SetTextInTextBox (TextBox textBox, string text)
    {
      textBox.Text = text;
    }
    #endregion // TextBox

    #region CheckBox
    /// <summary>
    /// Add a check box
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    public void AddCheckBox (string name, string labelText)
    {
      var control = new CheckBox ();
      control.Height = 20;
      AddControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.Checked;
      m_setters[name] = (v) => SetBoolInCheckBox (control, (bool)v);
    }

    void SetBoolInCheckBox (CheckBox checkBox, bool v)
    {
      checkBox.Checked = v;
    }
    #endregion // CheckBox

    #region NumericUpDown
    /// <summary>
    /// Add a NumericUpDown control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="maximum"></param>
    /// <param name="decimalPlaces"></param>
    public void AddNumericUpDown (string name, string labelText, decimal maximum, int decimalPlaces)
    {
      var control = new NumericUpDown ();
      ((System.ComponentModel.ISupportInitialize)control).BeginInit ();
      control.DecimalPlaces = decimalPlaces;
      control.Maximum = maximum;
      AddControl (name, labelText, control);
      ((System.ComponentModel.ISupportInitialize)control).EndInit ();
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.Value;
      m_setters[name] = (v) => control.Value = (decimal)v;
    }

    /// <summary>
    /// Add an optional NumericUpDown control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="maximum"></param>
    /// <param name="decimalPlaces"></param>
    public void AddOptionalNumericUpDown (string name, string labelText, decimal maximum, int decimalPlaces)
    {
      var control = new NumericUpDown ();
      ((System.ComponentModel.ISupportInitialize)control).BeginInit ();
      control.DecimalPlaces = decimalPlaces;
      control.Maximum = maximum;
      var checkBox = AddOptionalControl (name, labelText, control);
      ((System.ComponentModel.ISupportInitialize)control).EndInit ();
      m_loaders[name] = delegate { };
      m_getters[name] = () => checkBox.Checked ? control.Value : (decimal?)null;
      m_setters[name] = (v) => SetValueInOptionalNumericUpDown (checkBox, control, (decimal?)v);
    }

    void SetValueInOptionalNumericUpDown (CheckBox checkBox, NumericUpDown numericUpDown, decimal? v)
    {
      if (v.HasValue) {
        checkBox.Checked = true;
        numericUpDown.Value = v.Value;
      }
      else {
        checkBox.Checked = false;
      }
    }
    #endregion // TextBox

    #region DurationPicker
    /// <summary>
    /// Add a DurationPicker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="withMilliseconds"></param>
    public void AddDurationPicker (string name, string labelText, bool withMilliseconds)
    {
      var control = new Lemoine.BaseControls.DurationPicker ();
      control.WithMs = withMilliseconds;
      AddControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.Duration;
      m_setters[name] = (v) => control.Duration = (TimeSpan)v;
    }

    /// <summary>
    /// Add an optional Duration Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="withMilliseconds"></param>
    public void AddOptionalDurationPicker (string name, string labelText, bool withMilliseconds)
    {
      var control = new Lemoine.BaseControls.DurationPicker ();
      control.WithMs = withMilliseconds;
      var checkBox = AddOptionalControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => checkBox.Checked ? control.Duration : (TimeSpan?)null;
      m_setters[name] = (v) => SetValueInOptionalDurationPicker (checkBox, control, (TimeSpan?)v);
    }

    void SetValueInOptionalDurationPicker (CheckBox checkBox, BaseControls.DurationPicker durationPicker, TimeSpan? v)
    {
      if (v.HasValue) {
        checkBox.Checked = true;
        durationPicker.Duration = v.Value;
      }
      else {
        checkBox.Checked = false;
      }
    }
    #endregion // DurationPicker

    #region DatePicker
    /// <summary>
    /// Add a DatePicker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    public void AddDatePicker (string name, string labelText)
    {
      var control = new Lemoine.BaseControls.DatePicker ();
      AddControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.Value;
      m_setters[name] = (v) => control.Value = (DateTime)v;
    }

    /// <summary>
    /// Add an optional Date Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    public void AddOptionalDatePicker (string name, string labelText)
    {
      var control = new Lemoine.BaseControls.DatePicker ();
      var checkBox = AddOptionalControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => checkBox.Checked ? control.Value : (DateTime?)null;
      m_setters[name] = (v) => SetValueInOptionalDatePicker (checkBox, control, (DateTime?)v);
    }

    void SetValueInOptionalDatePicker (CheckBox checkBox, BaseControls.DatePicker datePicker, DateTime? v)
    {
      if (v.HasValue) {
        checkBox.Checked = true;
        datePicker.Value = v.Value;
      }
      else {
        checkBox.Checked = false;
      }
    }
    #endregion // DatePicker

    #region TimePicker
    /// <summary>
    /// Add a Time Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="withSeconds"></param>
    public void AddTimePicker (string name, string labelText, bool withSeconds = false)
    {
      var control = new Lemoine.BaseControls.TimePicker ();
      control.WithSeconds = withSeconds;
      AddControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.Value;
      m_setters[name] = (v) => control.Value = (DateTime)v;
    }

    /// <summary>
    /// Add an optional Time Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="withSeconds"></param>
    public void AddOptionalTimePicker (string name, string labelText, bool withSeconds = false)
    {
      var control = new Lemoine.BaseControls.TimePicker ();
      control.WithSeconds = withSeconds;
      var checkBox = AddOptionalControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => checkBox.Checked ? control.Value : (DateTime?)null;
      m_setters[name] = (v) => SetValueInOptionalTimePicker (checkBox, control, (DateTime?)v);
    }

    void SetValueInOptionalTimePicker (CheckBox checkBox, BaseControls.TimePicker timePicker, DateTime? v)
    {
      if (v.HasValue) {
        checkBox.Checked = true;
        timePicker.Value = v.Value;
      }
      else {
        checkBox.Checked = false;
      }
    }
    #endregion // DatePicker

    #region DateTimePicker
    /// <summary>
    /// Add a Date/Time Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="withSeconds"></param>
    public void AddDateTimePicker (string name, string labelText, bool withSeconds = false)
    {
      var control = new Lemoine.BaseControls.DateTimePicker ();
      control.WithSeconds = withSeconds;
      AddControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.Value;
      m_setters[name] = (v) => control.Value = (DateTime)v;
    }

    /// <summary>
    /// Add an optional Time Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="withSeconds"></param>
    public void AddOptionalDateTimePicker (string name, string labelText, bool withSeconds = false)
    {
      var control = new Lemoine.BaseControls.DateTimePicker ();
      control.WithSeconds = withSeconds;
      var checkBox = AddOptionalControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => checkBox.Checked ? control.Value : (DateTime?)null;
      m_setters[name] = (v) => SetValueInOptionalDateTimePicker (checkBox, control, (DateTime?)v);
    }

    void SetValueInOptionalDateTimePicker (CheckBox checkBox, BaseControls.DateTimePicker dateTimePicker, DateTime? v)
    {
      if (v.HasValue) {
        checkBox.Checked = true;
        dateTimePicker.Value = v.Value;
      }
      else {
        checkBox.Checked = false;
      }
    }
    #endregion // DatePicker

    #region UtcDateTimeRangePicker
    /// <summary>
    /// Add a Date/Time range Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    public void AddUtcDateTimeRangePicker (string name, string labelText)
    {
      var control = new Lemoine.BaseControls.DateTimeRangePicker ();
      AddControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.UtcDateTimeRange;
      m_setters[name] = (v) => control.UtcDateTimeRange = (UtcDateTimeRange)v;
    }

    /// <summary>
    /// Add an optional Time Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    public void AddOptionalUtcDateTimeRangePicker (string name, string labelText)
    {
      var control = new Lemoine.BaseControls.DateTimeRangePicker ();
      var checkBox = AddOptionalControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => checkBox.Checked ? control.UtcDateTimeRange : null;
      m_setters[name] = (v) => SetValueInOptionalUtcDateTimeRangePicker (checkBox, control, (UtcDateTimeRange)v);
    }

    void SetValueInOptionalUtcDateTimeRangePicker (CheckBox checkBox, BaseControls.DateTimeRangePicker dateTimeRangePicker, UtcDateTimeRange v)
    {
      if (null == v) {
        checkBox.Checked = true;
        dateTimeRangePicker.UtcDateTimeRange = v;
      }
      else {
        checkBox.Checked = false;
      }
    }
    #endregion // DatePicker

    #region DateRangePicker
    /// <summary>
    /// Add a Date range Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    public void AddDateRangePicker (string name, string labelText)
    {
      var control = new Lemoine.BaseControls.DateRangePicker ();
      AddControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => control.DateRange;
      m_setters[name] = (v) => control.DateRange = (IRange<DateTime>)v;
    }

    /// <summary>
    /// Add an optional Time Picker
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    public void AddOptionalDateRangePicker (string name, string labelText)
    {
      var control = new Lemoine.BaseControls.DateRangePicker ();
      var checkBox = AddOptionalControl (name, labelText, control);
      m_loaders[name] = delegate { };
      m_getters[name] = () => checkBox.Checked ? control.DateRange : null;
      m_setters[name] = (v) => SetValueInOptionalDateRangePicker (checkBox, control, (IRange<DateTime>)v);
    }

    void SetValueInOptionalDateRangePicker (CheckBox checkBox, BaseControls.DateRangePicker dateRangePicker, IRange<DateTime> v)
    {
      if (null == v) {
        checkBox.Checked = true;
        dateRangePicker.DateRange = v;
      }
      else {
        checkBox.Checked = false;
      }
    }
    #endregion // DateRangePicker

    #region ComboBox
    /// <summary>
    /// Add a combo box
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="getItems"></param>
    /// <param name="valueMember"></param>
    /// <param name="optional"></param>
    public void AddComboBox (string name, string labelText, Func<IEnumerable<object>> getItems, string valueMember, bool optional)
    {
      var comboBox = new ComboBox ();
      comboBox.Height = 20;
      comboBox.FormattingEnabled = true;
      if (!optional) {
        comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      }
      AddControl (name, labelText, comboBox);
      m_loaders[name] = delegate { ComboBoxLoader (comboBox, getItems, valueMember); };
      m_getters[name] = () => ComboBoxGetter (comboBox);
      m_setters[name] = (u) => ComboBoxSetter (comboBox, u);
    }

    /// <summary>
    /// Load a combo box
    /// </summary>
    /// <param name="comboBox"></param>
    /// <param name="getItems"></param>
    /// <param name="valueMember"></param>
    void ComboBoxLoader (ComboBox comboBox, Func<IEnumerable<object>> getItems, string valueMember)
    {
      var items = getItems ();
      comboBox.Items.Clear ();
      foreach (var item in items) {
        comboBox.Items.Add (item);
      }
      comboBox.ValueMember = valueMember;
    }

    object ComboBoxGetter (ComboBox comboBox)
    {
      return comboBox.SelectedItem;
    }

    void ComboBoxSetter (ComboBox comboBox, object u)
    {
      comboBox.SelectedIndex = -1;
      for (int i = 0; i < comboBox.Items.Count; ++i) {
        var item = comboBox.Items[i];
        if ((null != item) && object.Equals (item, u)) {
          comboBox.SelectedIndex = i;
        }
      }
    }
    #endregion // ComboBox

    #region ListBox
    /// <summary>
    /// Add a list box
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="getItems"></param>
    /// <param name="valueMember"></param>
    /// <param name="optional"></param>
    /// <param name="multiple"></param>
    /// <typeparam name="T"></typeparam>
    public void AddListBox<T> (string name, string labelText, Func<IEnumerable<object>> getItems, string valueMember, bool optional, bool multiple)
    {
      var listBox = new ListBox ();
      listBox.FormattingEnabled = true;
      if (multiple) {
        listBox.SelectionMode = SelectionMode.MultiExtended;
      }
      else {
        listBox.SelectionMode = SelectionMode.One;
      }
      AddControl (name, labelText, listBox);
      m_loaders[name] = delegate { ListBoxLoader (listBox, getItems, valueMember); };
      m_getters[name] = () => ListBoxGetter<T> (listBox);
      m_setters[name] = (u) => ListBoxSetter<T> (listBox, u);
    }

    /// <summary>
    /// Load a combo box
    /// </summary>
    /// <param name="listBox"></param>
    /// <param name="getItems"></param>
    /// <param name="valueMember"></param>
    void ListBoxLoader (ListBox listBox, Func<IEnumerable<object>> getItems, string valueMember)
    {
      var items = getItems ();
      listBox.Items.Clear ();
      foreach (var item in items) {
        listBox.Items.Add (item);
      }
      listBox.ValueMember = valueMember;
    }

    object ListBoxGetter<T> (ListBox listBox)
    {
      if (listBox.SelectionMode.Equals (SelectionMode.One)) {
        return listBox.SelectedItem;
      }
      else { // Multiple
        var list = new List<T> ();
        foreach (var item in listBox.SelectedItems) {
          list.Add ((T)item);
        }
        return list;
      }
    }

    void ListBoxSetter<T> (ListBox listBox, object u)
    {
      IEnumerable<T> l;
      if (null == u) {
        l = new List<T> ();
      }
      else if (u is IEnumerable<T>) {
        l = (IEnumerable<T>)u;
      }
      else {
        l = new List<T> { (T)u };
      }

      if (0 <= listBox.SelectedIndex) {
        listBox.SetSelected (listBox.SelectedIndex, false);
      }
      for (int i = 0; i < listBox.Items.Count; ++i) {
        var item = listBox.Items[i];
        if ((null != item) && l.Any (x => object.Equals (item, x))) {
          listBox.SetSelected (i, true);
        }
      }
    }
    #endregion // ListBox

    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.GuiBuilder.IPluginConfig"/>
    /// </summary>
    public void InitialLoad ()
    {
      if (!m_isInitialized) { // Postpone the load process later
        return;
      }

      if (!m_initialLoad) {
        foreach (var loadAction in m_loaders.Values) {
          loadAction ();
        }

        m_initialLoad = true;
      }
    }
  }
}
