// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.BaseControls.List
{
  /// <summary>
  /// Description of ListTextValue.
  /// </summary>
  public partial class ListTextValue : UserControl
  {
    private class ListboxItem
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
    /// Event emitted when an item is double-clicked
    /// The first argument is the text of the item
    /// The second argument is the value associated to it
    /// </summary>
    public event Action<string, object> ItemDoubleClicked;
    #endregion // Events
    
    #region Getters / Setters
    /// <summary>
    /// Enable or disable the multiselection
    /// </summary>
    [DefaultValue(false)]
    [Description("Allow several rows to be selected.")]
    public bool MultipleSelection {
      get { return (listBox.SelectionMode == SelectionMode.MultiExtended); }
      set {
        listBox.SelectionMode = value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }
    
    /// <summary>
    /// Text of the selected item
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SelectedText {
      get {
        var currentItem = (ListboxItem)listBox.SelectedItem;
        return (currentItem == null) ? "" : currentItem.Text;
      }
      set {
        foreach (ListboxItem item in listBox.Items) {
          if (object.Equals(item.Text, value)) {
            listBox.SelectedItem = item;
            return;
          }
        }
      }
    }
    
    /// <summary>
    /// Text of the selected items
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<string> SelectedTexts {
      get {
        if (DesignMode) {
          return null;
        }

        IList<string> txts = new List<string>();
        foreach (ListboxItem item in listBox.SelectedItems) {
          txts.Add(item.Text);
        }

        return txts;
      }
      set {
        for (int i = 0; i < listBox.Items.Count; i++) {
          var item = (ListboxItem)listBox.Items[i];
          listBox.SetSelected(i, value.Contains(item.Text));
        }
      }
    }
    
    /// <summary>
    /// Value of the selected item
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object SelectedValue {
      get {
        var currentItem = (ListboxItem)listBox.SelectedItem;
        return (currentItem == null) ? null : currentItem.Value;
      }
      set {
        foreach (ListboxItem item in listBox.Items) {
          if (object.Equals(item.Value, value)) {
            listBox.SelectedItem = item;
            return;
          }
        }
      }
    }
    
    /// <summary>
    /// Value of the selected items
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<object> SelectedValues {
      get {
        if (DesignMode) {
          return null;
        }

        IList<object> objects = new List<object>();
        foreach (ListboxItem item in listBox.SelectedItems) {
          objects.Add(item.Value);
        }

        return objects;
      }
      set {
        if (value == null) {
          for (int i = 0; i < listBox.Items.Count; i++) {
            listBox.SetSelected(i, false);
          }
        } else {
          for (int i = 0; i < listBox.Items.Count; i++) {
            listBox.SetSelected(i, value.Contains(((ListboxItem)listBox.Items[i]).Value));
          }
        }
      }
    }
    
    /// <summary>
    /// Get or set the index of the selected item
    /// Return -1 if nothing is selected
    /// If a invalid value is used to set the selected index, the selection is removed
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedIndex {
      get { return listBox.SelectedIndex; }
      set {
        if (value >= listBox.Items.Count || value < -1) {
          value = -1;
        }

        listBox.SelectedIndex = value;
      }
    }
    
    /// <summary>
    /// Index of the selected items
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<int> SelectedIndexes {
      get {
        if (DesignMode) {
          return null;
        }

        IList<int> indexes = new List<int>();
        for (int i = 0; i < listBox.Items.Count; i++) {
          if (listBox.GetSelected(i)) {
            indexes.Add(i);
          }
        }

        return indexes;
      }
      set {
        for (int i = 0; i < listBox.Items.Count; i++) {
          listBox.SetSelected(i, value.Contains(i));
        }
      }
    }
    
    /// <summary>
    /// Retrieve all values
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<object> Values {
      get {
        IList<object> listRet = new List<object>();
        foreach (ListboxItem item in listBox.Items) {
          listRet.Add(item.Value);
        }

        return listRet;
      }
    }
    
    /// <summary>
    /// Retrieve all texts
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<string> Texts {
      get {
        IList<string> listRet = new List<string>();
        foreach (ListboxItem item in listBox.Items) {
          listRet.Add(item.Text);
        }

        return listRet;
      }
    }
    
    /// <summary>
    /// Return the number of elements in the combobox
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Count { get { return listBox.Items.Count; } }
    
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
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ListTextValue()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add an element in the list
    /// </summary>
    /// <param name="text"></param>
    public void AddItem(string text)
    {
      if (text == null) {
        text = "";
      }

      var item = new ListboxItem();
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
    /// Add an element in the list associated to a value
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    public void AddItem(string text, object value)
    {
      if (text == null) {
        text = "";
      }

      var item = new ListboxItem();
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
    /// Add an element in the list associated to a value
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="order"></param>
    public void AddItem(string text, object value, IComparable order)
    {
      if (text == null) {
        text = "";
      }

      var item = new ListboxItem();
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
    /// Add an element in the list associated to a value
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
      if (text == null) {
        text = "";
      }

      var item = new ListboxItem();
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
    /// Add an element in the list associated to a value
    /// The style is specified + background color
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="order"></param>
    /// <param name="color"></param>
    /// <param name="italic"></param>
    /// <param name="bold"></param>
    /// <param name="backgroundColor"></param>
    public void AddItem(string text, object value, IComparable order, Color color, bool italic, bool bold, Color backgroundColor)
    {
      if (text == null) {
        text = "";
      }

      var item = new ListboxItem();
      item.Text = text;
      item.Value = value;
      item.Order = order;
      item.Color = color;
      item.Italic = italic;
      item.Bold = bold;
      item.BackgroundColor = backgroundColor;
      AddItem(item);
    }
    
    /// <summary>
    /// Insert an element at a specific location in the list associated to a value
    /// The style is specified
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <param name="italic"></param>
    /// <param name="bold"></param>
    public void InsertItem(string text, object value, int position, Color color, bool italic, bool bold)
    {
      if (text == null) {
        text = "";
      }

      var item = new ListboxItem();
      item.Text = text;
      item.Value = value;
      item.Order = text;
      item.Color = color;
      item.Italic = italic;
      item.Bold = bold;
      item.BackgroundColor = null;
      InsertItem(item, position);
    }
    
    /// <summary>
    /// Find the item corresponding to the value and change the text
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="newText"></param>
    public void UpdateItemText(object obj, string newText)
    {
      foreach (ListboxItem item in listBox.Items) {
        if (object.Equals(item.Value, obj)) {
          item.Text = newText;
          break;
        }
      }
    }
    
    /// <summary>
    /// Find the item corresponding to the old value and update the value
    /// </summary>
    /// <param name="oldObj">old value to find</param>
    /// <param name="newObj">replacement</param>
    public void UpdateItemValue(object oldObj, object newObj)
    {
      foreach (ListboxItem item in listBox.Items) {
        if (object.Equals(item.Value, oldObj)) {
          item.Value = newObj;
          break;
        }
      }
    }
    
    /// <summary>
    /// Return true if the list comprises a specific object
    /// </summary>
    /// <param name="obj">object to find</param>
    /// <returns></returns>
    public bool ContainsObject(Object obj)
    {
      foreach (ListboxItem listboxItem in listBox.Items) {
        if (object.Equals(listboxItem.Value, obj)) {
          return true;
        }
      }
      return false;
    }
    
    void AddItem(ListboxItem item)
    {
      if (Sorted) {
        // We find the position:
        // - null first
        // - if equal, the displayed text is compared
        int pos = 0;
        for (int i = 0; i < listBox.Items.Count; i++)
        {
          // Other item
          var otherItem = listBox.Items[i] as ListboxItem;
          
          int order = 0;
          if (item.Order == null) {
            if (otherItem.Order == null) {
              order = 0;
            }
            else {
              order = 1;
            }
          } else {
            if (otherItem.Order == null) {
              order = -1;
            }
            else {
              order = otherItem.Order.CompareTo(item.Order);
            }
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
        listBox.Items.Insert(pos, item);
      } else {
        listBox.Items.Add(item);
      }
    }
    
    void InsertItem(ListboxItem item, int position)
    {
      listBox.Items.Insert(position, item);
    }
    
    /// <summary>
    /// Clear all items
    /// </summary>
    public void ClearItems()
    {
      listBox.Items.Clear();
    }
    
    /// <summary>
    /// Remove the first item having a specific value
    /// </summary>
    /// <param name="obj"></param>
    public void RemoveValue(object obj)
    {
      foreach (ListboxItem listboxItem in listBox.Items) {
        if (object.Equals(listboxItem.Value, obj)) {
          listBox.Items.Remove(listboxItem);
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
      if (listBox.Items.Count > index && index >= 0) {
        listBox.Items.RemoveAt(index);
      }
    }
    
    /// <summary>
    /// Return the value held in the position "index"
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public object ValueAt(int index)
    {
      if (listBox.Items.Count > index && index >= 0) {
        return (listBox.Items[index] as ListboxItem).Value;
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
      if (listBox.Items.Count > index && index >= 0) {
        return (listBox.Items[index] as ListboxItem).Text;
      }

      return "";
    }
    #endregion // Methods
    
    #region Event reactions
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      if (ItemChanged != null) {
        ItemChanged (SelectedText, SelectedValue);
      }
    }
    
    void ListBoxMouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (ItemDoubleClicked != null && SelectedIndex != -1) {
        ItemDoubleClicked (SelectedText, SelectedValue);
      }
    }
    
    void ListBoxDrawItem(object sender, DrawItemEventArgs e)
    {
      if (e.Index < 0 || e.Index >= listBox.Items.Count) {
        return;
      }

      var item = (ListboxItem)listBox.Items[e.Index];
      
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
        if ((e.State & DrawItemState.Selected) != 0) {
          color = Color.White;
        }
        else {
          color = item.Color;
        }
      } else {
        if ((e.State & DrawItemState.Selected) != 0) {
          color = Color.White;
        }
        else {
          color = Color.DarkGray;
        }
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
