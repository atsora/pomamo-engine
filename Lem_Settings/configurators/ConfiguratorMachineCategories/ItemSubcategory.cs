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
  /// Description of ItemSubcategory.
  /// </summary>
  internal class ItemSubcategory : ItemCommon, IConfigurator
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ItemSubcategory() : base("Subcategories", typeof(IMachineSubCategory)) {}
    #endregion // Constructors

    #region Configurator methods
    protected override IList<Container.Element> GetElements()
    {
      IList<Container.Element> listRet = new List<Container.Element>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachineSubCategory> elements = ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.FindAll();
          foreach (IMachineSubCategory element in elements) {
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
          var subCategory = element.Item as IMachineSubCategory;
          if (subCategory != null) {
            ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.Lock(subCategory);
            foreach (IMachine machine in subCategory.Machines) {
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
            var item = element.Item as IMachineSubCategory;
            if (item == null) {
              item = ModelDAOHelper.ModelFactory.CreateMachineSubCategory();
            }
            else {
              ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.Lock(item);
            }

            item.Name = element.Name;
            item.Code = element.Code;
            item.DisplayPriority = displayPriority++;
            ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.MakePersistent (item);
          }
          
          // Remove the other elements
          foreach (Container.Element element in container.DeletedElements) {
            var item = element.Item as IMachineSubCategory;
            ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.Lock(item);
            ModelDAOHelper.DAOFactory.MachineSubCategoryDAO.MakeTransient(item);
          }
          
          transaction.Commit();
        }
      }
    }
    #endregion // Configurator methods
  }
}
