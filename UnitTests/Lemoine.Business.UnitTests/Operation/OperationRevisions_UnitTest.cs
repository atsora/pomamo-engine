// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business.Operation;
using NUnit.Framework;

namespace Lemoine.Business.UnitTests.Operation
{
  /// <summary>
  /// Unit tests of the name rules of <see cref="OperationRevisions"/>, without any database
  /// </summary>
  [TestFixture]
  public class OperationRevisions_UnitTest
  {
    /// <summary>
    /// Base name and number of a revision, from the suffix " - rev N" or the former " revN"
    /// </summary>
    [TestCase ("OP10", "OP10", 1)]
    [TestCase ("OP10 - rev 1", "OP10", 1)]
    [TestCase ("OP10 - rev 2", "OP10", 2)]
    [TestCase ("OP10 - rev 12", "OP10", 12)]
    [TestCase ("OP10 - Rev 3", "OP10", 3)]
    [TestCase ("OP10-rev2", "OP10", 2)]
    [TestCase ("OP10 -rev 2", "OP10", 2)]
    [TestCase ("OP10 rev2", "OP10", 2)]
    [TestCase ("OP 10 - part - rev 4", "OP 10 - part", 4)]
    [TestCase ("OP10 - rev 2 - rev 3", "OP10 - rev 2", 3)]
    [TestCase ("OP10 - rev", "OP10 - rev", 1)]
    [TestCase ("OP10 - revision", "OP10 - revision", 1)]
    [TestCase ("OP10 rev 2", "OP10 rev 2", 1)]
    [TestCase ("- rev 2", "- rev 2", 1)]
    public void TestParse (string name, string expectedBaseName, int expectedNumber)
    {
      var (baseName, number) = OperationRevisions.Parse (name);
      Assert.Multiple (() => {
        Assert.That (baseName, Is.EqualTo (expectedBaseName));
        Assert.That (number, Is.EqualTo (expectedNumber));
      });
    }

    /// <summary>
    /// No base name for an operation with no name
    /// </summary>
    [Test]
    public void TestParseNull ()
    {
      var (baseName, number) = OperationRevisions.Parse (null);
      Assert.Multiple (() => {
        Assert.That (baseName, Is.Null);
        Assert.That (number, Is.EqualTo (1));
      });
    }

    /// <summary>
    /// Name of a revision: no suffix for the first one, " - rev N" for the next ones, which
    /// <see cref="OperationRevisions.Parse"/> reads back
    /// </summary>
    [TestCase (1, "OP10")]
    [TestCase (2, "OP10 - rev 2")]
    [TestCase (10, "OP10 - rev 10")]
    public void TestGetName (int number, string expected)
    {
      var name = OperationRevisions.GetName ("OP10", number);
      Assert.Multiple (() => {
        Assert.That (name, Is.EqualTo (expected));
        Assert.That (OperationRevisions.Parse (name), Is.EqualTo (("OP10", number)));
      });
    }
  }
}
