// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// ListView with prefix selection. Does not use generic (lack of Designer support)
  /// Object type must have a public property returning a string on Get (Name by default, but can be Code etc)
  /// </summary>
  public partial class PrefixListView : ListView
  {
    #region Members
    string m_property;
    bool m_init = false;
    ICollection m_initCollection = new ArrayList();
    Hashtable m_itemToObject = new Hashtable();
    Hashtable m_objectNameToItem = new Hashtable();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (PrefixListView).FullName);

    #region Getters / Setters
    /// <summary>
    /// Property name to use to display item
    /// </summary>
    public string Property {
      get { return m_property; }
      set { m_property = value; }
    }
    
    bool IsInit {
      get { return m_init; }
      set { m_init = value; }
    }
    
    /// <summary>
    /// Initial collection used
    /// </summary>
    public ICollection InitCollection {
      get { return m_initCollection; }
      set { m_initCollection = value; }
      
    }
    
    /// <summary>
    /// Map from listview item to underlying object
    /// </summary>
    Hashtable ItemToObject {
      get { return m_itemToObject; }
    }
    
    /// <summary>
    /// Map from object.Name to item
    /// </summary>
    Hashtable ObjectToItem {
      get { return m_objectNameToItem; }
    }
    
    /// <summary>
    /// Currently selected object
    /// </summary>
    public object SelectedData {
      get {
        ListView.SelectedListViewItemCollection selItems = this.SelectedItems;

        if (selItems.Count > 0) {
          return ItemToObject[this.SelectedItems[0]];
        }
        else {
          return null;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PrefixListView() : base()
    {
      this.SuspendLayout();
      this.Property = "Name"; // default property
      this.Name = "List";
      this.Bounds = new System.Drawing.Rectangle(new System.Drawing.Point(10,10),
                                                 new System.Drawing.Size(300,200));
      
      this.MultiSelect = false; // single selection
      this.FullRowSelect = true;
      this.HideSelection = false; // selected item remains highlighted
      this.ResumeLayout();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize the listview items
    /// </summary>
    public void Initialize (ICollection initCollection)
    {
      if (this.IsInit) {
        return;
      }

      this.Items.Clear();
      this.View = View.Details;
      
      this.Columns.Add(this.Property, 250 /* -2: auto-size */,
                       HorizontalAlignment.Left);
      
      foreach (object obj in initCollection)
      {
        ListViewItem objItem = new ListViewItem();
        this.ItemToObject[objItem] = obj;
        System.Reflection.PropertyInfo propName = obj.GetType().GetProperty(this.Property);
        string name = (string) propName.GetValue(obj, null);
        this.ObjectToItem[name] = objItem;
        objItem.Text = name;
        this.Items.Add(objItem);
      }
      this.IsInit = true;
    }

    /// <summary>
    /// Search for an item based on prefix objName
    /// Does not respect case
    /// </summary>
    /// <param name="objName"></param>
    public void TrySetSelected(string objName)
    {
      if (this.Items.Count > 0) {
        ListViewItem item = FindItemWithText(objName, false, 0, true);
        
        if (item != null) {
          // object with corresponding name found in listview
          item.Focused = true;
          // item.Selected = true;
          item.EnsureVisible(); // ensures listview scrolls to selected item
        }
      }
    }
    
    #endregion // Methods
  }
}
