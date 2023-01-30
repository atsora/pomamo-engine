// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of ComboboxTextValue.
  /// </summary>
  public partial class ComboboxTextValue : UserControl
  {
    class ComboboxItem
    {
      public string Text { get; set; }
      public object Value { get; set; }
      public IComparable Order { get; set; }
      public override string ToString() { return Text; }
      
      public Color Color { get; set; }
      public bool Italic { get; set; }
      public bool Bold { get; set; }
      public Color? BackgroundColor { get; set; }
    }

    #region Events
    /// <summary>
    /// Event emitted when the current item changed
    /// The first argument is the text of the new item
    /// The second argument is the value associated to it
    /// </summary>
    public event Action<string, object> ItemChanged;
    
    /// <summary>
    /// Event emitted when the text changed
    /// Useful for an editable combobox
    /// </summary>
    new public event Action<string> TextChanged;
    #endregion // Events
    
    #region Getters / Setters
    /// <summary>
    /// Get the text of the current item or select the item having a specific text
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SelectedText {
      get {
        if (Editable) {
          return comboBox.Text;
        }
        else {
          var currentItem = (ComboboxItem)comboBox.SelectedItem;
          if (currentItem == null) {
            return "";
          }
          else {
            return currentItem.Text;
          }
        }
      }
      set {
        if (Editable) {
          comboBox.Text = value;
        }
        else {
          foreach (object item in comboBox.Items) {
            var comboboxItem = (ComboboxItem)item;
            if (Object.Equals(comboboxItem.Text, value)) {
              comboBox.SelectedItem = item;
              return;
            }
          }
        }
      }
    }
    
    /// <summary>
    /// Get the value of the current item or select the item having a specific value
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object SelectedValue {
      get {
        var currentItem = (ComboboxItem)comboBox.SelectedItem;
        if (currentItem == null) {
          return null;
        }

        // If the combobox is editable, we check that the text matches with the current item
        if (Editable && (currentItem == null || !string.Equals(currentItem.Text, comboBox.Text))) {
          return ValueFrom(comboBox.Text);
        }

        return currentItem.Value;
      }
      set {
        foreach (object item in comboBox.Items) {
          var comboboxItem = (ComboboxItem)item;
          if (Object.Equals(comboboxItem.Value, value)) {
            comboBox.SelectedItem = item;
            return;
          }
        }
      }
    }
    
    /// <summary>
    /// Get or set the current index
    /// -1 is returned if nothing is selected
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex {
      get { return comboBox.SelectedIndex; }
      set {
        if (value >= comboBox.Items.Count) {
          value = -1;
        }

        comboBox.SelectedIndex = value;
      }
    }
    
    /// <summary>
    /// Return the number of elements in the combobox
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Count { get { return comboBox.Items.Count; } }
    
    /// <summary>
    /// Sort or not the displayed elements
    /// </summary>
    [DefaultValue(false)]
    [Description("The items are automatically sorted.")]
    public bool Sorted { get; set; }
    
    /// <summary>
    /// Reverse order if sorted
    /// </summary>
    [DefaultValue(false)]
    [Description("If \"Sorted\" is true, the items will be sorted in the reverse order.")]
    public bool ReverseOrder { get; set; }
    
    /// <summary>
    /// Make the combobox editable
    /// </summary>
    [DefaultValue(false)]
    [Description("Make the combobox editable.")]
    public bool Editable {
      get { return comboBox.DropDownStyle != ComboBoxStyle.DropDownList; }
      set { comboBox.DropDownStyle = value ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList; }
    }
    
    /// <summary>
    /// Change the text color when the list is expanded
    /// </summary>
    [DefaultValue(typeof(Color), "WindowText")]
    [Description("Change the text color when the list is expanded.")]
    public Color DropDownTextColor {
      get { return comboBox.ForeColor; }
      set { comboBox.ForeColor = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ComboboxTextValue()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add an element in the combobox
    /// </summary>
    /// <param name="text"></param>
    public void AddItem(string text)
    {
      var item = new ComboboxItem();
      item.Text = text;
      item.Value = null;
      item.Order = text;
      item.Color = SystemColors.ControlText;
      item.Italic = false;
      item.Bold = false;
      item.BackgroundColor = null;
      AddItem(item);
    }
    
    /// <summary>
    /// Add an element in the combobox associated to a value
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    public void AddItem(string text, object value)
    {
      var item = new ComboboxItem();
      item.Text = text;
      item.Value = value;
      item.Order = text;
      item.Color = SystemColors.ControlText;
      item.Italic = false;
      item.Bold = false;
      item.BackgroundColor = null;
      AddItem(item);
    }
    
    /// <summary>
    /// Add an element in the combobox associated to a value
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="order"></param>
    public void AddItem(string text, object value, IComparable order)
    {
      var item = new ComboboxItem();
      item.Text = text;
      item.Value = value;
      item.Order = order;
      item.Color = SystemColors.ControlText;
      item.Italic = false;
      item.Bold = false;
      item.BackgroundColor = null;
      AddItem(item);
    }
    
    /// <summary>
    /// Add an element in the combobox associated to a value, at a specific position
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="position"></param>
    public void InsertItem(string text, object value, int position)
    {
      var item = new ComboboxItem();
      item.Text = text;
      item.Value = value;
      item.Order = text;
      item.Color = SystemColors.ControlText;
      item.Italic = false;
      item.Bold = false;
      item.BackgroundColor = null;
      InsertItem(item, position);
    }
    
    /// <summary>
    /// Add an element in the combobox associated to a value
    /// The style is specified
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="order"></param>
    /// <param name="color"></param>
    /// <param name="italic"></param>
    /// <param name="bold"></param>
    public void AddItem(string text, object value, IComparable order, Color color, bool italic, bool bold)
    {
      var item = new ComboboxItem();
      item.Text = text;
      item.Value = value;
      item.Order = order;
      item.Color = color;
      item.Italic = italic;
      item.Bold = bold;
      item.BackgroundColor = null;
      AddItem(item);
    }
    
    /// <summary>
    /// Return true if the combobox comprises a specific object
    /// </summary>
    /// <param name="obj">object to find</param>
    /// <returns></returns>
    public bool ContainsObject(Object obj)
    {
      foreach (object item in comboBox.Items) {
        var comboboxItem = (ComboboxItem)item;
        if (Object.Equals(comboboxItem.Value, obj)) {
          return true;
        }
      }
      return false;
    }
    
    void AddItem(ComboboxItem item)
    {
      if (Sorted) {
        // We find the position:
        // - null first
        // - if equal, the displayed text is compared
        int pos = 0;
        for (int i = 0; i < comboBox.Items.Count; i++)
        {
          // Other item
          var otherItem = comboBox.Items[i] as ComboboxItem;
          
          int order = 0;
          if (item.Order == null) {
            order = (otherItem.Order == null) ? 0 : 1;
          } else {
            order = (otherItem.Order == null) ? -1 : otherItem.Order.CompareTo(item.Order);
          }
          
          if (order == 0) {
            order = otherItem.Text.CompareTo(item.Text);
          }

          if (!ReverseOrder && order < 0 || ReverseOrder && order > 0) {
            pos++;
          }
          else {
            break;
          }
        }
        comboBox.Items.Insert(pos, item);
      } else {
        comboBox.Items.Add(item);
      }
    }
    
    void InsertItem(ComboboxItem item, int position)
    {
      comboBox.Items.Insert(position, item);
    }
    
    /// <summary>
    /// Find the item corresponding to the object and change the text
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="newText"></param>
    public void UpdateItemText(object obj, string newText)
    {
      foreach (ComboboxItem item in comboBox.Items) {
        if (object.Equals(item.Value, obj)) {
          item.Text = newText;
          break;
        }
      }
    }
    
    /// <summary>
    /// Clear all items
    /// </summary>
    public void ClearItems()
    {
      comboBox.Items.Clear();
    }
    
    /// <summary>
    /// Remove the first item having a specific value
    /// </summary>
    /// <param name="obj"></param>
    public void RemoveValue(object obj)
    {
      foreach (ComboboxItem comboboxItem in comboBox.Items) {
        if (object.Equals(comboboxItem.Value, obj)) {
          comboBox.Items.Remove(comboboxItem);
          break;
        }
      }
    }
    
    /// <summary>
    /// Remove an item by specifying the index
    /// </summary>
    /// <param name="index"></param>
    public void RemoveIndex(int index)
    {
      if (comboBox.Items.Count > index && index >= 0) {
        comboBox.Items.RemoveAt(index);
      }
    }
    
    /// <summary>
    /// Return the value held in the position "index"
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public object ValueAt(int index)
    {
      if (comboBox.Items.Count > index && index >= 0) {
        return (comboBox.Items[index] as ComboboxItem).Value;
      }

      return null;
    }
    
    /// <summary>
    /// Return the text displayed in the position "index"
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string TextAt(int index)
    {
      if (comboBox.Items.Count > index && index >= 0) {
        return (comboBox.Items[index] as ComboboxItem).Text;
      }

      return "";
    }
    
    /// <summary>
    /// Get the first value associated to a text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public object ValueFrom(string text)
    {
      foreach (ComboboxItem item in comboBox.Items) {
        if (string.Equals(item.Text, text)) {
          return item.Value;
        }
      }

      return null;
    }
    #endregion // Methods
    
    #region Event reactions
    void ComboBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      if (ItemChanged != null) {
        ItemChanged (SelectedText, SelectedValue);
      }
    }
    
    void ComboBoxTextUpdate(object sender, EventArgs e)
    {
      if (TextChanged != null) {
        TextChanged (SelectedText);
      }
    }
    
    void ComboBoxDrawItem(object sender, DrawItemEventArgs e)
    {
      if (e.Index < 0 || e.Index >= comboBox.Items.Count) {
        return;
      }

      var item = (ComboboxItem)comboBox.Items[e.Index];
      
      // Background color
      if ((e.State & DrawItemState.Selected) != 0) {
        if (this.Enabled) {
          e.Graphics.FillRectangle(new SolidBrush(Color.DodgerBlue), e.Bounds);
        }
        else {
          e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), e.Bounds);
        }
      } else if (item.BackgroundColor.HasValue) {
        e.Graphics.FillRectangle(new SolidBrush(item.BackgroundColor.Value), e.Bounds);
      }
      else {
        e.DrawBackground();
      }

      // Text color
      Color color;
      if (this.Enabled) {
        color = ((e.State & DrawItemState.Selected) != 0) ? Color.White : item.Color;
      } else {
        color = ((e.State & DrawItemState.Selected) != 0) ? Color.White:  Color.DarkGray;
      }
      
      
      // Font
      Font font = e.Font;
      if (item.Italic && item.Bold) {
        font = new Font(e.Font, FontStyle.Bold | FontStyle.Italic);
      }
      else if (item.Italic) {
        font = new Font(e.Font, FontStyle.Italic);
      }
      else if (item.Bold) {
        font = new Font(e.Font, FontStyle.Bold);
      }

      e.Graphics.DrawString(item.ToString(), font, new SolidBrush(color), e.Bounds);
      e.DrawFocusRectangle();
    }
    #endregion // Event reactions
  }
}
