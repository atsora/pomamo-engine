// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a machine mode
  /// </summary>
  public partial class FieldSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    bool m_onlyActive = true;
    IList<IField> m_fields = null;
    IField m_field = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (FieldSelection).FullName);
    
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
    /// Only display active fields ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(true), Description("Only active fields")]
    public bool OnlyActive {
      get { return m_onlyActive; }
      set { m_onlyActive = value; }
    }
    
    /// <summary>
    /// Is a null Field a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null field valid ?")]
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
    /// Selected Fieldss
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IField> SelectedFields {
      get
      {
        IList<IField> list = new List<IField> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IField Field = item as IField;
            list.Add (Field);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_fields = value;
        }
      }
    }

    /// <summary>
    /// Selected Fields
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IField SelectedField {
      get {
        IList<IField> fields = this.SelectedFields;
        if (fields.Count >= 1 && !this.nullCheckBox.Checked) {
          return fields[0] as IField;
        }
        else {
          return null;
        }
      }
      set {
        this.m_field = value;
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
    /// Description of the constructor
    /// </summary>
    public FieldSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("FieldNull");
      
      this.Nullable = false;
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
    /// Set Selected Fields in listbox
    /// </summary>
    private void SetSelectedField(){
      if(this.m_field != null){
        int index = this.listBox.Items.IndexOf(this.m_field);
        if(index >= 0){
          this.listBox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Fields in listbox
    /// </summary>
    private void SetSelectedFields(){
      if(this.m_fields != null){
        int index = -1;
        foreach(IField Field in this.m_fields){
          index = this.listBox.Items.IndexOf(Field);
          if(index >= 0){
            this.listBox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
    
    /// <summary>
    /// Comparer of fields
    /// Must cache Field.Display for performance issues (sorting is noticeably faster)
    /// </summary>
    class FieldComparer : IComparer, IComparer<IField> {
      
      IDictionary<int, string> m_idToDisplay = new Dictionary<int, string>();
      
      public int Compare(object x, object y)
      {
        return Compare((IField) x, (IField) y);
      }
      
      public int Compare(IField field1, IField field2)
      {
        if (null == field1) {
          return (null == field2)
            ? 0
            : -1; // null at the end
        }
        else if (null == field2) {
          Debug.Assert (null != field1);
          return 1;
        }
        
        Debug.Assert ( (null != field1) && (null != field2));
        
        string field1Display, field2Display;
        if (m_idToDisplay.ContainsKey(field1.Id)) {
          field1Display = m_idToDisplay[field1.Id];
        }
        else {
          field1Display = field1.Display;
          m_idToDisplay[field1.Id] = field1Display;          
        }
        if (m_idToDisplay.ContainsKey(field2.Id)) {
          field2Display = m_idToDisplay[field2.Id];
        }
        else {
          field2Display = field2.Display;
          m_idToDisplay[field2.Id] = field2Display;
        }
        return string.Compare (field1Display, field2Display);
      }
    }
    
    
    void FieldSelectionLoad(object sender, EventArgs e)
    {
      if (ModelDAO.ModelDAOHelper.DAOFactory == null) {
        // to allow use in designer
        return;
      }
      
      IList<IField> fields;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("FieldSelection.Load"))
      {
        fields =
          OnlyActive? daoFactory.FieldDAO.FindAllActive() : daoFactory.FieldDAO.FindAll ();
      }
      
      ArrayList.Adapter((IList) fields).Sort(new FieldComparer());
      
      listBox.BeginUpdate();
      listBox.Items.Clear ();
      
      foreach (IField field in fields) {
        listBox.Items.Add (field);
      }
      
      listBox.ValueMember = DisplayedProperty;
      listBox.DisplayMember = "Display";
      listBox.EndUpdate();
      
      this.SetSelectedField();
      this.SetSelectedFields();
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
    
    void ActiveCheckBoxCheckedChanged(object sender, System.EventArgs e)
    {
      this.OnlyActive = activeCheckBox.Checked;
      this.FieldSelectionLoad(sender, e);
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
  }
}
