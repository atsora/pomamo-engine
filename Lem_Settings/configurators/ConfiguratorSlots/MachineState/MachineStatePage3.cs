// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.MachineState
{
  /// <summary>
  /// Description of MachineStatePage3.
  /// </summary>
  internal partial class MachineStatePage3 : UserControl, IPartPage3
  {
    #region Getters / Setters
    /// <summary>
    /// Name of the category of element, plural with uppercase for the first letter
    /// Ex: "Machines"
    /// </summary>
    public string ElementName { get { return "Machines"; } }
    
    /// <summary>
    /// Description of the page
    /// </summary>
    public string Description { get {
        return "After having selected the period in the previous page, " +
          "you can add here a new slot of a machine state template.\n\n" +
          "Select the configurations you want (state template, shift, re-build) " +
          "and then validate.";
      }
    }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public IList<Type> EditableTypes { get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IMachineStateTemplate));
        types.Add(typeof(IShift));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStatePage3() : base()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    public void Initialize()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // State list
          listState.ClearItems();
          IList<IMachineStateTemplate> templates = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.FindAll();
          foreach (IMachineStateTemplate template in templates) {
            listState.AddItem(template.Display, template);
          }

          // Shift list
          comboShift.ClearItems();
          IList<IShift> shifts = ModelDAOHelper.DAOFactory.ShiftDAO.FindAll();
          foreach (IShift shift in shifts) {
            comboShift.AddItem(shift.Display, shift);
          }
        }
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Properties
      if (listState.ContainsObject(data.Get<IMachineStateTemplate>(MachineStateItem.MACHINE_STATE_TEMPLATE))) {
        listState.SelectedValue = data.Get<IMachineStateTemplate>(MachineStateItem.MACHINE_STATE_TEMPLATE);
      }
      else {
        listState.SelectedIndex = 0;
      }

      if (comboShift.ContainsObject(data.Get<IShift>(MachineStateItem.SHIFT))) {
        comboShift.SelectedValue = data.Get<IShift>(MachineStateItem.SHIFT);
      }
      else {
        comboShift.SelectedIndex = 0;
      }

      checkShift.Checked = data.Get<bool>(MachineStateItem.HAS_SHIFT);
      checkForceRebuild.Checked = data.Get<bool>(MachineStateItem.FORCE_REBUILD);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(MachineStateItem.MACHINE_STATE_TEMPLATE, listState.SelectedValue);
      data.Store(MachineStateItem.SHIFT, comboShift.SelectedValue);
      data.Store(MachineStateItem.HAS_SHIFT, checkShift.Checked);
      data.Store(MachineStateItem.FORCE_REBUILD, checkForceRebuild.Checked);
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      return null;
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
      IMachine machine = item as IMachine;
      ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
      IMachineStateTemplate mst = data.Get<IMachineStateTemplate>(MachineStateItem.MACHINE_STATE_TEMPLATE);
      ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.Lock(mst);
      IMachineStateTemplateAssociation assoc =
        ModelDAOHelper.ModelFactory.CreateMachineStateTemplateAssociation(
          machine, mst, data.Get<DateTime>(AbstractItem.PERIOD_START).ToUniversalTime());
      if (data.Get<bool>(AbstractItem.PERIOD_HAS_END)) {
        assoc.End = data.Get<DateTime>(AbstractItem.PERIOD_END).ToUniversalTime();
      }

      if (data.Get<bool>(MachineStateItem.HAS_SHIFT)) {
        IShift shift = data.Get<IShift>(MachineStateItem.SHIFT);
        ModelDAOHelper.DAOFactory.ShiftDAO.Lock(shift);
        assoc.Shift = shift;
      }
      assoc.Force = data.Get<bool>(MachineStateItem.FORCE_REBUILD);
      assoc.Option = AssociationOption.Synchronous;
      ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent(assoc);
      
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
            IMachine machine = item as IMachine;
            ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
            names.Add(machine.Display);
          }
        }
      }
      return names;
    }
    #endregion // Methods
    
    #region Events
    void CheckShiftCheckedChanged(object sender, EventArgs e)
    {
      comboShift.Enabled = checkShift.Checked;
    }
    #endregion // Events
  }
}
