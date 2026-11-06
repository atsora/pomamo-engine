// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Pulse.Web.Doc;

namespace Pulse.Web.UnitTests.Doc
{
  /// <summary>
  /// Unit tests of the optional parameters Tags and KindId of the Doc/Upload and Doc/UploadForm services
  /// </summary>
  public class DocUploadProperties_UnitTest
  {
    static Func<string, StringValues?> Parameters (Dictionary<string, StringValues> parameters) =>
      key => parameters.TryGetValue (key, out var values) ? values : (StringValues?)null;

    /// <summary>
    /// No parameter: the tags and the kind are left unchanged
    /// </summary>
    [Test]
    public void TestNoParameter ()
    {
      var error = DocUploadService.TryGetProperties (Parameters (new Dictionary<string, StringValues> ()), out var properties);
      Assert.Multiple (() => {
        Assert.That (error, Is.Null);
        Assert.That (properties.Tags, Is.Null);
        Assert.That (properties.SetKind, Is.False);
      });
    }

    /// <summary>
    /// Tags separated by a comma or repeated, and a kind
    /// </summary>
    [Test]
    public void TestTagsAndKind ()
    {
      var error = DocUploadService.TryGetProperties (Parameters (new Dictionary<string, StringValues> {
        ["Tags"] = new StringValues (new[] { "plan,client", "lubrication" }),
        ["KindId"] = "3",
      }), out var properties);
      Assert.Multiple (() => {
        Assert.That (error, Is.Null);
        Assert.That (properties.Tags, Is.EqualTo (new[] { "plan", "client", "lubrication" }));
        Assert.That (properties.SetKind, Is.True);
        Assert.That (properties.KindId, Is.EqualTo (3));
      });
    }

    /// <summary>
    /// Empty parameters: no tag and no kind
    /// </summary>
    [Test]
    public void TestEmptyParameters ()
    {
      var error = DocUploadService.TryGetProperties (Parameters (new Dictionary<string, StringValues> {
        ["Tags"] = "",
        ["KindId"] = "",
      }), out var properties);
      Assert.Multiple (() => {
        Assert.That (error, Is.Null);
        Assert.That (properties.Tags, Is.Not.Null);
        Assert.That (properties.SetKind, Is.True);
        Assert.That (properties.KindId, Is.Null);
      });
    }

    /// <summary>
    /// An invalid kind id is an error
    /// </summary>
    [Test]
    public void TestInvalidKindId ()
    {
      var error = DocUploadService.TryGetProperties (Parameters (new Dictionary<string, StringValues> {
        ["KindId"] = "drawing",
      }), out var _);
      Assert.That (error, Is.Not.Null);
    }
  }
}
