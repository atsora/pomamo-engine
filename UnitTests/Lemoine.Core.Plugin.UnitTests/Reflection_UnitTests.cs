// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Lemoine.Core.Log;
using Lemoine.Conversion;

namespace Lemoine.Core.Plugin.UnitTests
{
  /// <summary>
  /// Tests on <see cref="Lemoine.Core.Plugin.Reflection"/>
  /// </summary>
  public class Reflection_UnitTests
  {
    readonly ILog log = LogManager.GetLogger (typeof (Reflection_UnitTests).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Reflection_UnitTests ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestInvokeAuto ()
    {
      var result = Lemoine.Core.Plugin.Reflection.InvokeMethodAuto (this, "Test", "123", "1.23", "First", "s");
      Assert.That (result, Is.EqualTo (456));

      Assert.Throws<ArgumentException> (() => Lemoine.Core.Plugin.Reflection.InvokeMethodAuto (this, new IAutoConverter[] { }, null, "Test", "123", "1.23", "First", "s"));
    }


    [Test]
    public void TestInvokeStaticMethod ()
    {
      var result = Lemoine.Core.Plugin.Reflection.InvokeStaticMethod (new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader (), "Lemoine.Core.Plugin.UnitTests.Reflection_UnitTests, Lemoine.Core.Plugin.UnitTests", "StaticMethod");
      Assert.That (result, Is.EqualTo (987));

      var physicalMemory = (long)Lemoine.Core.Plugin.Reflection.InvokeStaticMethod (new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader (), "Lemoine.Info.ProgramInfo, Lemoine.Core", "GetPhysicalMemory");
      Assert.That (physicalMemory, Is.GreaterThan (0));
    }

    [Test]
    public void TestInvokeStaticMethodAuto ()
    {
      var result = Lemoine.Core.Plugin.Reflection.InvokeStaticMethodAuto (new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader (), "Lemoine.Core.Plugin.UnitTests.Reflection_UnitTests, Lemoine.Core.Plugin.UnitTests", "StaticMethod");
      Assert.That (result, Is.EqualTo (987));
    }

#pragma warning disable NUnit1028 // The non-test method is public
    public int Test (int x, double d, EnumTest enumTest, int y)
    {
      Assert.Fail ("Fail always");
      return 0;
    }
    public int Test (int x, double d, EnumTest enumTest, string s)
    {
      Assert.Multiple (() => {
        Assert.That (x, Is.EqualTo (123));
        Assert.That (d, Is.EqualTo (1.23));
        Assert.That (enumTest, Is.EqualTo (EnumTest.First));
        Assert.That (s, Is.EqualTo ("s"));
      });
      return 456;
    }

    public static int StaticMethod ()
    {
      return 987;
    }
#pragma warning restore NUnit1028 // The non-test method is public
  }
}
