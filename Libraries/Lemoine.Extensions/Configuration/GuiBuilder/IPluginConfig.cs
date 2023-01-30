// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder
{
  /// <summary>
  /// Interface of the plugin config control
  /// </summary>
  public interface IPluginConfig
  {
    /// <summary>
    /// Load of the component if not already loaded
    /// </summary>
    void InitialLoad ();

    /// <summary>
    /// Get the value of a component
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    object Get (string name);

    /// <summary>
    /// Set the value of a component
    /// </summary>
    /// <param name="name"></param>
    /// <param name="v"></param>
    void Set (string name, object v);

    /// <summary>
    /// To run before adding controls
    /// </summary>
    void BeforeAddingControls ();

    /// <summary>
    /// To run after adding controls
    /// </summary>
    void AfterAddingControls ();

    /// <summary>
    /// Add a text configuration control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    void AddText (string name, string label);

    /// <summary>
    /// Add a check box control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    void AddCheckBox (string name, string label);

    /// <summary>
    /// Add a combo box
    /// </summary>
    /// <param name="name"></param>
    /// <param name="labelText"></param>
    /// <param name="getItems"></param>
    /// <param name="valueMember"></param>
    /// <param name="optional"></param>
    void AddComboBox (string name, string labelText, Func<IEnumerable<object>> getItems, string valueMember, bool optional);

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
    void AddListBox<T> (string name, string labelText, Func<IEnumerable<object>> getItems, string valueMember, bool optional, bool multiple);

    /// <summary>
    /// Add a numeric up/down configuration control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="maximum"></param>
    /// <param name="decimalPlaces"></param>
    void AddNumericUpDown (string name, string label, decimal maximum, int decimalPlaces);

    /// <summary>
    /// Add an optional numeric up/down configuration control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="maximum"></param>
    /// <param name="decimalPlaces"></param>
    void AddOptionalNumericUpDown (string name, string label, decimal maximum, int decimalPlaces);

    /// <summary>
    /// Add a DurationPicker control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="withMilliseconds"></param>
    void AddDurationPicker (string name, string label, bool withMilliseconds);

    /// <summary>
    /// Add a DurationPicker control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="withMilliseconds"></param>
    void AddOptionalDurationPicker (string name, string label, bool withMilliseconds);

    /// <summary>
    /// Add a DatePicker control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    void AddDatePicker (string name, string label);

    /// <summary>
    /// Add an optional DatePicker control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    void AddOptionalDatePicker (string name, string label);

    /// <summary>
    /// Add a TimePicker control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="withSeconds"></param>
    void AddTimePicker (string name, string label, bool withSeconds = false);

    /// <summary>
    /// Add an optional TimePicker control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="withSeconds"></param>
    void AddOptionalTimePicker (string name, string label, bool withSeconds = false);

    /// <summary>
    /// Add a DateTimePicker control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="withSeconds"></param>
    void AddDateTimePicker (string name, string label, bool withSeconds = false);

    /// <summary>
    /// Add an optional DateTimePicker control
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="withSeconds"></param>
    void AddOptionalDateTimePicker (string name, string label, bool withSeconds = false);

    /// <summary>
    /// Add a DateTimeRangePicker control for a UTC date/time range
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    void AddUtcDateTimeRangePicker (string name, string label);

    /// <summary>
    /// Add an optional DateTimeRangePicker control for a UTC date/time range
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    void AddOptionalUtcDateTimeRangePicker (string name, string label);

    /// <summary>
    /// Add a DateRangePicker control for a date range
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    void AddDateRangePicker (string name, string label);

    /// <summary>
    /// Add an optional DateRangePicker control for a date range
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    void AddOptionalDateRangePicker (string name, string label);
  }
}
