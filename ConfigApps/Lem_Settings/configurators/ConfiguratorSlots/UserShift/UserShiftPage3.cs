// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.UserShift
{
  /// <summary>
  /// Description of UserShiftPage3.
  /// </summary>
  public partial class UserShiftPage3 : UserControl, IPartPage3
  {
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
        return "After having selected the period in the previous page, " +
          "you can add here a new slot of a shift.\n\n" +
          "Select the shift you want to apply and then validate.";
      }
    }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public IList<Type> EditableTypes { get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IShift));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UserShiftPage3() : base()
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
          // Shift list
          listShifts.ClearItems();
          IList<IShift> shifts = ModelDAOHelper.DAOFactory.ShiftDAO.FindAll();
          foreach (IShift shift in shifts) {
            listShifts.AddItem(shift.Display, shift);
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
      if (listShifts.ContainsObject(data.Get<IShift>(UserShiftItem.SHIFT))) {
        listShifts.SelectedValue = data.Get<IShift>(UserShiftItem.SHIFT);
      }
      else {
        listShifts.SelectedIndex = 0;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(UserShiftItem.SHIFT, listShifts.SelectedValue);
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
      IUser user = item as IUser;
      ModelDAOHelper.DAOFactory.UserDAO.Lock(user);
      IShift shift = data.Get<IShift>(UserShiftItem.SHIFT);
      ModelDAOHelper.DAOFactory.ShiftDAO.Lock(shift);
      IUserShiftAssociation assoc =
        ModelDAOHelper.ModelFactory.CreateUserShiftAssociation(
          user, new UtcDateTimeRange(data.Get<DateTime>(AbstractItem.PERIOD_START).ToUniversalTime(),
                                     data.Get<DateTime>(AbstractItem.PERIOD_END).ToUniversalTime()),
          shift);
      ModelDAOHelper.DAOFactory.UserShiftAssociationDAO.MakePersistent(assoc);
      
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
  }
}
