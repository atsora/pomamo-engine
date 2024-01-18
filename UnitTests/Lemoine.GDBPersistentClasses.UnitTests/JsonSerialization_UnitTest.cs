// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;

using Lemoine.Model;
using Lemoine.Database.Persistent;
using Lemoine.Core.Log;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Test some Json (De-)Serialization
  /// </summary>
  [TestFixture]
  public class JsonSerialization_UnitTest
  {
    string m_previousDSNName;

    static readonly ILog log = LogManager.GetLogger(typeof (JsonSerialization_UnitTest).FullName);

    /// <summary>
    /// Test the Json serialization of a dictionary
    /// </summary>
    [Test]
    public void TestDicoSerialization()
    {
      IDictionary<string, object> dico = new Dictionary<string, object> ();
      dico["Id"] = 30;
      dico["Name"] = "Toto";
      
      var json = JsonConvert.SerializeObject (dico);
      var deserialized = JsonConvert.DeserializeObject<IDictionary<string, object>> (json);

      Assert.Multiple (() => {
        Assert.That (deserialized["Id"], Is.EqualTo (30));
        Assert.That (deserialized["Name"], Is.EqualTo ("Toto"));
      });
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      Lemoine.ModelDAO.ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
      
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }
}
