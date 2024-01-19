// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Plugin.SetupSwitcher;
using NUnit.Framework;
using Lemoine.Core.Log;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugins.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class SetupSwitcher_UnitTest
    : Lemoine.UnitTests.WithDayTimeStamp
  {
    readonly ILog log = LogManager.GetLogger(typeof (SetupSwitcher_UnitTest).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public SetupSwitcher_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }
    
    /// <summary>
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestOperationDetectionExtension ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        // Reference data
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (2);
        IOperation oldOperation = ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (1);
        IOperation newOperation = ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (2);
        IMachineStateTemplate production = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (9);
        IMachineStateTemplate setup = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (7);
        
        // Existing operation slots and machine state templates
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateOperationMachineAssociation (machine, R(0, null));
          association.Operation = oldOperation;
          association.Apply ();
        }
        {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine, production, T(0));
          association.Apply ();
        }
        ModelDAOHelper.DAOFactory.Flush ();
        
        {
          IOperationSlot previousOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRange (machine, R(0, null))
            .First ();
          
          OperationDetectionExtension extension = new OperationDetectionExtension ();
          extension.SetTestConfiguration (@"<?xml version=""1.0"" encoding=""UTF-8""?>
<properties>
  <property>
    <key>SetupMachineStateTemplateId</key>
    <value>7</value>
  </property>
</properties>");
          extension.AddOperation (machine, oldOperation, R(1, null), T(0), previousOperationSlot);
          extension.AddOperation (machine, newOperation, R(2, null), T(2), previousOperationSlot);
        }
        
        { // Check the new machine state template
          IMachineStateTemplateAssociation newAssociation = ModelDAOHelper.DAOFactory
            .MachineStateTemplateAssociationDAO.FindAll ().First ();
          Assert.Multiple (() => {
            Assert.That (newAssociation.MachineStateTemplate, Is.EqualTo (setup));
            Assert.That (newAssociation.Begin.Value, Is.EqualTo (T (2)));
          });
        }
        
        transaction.Rollback ();
      }
    }
  }
}
