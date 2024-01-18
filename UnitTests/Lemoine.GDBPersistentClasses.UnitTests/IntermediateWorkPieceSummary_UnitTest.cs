// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml.Serialization;

using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.ModelDAO;
using Lemoine.Plugin.IntermediateWorkPieceSummary;
using NHibernate;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class IntermediateWorkPieceSummary
  /// </summary>
  [TestFixture]
  public class IntermediateWorkPieceSummary_UnitTest
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (IntermediateWorkPieceSummary_UnitTest).FullName);
    
    /// <summary>
    /// Test the de-serialization
    /// </summary>
    [Test]
    public void TestDeserialize ()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        TextReader textReader = new StringReader (@"<?xml version=""1.0"" encoding=""utf-16""?>
      <IntermediateWorkPieceSummary
        Targeted=""3""
        Checked=""2"">
        <IntermediateWorkPiece Name=""IntermediateWorkPieceName"" />
        <Component Name=""ComponentName"" />
        <WorkOrder Name=""WorkOrderName"" />
      </IntermediateWorkPieceSummary>");
        XmlSerializer deserializer = new XmlSerializer (typeof (IntermediateWorkPieceSummary));
        IntermediateWorkPieceSummary iwps =
          (IntermediateWorkPieceSummary) deserializer.Deserialize (textReader);
        Assert.Multiple (() => {
          Assert.That (iwps.IntermediateWorkPiece.Name, Is.EqualTo ("IntermediateWorkPieceName"));
          Assert.That (iwps.Component.Name, Is.EqualTo ("ComponentName"));
          Assert.That (iwps.WorkOrder.Name, Is.EqualTo ("WorkOrderName"));
        });
        transaction.Rollback ();
      }
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
