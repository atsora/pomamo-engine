// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;

namespace Lemoine.Conversion.UnitTests
{
  /// <summary>
  /// Unit tests for the class <see cref="JsonComparison"/>
  /// </summary>
  public class JsonComparison_UnitTest
  {
    /// <summary>
    /// The white spaces and the order of the object properties are not considered,
    /// like in the normalized form PostgreSQL returns a jsonb
    /// </summary>
    [Test]
    public void TestSameContent ()
    {
      Assert.Multiple (() => {
        Assert.That (JsonComparison.AreEqual ("""{"frequencyType":"FixedDuration","seconds":3600}""",
          """{"seconds": 3600, "frequencyType": "FixedDuration"}"""), Is.True);
        Assert.That (JsonComparison.AreEqual ("""{"a":{"x":1,"y":[1,2]},"b":"é"}""",
          """{"b": "é", "a": {"y": [1, 2], "x": 1}}"""), Is.True);
        Assert.That (JsonComparison.AreEqual ("{}", "{ }"), Is.True);
        Assert.That (JsonComparison.AreEqual ("null", "null"), Is.True);
      });
    }

    /// <summary>
    /// A different content
    /// </summary>
    [Test]
    public void TestDifferentContent ()
    {
      Assert.Multiple (() => {
        Assert.That (JsonComparison.AreEqual ("""{"seconds":3600}""", """{"seconds":3601}"""), Is.False);
        Assert.That (JsonComparison.AreEqual ("""{"seconds":3600}""", """{"seconds":"3600"}"""), Is.False);
        Assert.That (JsonComparison.AreEqual ("""{"a":1}""", """{"a":1,"b":null}"""), Is.False);
        Assert.That (JsonComparison.AreEqual ("[1,2]", "[2,1]"), Is.False);
        Assert.That (JsonComparison.AreEqual ("{}", "[]"), Is.False);
      });
    }

    /// <summary>
    /// A null text is only equal to another null text
    /// </summary>
    [Test]
    public void TestNull ()
    {
      Assert.Multiple (() => {
        Assert.That (JsonComparison.AreEqual (null, null), Is.True);
        Assert.That (JsonComparison.AreEqual (null, "{}"), Is.False);
        Assert.That (JsonComparison.AreEqual ("{}", null), Is.False);
        Assert.That (JsonComparison.AreEqual (null, "null"), Is.False);
      });
    }

    /// <summary>
    /// A text that is not a valid JSON is only equal to the same text, without any exception
    /// </summary>
    [Test]
    public void TestInvalidJson ()
    {
      Assert.Multiple (() => {
        Assert.That (JsonComparison.AreEqual ("{invalid", "{invalid"), Is.True);
        Assert.That (JsonComparison.AreEqual ("{invalid", "{ invalid"), Is.False);
        Assert.That (JsonComparison.AreEqual ("""{"a":1}""", "{invalid"), Is.False);
        Assert.That (JsonComparison.AreEqual ("""{"a":1,"a":1}""", """{"a": 1}"""), Is.False);
      });
    }
  }
}
