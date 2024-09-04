// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.DataReferenceControls;
using Lemoine.Model;

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    LineData m_lineData = null;
    bool m_isEdited = false;
    bool m_preparation = true;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Change machines"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "After having selected a line, it is possible to:\n"+
          "- change its properties,\n" +
          "- configure operations associated to the line,\n" +
          "- add, remove or configure machines assigned to an operation.\n\n" +
          "Note: best and dedicated performances not available yet."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    
    private IOperation CurrentOperation { get; set; }
    private IMachine CurrentMachine { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1(ItemContext context)
    {
      InitializeComponent();
      label4.Visible = label5.Visible = numericBest.Visible = numericExpected.Visible =
        unitBest.Visible = unitExpected.Visible = false;
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_lineData = data.Get<LineData>(Item.LINE_DATA);
      m_lineData.LoadLine(data.Get<ILine>(Item.LINE));
      CurrentOperation = data.Get<IOperation>(Item.OPERATION);
      CurrentMachine = data.Get<IMachine>(Item.MACHINE);
      
      EmitProtectAgainstQuit(data.Get<bool>(Item.LINE_EDITED));
      
      FillInterface();
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.OPERATION, CurrentOperation);
      data.Store(Item.MACHINE, CurrentMachine);
      if (m_isEdited) {
        data.Store(Item.LINE_EDITED, true);
      }
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      return m_lineData.GetErrorsBeforeValidation();
    }
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revisionId">Revision that is going to be applied when the function returns</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      m_lineData.Validate();
    }
    
    /// <summary>
    /// If the validation step is enabled, method called after the validation and after the possible progress
    /// bar linked to a revision (the user or the timeout could have canceled the progress bar but in that
    /// case a warning is displayed).
    /// Don't forget to emit "DataChangedEvent" if data changed
    /// </summary>
    /// <param name="data">data that can be processed before the page changes</param>
    public override void ProcessAfterValidation(ItemData data)
    {
      EmitDataChangedEvent(null);
    }
    #endregion // Page methods
    
    #region Private methods
    void FillInterface()
    {
      m_preparation = true;
      
      // Clear interface
      listMachines.ClearItems();
      listOperations.ClearItems();
      
      // Retrieve operations
      IList<OperationData> ops = m_lineData.GetOperations();
      foreach (OperationData op in ops) {
        listOperations.AddItem(op.Display, op.Operation);
      }

      if (listOperations.Count > 0) {
        buttonEditOperation.Enabled = true;
        if (CurrentOperation == null) {
          CurrentOperation = ops[0].Operation;
        }

        // We select the previous operation, if any
        listOperations.SelectedValue = CurrentOperation;
        
        // With the selection, the machine list is now populated
        if (!listMachines.Values.Contains(CurrentMachine) && listMachines.Count > 0) {
          CurrentMachine = (IMachine)listMachines.Values[0];
        }

        listMachines.SelectedValue = CurrentMachine;
        ListMachinesItemChanged("", "");
      }
      m_preparation = false;
    }
    #endregion // Private methods
    
    #region Event reactions
    void ListOperationsItemChanged(string arg1, object arg2)
    {
      if (!m_preparation) {
        CurrentOperation = (IOperation)listOperations.SelectedValue;
      }

      buttonEditOperation.Enabled = (CurrentOperation != null);
      listMachines.ClearItems();
      
      if (CurrentOperation != null)
      {
        // Fill machines
        IList<MachineData> machines = m_lineData.GetMachines(CurrentOperation);
        foreach (MachineData machine in machines) {
          listMachines.AddItem(machine.Display, machine.Machine);
        }
      }
      
      if (listMachines.Count > 0) {
        listMachines.SelectedIndex = 0;
        checkDedicated.Enabled = true;
      } else {
        checkDedicated.Enabled = false;
      }
    }
    
    void ListOperationsItemDoubleClicked(string arg1, object arg2)
    {
      ButtonEditOperationClick(null, null);
    }
    
    void ListMachinesItemChanged(string arg1, object arg2)
    {
      if (!m_preparation) {
        CurrentMachine = (IMachine)listMachines.SelectedValue;
      }

      if (CurrentOperation != null && CurrentMachine != null) {
        IList<MachineData> machines = m_lineData.GetMachines(CurrentOperation);
        foreach (MachineData machine in machines) {
          if (machine.Machine == CurrentMachine) {
            m_preparation = true;
            checkDedicated.Checked = machine.Dedicated;
            m_preparation = false;
          }
        }
      }
    }
    
    void ButtonEditOperationClick(object sender, EventArgs e)
    {
      if (CurrentOperation != null) {
        EmitDisplayPageEvent ("Page3", null);
      }
    }
    
    void ButtonAddMachineClick(object sender, EventArgs e)
    {
      MachineDialog dialog = new MachineDialog();
      dialog.MultipleSelection = true;
      dialog.ShowDialog(this);
      if (dialog.DialogResult == DialogResult.OK) {
        IList<IMachine> machines = dialog.SelectedValues;
        
        if (machines != null && machines.Count > 0)
        {
          // Add machines
          bool duplicatedMachine = false;
          foreach (IMachine machine in machines) {
            duplicatedMachine |= m_lineData.AddMachine(CurrentOperation, machine);
          }

          // Warning if duplicate
          if (duplicatedMachine) {
            MessageBoxCentered.Show(this, "A machine cannot be used twice for the same operation within a line.",
                                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          }

          // Update interface
          m_isEdited = true;
          CurrentMachine = machines[0];
          FillInterface();
          EmitProtectAgainstQuit(true);
        }
      }
      dialog.Dispose();
    }
    
    void ButtonRemoveMachineClick(object sender, EventArgs e)
    {
      if (CurrentMachine != null) {
        m_lineData.RemoveMachine(CurrentOperation, CurrentMachine);
        
        // Update interface
        m_isEdited = true;
        FillInterface();
        EmitProtectAgainstQuit(true);
      }
    }
    
    void CheckDedicatedCheckedChanged(object sender, EventArgs e)
    {
      if (m_preparation) {
        return;
      }

      if (CurrentMachine != null) {
        m_lineData.SetMachineDedicated(CurrentOperation, CurrentMachine, checkDedicated.Checked);
        
        // Update interface
        m_isEdited = true;
        FillInterface();
        EmitProtectAgainstQuit(true);
      }
    }
    #endregion // Event reactions
  }
}
