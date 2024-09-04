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
using Lemoine.ModelDAO;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
  internal partial class Page3 : GenericWizardPage, IWizardPage
  {
    private class ComboboxItem
    {
      public string Text { get; set; }
      public object Value { get; set; }
      public override string ToString() { return Text; }
    }
    
    #region Members
    IList<StructOperation> m_operations = new List<StructOperation>();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Involving machines"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "Each operation involves one or several machines.\n" +
          "The default parameters (if they have been entered in the previous page) can be recalled.\n\n" +
          "Performances are not available yet."; } }
    
    StructOperation CurrentOperation {
      get { return (StructOperation)((ComboboxItem)comboOperation.SelectedItem).Value; }
    }
    
    StructMachine CurrentMachine {
      get { return listBox.SelectedValue as StructMachine; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page3()
    {
      InitializeComponent();
      comboOperation.DisplayMember = "Text";
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
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_operations.Clear();
      IList<StructPart> parts = data.Get<List<StructPart>>(Item.PARTS);
      foreach (StructPart part in parts) {
        foreach (StructOperation op in part.Operations) {
          m_operations.Add(op);
        }
      }

      FillCombobox (0);
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Nothing to do: automatically saved
    }
    
    /// <summary>
    /// Called after SavePage and before going to the previous page
    /// </summary>
    /// <param name="data"></param>
    public override void DoSomethingBeforePrevious(ItemData data)
    {
      data.Store(Item.CURRENT_PART_INDEX, data.Get<int>(Item.CURRENT_PART_INDEX) - 1);
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return null;
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // All operations currently used
      IList<IOperation> currentOperations =  new List<IOperation>();
      foreach (StructPart part in data.Get<List<StructPart>>(Item.PARTS)) {
        foreach (StructOperation op in part.Operations) {
          currentOperations.Add(op.Operation);
        }
      }

      IList<string> alreadyDedicated = new List<string>();
      IList<string> alreadyUsed = new List<string>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Load dedicated and non dedicated machines of all operations not used in the current line
          IList<IMachine> alreadyDedicatedMachines = new List<IMachine>();
          IList<IMachine> alreadyNonDedicatedMachines = new List<IMachine>();
          IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO.FindAll();
          foreach (ILineMachine lineMachine in lineMachines) {
            if (!currentOperations.Contains(lineMachine.Operation)) {
              if (lineMachine.LineMachineStatus == LineMachineStatus.Dedicated &&
                  !alreadyDedicatedMachines.Contains(lineMachine.Machine)) {
                alreadyDedicatedMachines.Add(lineMachine.Machine);
              }
              else if (lineMachine.LineMachineStatus == LineMachineStatus.Extra &&
                       !alreadyNonDedicatedMachines.Contains(lineMachine.Machine)) {
                alreadyNonDedicatedMachines.Add(lineMachine.Machine);
              }
            }
          }
          
          // A machine already dedicated cannot be used anymore
          // A machine already used cannot be dedicated
          foreach (StructPart structPart in data.Get<List<StructPart>>(Item.PARTS)) {
            foreach (StructOperation structOperation in structPart.Operations) {
              foreach (StructMachine structMachine in structOperation.m_machines) {
                // Check if the machine can be used
                if (alreadyDedicatedMachines.Contains(structMachine.m_machine)) {
                  alreadyDedicated.Add(structMachine.m_machine.Display);
                }
                else if (structMachine.m_dedicated && alreadyNonDedicatedMachines.Contains(structMachine.m_machine)) {
                  alreadyUsed.Add(structMachine.m_machine.Display);
                }

                // Add the machine to the list
                if (structMachine.m_dedicated) {
                  alreadyDedicatedMachines.Add(structMachine.m_machine);
                }
                else {
                  alreadyNonDedicatedMachines.Add(structMachine.m_machine);
                }
              }
            }
          }
        }
      }
      
      if (alreadyDedicated.Count > 0) {
        errors.Add("a machine already dedicated to an operation cannot be used for another operation (\"" +
                   string.Join("\", \"", alreadyDedicated.ToArray()) + "\")");
      }
      
      if (alreadyUsed.Count > 0) {
        errors.Add("a machine already used by an operation cannot be dedicated to a new operation (\"" +
                   string.Join("\", \"", alreadyUsed.ToArray()) + "\")");
      }
      
      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      IList<StructOperation> operations =  new List<StructOperation>();
      IList<StructPart> parts = data.Get<List<StructPart>>(Item.PARTS);
      foreach (StructPart part in parts) {
        foreach (StructOperation op in part.Operations) {
          operations.Add(op);
        }
      }

      IList<string> warnings = new List<string>();
      
      // At least one machine per operation
      int count = 0;
      foreach (StructOperation operation in operations) {
        if (operation.m_machines.Count == 0) {
          count++;
        }
      }

      if (count > 0) {
        string plural = (count > 1) ? "s don't" : " doesn't";
        warnings.Add(count + " operation" + plural + " involve machines.");
      }
      
      return warnings;
    }
    
    /// <summary>
    /// Get a summary of the user inputs, which will be displayed in a tree
    /// Each string in the list will be a rootnode. If line breaks are present, all lines
    /// after the first one will be shown as child nodes.
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<StructOperation> operations =  new List<StructOperation>();
      IList<StructPart> parts = data.Get<List<StructPart>>(Item.PARTS);
      foreach (StructPart part in parts) {
        foreach (StructOperation op in part.Operations) {
          operations.Add(op);
        }
      }

      IList<string> summary = new List<string>();
      
      foreach (StructOperation operation in operations) {
        string text = "Operation \"" + operation.Name + "\" comprises";
        bool machineFound = false;
        foreach (StructMachine machine in operation.m_machines) {
          machineFound = true;
          text += "\n" + machine.m_name + " (";
          if (machine.m_dedicated) {
            text += "fully dedicated";
          }
          else {
            text += "not fully dedicated";
          }

          text += ")";
        }
        if (!machineFound) {
          text += " no machines";
        }

        summary.Add(text);
      }

      return summary;
    }
    #endregion // Methods
    
    #region Private methods
    void FillCombobox(int selectedIndex)
    {
      comboOperation.Items.Clear();
      foreach (StructOperation operation in m_operations) {
        comboOperation.Items.Add(new ComboboxItem {
                                   Text = operation.Name,
                                   Value = operation}
                                );
      }
      
      comboOperation.SelectedIndex = selectedIndex;
    }
    
    void FillList(int selectedIndex)
    {
      StructOperation currentOperation = CurrentOperation;
      listBox.ClearItems();
      foreach (StructMachine machine in currentOperation.m_machines) {
        listBox.AddItem(machine.m_name, machine);
      }

      listBox.SelectedIndex = selectedIndex;
      ListBoxItemChanged("", "");
    }
    #endregion
    
    #region Event reactions
    void ComboOperationSelectedIndexChanged(object sender, System.EventArgs e)
    {
      FillList(0);
    }
    
    void ListBoxItemChanged(string arg1, object arg2)
    {
      int selectedIndex = listBox.SelectedIndex;
      
      // Enable controls
      bool isIndexSelected = (selectedIndex != -1);
      checkDedicated.Enabled = isIndexSelected;
      buttonRemove.Enabled = isIndexSelected;
      
      // Fill values
      if (selectedIndex != -1) {
        StructMachine currentMachine = listBox.SelectedValue as StructMachine;
        checkDedicated.Checked = currentMachine.m_dedicated;
      }
    }
    
    void ButtonAddClick(object sender, EventArgs e)
    {
      MachineDialog dialog = new MachineDialog();
      dialog.MultipleSelection = true;
      dialog.ShowDialog(this);
      if (dialog.DialogResult == DialogResult.OK) {
        IList<IMachine> machines = dialog.SelectedValues;
        
        if (machines != null && machines.Count > 0) {
          StructOperation currentOperation = CurrentOperation;
          bool duplicated = false;
          foreach (IMachine machine in machines) {
            if (currentOperation.ContainsMachine(machine)) {
              duplicated = true;
            }
            else {
              StructMachine newMachine = new StructMachine(machine, machine.Display);
              currentOperation.m_machines.Add(newMachine);
              currentOperation.m_machines = currentOperation.m_machines.OrderBy(o => o.m_name).ToList();
            }
          }
          
          if (duplicated) {
            MessageBoxCentered.Show(this, "A machine cannot be used twice for the same operation.",
                                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          }

          FillList (currentOperation.m_machines.IndexOf(currentOperation.m_machines.Last()));
        }
      }
      dialog.Dispose();
    }
    
    void ButtonRemoveClick(object sender, EventArgs e)
    {
      int currentIndex = listBox.SelectedIndex;
      CurrentOperation.m_machines.RemoveAt(currentIndex);
      if (currentIndex >= CurrentOperation.m_machines.Count) {
        currentIndex--;
      }

      FillList (currentIndex);
    }
    
    void CheckDedicatedLeave(object sender, EventArgs e)
    {
      CurrentMachine.m_dedicated = checkDedicated.Checked;
    }
    #endregion // Event reactions
  }
}
