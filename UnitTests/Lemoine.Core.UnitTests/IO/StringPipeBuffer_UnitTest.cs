// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Test <see cref="StringPipeBuffer"/>
  /// </summary>
  public class StringPipeBuffer_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (StringPipeBuffer_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public StringPipeBuffer_UnitTest ()
    { }

    /// <summary>
    /// Test <see cref="StringPipeBuffer.Skip(int)"/>
    /// </summary>
    [Test]
    public void TestSkip ()
    {
      using var stringWriter = new StringWriter ();
      using var stringPipeBuffer = new StringPipeBuffer ();
      stringPipeBuffer.Add ("123456789");
      stringPipeBuffer.Release (stringWriter, 3);
      stringPipeBuffer.Skip (3);
      stringPipeBuffer.Release (stringWriter);
      Assert.That (stringWriter.ToString (), Is.EqualTo ("123789"));
    }

    /// <summary>
    /// Test <see cref="StringPipeBuffer.SkipAsync(int)"/>
    /// </summary>
    [Test]
    public async Task TestSkipAsync ()
    {
      using var stringWriter = new StringWriter ();
      using var stringPipeBuffer = new StringPipeBuffer ();
      stringPipeBuffer.Add ("123456789");
      stringPipeBuffer.Release (stringWriter, 3);
      await stringPipeBuffer.SkipAsync (3);
      stringPipeBuffer.Release (stringWriter);
      Assert.That (stringWriter.ToString (), Is.EqualTo ("123789"));
    }

    /// <summary>
    /// Test <see cref="StringPipeBuffer.SkipAsync(int)"/>
    /// </summary>
    [Test]
    public async Task TestSkipAsync2 ()
    {
      using var stringWriter = new StringWriter ();
      using var stringPipeBuffer = new StringPipeBuffer ();
      stringPipeBuffer.Add ("123456789");
      stringPipeBuffer.Release (stringWriter, 3);
      var n = await stringPipeBuffer.SkipAsync ();
      Assert.Multiple (() => {
        Assert.That (n, Is.EqualTo (6));
        Assert.That (stringWriter.ToString (), Is.EqualTo ("123"));
      });
      stringPipeBuffer.Release (stringWriter);
      Assert.That (stringWriter.ToString (), Is.EqualTo ("123"));
    }

    /// <summary>
    /// Test <see cref="StringPipeBuffer.SkipAsync(int)"/>
    /// </summary>
    [Test]
    public async Task TestReleaseAsync2 ()
    {
      using var stringWriter = new StringWriter ();
      using var stringPipeBuffer = new StringPipeBuffer ();
      stringPipeBuffer.Add ("123456789");
      stringPipeBuffer.Release (stringWriter, 3);
      Assert.That (stringWriter.ToString (), Is.EqualTo ("123"));
      var n = await stringPipeBuffer.ReleaseAsync (stringWriter);
      Assert.Multiple (() => {
        Assert.That (n, Is.EqualTo (6));
        Assert.That (stringWriter.ToString (), Is.EqualTo ("123456789"));
      });
    }
  }
}
