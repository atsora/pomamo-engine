// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Web.Operation;

namespace Pulse.Web.UnitTests.Operation
{
  /// <summary>
  /// Unit tests of the OperationSlots service
  /// </summary>
  public class OperationSlotsService_UnitTest
  {
    static readonly string RANGE = "[2001-01-01T01:30:00Z,2001-01-01T02:30:00Z)";

    static OperationSlotsResponseDTO Get (int machineId, bool skipDetails, bool workInformations)
    {
      var response = new OperationSlotsService ().GetWithoutCache (new OperationSlotsRequestDTO {
        MachineId = machineId,
        Range = RANGE,
        SkipDetails = skipDetails,
        WorkInformations = workInformations,
      });
      Assert.That (response, Is.InstanceOf<OperationSlotsResponseDTO> ());
      return (OperationSlotsResponseDTO)response;
    }

    /// <summary>
    /// Work informations of the details, only with WorkInformations=true
    /// </summary>
    [Test]
    public void TestWorkInformations ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var daoFactory = ModelDAOHelper.DAOFactory;
          var machine = daoFactory.MachineDAO.FindById (1);
          var operation = daoFactory.OperationDAO.FindById (1);
          var component = daoFactory.ComponentDAO.FindById (1);
          var workOrder = daoFactory.WorkOrderDAO.FindById (1);
          var slot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine, operation, component, workOrder,
            null, null, null, null,
            new UtcDateTimeRange (new DateTime (2001, 01, 01, 01, 00, 00, DateTimeKind.Utc),
                                  new DateTime (2001, 01, 01, 03, 00, 00, DateTimeKind.Utc)));
          daoFactory.OperationSlotDAO.MakePersistent (slot);
          daoFactory.Flush ();

          var withWorkInformations = Get (machine.Id, false, true);
          var withoutWorkInformations = Get (machine.Id, false, false);
          var skipDetails = Get (machine.Id, true, true);

          Assert.That (withWorkInformations.Blocks, Has.Count.EqualTo (1));
          var detail = withWorkInformations.Blocks[0].Details.Single ();
          Assert.That (detail.WorkInformations, Is.Not.Null);
          var operationItem = detail.WorkInformations.Single (x => x.Kind == WorkInformationKind.Operation);
          var componentItem = detail.WorkInformations
            .Single (x => x.Kind == WorkInformationKind.Component || x.Kind == WorkInformationKind.Part);
          var workOrderItem = detail.WorkInformations
            .Single (x => x.Kind == WorkInformationKind.WorkOrder || x.Kind == WorkInformationKind.Job);
          Assert.Multiple (() => {
            Assert.That (operationItem.Id, Is.EqualTo (operation.Id));
            Assert.That (operationItem.Value, Is.EqualTo (operation.Display));
            // A part shares the id of its component
            Assert.That (componentItem.Id, Is.EqualTo (component.Id));
            Assert.That (workOrderItem.Id, Is.Not.Null);
            if (workOrderItem.Kind == WorkInformationKind.WorkOrder) {
              Assert.That (workOrderItem.Id, Is.EqualTo (workOrder.Id));
            }
            Assert.That (withoutWorkInformations.Blocks[0].Details.Single ().WorkInformations, Is.Null);
            Assert.That (skipDetails.Blocks[0].Details, Is.Null);
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
