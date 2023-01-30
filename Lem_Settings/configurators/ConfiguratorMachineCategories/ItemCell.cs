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
  /// Description of ItemCell.
  /// </summary>
  internal class ItemCell : ItemCommon, IConfigurator
  {
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public ItemCell() : base("Cells", typeof(ICell)) {}
    #endregion // Constructors

    #region Configurator methods
    protected override IList<Container.Element> GetElements()
    {
      IList<Container.Element> listRet = new List<Container.Element>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var elements = ModelDAOHelper.DAOFactory.CellDAO.FindAll();
          foreach (var element in elements) {
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
          var cell = element.Item as ICell;
          if (cell != null) {
            ModelDAOHelper.DAOFactory.CellDAO.Lock(cell);
            foreach (IMachine machine in cell.Machines) {
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
            var item = element.Item as ICell;
            if (item == null) {
              item = ModelDAOHelper.ModelFactory.CreateCell();
            }
            else {
              ModelDAOHelper.DAOFactory.CellDAO.Lock(item);
            }

            item.Name = element.Name;
            item.Code = element.Code;
            item.DisplayPriority = displayPriority++;
            ModelDAOHelper.DAOFactory.CellDAO.MakePersistent (item);
          }
          
          // Remove the other elements
          foreach (Container.Element element in container.DeletedElements) {
            var item = element.Item as ICell;
            ModelDAOHelper.DAOFactory.CellDAO.Lock(item);
            ModelDAOHelper.DAOFactory.CellDAO.MakeTransient(item);
          }
          
          transaction.Commit();
        }
      }
    }
    #endregion // Configurator methods
  }
}
