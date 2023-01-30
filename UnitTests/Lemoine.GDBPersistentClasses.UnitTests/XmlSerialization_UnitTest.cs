// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml.Serialization;

using Lemoine.Collections;
using Lemoine.I18N;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using System.Xml;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Test some XML Serialization on some persistent classes
  /// </summary>
  [TestFixture]
  public class XmlSerialization_UnitTest
  {
    string m_previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (XmlSerialization_UnitTest).FullName);

    /// <summary>
    /// Test the XML serialization of a cnc value
    /// </summary>
    [Test]
    public void TestCncValueSerialization ()
    {
      var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
      machine.Name = "Machine";
      var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (machine, "MachineModule");
      var field = ModelDAOHelper.ModelFactory.CreateFieldFromName ("FieldTest", "FieldTest");
      field.Type = FieldType.Int32;
      var cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (machineModule, field, new DateTime (2017, 10, 01, 00, 00, 00, DateTimeKind.Utc));
      cncValue.Value = 3;
      XmlSerializer serializer = new XmlSerializer (typeof (CncValue));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
          stringWriter.NewLine = "\n";
          serializer.Serialize (xmlWriter, cncValue);
          var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
          Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<CncValue xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" String="3" Stopped="false" Begin="2017-10-01 00:00:00" LocalBeginString="01/10/2017 02:00:00" LocalBeginDateTimeG="01/10/2017 02:00:00" End="0001-01-01 00:00:00" LocalEndString="01/01/0001 01:00:00" LocalEndDateTimeG="01/01/0001 01:00:00">
  <MachineModule Name="MachineModule" ConfigPrefix="" Id="0">
    <MonitoredMachine Name="Machine" Id="0" />
  </MachineModule>
  <Field Code="FieldTest" Name="FieldTest" Type="Int32" />
  <Value xsi:type="xsd:int">3</Value>
</CncValue>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<CncValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Begin=""2017-10-01 00:00:00"" LocalBeginDateTimeG=""01/10/2017 02:00:00"" LocalBeginString=""01/10/2017 02:00:00"" End=""0001-01-01 00:00:00"" LocalEndDateTimeG=""01/01/0001 01:00:00"" LocalEndString=""01/01/0001 01:00:00"" String=""3"" Stopped=""false"">
  <MachineModule Name=""MachineModule"" ConfigPrefix="""" Id=""0"">
    <MonitoredMachine Name=""Machine"" Id=""0"" />
  </MachineModule>
  <Field Code=""FieldTest"" Name=""FieldTest"" Type=""Int32"" />
  <Value xsi:type=""xsd:int"">3</Value>
</CncValue>".Length,
                       s.Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of a cnc alarm
    /// </summary>
    [Test]
    public void TestCncAlarmSerialization ()
    {
      // Serializer for a cnc alarm
      var serializer = new XmlSerializer (typeof (CncAlarm));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
          stringWriter.NewLine = "\n";
          string s = "";

          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {

              // Use an existing machine module
              var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindById (1);

              // Create a cnc alarm
              var cncAlarm = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, new UtcDateTimeRange (
                new DateTime (2017, 10, 01, 00, 00, 00, DateTimeKind.Utc), new DateTime (2017, 10, 02, 00, 00, 00, DateTimeKind.Utc)),
                                                                        "CncTest", "sub info", "alarm type", "1000");
              cncAlarm.Message = "a message";
              cncAlarm.Properties["a property"] = "a value";
              cncAlarm.Properties["another property"] = "another value";
              ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (cncAlarm);
              ModelDAOHelper.DAOFactory.Flush ();
              ModelDAOHelper.DAOFactory.CncAlarmDAO.Reload (cncAlarm);

              // Check that the severity is not null (also load the severity object)
              cncAlarm.Unproxy ();
              Assert.IsNotNull (cncAlarm.Severity, "severity should not be null");
              Assert.IsTrue (cncAlarm.Severity.Id != 0, "severity id should have been different from 0");

              // Get the text of the serialized cnc alarm
              serializer.Serialize (xmlWriter, cncAlarm);
              s = stringWriter.ToString ();

              transaction.Rollback ();
            }
          }

          // Compare the text
#if NET6_0_OR_GREATER
          Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<CncAlarm xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Display="a message (small problem)" Message="a message" CncInfo="CncTest" CncSubInfo="sub info" Type="alarm type" Number="1000" Properties="a property=a value;another property=another value">
  <DateTimeRange>[2017-10-01 00:00:00,2017-10-02 00:00:00)</DateTimeRange>
  <BeginDateTime>2017-10-01 00:00:00</BeginDateTime>
  <LocalBeginDateTime>2017-10-01 02:00:00</LocalBeginDateTime>
  <LocalBeginDateTimeG>01/10/2017 02:00:00</LocalBeginDateTimeG>
  <EndDateTime>2017-10-02 00:00:00</EndDateTime>
  <LocalEndDateTime>2017-10-02 02:00:00</LocalEndDateTime>
  <LocalEndDateTimeG>02/10/2017 02:00:00</LocalEndDateTimeG>
  <Duration>1.00:00:00</Duration>
  <MachineModule Name="machinemodule-1" ConfigPrefix="" Id="1">
    <CncAcquisition Name="MACHINE_A17" ConfigPrefix="" UseProcess="false" StaThread="false" UseCoreService="false" Id="1" Every="00:00:02" NotRespondingTimeout="00:02:00" SleepBeforeRestart="00:00:10">
      <Computer Name="PC17" Address="PC17" IsLctr="true" IsLpst="true" IsCnc="false" IsWeb="true" IsAutoReason="false" IsAlert="true" IsSynchronization="true" Id="1" />
    </CncAcquisition>
    <MonitoredMachine Name="MACHINE_A17" Id="1">
      <MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" />
    </MonitoredMachine>
  </MachineModule>
  <Severity CncInfo="CncTest" Name="small problem" Status="DEFAULT_VALUE" Description="This is a small problem." StopStatus="No" Focus="False" />
</CncAlarm>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<CncAlarm xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Display=""a message (small problem)"" Message=""a message"" CncInfo=""CncTest"" CncSubInfo=""sub info"" Type=""alarm type"" Number=""1000"" Properties=""a property=a value;another property=another value"">
  <DateTimeRange>[2017-10-01 00:00:00,2017-10-02 00:00:00)</DateTimeRange>
  <BeginDateTime>2017-10-01 00:00:00</BeginDateTime>
  <LocalBeginDateTime>2017-10-01 02:00:00</LocalBeginDateTime>
  <LocalBeginDateTimeG>01/10/2017 02:00:00</LocalBeginDateTimeG>
  <EndDateTime>2017-10-02 00:00:00</EndDateTime>
  <LocalEndDateTime>2017-10-02 02:00:00</LocalEndDateTime>
  <LocalEndDateTimeG>02/10/2017 02:00:00</LocalEndDateTimeG>
  <Duration>1.00:00:00</Duration>
  <MachineModule Name=""machinemodule-1"" ConfigPrefix="""" Id=""1"">
    <CncAcquisition Id=""1"" Name=""MACHINE_A17"" ConfigPrefix="""" UseProcess=""false"" UseCoreService=""false"" StaThread=""false"" Every=""00:00:02"" NotRespondingTimeout=""00:02:00"" SleepBeforeRestart=""00:00:10"">
      <Computer Id=""1"" Name=""PC17"" Address=""PC17"" IsSynchronization=""true"" IsAlert=""true"" IsAutoReason=""false"" IsLctr=""true"" IsLpst=""true"" IsCnc=""false"" IsWeb=""true"" />
    </CncAcquisition>
    <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
      <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
    </MonitoredMachine>
  </MachineModule>
  <Severity CncInfo=""CncTest"" Name=""small problem"" Status=""DEFAULT_VALUE"" Description=""This is a small problem."" StopStatus=""No"" Focus=""False"" />
</CncAlarm>".ReplaceLineEndings ().Length,
                      s.ReplaceLineEndings ().Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of pair [cnc value / list of cnc alarms]
    /// </summary>
    [Test]
    public void TestCncAlarmWithCncValueSerialization ()
    {
      // Serializer for a cnc alarm
      var serializer = new XmlSerializer (typeof (Lemoine.Collections.SerializableTuple<CncValue, System.Collections.Generic.List<CncAlarm>>));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = false })) {
          stringWriter.NewLine = "\n";
          string s = "";

          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {

              // Use an existing machine module
              var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindById (1);

              /* ************************ *
               * CREATE A FIRST CNC ALARM * 
               * ************************ */

              var cncAlarm = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, new UtcDateTimeRange (
                new DateTime (2017, 10, 01, 00, 00, 00, DateTimeKind.Utc), new DateTime (2017, 10, 02, 00, 00, 00, DateTimeKind.Utc)),
                                                                        "CncTest", "sub info", "alarm type", "1000") as CncAlarm;
              cncAlarm.Message = "a message";
              cncAlarm.Properties["a property"] = "a value";
              cncAlarm.Properties["another property"] = "another value";
              ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (cncAlarm);
              ModelDAOHelper.DAOFactory.Flush ();
              ModelDAOHelper.DAOFactory.CncAlarmDAO.Reload (cncAlarm);

              // Check that the severity is not null (also load the severity object)
              cncAlarm.Unproxy ();
              Assert.IsNotNull (cncAlarm.Severity, "severity should not be null");
              Assert.IsTrue (cncAlarm.Severity.Id != 0, "severity id should have been different from 0");

              /* ************************* *
               * CREATE A SECOND CNC ALARM * 
               * ************************* */

              var cncAlarm2 = ModelDAOHelper.ModelFactory.CreateCncAlarm (machineModule, new UtcDateTimeRange (
                new DateTime (2017, 10, 03, 00, 00, 00, DateTimeKind.Utc), new DateTime (2017, 10, 04, 00, 00, 00, DateTimeKind.Utc)),
                                                                         "CncTest", "sub info", "alarm type", "1001") as CncAlarm;
              cncAlarm2.Message = "a second message";
              cncAlarm2.Properties["a property"] = "a second value";
              cncAlarm2.Properties["another property"] = "another value";
              ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (cncAlarm2);
              ModelDAOHelper.DAOFactory.Flush ();
              ModelDAOHelper.DAOFactory.CncAlarmDAO.Reload (cncAlarm2);

              // Check that the severity is not null (also load the severity object)
              cncAlarm2.Unproxy ();
              Assert.IsNotNull (cncAlarm2.Severity, "severity should not be null");
              Assert.IsTrue (cncAlarm2.Severity.Id != 0, "severity id should have been different from 0");

              /* ****************** *
               * CREATE A CNC VALUE * 
               * ****************** */
              IField field = ModelDAOHelper.DAOFactory.FieldDAO.FindById (1);
              var cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (
                machineModule, field, new DateTime (2017, 10, 01, 00, 00, 00, DateTimeKind.Utc)) as CncValue;
              cncValue.End = new DateTime (2017, 10, 04, 00, 00, 00, DateTimeKind.Utc);
              cncValue.Value = 123456;
              ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue);
              ModelDAOHelper.DAOFactory.Flush ();
              ModelDAOHelper.DAOFactory.CncValueDAO.Reload (cncValue);

              // Put all data together
              var list = new System.Collections.Generic.List<CncAlarm> ();
              list.Add (cncAlarm);
              list.Add (cncAlarm2);
              var pair = new Lemoine.Collections.SerializableTuple<CncValue, System.Collections.Generic.List<CncAlarm>> (cncValue, list);

              // Get the text of the serialized structure
              serializer.Serialize (xmlWriter, pair);
              s = stringWriter.ToString ();

              transaction.Rollback ();
            }
          }

          // Compare the text
#if NET6_0_OR_GREATER
          Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?><SerializableTupleOfCncValueListOfCncAlarm xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"><Item1 String="123456" Stopped="false" Begin="2017-10-01 00:00:00" LocalBeginString="01/10/2017 02:00:00" LocalBeginDateTimeG="01/10/2017 02:00:00" End="2017-10-04 00:00:00" LocalEndString="04/10/2017 02:00:00" LocalEndDateTimeG="04/10/2017 02:00:00"><MachineModule Name="machinemodule-1" ConfigPrefix="" Id="1"><CncAcquisition Name="MACHINE_A17" ConfigPrefix="" UseProcess="false" StaThread="false" UseCoreService="false" Id="1" Every="00:00:02" NotRespondingTimeout="00:02:00" SleepBeforeRestart="00:00:10"><Computer Name="PC17" Address="PC17" IsLctr="true" IsLpst="true" IsCnc="false" IsWeb="true" IsAutoReason="false" IsAlert="true" IsSynchronization="true" Id="1" /></CncAcquisition><MonitoredMachine Name="MACHINE_A17" Id="1"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine></MachineModule><Field Display="CAD model name" Code="CadName" TranslationKey="FieldCADModelName" Type="String" /><Value xsi:type="xsd:string">123456</Value></Item1><Item2><CncAlarm Display="a message (small problem)" Message="a message" CncInfo="CncTest" CncSubInfo="sub info" Type="alarm type" Number="1000" Properties="a property=a value;another property=another value"><DateTimeRange>[2017-10-01 00:00:00,2017-10-02 00:00:00)</DateTimeRange><BeginDateTime>2017-10-01 00:00:00</BeginDateTime><LocalBeginDateTime>2017-10-01 02:00:00</LocalBeginDateTime><LocalBeginDateTimeG>01/10/2017 02:00:00</LocalBeginDateTimeG><EndDateTime>2017-10-02 00:00:00</EndDateTime><LocalEndDateTime>2017-10-02 02:00:00</LocalEndDateTime><LocalEndDateTimeG>02/10/2017 02:00:00</LocalEndDateTimeG><Duration>1.00:00:00</Duration><MachineModule Name="machinemodule-1" ConfigPrefix="" Id="1"><CncAcquisition Name="MACHINE_A17" ConfigPrefix="" UseProcess="false" StaThread="false" UseCoreService="false" Id="1" Every="00:00:02" NotRespondingTimeout="00:02:00" SleepBeforeRestart="00:00:10"><Computer Name="PC17" Address="PC17" IsLctr="true" IsLpst="true" IsCnc="false" IsWeb="true" IsAutoReason="false" IsAlert="true" IsSynchronization="true" Id="1" /></CncAcquisition><MonitoredMachine Name="MACHINE_A17" Id="1"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine></MachineModule><Severity CncInfo="CncTest" Name="small problem" Status="DEFAULT_VALUE" Description="This is a small problem." StopStatus="No" Focus="False" /></CncAlarm><CncAlarm Display="a second message (small problem)" Message="a second message" CncInfo="CncTest" CncSubInfo="sub info" Type="alarm type" Number="1001" Properties="a property=a second value;another property=another value"><DateTimeRange>[2017-10-03 00:00:00,2017-10-04 00:00:00)</DateTimeRange><BeginDateTime>2017-10-03 00:00:00</BeginDateTime><LocalBeginDateTime>2017-10-03 02:00:00</LocalBeginDateTime><LocalBeginDateTimeG>03/10/2017 02:00:00</LocalBeginDateTimeG><EndDateTime>2017-10-04 00:00:00</EndDateTime><LocalEndDateTime>2017-10-04 02:00:00</LocalEndDateTime><LocalEndDateTimeG>04/10/2017 02:00:00</LocalEndDateTimeG><Duration>1.00:00:00</Duration><MachineModule Name="machinemodule-1" ConfigPrefix="" Id="1"><CncAcquisition Name="MACHINE_A17" ConfigPrefix="" UseProcess="false" StaThread="false" UseCoreService="false" Id="1" Every="00:00:02" NotRespondingTimeout="00:02:00" SleepBeforeRestart="00:00:10"><Computer Name="PC17" Address="PC17" IsLctr="true" IsLpst="true" IsCnc="false" IsWeb="true" IsAutoReason="false" IsAlert="true" IsSynchronization="true" Id="1" /></CncAcquisition><MonitoredMachine Name="MACHINE_A17" Id="1"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine></MachineModule><Severity CncInfo="CncTest" Name="small problem" Status="DEFAULT_VALUE" Description="This is a small problem." StopStatus="No" Focus="False" /></CncAlarm></Item2></SerializableTupleOfCncValueListOfCncAlarm>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<SerializableTupleOfCncValueListOfCncAlarm xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Item1 Begin=""2017-10-01 00:00:00"" LocalBeginDateTimeG=""01/10/2017 02:00:00"" LocalBeginString=""01/10/2017 02:00:00"" End=""2017-10-04 00:00:00"" LocalEndDateTimeG=""04/10/2017 02:00:00"" LocalEndString=""04/10/2017 02:00:00"" String=""123456"" Stopped=""false"">
    <MachineModule Id=""1"" ConfigPrefix="""" Name=""machinemodule-1"">
      <CncAcquisition Id=""1"" Name=""MACHINE_A17"" ConfigPrefix="""" UseProcess=""false"" UseCoreService=""false"" StaThread=""false"" Every=""00:00:02"" NotRespondingTimeout=""00:02:00"" SleepBeforeRestart=""00:00:10"">
        <Computer Id=""1"" Name=""PC17"" Address=""PC17"" IsSynchronization=""true"" IsAlert=""true"" IsAutoReason=""false"" IsLctr=""true"" IsLpst=""true"" IsCnc=""false"" IsWeb=""true"" />
      </CncAcquisition>
      <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
        <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
      </MonitoredMachine>
    </MachineModule>
    <Field Display=""CAD model name"" TranslationKey=""FieldCADModelName"" Code=""CadName"" Type=""String"" />
    <Value xsi:type=""xsd:string"">123456</Value>
  </Item1>
  <Item2>
    <CncAlarm Display=""a message (small problem)"" CncInfo=""CncTest"" CncSubInfo=""sub info"" Type=""alarm type"" Number=""1000"" Message=""a message"" Properties=""a property=a value;another property=another value"">
      <DateTimeRange>[2017-10-01 00:00:00,2017-10-02 00:00:00)</DateTimeRange>
      <BeginDateTime>2017-10-01 00:00:00</BeginDateTime>
      <LocalBeginDateTime>2017-10-01 02:00:00</LocalBeginDateTime>
      <LocalBeginDateTimeG>01/10/2017 02:00:00</LocalBeginDateTimeG>
      <EndDateTime>2017-10-02 00:00:00</EndDateTime>
      <LocalEndDateTime>2017-10-02 02:00:00</LocalEndDateTime>
      <LocalEndDateTimeG>02/10/2017 02:00:00</LocalEndDateTimeG>
      <Duration>1.00:00:00</Duration>
      <MachineModule Id=""1"" ConfigPrefix="""" Name=""machinemodule-1"">
        <CncAcquisition Id=""1"" Name=""MACHINE_A17"" ConfigPrefix="""" UseProcess=""false"" UseCoreService=""false"" StaThread=""false"" Every=""00:00:02"" NotRespondingTimeout=""00:02:00"" SleepBeforeRestart=""00:00:10"">
          <Computer Id=""1"" Name=""PC17"" Address=""PC17"" IsSynchronization=""true"" IsAlert=""true"" IsAutoReason=""false"" IsLctr=""true"" IsLpst=""true"" IsCnc=""false"" IsWeb=""true"" />
        </CncAcquisition>
        <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
          <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
        </MonitoredMachine>
      </MachineModule>
      <Severity CncInfo=""CncTest"" Name=""small problem"" Status=""DEFAULT_VALUE"" Description=""This is a small problem."" StopStatus=""No"" Focus=""False"" />
    </CncAlarm>
    <CncAlarm Display=""a second message (small problem)"" CncInfo=""CncTest"" CncSubInfo=""sub info"" Type=""alarm type"" Number=""1001"" Message=""a second message"" Properties=""a property=a second value;another property=another value"">
      <DateTimeRange>[2017-10-03 00:00:00,2017-10-04 00:00:00)</DateTimeRange>
      <BeginDateTime>2017-10-03 00:00:00</BeginDateTime>
      <LocalBeginDateTime>2017-10-03 02:00:00</LocalBeginDateTime>
      <LocalBeginDateTimeG>03/10/2017 02:00:00</LocalBeginDateTimeG>
      <EndDateTime>2017-10-04 00:00:00</EndDateTime>
      <LocalEndDateTime>2017-10-04 02:00:00</LocalEndDateTime>
      <LocalEndDateTimeG>04/10/2017 02:00:00</LocalEndDateTimeG>
      <Duration>1.00:00:00</Duration>
      <MachineModule Id=""1"" ConfigPrefix="""" Name=""machinemodule-1"">
        <CncAcquisition Id=""1"" Name=""MACHINE_A17"" ConfigPrefix="""" UseProcess=""false"" UseCoreService=""false"" StaThread=""false"" Every=""00:00:02"" NotRespondingTimeout=""00:02:00"" SleepBeforeRestart=""00:00:10"">
          <Computer Id=""1"" Name=""PC17"" Address=""PC17"" IsSynchronization=""true"" IsAlert=""true"" IsAutoReason=""false"" IsLctr=""true"" IsLpst=""true"" IsCnc=""false"" IsWeb=""true"" />
        </CncAcquisition>
        <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
          <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
        </MonitoredMachine>
      </MachineModule>
      <Severity CncInfo=""CncTest"" Name=""small problem"" Status=""DEFAULT_VALUE"" Description=""This is a small problem."" StopStatus=""No"" Focus=""False"" />
    </CncAlarm>
  </Item2>
</SerializableTupleOfCncValueListOfCncAlarm>".Length,
                      s.Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of persistent class Revision
    /// </summary>
    [Test]
    public void TestRevisionSerialization ()
    {
      IMachine machine = ModelDAOHelper.ModelFactory.CreateMachine ();
      Revision revision = new Revision ();
      revision.AddModification (ModelDAOHelper.ModelFactory
                                .CreateWorkOrderMachineAssociation (machine,
                                                                    null,
                                                                    new DateTime (1980, 01, 31)));
      User user = new User ();
      revision.Updater = user;
      user.Login = "LOGIN";
      XmlSerializer serializer = new XmlSerializer (typeof (Revision));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
          stringWriter.NewLine = "\n";
          serializer.Serialize (xmlWriter, revision);
          var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
          Assert.AreEqual ($"""
<?xml version="1.0" encoding="utf-16"?>
<Revision xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" DateTime="{revision.SqlDateTime}">
  <Updater xsi:type="User" Login="LOGIN" />
</Revision>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      Assert.AreEqual (string.Format (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Revision xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""{0}"">
  <Updater xsi:type=""User"" Login=""LOGIN"" />
</Revision>",
                                      revision.SqlDateTime).Length,
                       s.Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML de-serialization to a persistent class Revision
    /// </summary>
    [Test]
    public void TestRevisionDeserialization ()
    {
      TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Revision xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""0001-01-01 00:00:00"">
  <Other Id=""3"" />
</Revision>");
      XmlSerializer deserializer = new XmlSerializer (typeof (Revision));
      Revision revision = (Revision)deserializer.Deserialize (textReader);
    }

    /// <summary>
    /// Test the XML de-serialization to a persistent class Component
    /// </summary>
    [Test]
    public void TestComponentDeserialization ()
    {
      {
        TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Component xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
           xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
           Name=""NEWCOMPONENT"" other=""to ignore""
           pulse:action='reference' pulse:notfound='log create'>
  <Project Name=""NEWPROJECT""
           pulse:action='reference' pulse:notfound='log create' />
  <Type TranslationKey=""UndefinedValue"" pulse:action='reference' pulse:notfound='fail' />
</Component>");
        XmlSerializer deserializer = new XmlSerializer (typeof (Component));
        Component component = (Component)deserializer.Deserialize (textReader);
        Assert.AreEqual (false, component.EstimatedHours.HasValue);
      }

      {
        TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Component xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
           xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
           Name=""NEWCOMPONENT"" other=""to ignore"" EstimatedHours=""2.5""
           pulse:action='reference' pulse:notfound='log create'>
  <Project Name=""NEWPROJECT""
           pulse:action='reference' pulse:notfound='log create' />
  <Type TranslationKey=""UndefinedValue"" pulse:action='reference' pulse:notfound='fail' />
</Component>");
        XmlSerializer deserializer = new XmlSerializer (typeof (Component));
        Component component = (Component)deserializer.Deserialize (textReader);
        Assert.AreEqual (true, component.EstimatedHours.HasValue);
        Assert.AreEqual (2.5, component.EstimatedHours.Value);
      }
    }

    /// <summary>
    /// Test the XML serialization of the persistent class
    /// ComponentIntermediateWorkPiece
    /// </summary>
    [Test]
    public void TestComponentIntermediateWorkPieceSerialization ()
    {
      IComponentType componentType = ModelDAOHelper.ModelFactory.CreateComponentTypeFromName ("Test");
      Component component = (Component)ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
      component.Name = "COMPONENT";
      IntermediateWorkPiece intermediateWorkPiece =
        new IntermediateWorkPiece (null);
      intermediateWorkPiece.Name = "IWP";
      ComponentIntermediateWorkPiece componentIntermediateWorkPiece =
        new ComponentIntermediateWorkPiece (component,
                                            intermediateWorkPiece);
      componentIntermediateWorkPiece.OrderAsString = "50";
      XmlSerializer serializer = new XmlSerializer (typeof (ComponentIntermediateWorkPiece));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
          stringWriter.NewLine = "\n";
          serializer.Serialize (xmlWriter, componentIntermediateWorkPiece);
          string s = stringWriter.ToString ();
          log.Info (stringWriter.ToString ());
#if NET6_0_OR_GREATER
          Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<ComponentIntermediateWorkPiece xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Order="50">
  <Component Name="COMPONENT">
    <Type Name="Test" Display="Test" />
  </Component>
  <IntermediateWorkPiece Name="IWP" OperationQuantity="1" />
</ComponentIntermediateWorkPiece>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentIntermediateWorkPiece xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Order=""50"">
  <Component Name=""COMPONENT"">
    <Type Name=""Test"" Display=""Test"" />
  </Component>
  <IntermediateWorkPiece Name=""IWP"" OperationQuantity=""1"" />
</ComponentIntermediateWorkPiece>".Length,
                       s.Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML de-serialization to a persistent class ComponentIntermediateWorkPiece
    /// </summary>
    [Test]
    public void TestComponentIntermediateWorkPieceDeserialization ()
    {
      TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentIntermediateWorkPiece xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Order=""50"">
  <Component Name=""COMPONENT"" />
  <IntermediateWorkPiece Name=""IWP"" OperationQuantity=""1"" />
</ComponentIntermediateWorkPiece>");
      XmlSerializer deserializer = new XmlSerializer (typeof (ComponentIntermediateWorkPiece));
      ComponentIntermediateWorkPiece componentIntermediateWorkPiece =
        (ComponentIntermediateWorkPiece)deserializer.Deserialize (textReader);
      Assert.AreEqual ("COMPONENT", componentIntermediateWorkPiece.Component.Name);
      Assert.AreEqual ("IWP", componentIntermediateWorkPiece.IntermediateWorkPiece.Name);
    }

    /// <summary>
    /// Test the XML serialization of the persistent class
    /// ComponentMachineAssociation
    /// </summary>
    [Test]
    public void TestComponentMachineAssociationSerialization ()
    {
      IComponentType componentType = ModelDAOHelper.ModelFactory.CreateComponentTypeFromName ("Test");
      IComponent component = ModelDAOHelper.ModelFactory.CreateComponentFromName (null, "COMPONENT", componentType);
      component.EstimatedHours = 2.5;
      IMachine machine = ModelDAOHelper.ModelFactory.CreateMachine ();
      machine.Name = "MACHINE";
      ComponentMachineAssociation componentMachineAssociation = (ComponentMachineAssociation)ModelDAOHelper.ModelFactory
        .CreateComponentMachineAssociation (machine, component, new UtcDateTimeRange (new DateTime ()));
      componentMachineAssociation.Option = AssociationOption.AssociateToSlotOption;
      XmlSerializer serializer = new XmlSerializer (typeof (ComponentMachineAssociation));
      {
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (xmlWriter, componentMachineAssociation);
            var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
            Assert.AreEqual ($"""
<?xml version="1.0" encoding="utf-16"?>
<ComponentMachineAssociation xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Priority="100" DateTime="{componentMachineAssociation.SqlDateTime}" Begin="0001-01-01 00:00:00">
  <Machine Name="MACHINE" Id="0" />
  <Option>AssociateToSlotOption</Option>
  <Component Name="COMPONENT" EstimatedHours="2.5">
    <Type Name="Test" Display="Test" />
  </Component>
</ComponentMachineAssociation>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      string oracle = string.Format (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentMachineAssociation xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""{0}"" Priority=""100"" Begin=""0001-01-01 00:00:00"">
  <Machine Name=""MACHINE"" Id=""0"" />
  <Component Name=""COMPONENT"" EstimatedHours=""2.5"">
    <Type Name=""Test"" Display=""Test"" />
  </Component>
  <Option>AssociateToSlotOption</Option>
</ComponentMachineAssociation>", componentMachineAssociation.SqlDateTime);
      Assert.AreEqual (oracle.Length,
                       s.Length);
#endif
          }
        }
      }

      {
        componentMachineAssociation.Component = null;
        using (var stringWriter2 = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter2, new XmlWriterSettings { Indent = true })) {
            stringWriter2.NewLine = "\n";
            serializer.Serialize (xmlWriter, componentMachineAssociation);
            var s = stringWriter2.ToString ();
#if NET6_0_OR_GREATER
            Assert.AreEqual ($"""
<?xml version="1.0" encoding="utf-16"?>
<ComponentMachineAssociation xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Priority="100" DateTime="{componentMachineAssociation.SqlDateTime}" Begin="0001-01-01 00:00:00">
  <Machine Name="MACHINE" Id="0" />
  <Option>AssociateToSlotOption</Option>
</ComponentMachineAssociation>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
        Assert.AreEqual ((string.Format (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentMachineAssociation xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""{0}"" Priority=""100"" Begin=""0001-01-01 00:00:00"">
  <Machine Name=""MACHINE"" Id=""0"" />
  <Option>AssociateToSlotOption</Option>
</ComponentMachineAssociation>", componentMachineAssociation.SqlDateTime)).Length,
                         s.Length);
#endif
          }
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of the persistent class
    /// UserAttendance
    /// </summary>
    [Test]
    public void TestUserAttendanceSerialization ()
    {
      UserAttendance userAttendance = (UserAttendance)ModelDAOHelper.ModelFactory
        .CreateUserAttendance (new User ());
      userAttendance.LocalBegin = "01/01/1950";
      userAttendance.End = System.DateTime.Parse ("02/01/1950");
      userAttendance.User.Name = "USER";

      XmlSerializer serializer = new XmlSerializer (typeof (UserAttendance));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
          stringWriter.NewLine = "\n";
          serializer.Serialize (xmlWriter, userAttendance);
          string s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
          Assert.AreEqual ($"""
<?xml version="1.0" encoding="utf-16"?>
<UserAttendance xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Priority="100" DateTime="{userAttendance.SqlDateTime}" Begin="{userAttendance.Begin.Value.ToString ("yyyy-MM-dd HH:mm:ss")}" End="{userAttendance.End.Value.ToString ("yyyy-MM-dd HH:mm:ss")}">
  <User Name="USER" />
</UserAttendance>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      string oracle = string.Format (@"<?xml version=""1.0"" encoding=""utf-16""?>
<UserAttendance xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""{0}"" Priority=""100"" Begin=""{1}"" End=""{2}"">
  <User Name=""USER"" />
</UserAttendance>", userAttendance.SqlDateTime,
                                     userAttendance.Begin.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                                     userAttendance.End.Value.ToString("yyyy-MM-dd HH:mm:ss"));
      
      Assert.AreEqual (oracle.Length, s.Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML de-serialization to a persistent class UserAttendance
    /// </summary>
    [Test]
    public void TestUserAttendanceDeserialization ()
    {
      TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<UserAttendance xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""2011-01-01 00:00:01"" Priority=""100"" Begin=""2011-01-01 00:00:02"" End=""2011-01-01 00:00:03"">
  <User Name=""USER"" />
</UserAttendance>");
      XmlSerializer deserializer = new XmlSerializer (typeof (UserAttendance));
      UserAttendance userAttendance =
        (UserAttendance)deserializer.Deserialize (textReader);
      Assert.AreEqual ("USER", userAttendance.User.Name);
      Assert.AreEqual (DateTime.Parse ("2011-01-01 00:00:01"), userAttendance.DateTime);
      Assert.AreEqual (DateTime.Parse ("2011-01-01 00:00:02"), userAttendance.Begin);
      Assert.AreEqual (DateTime.Parse ("2011-01-01 00:00:03"), userAttendance.End);

      TextReader textReader2 = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<UserAttendance xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""2011-01-01 00:00:01"" LocalBegin=""2011-01-01 00:00:02"" LocalEnd=""2011-01-01 00:00:03"">
  <User Name=""USER"" />
</UserAttendance>");
      XmlSerializer deserializer2 = new XmlSerializer (typeof (UserAttendance));
      UserAttendance userAttendance2 =
        (UserAttendance)deserializer2.Deserialize (textReader2);
      Assert.AreEqual ("USER", userAttendance2.User.Name);
      Assert.AreEqual (DateTime.Parse ("2011-01-01 00:00:01"), userAttendance2.DateTime);

      Assert.AreEqual ("2011-01-01 00:00:02", userAttendance2.LocalBegin);
      Assert.AreEqual ("2011-01-01 00:00:03", userAttendance2.LocalEnd);
      Assert.AreEqual (true, userAttendance2.Begin.HasValue);
      Assert.AreEqual (true, userAttendance2.End.HasValue);
      Assert.AreEqual (DateTime.Parse ("2011-01-01 00:00:02").ToUniversalTime (), userAttendance2.Begin.Value);
      Assert.AreEqual (DateTime.Parse ("2011-01-01 00:00:03").ToUniversalTime (), userAttendance2.End.Value);
    }

    /// <summary>
    /// Test the XML serialization of the persistent class
    /// ComponentMachineAssociation
    /// </summary>
    [Test]
    public void TestMachineObservationStateAssociationSerialization ()
    {
      IMachineObservationStateAssociation machineObservationStateAssociation = ModelDAOHelper.ModelFactory
        .CreateMachineObservationStateAssociation (new Machine (),
                                                   ModelDAOHelper.ModelFactory.CreateMachineObservationState (),
                                                   new DateTime ());
      machineObservationStateAssociation.Machine.Name = "MACHINE";
      machineObservationStateAssociation.User = new User ();
      machineObservationStateAssociation.User.Name = "USER";

      XmlSerializer serializer = new XmlSerializer (typeof (MachineObservationStateAssociation));
      {
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (stringWriter, machineObservationStateAssociation);
            string s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
            Assert.AreEqual ($"""
<?xml version="1.0" encoding="utf-16"?>
<MachineObservationStateAssociation xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Priority="100" DateTime="{((MachineObservationStateAssociation)machineObservationStateAssociation).SqlDateTime}" Begin="0001-01-01 00:00:00">
  <Machine Name="MACHINE" Id="0" />
  <Option xsi:nil="true" />
  <MachineObservationState UserRequired="false" ShiftRequired="false" IsProduction="false" />
  <User Name="USER" />
</MachineObservationStateAssociation>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
        string oracle = string.Format (@"<?xml version=""1.0"" encoding=""utf-16""?>
<MachineObservationStateAssociation xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""{0}"" Priority=""100"" Begin=""0001-01-01 00:00:00"">
  <Option xsi:nil=""true"" />
  <Machine Id=""0"" Name=""MACHINE"" />
  <MachineObservationState UserRequired=""false"" ShiftRequired=""false"" IsProduction=""false"" />
  <User Name=""USER"" />
</MachineObservationStateAssociation>", ((MachineObservationStateAssociation)machineObservationStateAssociation).SqlDateTime);

        Assert.AreEqual (oracle.Length, s.Length);
#endif
          }
        }
      }

      machineObservationStateAssociation.MachineObservationState.OnSite = true;
      ((MachineObservationStateAssociation)machineObservationStateAssociation).LocalBegin = "01/01/1950";
      ((MachineObservationStateAssociation)machineObservationStateAssociation).LocalEnd = "01/01/1950";
      {
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (xmlWriter, machineObservationStateAssociation);
            string s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
            Assert.AreEqual ($"""
<?xml version="1.0" encoding="utf-16"?>
<MachineObservationStateAssociation xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Priority="100" DateTime="{((MachineObservationStateAssociation)machineObservationStateAssociation).SqlDateTime}" Begin="{machineObservationStateAssociation.Begin.Value.ToString ("yyyy-MM-dd HH:mm:ss")}" End="{machineObservationStateAssociation.End.Value.ToString ("yyyy-MM-dd HH:mm:ss")}">
  <Machine Name="MACHINE" Id="0" />
  <Option xsi:nil="true" />
  <MachineObservationState UserRequired="false" ShiftRequired="false" IsProduction="false" OnSite="true" />
  <User Name="USER" />
</MachineObservationStateAssociation>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
        string oracle2 = string.Format (@"<?xml version=""1.0"" encoding=""utf-16""?>
<MachineObservationStateAssociation xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""{0}"" Priority=""100"" Begin=""{1}"" End=""{2}"">
  <Option xsi:nil=""true"" />
  <Machine Id=""0"" Name=""MACHINE"" />
  <MachineObservationState UserRequired=""false"" ShiftRequired=""false"" OnSite=""true"" IsProduction=""false"" />
  <User Name=""USER"" />
</MachineObservationStateAssociation>", ((MachineObservationStateAssociation)machineObservationStateAssociation).SqlDateTime,
                                        machineObservationStateAssociation.Begin.Value.ToString ("yyyy-MM-dd HH:mm:ss"),
                                        machineObservationStateAssociation.End.Value.ToString ("yyyy-MM-dd HH:mm:ss"));

        Assert.AreEqual (oracle2.Length, s.Length);
#endif
          }
        }
      }
    }

    /// <summary>
    /// Test the XML de-serialization to a persistent class MachineObservationStateAssociation
    /// </summary>
    [Test]
    public void TestMachineObservationStateAssociationDeserialization ()
    {
      TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<MachineObservationStateAssociation xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Start=""2011-01-01 00:00:00"">
  <Machine Name=""MACHINE"" />
  <MachineObservationState UserRequired=""false"" />
  <User Name=""USER"" />
</MachineObservationStateAssociation>");
      XmlSerializer deserializer = new XmlSerializer (typeof (MachineObservationStateAssociation));
      MachineObservationStateAssociation machineObservationStateAssociation =
        (MachineObservationStateAssociation)deserializer.Deserialize (textReader);
      Assert.AreEqual ("MACHINE", machineObservationStateAssociation.Machine.Name);
      Assert.AreEqual ("USER", machineObservationStateAssociation.User.Name);
      Assert.AreEqual (false, machineObservationStateAssociation.MachineObservationState.UserRequired);
      Assert.AreEqual (null, machineObservationStateAssociation.MachineObservationState.OnSite);

      TextReader textReader2 = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<MachineObservationStateAssociation xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Start=""2011-01-01 00:00:00"">
  <Machine Name=""MACHINE"" />
  <MachineObservationState UserRequired=""false"" ShiftRequired=""false"" OnSite=""true"" />
  <User Name=""USER"" />
</MachineObservationStateAssociation>");
      XmlSerializer deserializer2 = new XmlSerializer (typeof (MachineObservationStateAssociation));
      MachineObservationStateAssociation machineObservationStateAssociation2 =
        (MachineObservationStateAssociation)deserializer.Deserialize (textReader2);
      Assert.AreEqual ("MACHINE", machineObservationStateAssociation2.Machine.Name);
      Assert.AreEqual ("USER", machineObservationStateAssociation2.User.Name);
      Assert.AreEqual (false, machineObservationStateAssociation2.MachineObservationState.UserRequired);
      Assert.AreEqual (true, machineObservationStateAssociation2.MachineObservationState.OnSite);

    }


    /// <summary>
    /// Test the XML de-serialization to a persistent class ComponentMachineAssociation
    /// </summary>
    [Test]
    public void TestComponentMachineAssociationDeserialization ()
    {
      TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentMachineAssociation xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Start=""2011-01-01 00:00:00"">
  <Component Name=""COMPONENT"" EstimatedHours=""2.5"" />
  <Machine Name=""MACHINE"" />
</ComponentMachineAssociation>");
      XmlSerializer deserializer = new XmlSerializer (typeof (ComponentMachineAssociation));
      ComponentMachineAssociation componentMachineAssociation =
        (ComponentMachineAssociation)deserializer.Deserialize (textReader);
      Assert.AreEqual ("COMPONENT", componentMachineAssociation.Component.Name);
      Assert.AreEqual (2.5, componentMachineAssociation.Component.EstimatedHours);
      Assert.AreEqual ("MACHINE", componentMachineAssociation.Machine.Name);

      TextReader textReader2 = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentMachineAssociation xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Component Name=""COMPONENT"" />
  <Machine Name=""MACHINE"" />
</ComponentMachineAssociation>");
      XmlSerializer deserializer2 = new XmlSerializer (typeof (Modification));
      Modification modification =
        (Modification)deserializer.Deserialize (textReader2);
      Assert.IsInstanceOf<ComponentMachineAssociation> (modification);
      ComponentMachineAssociation componentMachineAssociation2 =
        (ComponentMachineAssociation)modification;
      Assert.AreEqual ("COMPONENT", componentMachineAssociation2.Component.Name);
      Assert.AreEqual ("MACHINE", componentMachineAssociation2.Machine.Name);
    }

    /// <summary>
    /// Test the XML serialization of the persistent class
    /// Component
    /// </summary>
    [Test]
    public void TestComponentSerialization ()
    {
      IComponentType componentType = ModelDAOHelper.ModelFactory.CreateComponentTypeFromName ("Test");
      IComponent component = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
      component.Name = "COMPONENT";
      component.EstimatedHours = null;
      XmlSerializer serializer = new XmlSerializer (typeof (Component));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
          stringWriter.NewLine = "\n";
          serializer.Serialize (xmlWriter, component);
          var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
          Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<Component xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Name="COMPONENT">
  <Type Name="Test" Display="Test" />
</Component>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Component xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Name=""COMPONENT"">
  <Type Name=""Test"" Display=""Test"" />
</Component>".Length,
                       s.Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of the persistent class
    /// Job
    /// </summary>
    [Test]
    public void TestJobSerialization ()
    {
      IWorkOrderStatus status = ModelDAOHelper.ModelFactory.CreateWorkOrderStatusFromName ("status");
      JobView job = new Lemoine.GDBPersistentClasses.JobView ();
      job.Status = status;
      job.Name = "JOB";
      XmlSerializer serializer = new XmlSerializer (typeof (JobView));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
          stringWriter.NewLine = "\n";
          serializer.Serialize (xmlWriter, job);
          var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
          Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<JobView xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Name="JOB">
  <Status Name="status" Display="status" />
</JobView>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<JobView xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Name=""JOB"">
  <Status Name=""status"" Display=""status"" />
</JobView>".Length,
                       s.Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML de-serialization to a persistent class Job
    /// </summary>
    [Test]
    public void TestJobDeserialization ()
    {
      {
        TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<JobView xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
         xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
         Name=""JOB""
         pulse:action='reference'>
  <Status Name=""status"" Display=""status"" />
</JobView>");
        XmlSerializer deserializer = new XmlSerializer (typeof (JobView));
        JobView job = (JobView)deserializer.Deserialize (textReader);
        Assert.AreEqual ("JOB", job.Name);
      }
    }

    /// <summary>
    /// Test the XML serialization of the persistent class
    /// ComponentType
    /// </summary>
    [Test]
    public void TestComponentTypeSerialization ()
    {
      IComponentType t = ModelDAOHelper.ModelFactory.CreateComponentTypeFromName ("type name");
      XmlSerializer serializer = new XmlSerializer (typeof (ComponentType));
      using (var stringWriter = new StringWriter ()) {
        using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
          stringWriter.NewLine = "\n";
          serializer.Serialize (xmlWriter, t);
          var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
          Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<ComponentType xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" Name="type name" Display="type name" />
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
      Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentType xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Name=""type name"" Display=""type name"" />".Length,
                       s.Length);
#endif
        }
      }
    }

    /// <summary>
    /// Test the XML de-serialization to a persistent class ComponentType
    /// </summary>
    [Test]
    public void TestComponentTypeDeserialization ()
    {
      {
        TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
<ComponentType Name=""type name"" />");
        XmlSerializer deserializer = new XmlSerializer (typeof (ComponentType));
        IComponentType componentType = (IComponentType)deserializer.Deserialize (textReader);
        Assert.AreEqual ("type name", componentType.Name);
      }
    }

    /// <summary>
    /// Test the XML serialization of the EventLongPeriod class
    /// </summary>
    [Test]
    public void TestEventLongPeriodSerialization ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        IEventLevel eventLevel = ModelDAO.ModelDAOHelper.DAOFactory
          .EventLevelDAO.FindById (1);
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory
          .MonitoredMachineDAO.FindByIdForXmlSerialization (1);
        IMachineMode inactive = ModelDAOHelper.DAOFactory
          .MachineModeDAO.FindById ((int)MachineModeId.Inactive);
        IMachineObservationState attended = ModelDAOHelper.DAOFactory
          .MachineObservationStateDAO.FindById ((int)MachineObservationStateId.Attended);
        IEventLongPeriod longPeriod =
          ModelDAOHelper.ModelFactory.CreateEventLongPeriod (eventLevel,
                                                             new DateTime (2012, 12, 12),
                                                             machine,
                                                             inactive,
                                                             attended,
                                                             TimeSpan.FromMinutes (20));
        {
          XmlSerializer serializer = new XmlSerializer (typeof (Event));
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = false })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (xmlWriter, longPeriod);
              var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?><Event xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:type="EventLongPeriod" DateTime="2012-12-12 00:00:00" LocalDateTimeString="12/12/2012 01:00:00" LocalDateTimeG="12/12/2012 01:00:00" TriggerDuration="00:20:00"><Level Name="" TranslationKey="EventLevelAlert" Display="Alert" Priority="100" /><MonitoredMachine Name="MACHINE_A17" Id="1"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine><MachineMode TranslationKey="MachineModeInactive" Display="Inactive" AutoSequence="false" Running="false" /><MachineObservationState TranslationKey="MachineObservationStateAttended" Display="Machine ON with operator (attended)" UserRequired="true" ShiftRequired="false" IsProduction="true" OnSite="true" /></Event>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
          Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Event xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xsi:type=""EventLongPeriod"" DateTime=""2012-12-12 00:00:00"" LocalDateTimeG=""12/12/2012 01:00:00"" LocalDateTimeString=""12/12/2012 01:00:00"" TriggerDuration=""00:20:00"">
  <Level Name="""" TranslationKey=""EventLevelAlert"" Display=""Alert"" Priority=""100"" />
  <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
    <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
  </MonitoredMachine>
  <MachineMode TranslationKey=""MachineModeInactive"" Display=""Inactive"" Running=""false"" AutoSequence=""false"" />
  <MachineObservationState TranslationKey=""MachineObservationStateAttended"" Display=""Machine ON with operator (attended)"" UserRequired=""true"" ShiftRequired=""false"" OnSite=""true"" IsProduction=""true"" />
</Event>".Length,
                           s.Length);
#endif
            }
          }
        }
        {
          XmlSerializer serializer = new XmlSerializer (typeof (EventLongPeriod));
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (xmlWriter, longPeriod);
              var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<EventLongPeriod xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" DateTime="2012-12-12 00:00:00" LocalDateTimeString="12/12/2012 01:00:00" LocalDateTimeG="12/12/2012 01:00:00" TriggerDuration="00:20:00">
  <Level Name="" TranslationKey="EventLevelAlert" Display="Alert" Priority="100" />
  <MonitoredMachine Name="MACHINE_A17" Id="1">
    <MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" />
  </MonitoredMachine>
  <MachineMode TranslationKey="MachineModeInactive" Display="Inactive" AutoSequence="false" Running="false" />
  <MachineObservationState TranslationKey="MachineObservationStateAttended" Display="Machine ON with operator (attended)" UserRequired="true" ShiftRequired="false" IsProduction="true" OnSite="true" />
</EventLongPeriod>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
          Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<EventLongPeriod xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""2012-12-12 00:00:00"" LocalDateTimeG=""12/12/2012 01:00:00"" LocalDateTimeString=""12/12/2012 01:00:00"" TriggerDuration=""00:20:00"">
  <Level Name="""" TranslationKey=""EventLevelAlert"" Display=""Alert"" Priority=""100"" />
  <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
    <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
  </MonitoredMachine>
  <MachineMode TranslationKey=""MachineModeInactive"" Display=""Inactive"" Running=""false"" AutoSequence=""false"" />
  <MachineObservationState TranslationKey=""MachineObservationStateAttended"" Display=""Machine ON with operator (attended)"" UserRequired=""true"" ShiftRequired=""false"" OnSite=""true"" IsProduction=""true"" />
</EventLongPeriod>".Length, s.Length);
#endif
            }
          }
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of the EventCncValue class
    /// </summary>
    [Test]
    public void TestEventCncValueSerialization ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      IEvent ev;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        ev = ModelDAOHelper.DAOFactory.EventDAO.FindById (910);
      }
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        ev = ModelDAOHelper.DAOFactory.EventDAO.FindById (ev.Id);
        ev.Unproxy ();
      }
      {
        XmlSerializer serializer = new XmlSerializer (typeof (Event));
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = false })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (xmlWriter, ev);
            var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
            Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?><Event xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:type="EventCncValue" DateTime="2012-12-12 00:00:00" LocalDateTimeString="12/12/2012 01:00:00" LocalDateTimeG="12/12/2012 01:00:00" Message="message" Duration="00:20:00"><Level Name="" TranslationKey="EventLevelAlert" Display="Alert" Priority="100" /><MachineModule Name="machinemodule-1" ConfigPrefix="" Id="1"><CncAcquisition Name="MACHINE_A17" ConfigPrefix="" UseProcess="false" StaThread="false" UseCoreService="false" Id="1" Every="00:00:02" NotRespondingTimeout="00:02:00" SleepBeforeRestart="00:00:10"><Computer Name="PC17" Address="PC17" IsLctr="true" IsLpst="true" IsCnc="false" IsWeb="true" IsAutoReason="false" IsAlert="true" IsSynchronization="true" Id="1" /></CncAcquisition><MonitoredMachine Name="MACHINE_A17" Id="1"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine></MachineModule><MonitoredMachine Name="MACHINE_A17" Id="1"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine><Field Display="Feedrate override" Code="FeedrateOverride" TranslationKey="FieldFeedrateOverride" Type="Double" /><Value xsi:type="xsd:double">80</Value></Event>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
        Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Event xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xsi:type=""EventCncValue"" DateTime=""2012-12-12 00:00:00"" LocalDateTimeG=""12/12/2012 01:00:00"" LocalDateTimeString=""12/12/2012 01:00:00"" Message=""message"" Duration=""00:20:00"">
  <Level Name="""" TranslationKey=""EventLevelAlert"" Display=""Alert"" Priority=""100"" />
  <MachineModule Id=""1"" ConfigPrefix="""" Name=""machinemodule-1"">
    <CncAcquisition Id=""1"" Name=""MACHINE_A17"" ConfigPrefix="""" UseProcess=""false"" UseCoreService=""false"" StaThread=""false"" Every=""00:00:02"" NotRespondingTimeout=""00:02:00"" SleepBeforeRestart=""00:00:10"">
      <Computer Id=""1"" Name=""PC17"" Address=""PC17"" IsSynchronization=""true"" IsAlert=""true"" IsAutoReason=""false"" IsLctr=""true"" IsLpst=""true"" IsCnc=""false"" IsWeb=""true"" />
    </CncAcquisition>
    <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
      <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
    </MonitoredMachine>
  </MachineModule>
  <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
    <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
  </MonitoredMachine>
  <Field Display=""Feedrate override"" TranslationKey=""FieldFeedrateOverride"" Code=""FeedrateOverride"" Type=""Double"" />
  <Value xsi:type=""xsd:double"">80</Value>
</Event>".Length,
                         s.Length);
#endif
          }
        }
      }
      {
        XmlSerializer serializer = new XmlSerializer (typeof (EventCncValue));
        using (var stringWriter = new StringWriter ()) {
          using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = false })) {
            stringWriter.NewLine = "\n";
            serializer.Serialize (xmlWriter, ev);
            var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
            Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?><EventCncValue xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" DateTime="2012-12-12 00:00:00" LocalDateTimeString="12/12/2012 01:00:00" LocalDateTimeG="12/12/2012 01:00:00" Message="message" Duration="00:20:00"><Level Name="" TranslationKey="EventLevelAlert" Display="Alert" Priority="100" /><MachineModule Name="machinemodule-1" ConfigPrefix="" Id="1"><CncAcquisition Name="MACHINE_A17" ConfigPrefix="" UseProcess="false" StaThread="false" UseCoreService="false" Id="1" Every="00:00:02" NotRespondingTimeout="00:02:00" SleepBeforeRestart="00:00:10"><Computer Name="PC17" Address="PC17" IsLctr="true" IsLpst="true" IsCnc="false" IsWeb="true" IsAutoReason="false" IsAlert="true" IsSynchronization="true" Id="1" /></CncAcquisition><MonitoredMachine Name="MACHINE_A17" Id="1"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine></MachineModule><MonitoredMachine Name="MACHINE_A17" Id="1"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine><Field Display="Feedrate override" Code="FeedrateOverride" TranslationKey="FieldFeedrateOverride" Type="Double" /><Value xsi:type="xsd:double">80</Value></EventCncValue>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
        Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<EventCncValue xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""2012-12-12 00:00:00"" LocalDateTimeG=""12/12/2012 01:00:00"" LocalDateTimeString=""12/12/2012 01:00:00"" Message=""message"" Duration=""00:20:00"">
  <Level Name="""" TranslationKey=""EventLevelAlert"" Display=""Alert"" Priority=""100"" />
  <MachineModule ConfigPrefix="""" Id=""1"" Name=""machinemodule-1"">
    <CncAcquisition Name=""MACHINE_A17"" ConfigPrefix="""" UseProcess=""false"" UseCoreService=""false"" StaThread=""false"" Id=""1"" Every=""00:00:02"" NotRespondingTimeout=""00:02:00"" SleepBeforeRestart=""00:00:10"">
      <Computer Name=""PC17"" Address=""PC17"" IsSynchronization=""true"" IsAlert=""true"" IsAutoReason=""false"" IsLctr=""true"" IsLpst=""true"" IsCnc=""false"" IsWeb=""true"" Id=""1"" />
    </CncAcquisition>
    <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
      <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
    </MonitoredMachine>
  </MachineModule>
  <MonitoredMachine Id=""1"" Name=""MACHINE_A17"">
    <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
  </MonitoredMachine>
  <Field Display=""Feedrate override"" TranslationKey=""FieldFeedrateOverride"" Code=""FeedrateOverride"" Type=""Double"" />
  <Value xsi:type=""xsd:double"">80</Value>
</EventCncValue>".Length, s.Length);
#endif
          }
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of the EventToolLife class
    /// </summary>
    [Test]
    public void TestEventToolLifeSerialization ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          // Creation of a new tool life event
          IEventLevel eventLevel = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById (1);
          IMachineModule machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindById (2);
          IEventToolLife toolLifeEvent = ModelDAOHelper.ModelFactory.CreateEventToolLife (
            eventLevel, EventToolLifeType.ExpirationReached, new DateTime (2012, 12, 12), machineModule);
          toolLifeEvent.ToolId = "1";
          toolLifeEvent.Unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById (7); // mm
          toolLifeEvent.Unproxy ();

          // Serialization of the Event type
          {
            var serializer = new XmlSerializer (typeof (Event));
            using (var stringWriter = new StringWriter ()) {
              using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = false })) {
                stringWriter.NewLine = "\n";
                serializer.Serialize (xmlWriter, toolLifeEvent);
                var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
                Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?><Event xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:type="EventToolLife" DateTime="2012-12-12 00:00:00" LocalDateTimeString="12/12/2012 01:00:00" LocalDateTimeG="12/12/2012 01:00:00"><Level Name="" TranslationKey="EventLevelAlert" Display="Alert" Priority="100" /><Message /><OldToolState>Unknown</OldToolState><NewToolState>Unknown</NewToolState><Direction>Unknown</Direction><ElapsedTime>0</ElapsedTime><ToolId>1</ToolId><MachineModule ConfigPrefix="" Id="2"><MonitoredMachine Name="MACHINE_B17" Id="2"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine></MachineModule><MonitoredMachine Name="MACHINE_B17" Id="2"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine><EventType>ExpirationReached</EventType><OldMagazine>-</OldMagazine><NewMagazine>-</NewMagazine><OldPot>-</OldPot><NewPot>-</NewPot><OldValue>-</OldValue><NewValue>-</NewValue><OldWarning>-</OldWarning><NewWarning>-</NewWarning><OldLimit>-</OldLimit><NewLimit>-</NewLimit><Unit TranslationKey="UnitDistanceMillimeter" Display="mm" Description="Distance (mm)" /><PreviousLocalDateString>12/12/2012 01:00:00</PreviousLocalDateString><OldToolStateName>ToolStateUnknown</OldToolStateName><NewToolStateName>ToolStateUnknown</NewToolStateName></Event>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
            Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Event xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xsi:type=""EventToolLife"" DateTime=""2012-12-12 00:00:00"" LocalDateTimeG=""12/12/2012 01:00:00"" LocalDateTimeString=""12/12/2012 01:00:00"">
  <Level Name="""" TranslationKey=""EventLevelAlert"" Display=""Alert"" Priority=""100"" />
  <Message />
  <OldToolState>Unknown</OldToolState>
  <NewToolState>Unknown</NewToolState>
  <ToolId>1</ToolId>
  <Direction>Unknown</Direction>
  <ElapsedTime>0</ElapsedTime>
  <MachineModule Id=""2"" ConfigPrefix="""">
    <MonitoredMachine Name=""MACHINE_B17"" Id=""2"">
      <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
    </MonitoredMachine>
  </MachineModule>
  <MonitoredMachine Name=""MACHINE_B17"" Id=""2"">
    <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
  </MonitoredMachine>
  <EventType>ExpirationReached</EventType>
  <OldMagazine>-</OldMagazine>
  <NewMagazine>-</NewMagazine>
  <OldPot>-</OldPot>
  <NewPot>-</NewPot>
  <OldValue>-</OldValue>
  <NewValue>-</NewValue>
  <OldWarning>-</OldWarning>
  <NewWarning>-</NewWarning>
  <OldLimit>-</OldLimit>
  <NewLimit>-</NewLimit>
  <Unit TranslationKey=""UnitDistanceMillimeter"" Display=""mm"" Description=""Distance (mm)"" />
  <PreviousLocalDateString>12/12/2012 01:00:00</PreviousLocalDateString>
  <OldToolStateName>ToolStateUnknown</OldToolStateName>
  <NewToolStateName>ToolStateUnknown</NewToolStateName>
</Event>".Length, s.Length);
#endif
              }
            }
          }

          // Serialization of the EventToolLife type
          {
            var serializer = new XmlSerializer (typeof (EventToolLife));
            using (var stringWriter = new StringWriter ()) {
              using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = false })) {
                stringWriter.NewLine = "\n";
                serializer.Serialize (xmlWriter, toolLifeEvent);
                var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
                Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?><EventToolLife xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" DateTime="2012-12-12 00:00:00" LocalDateTimeString="12/12/2012 01:00:00" LocalDateTimeG="12/12/2012 01:00:00"><Level Name="" TranslationKey="EventLevelAlert" Display="Alert" Priority="100" /><Message /><OldToolState>Unknown</OldToolState><NewToolState>Unknown</NewToolState><Direction>Unknown</Direction><ElapsedTime>0</ElapsedTime><ToolId>1</ToolId><MachineModule ConfigPrefix="" Id="2"><MonitoredMachine Name="MACHINE_B17" Id="2"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine></MachineModule><MonitoredMachine Name="MACHINE_B17" Id="2"><MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" /></MonitoredMachine><EventType>ExpirationReached</EventType><OldMagazine>-</OldMagazine><NewMagazine>-</NewMagazine><OldPot>-</OldPot><NewPot>-</NewPot><OldValue>-</OldValue><NewValue>-</NewValue><OldWarning>-</OldWarning><NewWarning>-</NewWarning><OldLimit>-</OldLimit><NewLimit>-</NewLimit><Unit TranslationKey="UnitDistanceMillimeter" Display="mm" Description="Distance (mm)" /><PreviousLocalDateString>12/12/2012 01:00:00</PreviousLocalDateString><OldToolStateName>ToolStateUnknown</OldToolStateName><NewToolStateName>ToolStateUnknown</NewToolStateName></EventToolLife>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
            Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<EventToolLife xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""2012-12-12 00:00:00"" LocalDateTimeG=""12/12/2012 01:00:00"" LocalDateTimeString=""12/12/2012 01:00:00"">
  <Level Name="""" TranslationKey=""EventLevelAlert"" Display=""Alert"" Priority=""100"" />
  <Message />
  <OldToolState>Unknown</OldToolState>
  <NewToolState>Unknown</NewToolState>
  <Direction>Unknown</Direction>
  <ElapsedTime>0</ElapsedTime>
  <MachineModule ConfigPrefix="""" Id=""2"">
    <MonitoredMachine Name=""MACHINE_B17"" Id=""2"">
      <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
    </MonitoredMachine>
  </MachineModule>
  <MonitoredMachine Name=""MACHINE_B17"" Id=""2"">
    <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
  </MonitoredMachine>
  <EventType>ExpirationReached</EventType>
  <OldMagazine>-</OldMagazine>
  <OldPot>-</OldPot>
  <NewMagazine>-</NewMagazine>
  <NewPot>-</NewPot>
  <OldValue>-</OldValue>
  <NewValue>-</NewValue>
  <OldWarning>-</OldWarning>
  <NewWarning>-</NewWarning>
  <OldLimit>-</OldLimit>
  <NewLimit>-</NewLimit>
  <Unit TranslationKey=""UnitDistanceMillimeter"" Display=""mm"" Description=""Distance (mm)"" />
  <PreviousLocalDateString>12/12/2012 01:00:00</PreviousLocalDateString>
  <OldToolStateName>ToolStateUnknown</OldToolStateName>
  <NewToolStateName>ToolStateUnknown</NewToolStateName>
  <ToolId>1</ToolId>
</EventToolLife>".Length, s.Length);
#endif
              }
            }
          }

          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the XML serialization/deserialization of the persistent class
    /// Sequence
    /// </summary>
    [Test]
    public void TestSequenceSerializationDeserialization ()
    {
      Sequence sequence1 = new Sequence ();
      sequence1.Name = "Seq1";
      sequence1.Order = 0;
      Sequence sequence2 = new Sequence ();
      sequence2.Name = "Seq2";
      sequence2.Order = 1;
      // important to have different orders before inserting in path
      Path path = new Path ();
      path.Number = 1;
      IOperationType operationType = ModelDAOHelper.ModelFactory.CreateOperationTypeFromName ("Test");
      IOperation operation = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
      operation.Name = "Op1";
      sequence1.Path = path;
      sequence2.Path = path;
      path.Operation = operation;

      // should have sequence1/2.Operation == operation
      Assert.AreEqual (sequence1.Operation, operation);
      Assert.AreEqual (sequence2.Operation, operation);

      XmlSerializer serializer = new XmlSerializer (typeof (Sequence));
      TextWriter stringWriter1 = new StringWriter ();
      TextWriter stringWriter2 = new StringWriter ();
      serializer.Serialize (stringWriter1, sequence1);
      serializer.Serialize (stringWriter2, sequence2);
      XmlSerializer deserializer = new XmlSerializer (typeof (Sequence));
      TextReader textReader1 = new StringReader (stringWriter1.ToString ());
      TextReader textReader2 = new StringReader (stringWriter2.ToString ());
      Sequence sequence1bis = (Sequence)deserializer.Deserialize (textReader1);
      Sequence sequence2bis = (Sequence)deserializer.Deserialize (textReader2);

      Assert.AreEqual (sequence1.Name, sequence1bis.Name);
      Assert.AreEqual (sequence1bis.Operation.Name, operation.Name);
      Assert.AreEqual (sequence1bis.Path.Number, path.Number);
      Assert.AreEqual (sequence1bis.Path.Operation.Name, operation.Name);

      Assert.AreEqual (sequence2.Name, sequence2bis.Name);
      Assert.AreEqual (sequence2bis.Operation.Name, operation.Name);
      Assert.AreEqual (sequence2bis.Path.Number, path.Number);
      Assert.AreEqual (sequence2bis.Path.Operation.Name, operation.Name);

    }

    /// <summary>
    /// Test the XML serialization of the AnalysisLog class
    /// </summary>
    [Test]
    public void TestAnalysisLogSerialization ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        IShiftTemplate shiftTemplate = ModelDAOHelper.ModelFactory
          .CreateShiftTemplate ("template");
        ModelDAOHelper.DAOFactory.ShiftTemplateDAO.MakePersistent (shiftTemplate);

        IShiftTemplateAssociation shiftTemplateAssociation = ModelDAOHelper.ModelFactory
          .CreateShiftTemplateAssociation (shiftTemplate,
                                           new DateTime (2012, 12, 12));
        shiftTemplateAssociation.Priority = 50;
        shiftTemplateAssociation.DateTime = new DateTime (2012, 12, 02);
        ModelDAOHelper.DAOFactory.ShiftTemplateAssociationDAO.MakePersistent (shiftTemplateAssociation);
        IGlobalModificationLog analysisLog = ModelDAOHelper.ModelFactory.CreateGlobalModificationLog (LogLevel.INFO,
                                                                                                      "Test",
                                                                                                      shiftTemplateAssociation);
        analysisLog.DateTime = new DateTime (2012, 12, 01);
        {
          XmlSerializer serializer = new XmlSerializer (typeof (Log));
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (xmlWriter, analysisLog);
              var s = stringWriter.ToString ();
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<Log xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:type="GlobalModificationLog" DateTime="2012-12-01T00:00:00" Level="INFO" Message="Test">
  <Modification xsi:type="ShiftTemplateAssociation" Priority="50" DateTime="2012-12-02 00:00:00" Begin="2012-12-12 00:00:00">
    <Option xsi:nil="true" />
    <ShiftTemplate Name="template" />
  </Modification>
</Log>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
            }
          }
        }
        {
          XmlSerializer serializer = new XmlSerializer (typeof (GlobalModificationLog));
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (xmlWriter, analysisLog);
              var s = stringWriter.ToString ();
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<GlobalModificationLog xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" DateTime="2012-12-01T00:00:00" Level="INFO" Message="Test">
  <Modification xsi:type="ShiftTemplateAssociation" Priority="50" DateTime="2012-12-02 00:00:00" Begin="2012-12-12 00:00:00">
    <Option xsi:nil="true" />
    <ShiftTemplate Name="template" />
  </Modification>
</GlobalModificationLog>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
            }
          }
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the XML serialization of the DetectionAnalysisLog class
    /// </summary>
    [Test]
    public void TestDetectionAnalysisLogSerialization ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory
          .MonitoredMachineDAO.FindByIdForXmlSerialization (1);
        IDetectionAnalysisLog analysisLog = ModelDAOHelper.ModelFactory.CreateDetectionAnalysisLog (LogLevel.INFO,
                                                                                                    "Test",
                                                                                                    machine);
        analysisLog.DateTime = new DateTime (2012, 12, 01);
        {
          XmlSerializer serializer = new XmlSerializer (typeof (Log));
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (xmlWriter, analysisLog);
              var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<Log xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:type="DetectionAnalysisLog" DateTime="2012-12-01T00:00:00" Level="INFO" Message="Test">
  <Machine xsi:type="MonitoredMachine" Name="MACHINE_A17" Id="1">
    <MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" />
  </Machine>
</Log>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
          Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Log xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xsi:type=""DetectionAnalysisLog"" DateTime=""2012-12-01T00:00:00"" Level=""INFO"" Message=""Test"">
  <Machine xsi:type=""MonitoredMachine"" Name=""MACHINE_A17"" Id=""1"">
    <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
  </Machine>
</Log>".Length,
                           s.Length);
#endif
            }
          }
        }
        {
          XmlSerializer serializer = new XmlSerializer (typeof (DetectionAnalysisLog));
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (xmlWriter, analysisLog);
              var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<DetectionAnalysisLog xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" DateTime="2012-12-01T00:00:00" Level="INFO" Message="Test">
  <Machine xsi:type="MonitoredMachine" Name="MACHINE_A17" Id="1">
    <MonitoringType TranslationKey="MonitoringTypeMonitored" Display="Monitored" Id="2" />
  </Machine>
</DetectionAnalysisLog>
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
          Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<DetectionAnalysisLog xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""2012-12-01T00:00:00"" Level=""INFO"" Message=""Test"">
  <Machine xsi:type=""MonitoredMachine"" Name=""MACHINE_A17"" Id=""1"">
    <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
  </Machine>
</DetectionAnalysisLog>".Length,
                           s.Length);
#endif
            }
          }
        }
      }
    }

    /// <summary>
    /// Test the XML serialization of the SynchronizationLog class
    /// </summary>
    [Test]
    public void TestSynchronizationLogSerialization ()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        ISynchronizationLog synchronizationLog = ModelDAOHelper.ModelFactory.CreateSynchronizationLog (LogLevel.INFO,
                                                                                                       "Test",
                                                                                                       "<Test>test</Test>");
        synchronizationLog.DateTime = new DateTime (2012, 12, 01);
        {
          XmlSerializer serializer = new XmlSerializer (typeof (Log));
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (xmlWriter, synchronizationLog);
              var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<Log xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xsi:type="SynchronizationLog" DateTime="2012-12-01T00:00:00" Level="INFO" Message="Test" XmlElement="&lt;Test&gt;test&lt;/Test&gt;" />
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
          Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<Log xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xsi:type=""SynchronizationLog"" DateTime=""2012-12-01T00:00:00"" Level=""INFO"" Message=""Test"" XmlElement=""&lt;Test&gt;test&lt;/Test&gt;"" />".Length,
                           s.Length);
#endif
            }
          }
        }
        {
          XmlSerializer serializer = new XmlSerializer (typeof (SynchronizationLog));
          using (var stringWriter = new StringWriter ()) {
            using (var xmlWriter = XmlWriter.Create (stringWriter, new XmlWriterSettings { Indent = true })) {
              stringWriter.NewLine = "\n";
              serializer.Serialize (xmlWriter, synchronizationLog);
              var s = stringWriter.ToString ();
#if NET6_0_OR_GREATER
              Assert.AreEqual ("""
<?xml version="1.0" encoding="utf-16"?>
<SynchronizationLog xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" DateTime="2012-12-01T00:00:00" Level="INFO" Message="Test" XmlElement="&lt;Test&gt;test&lt;/Test&gt;" />
""".ReplaceLineEndings (), s.ReplaceLineEndings ());
#else // NET48
          Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>
<SynchronizationLog xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" DateTime=""2012-12-01T00:00:00"" Level=""INFO"" Message=""Test"" XmlElement=""&lt;Test&gt;test&lt;/Test&gt;"" />".Length,
                           s.Length);
#endif
            }
          }
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();

      {
        var multiCatalog = new MultiCatalog ();
        multiCatalog.Add (new StorageCatalog (new Lemoine.ModelDAO.TranslationDAOCatalog (),
                                              new TextFileCatalog ("AlertServiceI18N",
                                                                   Lemoine.Info.PulseInfo.LocalConfigurationDirectory)));
        //multiCatalog.Add (new DefaultTextFileCatalog ());
        PulseCatalog.Implementation = new CachedCatalog (multiCatalog);
      }
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }
}
