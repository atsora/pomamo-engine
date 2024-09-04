// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Linq;

namespace WizardCreateMachine
{
  /// <summary>
  /// Base class for Item and Item2
  /// </summary>
  internal abstract class AbstractItem : GenericItem
  {
    internal const string MACHINE_NAME = "machine_name";
    internal const string MACHINE_CODE = "machine_code";
    internal const string COMPANY = "company";
    internal const string DEPARTMENT = "department";
    internal const string CATEGORY = "category";
    internal const string SUBCATEGORY = "subcategory";
    internal const string CELL = "cell_id";
    internal const string MACHINE_PRIORITY = "machine_priority";
    internal const string MACHINE = "machine";
    internal const string MODIFICATION = "modification";
    
    #region Getters / Setters
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "machines" };
      }
    }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Machines"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Configuration"; } }

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachineCategory)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachineSubCategory)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(ICompany)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IDepartment)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(ICell)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }
    #endregion // Getters / Setters

    #region Wizard methods
    /// <summary>
    /// All settings are done, changes will take effect
    /// This method is already within a try / catch
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public void Finalize(ItemData data, ref IList<string> warnings, ref IRevision revision)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          IModelFactory factory = ModelDAOHelper.ModelFactory;
          IDAOFactory factoryDAO = ModelDAOHelper.DAOFactory;

          // Current machine
          IMachine currentMachine = null;
          if (data.Get<bool>(AbstractItem.MODIFICATION)) {
            currentMachine = data.Get<IMachine>(AbstractItem.MACHINE);
            ModelDAOHelper.DAOFactory.MachineDAO.Lock(currentMachine);
          } else {
            // New machine, not monitored yet
            currentMachine = ModelDAOHelper.ModelFactory.CreateMachine();
            currentMachine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById(3);
            data.Store(AbstractItem.MACHINE, currentMachine);
          }
          
          if (currentMachine == null) {
            throw new Exception("Current machine cannot be null.");
          }

          // Machine identification
          currentMachine.Name = data.Get<string>(MACHINE_NAME);
          currentMachine.Code = data.Get<string>(MACHINE_CODE);
          
          // Machine attributes
          if (data.Get<ICompany>(COMPANY) != null) {
            ICompany company = data.Get<ICompany>(COMPANY);
            ModelDAOHelper.DAOFactory.CompanyDAO.Lock(company);
            currentMachine.Company = company;
          } else {
            currentMachine.Company = null;
          }

          if (data.Get<IDepartment>(DEPARTMENT) != null) {
            IDepartment department = data.Get<IDepartment>(DEPARTMENT);
            ModelDAOHelper.DAOFactory.DepartmentDAO.Lock(department);
            currentMachine.Department = department;
          } else {
            currentMachine.Department = null;
          }

          if (data.Get<IMachineCategory>(CATEGORY) != null) {
            IMachineCategory category = data.Get<IMachineCategory>(CATEGORY);
            ModelDAOHelper.DAOFactory.MachineCategoryDAO.Lock(category);
            currentMachine.Category = category;
          } else {
            currentMachine.Category = null;
          }

          if (data.Get<IMachineSubCategory>(SUBCATEGORY) != null) {
            IMachineSubCategory subCategory = data.Get<IMachineSubCategory>(SUBCATEGORY);
            ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.Lock(subCategory);
            currentMachine.SubCategory = subCategory;
          } else {
            currentMachine.SubCategory = null;
          }

          if (data.Get<ICell>(CELL) != null) {
            ICell cell = data.Get<ICell>(CELL);
            ModelDAOHelper.DAOFactory.CellDAO.Lock(cell);
            currentMachine.Cell = cell;
          } else {
            currentMachine.Cell = null;
          }

          // Priority
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          if (data.Get<bool>(AbstractItem.MODIFICATION)) {
            machines.Remove(currentMachine);
          }

          var machinePriorities = new Dictionary<double, IList<IMachine>>();
          foreach (var machine in machines) {
            double priority = -1.0;
            if (machine.DisplayPriority.HasValue) {
              priority = (double) machine.DisplayPriority.Value;
            }

            if (!machinePriorities.ContainsKey(priority)) {
              machinePriorities[priority] = new List<IMachine>();
            }

            machinePriorities[priority].Add(machine);
          }
          
          double currentPriority = data.Get<double>(MACHINE_PRIORITY);
          if (!machinePriorities.ContainsKey(currentPriority)) {
            machinePriorities[currentPriority] = new List<IMachine>();
          }

          machinePriorities[currentPriority].Add(currentMachine);
          
          var priorities = machinePriorities.Keys.ToList();
          priorities.Sort();
          int increasingPriority = 0;
          foreach (double priority in priorities) {
            foreach (var machine in machinePriorities[priority]) {
              machine.DisplayPriority = increasingPriority;
            }

            increasingPriority++;
          }
          ModelDAOHelper.DAOFactory.MachineDAO.MakePersistent(currentMachine);

          transaction.Commit();
        }
      }
    }
    #endregion // Wizard methods
  }
}
