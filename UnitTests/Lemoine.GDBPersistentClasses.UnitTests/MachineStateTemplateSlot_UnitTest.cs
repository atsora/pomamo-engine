// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class MachineStateTemplateSlot.
  /// </summary>
  [TestFixture]
  public class MachineStateTemplateSlot_UnitTest
    : Lemoine.UnitTests.WithDayTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateSlot_UnitTest).FullName);

    public MachineStateTemplateSlot_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }
    
    /// <summary>
    /// Test the DAO methods
    /// </summary>
    [Test]
    public void TestDAO()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        // Reference data
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (3);
        IMachineStateTemplate unattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (2);
        IMachineStateTemplate production = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (9);
        IMachineStateTemplate setup = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (7);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO
          .FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO
          .FindById (2);
                
        // Existing machine state templates
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, setup, T(0));
          association.Shift = shift1;
          association.Apply ();
        }
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, setup, T(1));
          association.Shift = shift2;
          association.Apply ();
        }
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, production, T(2));
          association.Shift = shift1;
          association.Apply ();
        }
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, production, T(3));
          association.Shift = shift2;
          association.Apply ();
        }
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, production, T(4));
          association.Shift = shift1;
          association.Apply ();
        }
        ModelDAOHelper.DAOFactory.Flush ();
        // 
        // (,0): unattended
        // [0,2): setup
        // [2,): production
        
        {
          IList<IMachineStateTemplateSlot> slots = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
            .FindAll (machine);
          Assert.AreEqual (3, slots.Count);
          Assert.AreEqual (T(0), slots[0].DateTimeRange.Upper.Value);
          Assert.AreEqual (unattended, slots [0].MachineStateTemplate);
          Assert.AreEqual (R(0, 2), slots[1].DateTimeRange);
          Assert.AreEqual (setup, slots[1].MachineStateTemplate);
          Assert.AreEqual (R(2, null), slots[2].DateTimeRange);
          Assert.AreEqual (production, slots[2].MachineStateTemplate);
        }
        {
          UtcDateTimeRange range = R(1,3);
          IList<IMachineStateTemplateSlot> slots = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
            .FindOverlapsRange (machine, R(1, 3));
          Assert.AreEqual (2, slots.Count);
          Assert.AreEqual (R(0, 2), slots[0].DateTimeRange);
          Assert.AreEqual (setup, slots[0].MachineStateTemplate);
          Assert.AreEqual (R(2, null), slots[1].DateTimeRange);
          Assert.AreEqual (production, slots[1].MachineStateTemplate);
        }
        {
          IMachineStateTemplateSlot slot = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
            .FindAt (machine, T(3));
          Assert.AreEqual (R(2, null), slot.DateTimeRange);
          Assert.AreEqual (production, slot.MachineStateTemplate);
        }
        
        transaction.Rollback ();
      }
    }
  }
}
