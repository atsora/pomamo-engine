// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using Lemoine.Settings;
using Lemoine.BaseControls.List;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorMachineCategories
{
  /// <summary>
  /// Description of ItemDepartment.
  /// </summary>
  internal class ItemDepartment : ItemCommon, IConfigurator
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ItemDepartment() : base("Departments", typeof(IDepartment)) {}
    #endregion // Constructors

    #region Configurator methods
    protected override IList<Container.Element> GetElements()
    {
      IList<Container.Element> listRet = new List<Container.Element>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IDepartment> elements = ModelDAOHelper.DAOFactory.DepartmentDAO.FindAll();
          foreach (IDepartment element in elements) {
            int displayPriority = -1;
            if (element.DisplayPriority.HasValue) {
              displayPriority = element.DisplayPriority.Value;
            }

            listRet.Add(new Container.Element(element, element.Name, element.Code, displayPriority));
          }
        }
      }
      
      return listRet;
    }
    
    protected override void FillMachines(ListTextValue list, Container.Element element)
    {
      list.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var department = element.Item as IDepartment;
          if (department != null) {
            ModelDAOHelper.DAOFactory.DepartmentDAO.Lock(department);
            foreach (IMachine machine in department.Machines) {
              if (machine.IsObsolete()) {
                list.AddItem(machine.Display + " (obsolete)", null, machine.DisplayPriority,
                             SystemColors.ControlDarkDark, true, false);
              }
              else {
                list.AddItem(machine.Display, null, machine.DisplayPriority);
              }
            }
          }
        }
      }
    }
    
    protected override void Validate(Container container)
    {
      int displayPriority = 1;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          // Add and edit existing elements
          foreach (Container.Element element in container.Elements) {
            var item = element.Item as IDepartment;
            if (item == null) {
              item = ModelDAOHelper.ModelFactory.CreateDepartment();
            }
            else {
              ModelDAOHelper.DAOFactory.DepartmentDAO.Lock(item);
            }

            item.Name = element.Name;
            item.Code = element.Code;
            item.DisplayPriority = displayPriority++;
            ModelDAOHelper.DAOFactory.DepartmentDAO.MakePersistent (item);
          }
          
          // Remove the other elements
          foreach (Container.Element element in container.DeletedElements) {
            var item = element.Item as IDepartment;
            ModelDAOHelper.DAOFactory.DepartmentDAO.Lock(item);
            ModelDAOHelper.DAOFactory.DepartmentDAO.MakeTransient(item);
          }
          
          transaction.Commit();
        }
      }
    }
    #endregion // Configurator methods
  }
}
