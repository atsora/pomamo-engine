// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.DataRepository.UnitTests
{
  /// <summary>
  /// Unit tests for the class XMLFactory.
  /// </summary>
  [TestFixture]
  public class XMLFactory_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (XMLFactory_UnitTest).FullName);

    string m_unitTestsIn;

    /// <summary>
    /// Test the constructor
    /// </summary>
    [Test]
    public void TestXMLFactory()
    {
      XMLFactory factory;
      
      // With a file
      factory = new XMLFactory (XmlSourceType.URI,
                                Path.Combine (m_unitTestsIn, "testXMLFactory-Valid.xml"));
      
      // With a string
      string xmlStructure =
        "<root>" +
        "<job name=\"JOBNAME\" />" +
        "</root>";
      factory = new XMLFactory (XmlSourceType.STRING,
                                xmlStructure);
    }
    
    /// <summary>
    /// Test the GetData method
    /// </summary>
    [Test]
    public void TestGetData ()
    {
      XMLFactory f;
      XmlDocument d;
      
      // With a valid XML structure
      string xmlStructure =
        "<root>" +
        "<job name=\"JOBNAME\" />" +
        "</root>";
      f = new XMLFactory (XmlSourceType.STRING,
                          xmlStructure);
      d = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      Assert.That (d, Is.Not.EqualTo (null));
      XmlElement root = d.DocumentElement;
      Assert.That (root, Is.Not.EqualTo (null));
      Assert.Multiple (() => {
        Assert.That (root.Name, Is.EqualTo ("root"));
        Assert.That (root.GetElementsByTagName ("job"), Has.Count.EqualTo (1));
      });
      XmlElement job = root.GetElementsByTagName ("job") [0] as XmlElement;
      Assert.That (job, Is.Not.EqualTo (null));
      Assert.That (job.GetAttribute ("name"), Is.EqualTo ("JOBNAME"));
      
      // With a valid XML file
      f = new XMLFactory (XmlSourceType.URI,
                          Path.Combine (m_unitTestsIn, "testXMLFactory-Valid.xml"));
      d = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      Assert.That (d, Is.Not.EqualTo (null));
      root = d.DocumentElement;
      Assert.That (root, Is.Not.EqualTo (null));
      Assert.Multiple (() => {
        Assert.That (root.Name, Is.EqualTo ("root"));
        Assert.That (root.GetElementsByTagName ("job"), Has.Count.EqualTo (1));
      });
      job = root.GetElementsByTagName ("job") [0] as XmlElement;
      Assert.That (job, Is.Not.EqualTo (null));
      Assert.That (job.GetAttribute ("name"), Is.EqualTo ("JOBNAME"));

      // With an unknown XML file (but a valid directory)
      f = new XMLFactory (XmlSourceType.URI, "file:///C:\\Windows\\"  +
        "\\it would be weird to create a file with such a name.xml");
      Assert.Throws<FileNotFoundException> ( delegate { d = f.GetData (cancellationToken: System.Threading.CancellationToken.None); });
    }
    
    /// <summary>
    /// Test a CNC configuration repository
    /// </summary>
    [Test]
    public void TestCncData ()
    {
      XMLFactory f;
      XmlDocument d;
      
      // With a valid XML structure
      string xmlStructure = @"<root>
  <MachineModule xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Id=""1"" ConfigPrefix="""" MachinePostId=""0"">
    <CncAcquisition Id=""1"" Name=""Nakamura-1"" ConfigFile=""Fanuc-Nakamura-NTY3-Inches.xml"" ConfigPrefix="""" ConfigParameters=""#10.18.4.101"" UseProcess=""false"" Every=""00:00:02"" NotRespondingTimeout=""00:02:00"" SleepBeforeRestart=""00:00:10"">
      <Computer Id=""2"" Name=""Lpost"" Address=""Lpost"" IsLctr=""false"" IsLpst=""true"" IsCnc=""false"" />
    </CncAcquisition>
    <MonitoredMachine Name=""Nakamura-1"">
      <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
    </MonitoredMachine>
  </MachineModule>
  <MachineModule xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Id=""14"" Name=""Main spindle"" ConfigPrefix=""MainSpindle-"" MachinePostId=""0"">
    <CncAcquisition Id=""1"" Name=""Nakamura-1"" ConfigFile=""Fanuc-Nakamura-NTY3-Inches.xml"" ConfigPrefix="""" ConfigParameters=""#10.18.4.101"" UseProcess=""false"" Every=""00:00:02"" NotRespondingTimeout=""00:02:00"" SleepBeforeRestart=""00:00:10"">
      <Computer Id=""2"" Name=""Lpost"" Address=""Lpost"" IsLctr=""false"" IsLpst=""true"" IsCnc=""false"" />
    </CncAcquisition>
    <MonitoredMachine Name=""Nakamura-1"">
      <MonitoringType TranslationKey=""MonitoringTypeMonitored"" Display=""Monitored"" Id=""2"" />
    </MonitoredMachine>
  </MachineModule>
</root>";
      f = new XMLFactory (XmlSourceType.STRING,
                          xmlStructure);
      d = f.GetData (cancellationToken: System.Threading.CancellationToken.None);
      ICollection<ICncAcquisition> cncAcquisitions = new HashSet<ICncAcquisition> ();
      foreach (XmlNode child in d.DocumentElement.ChildNodes) {
        if (!(child is XmlElement)) {
          continue;
        }
        try {
          XmlSerializer xmlSerializer = new XmlSerializer (typeof (Lemoine.GDBPersistentClasses.MachineModule));
          IMachineModule machineModule;
          using (TextReader reader = new StringReader (child.OuterXml))
          {
            machineModule = (IMachineModule) xmlSerializer.Deserialize (reader);
          }
          cncAcquisitions.Add (machineModule.CncAcquisition);
        }
        catch (Exception ex) {
          log.ErrorFormat ("OnStart: " +
                           "error {0} while trying to deserialize {1}",
                           ex, child.OuterXml);
        }
      }

      Assert.That (cncAcquisitions, Has.Count.EqualTo (1));
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      m_unitTestsIn = TestContext.CurrentContext.TestDirectory;
    }
  }
}
