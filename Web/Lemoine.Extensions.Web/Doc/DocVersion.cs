// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Web.Doc
{
  /// <summary>
  /// One available version of a document
  /// 
  /// In the git implementation, a version is associated to a commit
  /// </summary>
  public struct DocVersion
  {
    /// <summary>
    /// Version number
    /// 
    /// The greater the number, the more recent the version
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Description of this version: what was changed in it
    /// 
    /// In the git implementation, the commit message
    /// 
    /// Possibly empty
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Author of this version
    /// 
    /// In the git implementation, the commit author
    /// 
    /// Possibly empty
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// UTC date/time when this version was created
    /// 
    /// In the git implementation, the commit date/time
    /// </summary>
    public DateTime DateTime { get; set; }
  }
}
