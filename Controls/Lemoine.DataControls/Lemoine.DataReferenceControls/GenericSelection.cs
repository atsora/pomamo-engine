// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of GenericSelection.
  /// </summary>
  public partial class GenericSelection<T> : UserControl //where T : IDataWithIdentifiers
  {
    #region Members
    string m_displayedProperty = "Display";
    string m_noTi18n = "No One";
    IList<T> m_Ts = null;
    T m_T = default(T);
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (GenericSelection<T>).FullName+typeof(T).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Allow multi-selection")]
    public bool MultiSelect {
      get { return (listBox.SelectionMode != SelectionMode.One); }
      set
      {
        listBox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }
    
    /// <summary>
    /// Is a null T a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null MachineObservation valid ?")]
    public bool Nullable {
      get { return nullCheckBox.Visible; }
      set
      {
        if (value) {
          tableLayoutPanel1.RowStyles [1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles [1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue("Display"), Description("Property to display")]
    public string DisplayedProperty {
      get { return  m_displayedProperty; }
      set { m_displayedProperty = value; }
    }
    
    /// <summary>
    /// I18N Key for No Selection text
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue("No One"), Description("I18N for No Selection")]
    public string NoSelectionText {
      get {
        return this.m_noTi18n;
      }
      set {
        this.m_noTi18n = value;
      }
    }
    
    /// <summary>
    /// Selected Ts
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<T> SelectedTs {
      get
      {
        IList<T> list = new List<T> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            T t = (T)item;
            list.Add (t);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_Ts = value;
        }
      }
    }
    
    /// <summary>
    /// Selected MachineCategory
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public T SelectedT {
      get {
        IList<T> Ts = this.SelectedTs;
        if (Ts.Count >= 1 && !this.nullCheckBox.Checked) {
          return (T)Ts[0];
        }
        else {
          return default(T);
        }
      }
      set {
        this.m_T = value;
      }
    }
    #endregion // Getters / Setters
    
    #region Events
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category("Behavior"), Description("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Generic Selection Solution
    /// Use for "standard" data selection
    /// 
    /// Set NoSelectionText with I18N Key or text
    /// </summary>
    public GenericSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Raise the AfterSelect event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAfterSelect (EventArgs e)
    {
      if (null != AfterSelect) {
        AfterSelect (this, e);
      }
    }
    
    /// <summary>
    /// Set Selected MachineCategory in listbox
    /// </summary>
    private void SetSelectedT(){
      if(this.m_T != null){
        int index = this.listBox.Items.IndexOf(this.m_T);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected MachineCategoryies in listbox
    /// </summary>
    private void SetSelectedTs(){
      if(this.m_Ts != null){
        int index = -1;
        foreach(T t in this.m_Ts){
          index = this.listBox.Items.IndexOf(t);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    
    void GenericSelectionLoad(object sender, EventArgs e)
    {
      this.SelectionLoad();
      
      this.SetSelectedT();
      this.SetSelectedTs();
      this.nullCheckBox.Text = PulseCatalog.GetString(m_noTi18n);
    }
    
    /// <summary>
    /// Refresh Data
    /// </summary>
    public void SelectionLoad()
    {
      IList<T> Ts = this.SelectionDataLoad();
      listBox.Items.Clear();
      
      if (Ts == null) {
        return;
      }

      PropertyInfo propertyInfo = typeof(T).GetProperty(DisplayedProperty);
      if (propertyInfo != null)
      {
        foreach (T t in Ts)
        {
          // Only objects whose property is not empty are added
          object value = propertyInfo.GetValue(t, null);
          if (value != null && (string)value != "") {
            listBox.Items.Add(t);
          }
        }
      }
      else
      {
        foreach (T t in Ts)
        {
          listBox.Items.Add(t);
        }
      }
      
      listBox.ValueMember = DisplayedProperty;
      listBox.Sorted = true;
    }
    
    /// <summary>
    /// Func. to override
    /// Needed for loading data into listBox.Items
    /// </summary>
    public virtual IList<T> SelectionDataLoad(){
      return null;
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        listBox.Enabled = false;
      }
      else {
        listBox.Enabled = true;
      }
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
    #endregion // Methods
  }
}
