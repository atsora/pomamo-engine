// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NUnit.Framework;

using Lemoine.Model;

namespace Lemoine.ModelDAO.UnitTests
{
  /// <summary>
  /// Unit tests for the UpperBound structure
  /// </summary>
  [TestFixture]
  public class UpperBound_UnitTests
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UpperBound_UnitTests).FullName);

    /// <summary>
    /// Test the constructors
    /// </summary>
    [Test]
    public void TestConstructor ()
    {
      {
        UpperBound<double> test = new UpperBound<double> (null);
        Assert.That (test.HasValue, Is.False);
      }
      
      {
        UpperBound<double> test = new UpperBound<double> (3);
        Assert.That (test.HasValue, Is.True);
      }
    }
  }
}
