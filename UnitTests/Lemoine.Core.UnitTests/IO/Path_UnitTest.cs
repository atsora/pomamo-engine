// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.IO;

using NUnit.Framework;

namespace Lemoine.Core.UnitTests.IO
{
  /// <summary>
  /// Test <see cref="Lemoine.IO.Path"/>
  /// </summary>
  public class Path_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (Path_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Path_UnitTest ()
    { }

    /// <summary>
    /// Test IsCaseSensitive
    /// </summary>
    [Test]
    public void TestIsCaseSensitive ()
    {
      Assert.That (Lemoine.IO.Path.IsCaseSensitive (), Is.False);
    }

    /// <summary>
    /// Test IsSame
    /// </summary>
    [Test]
    public void TestIsSame ()
    {
      Assert.That (Lemoine.IO.Path.IsSame ("C:\\Test", "C:\\test"), Is.True);
      Assert.That (Lemoine.IO.Path.IsSame ("C:\\Test", "C:\\Test2"), Is.False);
    }
  }
}
