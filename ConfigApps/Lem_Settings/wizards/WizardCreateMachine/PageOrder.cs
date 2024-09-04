// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateMachine
{
  /// <summary>
  /// Description of PageOrder.
  /// </summary>
  internal partial class PageOrder : GenericWizardPage, IWizardPage
  {
    #region Members
    double m_currentPriority = -1;
    string m_machineName = "";
    IMachine m_currentMachine = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Display order"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "You can edit here the order of the machines when " +
          "they are displayed in a list. To move the current machine, use the arrows."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageOrder()
    {
      InitializeComponent();
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
      m_currentPriority = data.Get<double>(AbstractItem.MACHINE_PRIORITY);
      m_machineName = data.Get<string>(AbstractItem.MACHINE_NAME);
      m_currentMachine = data.Get<IMachine>(AbstractItem.MACHINE);
      RefreshList();
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(AbstractItem.MACHINE_PRIORITY, m_currentPriority);
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
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      // Find the closest machines
      int beforeId;
      int afterId;
      int closestPriorityBefore;
      int closestPriorityAfter;
      GetClosestMachines(data.Get<double>(AbstractItem.MACHINE_PRIORITY),
                         out beforeId, out closestPriorityBefore,
                         out afterId, out closestPriorityAfter);
      if (beforeId == -1) {
        summary.Add("First position");
      }
      else if (afterId == -1) {
        summary.Add("Last position");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          summary.Add("After machine \"" + ModelDAOHelper.DAOFactory.MachineDAO.FindById(beforeId).Display + "\"");
          summary.Add("Before machine \"" + ModelDAOHelper.DAOFactory.MachineDAO.FindById(afterId).Display + "\"");
        }
      }
      
      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    void RefreshList()
    {
      // Sorted machines by priority
      IDictionary<double, List<string>> elements = new Dictionary<double, List<string>>();
      
      // Add existing machines
      int maxPriority = 0;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
        if (m_currentMachine != null) {
          machines.Remove(m_currentMachine);
        }

        foreach (IMachine machine in machines) {
          int priority = 0;
          if (machine.DisplayPriority.HasValue) {
            priority = machine.DisplayPriority.Value;
          }

          if (priority > maxPriority) {
            maxPriority = priority;
          }

          if (!elements.ContainsKey((double)priority)) {
            elements[(double)priority] = new List<string>();
          }

          elements[(double)priority].Add(machine.Display);
        }
      }
      
      // Add the new machine
      if (m_currentPriority < -0.7) {
        m_currentPriority = maxPriority + 0.5;
      }

      if (!elements.ContainsKey(m_currentPriority)) {
        elements[m_currentPriority] = new List<string>();
      }

      elements[m_currentPriority].Add("current machine \"" + m_machineName + "\"");
      
      // Fill the list
      listMachines.ClearItems();
      foreach (KeyValuePair<double, List<string>> strings in elements.OrderBy(i => i.Key)) {
        List<string> displayedElements = strings.Value;
        displayedElements.Sort();
        foreach (string str in displayedElements) {
          bool isNewMachine = (Math.Abs(strings.Key - (int)strings.Key) > 0.2);
          if (isNewMachine) {
            listMachines.AddItem(str, strings.Key, str, Color.Blue, false, true, SystemColors.Highlight);
          }
          else {
            listMachines.AddItem(str, strings.Key, str, SystemColors.ControlText, false, false, SystemColors.Window);
          }
        }
      }
      listMachines.SelectedValue = m_currentPriority;
    }
    
    void GetClosestMachines(double priority, out int beforeId, out int beforePriority, out int afterId, out int afterPriority)
    {
      // Find the closest machines
      beforeId = -1;
      afterId = -1;
      beforePriority = -1;
      afterPriority = -1;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
        if (m_currentMachine != null) {
          machines.Remove(m_currentMachine);
        }

        foreach (IMachine machine in machines) {
          int priorityTmp = 0;
          if (machine.DisplayPriority.HasValue) {
            priorityTmp = machine.DisplayPriority.Value;
          }

          if (priority > priorityTmp)
          {
            // Other machine is before
            if (beforeId == -1)
            {
              // Not defined yet
              beforeId = machine.Id;
              beforePriority = priorityTmp;
            }
            else if (beforePriority < priorityTmp)
            {
              // Closest machine found
              beforeId = machine.Id;
              beforePriority = priorityTmp;
            }
            else if (beforePriority == priorityTmp)
            {
              // Ambiguïty: alphabetical order of the Display field
              IMachine otherMachine = ModelDAOHelper.DAOFactory.MachineDAO.FindById(beforeId);
              if (string.Compare(otherMachine.Display, machine.Display) < 0) {
                beforeId = machine.Id;
              }
            }
          }
          else
          {
            // Other machine is after
            if (afterId == -1)
            {
              // Not defined yet
              afterId = machine.Id;
              afterPriority = priorityTmp;
            }
            else if (afterPriority > priorityTmp)
            {
              // Closest machine found
              afterId = machine.Id;
              afterPriority = priorityTmp;
            }
            else if (afterPriority == priorityTmp)
            {
              // Ambiguïty: alphabetical order of the Display field
              IMachine otherMachine = ModelDAOHelper.DAOFactory.MachineDAO.FindById(afterId);
              if (string.Compare(machine.Display, otherMachine.Display) < 0) {
                afterId = machine.Id;
              }
            }
          }
        }
      }
    }
    #endregion // Private methods
    
    #region Event reactions
    void ButtonUpClick(object sender, EventArgs e)
    {
      int beforeId;
      int afterId;
      int closestPriorityBefore;
      int closestPriorityAfter;
      GetClosestMachines(m_currentPriority,
                         out beforeId, out closestPriorityBefore,
                         out afterId, out closestPriorityAfter);
      
      if (beforeId != -1) {
        m_currentPriority = (double)closestPriorityBefore - 0.5;
        RefreshList();
      }
    }
    
    void ButtonDownClick(object sender, EventArgs e)
    {
      int beforeId;
      int afterId;
      int closestPriorityBefore;
      int closestPriorityAfter;
      GetClosestMachines(m_currentPriority,
                         out beforeId, out closestPriorityBefore,
                         out afterId, out closestPriorityAfter);
      
      if (afterId != -1) {
        m_currentPriority = (double)closestPriorityAfter + 0.5;
        RefreshList();
      }
    }
    
    void ButtonTopClick(object sender, EventArgs e)
    {
      m_currentPriority = -0.5;
      RefreshList();
    }
    
    void ButtonBottomClick(object sender, EventArgs e)
    {
      m_currentPriority = -1;
      RefreshList();
    }
    #endregion // Event reactions
  }
}
