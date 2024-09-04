// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.UserMachine
{
  /// <summary>
  /// Description of UserMachinePage3.
  /// </summary>
  public partial class UserMachinePage3 : UserControl, IPartPage3
  {
    #region Members
    IDictionary<IMachine, IMachineStateTemplate> m_msts = new Dictionary<IMachine, IMachineStateTemplate>();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Name of the category of element, plural with uppercase for the first letter
    /// Ex: "Machines"
    /// </summary>
    public string ElementName { get { return "Users"; } }
    
    /// <summary>
    /// Description of the page
    /// </summary>
    public string Description { get {
        return "After having selected the period and the user(s) in the previous pages, " +
          "you can add here a new slot of machine attendances.\n\n" +
          "You can select one or several machines, and for each machine a machine state template " +
          "which relates to the kind of job the user(s) will do.";
      }
    }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public IList<Type> EditableTypes { get { return null; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UserMachinePage3() : base()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    public void Initialize() {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_msts.Clear();
      IList<IMachine> machines = data.Get<IList<IMachine>>(UserMachineItem.MACHINES);
      IList<IMachineStateTemplate> msts = data.Get<IList<IMachineStateTemplate>>(UserMachineItem.MACHINE_STATE_TEMPLATES);
      for (int i = 0; i < machines.Count; i++) {
        m_msts[machines[i]] = msts[i];
      }

      Display (null);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      IList<IMachine> machines = new List<IMachine>();
      IList<IMachineStateTemplate> msts = new List<IMachineStateTemplate>();
      foreach (var element in m_msts) {
        machines.Add(element.Key);
        msts.Add(element.Value);
      }
      
      data.Store(UserMachineItem.MACHINES, machines);
      data.Store(UserMachineItem.MACHINE_STATE_TEMPLATES, msts);
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      IList<IMachine> machines = data.Get<IList<IMachine>>(UserMachineItem.MACHINES);
      if (machines.Count == 0) {
        errors.Add("you must add at least one machine");
      }
      else {
        IList<IMachine> machines2 = new List<IMachine>();
        foreach (IMachine machine in machines) {
          if (machines2.Contains(machine)) {
            errors.Add("a machine cannot be added several times");
            break;
          } else {
            machines2.Add(machine);
          }
        }
      }
      
      return errors;
    }
    
    /// <summary>
    /// Prepare a modification to save
    /// This method is within a transaction
    /// </summary>
    /// <param name="data"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public IModification PrepareModification(ItemData data, object item)
    {
      // Creation of the association with its attributes
      IUser user = item as IUser;
      ModelDAOHelper.DAOFactory.UserDAO.Lock(user);
      IList<IMachine> machines = data.Get<IList<IMachine>>(UserMachineItem.MACHINES);
      IList<IMachineStateTemplate> msts = data.Get<IList<IMachineStateTemplate>>(UserMachineItem.MACHINE_STATE_TEMPLATES);
      
      IUserMachineAssociation assoc =
        ModelDAOHelper.ModelFactory.CreateUserMachineAssociation(
          user, new UtcDateTimeRange(data.Get<DateTime>(AbstractItem.PERIOD_START).ToUniversalTime(),
                                         data.Get<DateTime>(AbstractItem.PERIOD_END).ToUniversalTime()));
      for (int i = 0; i < machines.Count; i++) {
        assoc.Add(machines[i], msts[i]);
      }

      ModelDAOHelper.DAOFactory.UserMachineAssociationDAO.MakePersistent(assoc);
      return assoc;
    }
    
    /// <summary>
    /// Get the name of the elements that are going to be modified
    /// </summary>
    /// <param name="items"></param>
    /// <returns>list of name, may be null</returns>
    public IList<string> GetElementName(IList<object> items)
    {
      IList<string> names = new List<string>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (object item in items) {
            IUser user = item as IUser;
            ModelDAOHelper.DAOFactory.UserDAO.Lock(user);
            names.Add(user.Display);
          }
        }
      }
      return names;
    }
    #endregion // Methods
    
    #region Private methods
    void Display(IMachine machine)
    {
      listMachines.ClearItems();
      foreach (var element in m_msts) {
        listMachines.AddItem(String.Format("{0} \u2192 {1}", element.Key.Display, element.Value.Display),
                             element.Key);
      }

      listMachines.SelectedValue = machine;
    }
    #endregion // Private methods
    
    #region Event reactions
    void ListMachinesItemChanged(string arg1, object arg2)
    {
      buttonRemove.Enabled = (listMachines.SelectedValue != null);
    }
    
    void ButtonRemoveClick(object sender, EventArgs e)
    {
      int index = listMachines.SelectedIndex;
      if (index >= 0) {
        m_msts.Remove(listMachines.SelectedValue as IMachine);
        Display(null);
        
        if (index >= listMachines.Count) {
          index--;
        }

        listMachines.SelectedIndex = index;
      }
      
      if (listMachines.SelectedIndex == -1) {
        buttonRemove.Enabled = false;
      }
    }
    
    void ButtonAddClick(object sender, EventArgs e)
    {
      var dialog = new DialogMachineStateTemplate();
      if (dialog.ShowDialog() == DialogResult.OK) {
        m_msts[dialog.Machine] = dialog.MachineStateTemplate;
        Display(dialog.Machine);
      }
    }
    #endregion // Event reactions
  }
}
