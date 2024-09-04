// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  public partial class Page2 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    AlarmManager m_alarmManager = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Filter"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "You can choose here which alerts will be displayed, by filtering:\n\n" +
      " - on the event type,\n" +
      " - on the event level,\n" +
      " - on the machines,\n" +
      " - on the users."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.DONT_LOG_VALIDATION |
        LemSettingsGlobal.PageFlag.DONT_SHOW_SUCCESS_INFORMATION |
        LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2()
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
    public void Initialize(ItemContext context)
    {
      // Event types
      foreach (string dataType in SingletonEventLevel.GetDataTypes()) {
        comboBoxEventType.AddItem(SingletonEventLevel.GetDisplayedName(dataType), dataType);
      }

      comboBoxEventType.InsertItem("All", "", 0);
      
      // Machine list
      treeViewMachines.ClearOrders();
      treeViewMachines.AddOrder("Sort by department", new string[] {
        "Company",
        "Department"
      });
      treeViewMachines.AddOrder("Sort by category", new string[] {
        "Company",
        "Category"
      });
      treeViewMachines.TreeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
      treeViewMachines.TreeView.DrawNode += TreeMachines_DrawNode;
    
      // Event levels
      comboBoxEventLevel.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IEventLevel> levels = ModelDAOHelper.DAOFactory.EventLevelDAO.FindAll();
          foreach (var level in levels) {
            comboBoxEventLevel.AddItem(level.Display, level);
          }
        }
      }
      comboBoxEventLevel.InsertItem("All", null, 0);

      // input items
      comboboxInputItems.ClearItems();
      comboboxInputItems.InsertItem("All", null, 0);
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_alarmManager = data.Get<AlarmManager>(Item.ALARM_MANAGER);
      listUsers.AlarmManager = m_alarmManager;
      
      comboBoxEventType.SelectedValue = data.Get<string>(Item.FILTER_EVENT_TYPE);
      checkBoxMachinesLinked.Checked = data.Get<bool>(Item.TREEVIEW_MACHINE_LINKED);
      checkBoxUsersLinked.Checked = data.Get<bool>(Item.LIST_EMAILS_LINKED);
      treeViewMachines.SelectedOrder = data.Get<int>(Item.TREEVIEW_MACHINE_ORDER);
      treeViewMachines.SelectedElements = data.Get<IList<IMachine>>(Item.FILTER_MACHINES).Cast<IDisplayable>().ToList();
      FillUsers(data.Get<IList<EmailWithName>>(Item.FILTER_USERS));
      comboBoxEventLevel.SelectedValue = data.Get<IEventLevel>(Item.FILTER_LEVEL);
      comboboxInputItems.SelectedValue = data.Get<string>(Item.FILTER_ITEM);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.TREEVIEW_MACHINE_LINKED, checkBoxMachinesLinked.Checked);
      data.Store(Item.LIST_EMAILS_LINKED, checkBoxUsersLinked.Checked);
      data.Store(Item.TREEVIEW_MACHINE_ORDER, treeViewMachines.SelectedOrder);
      
      // The filters are saved in the validate method
    }
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      data.Store(Item.FILTER_EVENT_TYPE, comboBoxEventType.SelectedValue);
      data.Store(Item.FILTER_MACHINES, treeViewMachines.SelectedElements.Cast<IMachine>().ToList());
      data.Store(Item.FILTER_LEVEL, comboBoxEventLevel.SelectedValue);
      data.Store(Item.FILTER_ITEM, comboboxInputItems.SelectedValue);
      data.Store(Item.FILTER_USERS, listUsers.CheckedItems.Cast<EmailWithName>().ToList());
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
    void FillMachines(IList<IMachine> selectedMachines)
    {
      treeViewMachines.ClearElements();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAllNotObsolete();
          if (checkBoxMachinesLinked.Checked) {
            // Only insert linked machines
            string dataType = comboBoxEventType.SelectedValue as string;
            foreach (IMachine machine in machines) {
              if (m_alarmManager.IsMachineLinked(machine, dataType)) {
                treeViewMachines.AddElement(machine);
              }
            }
          } else {
            // Insert all machines
            foreach (var machine in machines) {
              treeViewMachines.AddElement(machine);
            }
          }
        }
      }
      treeViewMachines.RefreshTreeview();
      if (selectedMachines != null) {
        treeViewMachines.SelectedElements = selectedMachines.Cast<IDisplayable>().ToList();
      }
    }
    
    void FillUsers(IList<EmailWithName> selectedUsers)
    {
      IList<EmailWithName> emails = m_alarmManager.ListEmails;
      
      using (var suspendDrawing = new SuspendDrawing(listUsers)) {
        listUsers.Items.Clear();
        if (checkBoxUsersLinked.Checked) {
          // Only insert linked users
          string dataType = comboBoxEventType.SelectedValue as string;
          foreach (EmailWithName email in emails) {
            if (m_alarmManager.IsEmailLinked(email, dataType)) {
              int index = listUsers.Items.Add(email);
              if (selectedUsers != null && selectedUsers.Contains(email)) {
                listUsers.SetItemChecked(index, true);
              }
            }
          }
        } else {
          // Insert all users
          foreach (EmailWithName email in emails) {
            int index = listUsers.Items.Add(email);
            if (selectedUsers != null && selectedUsers.Contains(email)) {
              listUsers.SetItemChecked(index, true);
            }
          }
        }
      }
    }
    #endregion // Private methods
    
    #region Event reactions
    void TreeMachines_DrawNode(object sender, DrawTreeNodeEventArgs e)
    {
      // Font
      Font nodeFont = e.Node.NodeFont;
      Color nodeColor = e.Node.ForeColor;
      var machine = e.Node.Tag as IMachine;
      if (machine != null) {
        if (checkBoxMachinesLinked.Checked ||
            m_alarmManager.IsMachineLinked(machine, (string)comboBoxEventType.SelectedValue)) {
          nodeColor = Color.Blue;
        }
      }

      TextRenderer.DrawText(e.Graphics, e.Node.Text, nodeFont, e.Bounds, nodeColor);
    }
    
    void CheckBoxMachinesLinkedCheckedChanged(object sender, EventArgs e)
    {
      FillMachines(treeViewMachines.SelectedElements.Cast<IMachine>().ToList());
    }
    
    void CheckBoxUsersLinkedCheckedChanged(object sender, EventArgs e)
    {
      FillUsers(listUsers.CheckedItems.Cast<EmailWithName>().ToList());
    }
    
    void ComboBoxEventTypeItemChanged(string arg1, object arg2)
    {
      { // Load levels
        comboBoxEventLevel.ClearItems();
        foreach (IEventLevel level in SingletonEventLevel.GetLevels(comboBoxEventType.SelectedValue.ToString())) {
          comboBoxEventLevel.AddItem(level.Display, level);
        }

        comboBoxEventLevel.InsertItem("All", null, 0);
        comboBoxEventLevel.SelectedIndex = 0;

        // Update everything (colors can change in lists and alarm list changes)
        listUsers.Datatype = comboBoxEventType.SelectedValue as string;
        FillMachines(treeViewMachines.SelectedElements.Cast<IMachine>().ToList());
        FillUsers(listUsers.CheckedItems.Cast<EmailWithName>().ToList());
      }
      { // Items
        comboboxInputItems.ClearItems();
        foreach (string item in SingletonEventLevel.GetInputItems(comboBoxEventType.SelectedValue.ToString())) {
          comboboxInputItems.AddItem(item, item);
        }

        comboboxInputItems.InsertItem("All", "", 0);
        comboboxInputItems.SelectedIndex = 0;

        // Update everything (colors can change in lists and alarm list changes)
        listUsers.Datatype = comboBoxEventType.SelectedValue as string;
        FillMachines(treeViewMachines.SelectedElements.Cast<IMachine>().ToList());
        FillUsers(listUsers.CheckedItems.Cast<EmailWithName>().ToList());
      }
    }
    #endregion // Event reactions
  }
}
