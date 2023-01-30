// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a machine mode
  /// </summary>
  public partial class MachineModeSelection : UserControl
  {
    #region Members
    IList<IMachineMode> m_machineModes = new List<IMachineMode>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Can several rows be selected ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Allow multi-selection")]
    public bool MultiSelect {
      get { return treeView.MultiSelection; }
      set { treeView.MultiSelection = value; }
    }
    
    /// <summary>
    /// Is a null MachineMode a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null MachineObservation valid?")]
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
      get { return treeView.DisplayedProperty; }
      set { treeView.DisplayedProperty = value; }
    }

    /// <summary>
    /// Selected MachineModes
    /// 
    /// The returned list is not null, but may be empty or contain only one null element
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<IMachineMode> SelectedMachineModes {
      get {
        return nullCheckBox.Checked ?
          new List<IMachineMode>() :
          treeView.SelectedElements.Cast<IMachineMode>().ToList();
      }
      set {
        if (value != null && value.Count >= 1) {
          m_machineModes = value;
        }
      }
    }

    /// <summary>
    /// Selected MachineMode
    /// Return the first selected (if multiselection) or null
    /// </summary>
    [Browsable (false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IMachineMode SelectedMachineMode {
      get {
        if (this.nullCheckBox.Checked) {
          return null;
        }

        IList<IMachineMode> machineModes = this.SelectedMachineModes;
        return (machineModes.Count >= 1) ? machineModes[0] as IMachineMode : null;
      }
      set {
        m_machineModes.Clear();
        m_machineModes.Add(value);
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
    public MachineModeSelection()
    {
      InitializeComponent();
      nullCheckBox.Text = PulseCatalog.GetString ("MachineModeNull");
      this.Nullable = false;
      this.DisplayedProperty = "Display";
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
        AfterSelect(this, e);
      }
    }

    /// <summary>
    /// Set Selected MachineModes in listbox
    /// </summary>
    void SetSelectedMachineModes()
    {
      if (this.m_machineModes != null) {
        treeView.SelectedElements = m_machineModes.Cast<object>().ToList();
      }
    }
    
    /// <summary>
    /// Add a function to determine the color of an element
    /// </summary>
    public void AddDetermineColorFunction(Func<object, Color> lambda)
    {
      treeView.AddDetermineColorFunction(lambda);
    }
    #endregion // Methods
    
    /// <summary>
    /// Reload the control
    /// </summary>
    public void Reload ()
    {
      MachineModeSelectionLoad();
    }
    
    void MachineModeSelectionLoad(object sender, EventArgs e)
    {
      MachineModeSelectionLoad();
    }
    
    void MachineModeSelectionLoad()
    {
      IList<IMachineMode> machineModes;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      // To allow use in designer
      if (daoFactory == null) {
        return;
      }

      using (IDAOSession session = daoFactory.OpenSession())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction("MachineModeSelection.Load"))
      {
        machineModes = daoFactory.MachineModeDAO.FindAll();
      }
      
      // Populate the tree
      treeView.ClearElements();
      foreach (IMachineMode machineMode in machineModes) {
        treeView.AddElement(machineMode);
      }

      treeView.RefreshTreeview();
      
      // Restore selection
      if (MultiSelect) {
        treeView.SelectedElements = (m_machineModes == null) ? null :
          m_machineModes.Cast<object>().ToList();
      }
      else if (m_machineModes.Count > 0) {
        treeView.SelectedElement = m_machineModes[0];
      }
      else {
        treeView.SelectedElement = null;
      }
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      treeView.Enabled = !nullCheckBox.Checked;
      OnAfterSelect(new EventArgs());
    }
    
    void TreeViewSelectionChanged()
    {
      OnAfterSelect(new EventArgs());
    }
  }
}
