// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of EventLevelSelection.
  /// </summary>
  public partial class EventLevelSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "SelectionText";
    IList<IEventLevel> m_eventLevels = null;
    IEventLevel m_eventLevel = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventLevelSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    public bool MultiSelect {
      get { return (eventListbox.SelectionMode != SelectionMode.One); }
      set
      {
        eventListbox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }
    
    /// <summary>
    /// Is a null EventLevel a valid value ?
    /// </summary>
    public bool Nullable {
      get { return nullCheckBox.Visible; }
      set
      {
        if (value) {
          tableLayoutPanel.RowStyles [1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel.RowStyles [1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  m_displayedProperty; }
      set { m_displayedProperty = value; }
    }

    /// <summary>
    /// Selected EventLevelss
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IEventLevel> SelectedEventLevels {
      get
      {
        IList<IEventLevel> list = new List<IEventLevel> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in eventListbox.SelectedItems) {
            IEventLevel EventLevel = item as IEventLevel;
            list.Add (EventLevel);
          }
        }
        return list;
      }
      set {
        if(value != null && value.Count >= 1){
          this.m_eventLevels = value;
        }
      }
    }

    /// <summary>
    /// Selected EventLevels
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IEventLevel SelectedEventLevel {
      get {
        IList<IEventLevel> eventLevels = this.SelectedEventLevels;
        if (eventLevels.Count >= 1 && !this.nullCheckBox.Checked) {
          return eventLevels[0] as IEventLevel;
        }
        else {
          return null;
        }
      }
      set {
        this.m_eventLevel = value;
      }
    }
    #endregion // Getters / Setters
    
    #region Event
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category("Behavior"), Description("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// EventLevel Selection Listbox
    /// </summary>
    public EventLevelSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("EventLevelNull");
      
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
    
    void EventLevelSelectionLoad(object sender, EventArgs e)
    {
      IList<IEventLevel> eventLevels;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if (daoFactory == null) {
        // to allow use in designer
        return;
      }
      
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("EventLevelSelection.Load"))
      {
        eventLevels = daoFactory.EventLevelDAO.FindAll ();
      }
      
      eventListbox.Items.Clear ();
      foreach (IEventLevel eventLevel in eventLevels) {
        eventListbox.Items.Add (eventLevel);
      }
      eventListbox.ValueMember = DisplayedProperty;
      
      this.SetSelectedEventLevel();
      this.SetSelectedEventLevels();
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        eventListbox.Enabled = false;
      }
      else {
        eventListbox.Enabled = true;
      }
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
    
    /// <summary>
    /// Set Selected EventLevels in listbox
    /// </summary>
    private void SetSelectedEventLevel(){
      if(this.m_eventLevel != null){
        int index = this.eventListbox.Items.IndexOf(this.m_eventLevel);
        if(index >= 0){
          this.eventListbox.SetSelected(index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected EventLevels in listbox
    /// </summary>
    private void SetSelectedEventLevels(){
      if(this.m_eventLevels != null){
        int index = -1;
        foreach(IEventLevel EventLevel in this.m_eventLevels){
          index = this.eventListbox.Items.IndexOf(EventLevel);
          if(index >= 0){
            this.eventListbox.SetSelected(index,true);
          }
        }
      }
    }
    #endregion // Methods
  }
}
