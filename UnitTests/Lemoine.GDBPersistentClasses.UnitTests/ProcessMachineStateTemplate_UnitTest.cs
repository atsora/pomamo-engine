// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class OperationMachineAssociation
  /// </summary>
  [TestFixture]
  public class ProcessMachineStateTemplate_UnitTest: WithHourTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ProcessMachineStateTemplate_UnitTest).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public ProcessMachineStateTemplate_UnitTest ()
      : base (UtcDateTime.From (2017, 01, 17, 23, 00, 00)) // local: 2017-01-18 0:00:00
    {
    }
    
    /// <summary>
    /// With operation slot
    /// </summary>
    [Test]
    public void TestWithOperationSlot_Bug218 ()
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        try {
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById(4);
          Assert.That (machine, Is.Not.Null);
          IWorkOrder workOrder1 = ModelDAOHelper.DAOFactory.WorkOrderDAO.FindById(1);
          Assert.That (workOrder1, Is.Not.Null);
          IComponent component1 = ModelDAOHelper.DAOFactory.ComponentDAO.FindById(1);
          Assert.That (component1, Is.Not.Null);
          IOperation operation1 = ModelDAOHelper.DAOFactory.OperationDAO.FindById(13157);
          Assert.That (operation1, Is.Not.Null);
          var machineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (101); // Shift1-2
          var production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById (9);
          var shift1 = ModelDAOHelper.DAOFactory.ShiftDAO
            .FindById (1);
          
          Lemoine.Info.ConfigSet.ForceValue ("Analysis.OperationSlotSplitOption",
                                             (int)(OperationSlotSplitOption.ByDay | OperationSlotSplitOption.ByMachineShift));
          
          {
            var operationSlotSplit = ModelDAOHelper.ModelFactory.CreateOperationSlotSplit (machine);
            operationSlotSplit.End = DateTime.UtcNow;
            ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
          }
          
          { // 1.a) Apply Machine state template between T-2 and T+6
            var association = ModelDAOHelper.ModelFactory.CreateMachineStateTemplateAssociation (machine,
                                                                                                 machineStateTemplate,
                                                                                                 R(-2,+6));
            association.Force = true;
            association.Option = AssociationOption.Synchronous; // To trigger some ProcessMachineStateTemplate
            ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
            AnalysisUnitTests.RunMakeAnalysis ();
            AnalysisAccumulator.Store ("UnitTest");
          }
          { // 1.b) Check the data
            var slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindOverlapsRange (machine, R(-2, +6));
            Assert.That (slots, Has.Count.EqualTo (3));
            int i = 0;
            {
              var slot = slots[i];
              Assert.Multiple (() => {
                Assert.That (slot.MachineStateTemplate, Is.EqualTo (machineStateTemplate));
                Assert.That (slot.MachineObservationState, Is.EqualTo (production));
                Assert.That (slot.Shift, Is.EqualTo (shift1));
                Assert.That (slot.DateTimeRange, Is.EqualTo (R (-2, +1)));
              });
            }
          }
          
          { // 2.a) Add the operation between T-2 and T+6
            var association = ModelDAOHelper.ModelFactory.CreateOperationMachineAssociation (machine,
                                                                                             R(-2, +6));
            association.Operation = operation1;
            ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
            AnalysisUnitTests.RunMakeAnalysis ();
            AnalysisAccumulator.Store ("UnitTest");
          }
          { // 2.b) Check there is a unique operation slot
            var slots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R(-2, +6));
            Assert.That (slots, Has.Count.EqualTo (1));
            int i = 0;
            {
              var slot = slots [i];
              Assert.That (slot.Shift, Is.EqualTo (shift1));
            }
          }
          
          { // 3.a) Apply Machine state template again
            var association = ModelDAOHelper.ModelFactory.CreateMachineStateTemplateAssociation (machine,
                                                                                                 machineStateTemplate,
                                                                                                 R(-2,+6));
            association.Force = true;
            association.Option = AssociationOption.Synchronous; // To trigger some ProcessMachineStateTemplate
            ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
            AnalysisUnitTests.RunMakeAnalysis ();
            AnalysisAccumulator.Store ("UnitTest");
            AnalysisUnitTests.RunMakeAnalysis ();
          }
          { // 3.b) Check the observation state slots
            var slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindOverlapsRange (machine, R(-2, +6));
            Assert.That (slots, Has.Count.EqualTo (3));
            int i = 0;
            {
              var slot = slots[i];
              Assert.Multiple (() => {
                Assert.That (slot.MachineStateTemplate, Is.EqualTo (machineStateTemplate));
                Assert.That (slot.MachineObservationState, Is.EqualTo (production));
                Assert.That (slot.Shift, Is.EqualTo (shift1));
                Assert.That (slot.DateTimeRange, Is.EqualTo (R (-2, +1)));
              });
            }
          }
          { // 3.c) Check again there is a unique operation slot
            var slots = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindOverlapsRange (machine, R(-2, +6));
            Assert.That (slots, Has.Count.EqualTo (1));
            int i = 0;
            {
              var slot = slots [i];
              Assert.That (slot.Shift, Is.EqualTo (shift1));
            }
          }
        }
        finally {
          // Reset the config
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    #region XML Serialization
    /// <summary>
    /// Test the XML serialization/deserialization of the persistent class
    /// ProcessMachineStateTemplate
    /// </summary>
    [Test]
    public void TestSerializationDeserialization()
    {
      {
        var machine = new Machine ();
        machine.Name = "MACHINE";
        var range = new UtcDateTimeRange (T(1), T(2));
        var processMachineStateTemplate = new ProcessMachineStateTemplate (machine, range);
        
        XmlSerializer serializer = new XmlSerializer (typeof (ProcessMachineStateTemplate));
        TextWriter stringWriter = new StringWriter ();
        stringWriter.NewLine = "\n";
        serializer.Serialize (stringWriter, processMachineStateTemplate);
        string result = stringWriter.ToString();
        
        XmlSerializer deserializer = new XmlSerializer (typeof (ProcessMachineStateTemplate));
        TextReader textReader = new StringReader(stringWriter.ToString());
        ProcessMachineStateTemplate processMachineStateTemplateBis = (ProcessMachineStateTemplate) deserializer.Deserialize (textReader);

        Assert.Multiple (() => {
          Assert.That (processMachineStateTemplateBis.Machine.Name, Is.EqualTo (processMachineStateTemplate.Machine.Name));
          Assert.That (processMachineStateTemplateBis.Range, Is.EqualTo (processMachineStateTemplate.Range));
        });
      }

      {
        var machine = new Machine ();
        machine.Name = "MACHINE";
        var range = new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null));
        var processMachineStateTemplate = new ProcessMachineStateTemplate (machine, range);
        
        XmlSerializer serializer = new XmlSerializer (typeof (ProcessMachineStateTemplate));
        TextWriter stringWriter = new StringWriter ();
        stringWriter.NewLine = "\n";
        serializer.Serialize (stringWriter, processMachineStateTemplate);
        string result = stringWriter.ToString();
        
        XmlSerializer deserializer = new XmlSerializer (typeof (ProcessMachineStateTemplate));
        TextReader textReader = new StringReader(stringWriter.ToString());
        ProcessMachineStateTemplate processMachineStateTemplateBis = (ProcessMachineStateTemplate) deserializer.Deserialize (textReader);

        Assert.Multiple (() => {
          Assert.That (processMachineStateTemplateBis.Machine.Name, Is.EqualTo (processMachineStateTemplate.Machine.Name));
          Assert.That (processMachineStateTemplateBis.Range, Is.EqualTo (processMachineStateTemplate.Range));
        });
      }


      {
        var machine = new Machine ();
        machine.Name = "MACHINE";
        var range = new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null));
        var processMachineStateTemplate = new ProcessMachineStateTemplate (machine, range);
        
        XmlSerializer serializer = new XmlSerializer (typeof (MachineModification));
        TextWriter stringWriter = new StringWriter ();
        stringWriter.NewLine = "\n";
        serializer.Serialize (stringWriter, processMachineStateTemplate);
        string result = stringWriter.ToString();
        
        XmlSerializer deserializer = new XmlSerializer (typeof (MachineModification));
        TextReader textReader = new StringReader(stringWriter.ToString());
        ProcessMachineStateTemplate processMachineStateTemplateBis = (ProcessMachineStateTemplate) deserializer.Deserialize (textReader);

        Assert.Multiple (() => {
          Assert.That (processMachineStateTemplateBis.Machine.Name, Is.EqualTo (processMachineStateTemplate.Machine.Name));
          Assert.That (processMachineStateTemplateBis.Range, Is.EqualTo (processMachineStateTemplate.Range));
        });
      }
    }
    #endregion // XML Serialization
  }
}
