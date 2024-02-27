// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Lemoine.Conversion.Json;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Conversion.UnitTests
{
  /// <summary>
  /// Unit test of <see cref="TypeExtensions_UnitTest"/>
  /// </summary>
  public class TypeExtensions_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (TypeExtensions_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public TypeExtensions_UnitTest ()
    { }

    /// <summary>
    /// Test GetLightQualifiedName
    /// </summary>
    [Test]
    public void TestGetLightQualifiedName ()
    {
      {
        var n = 123;
        var s = n.GetType ().GetLightQualifiedName ();
        Assert.That (s, Is.EqualTo ("System.Int32"));
      }
      {
        IList<Lemoine.Cnc.CncAlarm> alarms = new List<Lemoine.Cnc.CncAlarm> ();
        var s = alarms.GetType ().GetLightQualifiedName ();
        Assert.That (s, Is.EqualTo ("System.Collections.Generic.List`1[[Lemoine.Cnc.CncAlarm, Lemoine.Cnc.CncAlarm]]"));
        var t = Type.GetType (s);
        Assert.That (t.GetLightQualifiedName (), Is.EqualTo (s));
      }
      {
        IDictionary<string, object> d = new Dictionary<string, object> ();
        d["123"] = 123;
        var s = d.GetType ().GetLightQualifiedName ();
        Assert.That (s, Is.EqualTo ("System.Collections.Generic.Dictionary`2[[System.String],[System.Object]]"));
        var t = Type.GetType (s);
        Assert.That (t.GetLightQualifiedName (), Is.EqualTo (s));
      }
    }
  }
}
