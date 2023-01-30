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
  /// Control to select a machine mode
  /// </summary>
  public partial class RoleSelection : UserControl
  {
    #region Members
    string m_displayedProperty = "Display";
    IList<IRole> m_roles = null;
    IRole m_role = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (RoleSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue (false), Description ("Allow multi-selection")]
    public bool MultiSelect
    {
      get { return (listBox.SelectionMode != SelectionMode.One); }
      set
      {
        listBox.SelectionMode =
          value ? SelectionMode.MultiExtended : SelectionMode.One;
      }
    }

    /// <summary>
    /// Is a null Xxx a valid value ?
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue (false), Description ("Is a null MachineObservation valid ?")]
    public bool Nullable
    {
      get { return nullCheckBox.Visible; }
      set
      {
        if (value) {
          tableLayoutPanel1.RowStyles[1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles[1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }

    /// <summary>
    /// Property that is displayed
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue ("Display"), Description ("Property to display")]
    public string DisplayedProperty
    {
      get { return m_displayedProperty; }
      set { m_displayedProperty = value; }
    }

    /// <summary>
    /// Selected Roles
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public IList<IRole> SelectedRoles
    {
      get
      {
        IList<IRole> list = new List<IRole> ();
        if (!nullCheckBox.Checked) {
          foreach (object item in listBox.SelectedItems) {
            IRole role = item as IRole;
            list.Add (role);
          }
        }
        return list;
      }
      set
      {
        if (value != null && value.Count >= 1) {
          this.m_roles = value;
        }
      }
    }

    /// <summary>
    /// Selected Role
    /// Return the first selected (if multiselection) or null
    /// </summary>
    public IRole SelectedRole
    {
      get
      {
        IList<IRole> roles = this.SelectedRoles;
        if (roles.Count >= 1 && !this.nullCheckBox.Checked) {
          return roles[0] as IRole;
        }
        else {
          return null;
        }
      }
      set
      {
        this.m_role = value;
      }
    }
    #endregion // Getters / Setters

    #region Events
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category ("Behavior"), Description ("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RoleSelection ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      nullCheckBox.Text = PulseCatalog.GetString ("RoleNull");

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
    /// Set Selected Role in listbox
    /// </summary>
    private void SetSelectedRole ()
    {
      if (this.m_role != null) {
        int index = this.listBox.Items.IndexOf (this.m_role);
        if (index >= 0) {
          this.listBox.SetSelected (index, true);
        }
      }
    }

    /// <summary>
    /// Set Selected Roles in listbox
    /// </summary>
    private void SetSelectedRoles ()
    {
      if (this.m_roles != null) {
        int index = -1;
        foreach (IRole role in this.m_roles) {
          index = this.listBox.Items.IndexOf (role);
          if (index >= 0) {
            this.listBox.SetSelected (index, true);
          }
        }
      }
    }
    #endregion // Methods

    void RoleSelectionLoad (object sender, EventArgs e)
    {
      RoleSelectionLoad ();
    }

    void RoleSelectionLoad ()
    {
      IList<IRole> allRoles;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;

      if (daoFactory == null) {
        // to allow use in designer
        return;
      }

      using (IDAOSession session = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("RoleSelection.Load")) {
        allRoles =
          daoFactory.RoleDAO.FindAll ();
      }

      listBox.Items.Clear ();
      foreach (IRole role in allRoles) {
        listBox.Items.Add (role);
      }
      listBox.ValueMember = DisplayedProperty;

      if (listBox.Items.Count > 0) {
        listBox.SelectedIndex = 0;
      }
      else {
        listBox.Enabled = false;
        this.nullCheckBox.Checked = true;
      }

      this.SetSelectedRole ();
      this.SetSelectedRoles ();
    }

    void NullCheckBoxCheckedChanged (object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        listBox.Enabled = false;
        listBox.SelectedIndex = -1;
      }
      else {
        if (listBox.Items.Count > 0) {
          listBox.SelectedIndex = 0;
          listBox.Enabled = true;
        }
        else {
          nullCheckBox.Checked = true;
        }
      }

      OnAfterSelect (new EventArgs ());
    }

    void ListBoxSelectedIndexChanged (object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
  }
}
