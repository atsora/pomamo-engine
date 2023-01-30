// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Info.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class AssemblyName_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (AssemblyName_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AssemblyName_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      var assemblyName = typeof (Lemoine.Info.AssemblyInfo).Assembly.GetName ();

      {
        var testName = $"{assemblyName.Name}";
        Assert.AreEqual (null, new AssemblyName (testName).Version);
      }

      {
        var testName = $"{assemblyName.Name}, Version={assemblyName.Version}";
        Assert.IsTrue (AssemblyName.ReferenceMatchesDefinition (assemblyName, new AssemblyName (testName)));
      }

      {
        var testName = $"{assemblyName.Name}, Version=0.0.9.0";
        Assert.IsTrue (AssemblyName.ReferenceMatchesDefinition (assemblyName, new AssemblyName (testName)));
      }
    }
  }
}
