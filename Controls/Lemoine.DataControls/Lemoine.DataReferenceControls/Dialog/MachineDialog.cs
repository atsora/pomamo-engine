// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of MachineDialog.
  /// </summary>
  public partial class MachineDialog : OKCancelDialog, IValueDialog<IMachine>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineDialog).FullName);
    
    #region Members
    bool m_isNullable = false;
    IList<IMachine> m_selectedMachines = null;
    #endregion
    
    #region Getters / Setters
    /// <summary>
    /// If true, return null instead of a list of 0 machines with the function SelectedValues
    /// </summary>
    public bool Nullable {
      get {
        return m_isNullable;
      }
      set {
        m_isNullable = value;
        if (m_isNullable) {
          okButton.Enabled = true;
        }
        else {
          okButton.Enabled = (displayableTreeView.SelectedElements.Count > 0);
        }
      }
    }
    
    /// <summary>
    /// Enable or disable multiple selection in the tree
    /// </summary>
    public bool MultipleSelection {
      get {
        return displayableTreeView.MultiSelection;
      }
      set {
        displayableTreeView.MultiSelection = value;
      }
    }
    
    /// <summary>
    /// Selected Machine or null if no Machine is selected
    /// </summary>
    public IMachine SelectedValue {
      get
      {
        var selectedElements = SelectedValues;
        return (selectedElements != null && selectedElements.Count > 0) ?
          selectedElements[0] : null;
      }
      set {
        IList<IDisplayable> selectedElements = new List<IDisplayable>();
        selectedElements.Add(value);
        displayableTreeView.SelectedElements = selectedElements;
      }
    }
    
    /// <summary>
    /// Selected Machines or null if no Machine is selected
    /// </summary>
    public IList<IMachine> SelectedValues {
      get
      {
        if ((m_selectedMachines == null || m_selectedMachines.Count == 0) && Nullable) {
          return null;
        }

        return m_selectedMachines;
      }
      set {
        displayableTreeView.SelectedElements = value.Cast<IDisplayable>().ToList();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// To display the dialog, use "ShowDialog", not "Show"
    /// </summary>
    public MachineDialog()
    {
      InitializeComponent();
      displayableTreeView.AddOrder("Sort by category", new string[] {"Company", "Category"});
      displayableTreeView.AddOrder("Sort by department", new string[] {"Company", "Department"});

      this.Text = PulseCatalog.GetString("MachineDialogTitle");
    }
    #endregion // Constructors
    
    #region Methods
    void LoadMachines()
    {
      IList<IMachine> machines;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;

      using (IDAOSession session = daoFactory.OpenSession ()) {
        machines = daoFactory.MachineDAO.FindAllOrderByName();
      }

      foreach (IMachine machine in machines) {
        displayableTreeView.AddElement(machine);
      }
    }
    #endregion // Methods

    #region Event reactions
    void MachineDialogLoad(object sender, EventArgs e)
    {
      LoadMachines();
      displayableTreeView.SelectedOrder = 0;
    }
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    
    void DisplayableTreeViewSelectionChanged()
    {
      m_selectedMachines = displayableTreeView.SelectedElements.Cast<IMachine>().ToList();
      okButton.Enabled = (m_isNullable || m_selectedMachines.Count > 0);
    }
    #endregion // Event reactions
  }
}
