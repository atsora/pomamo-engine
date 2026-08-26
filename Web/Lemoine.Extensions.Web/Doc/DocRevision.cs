// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Web.Doc
{
  /// <summary>
  /// One available revision of a document
  /// 
  /// In the git implementation, a revision is associated to a commit
  /// </summary>
  public struct DocRevision
  {
    /// <summary>
    /// Revision number
    /// 
    /// The greater the number, the more recent the revision
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// Description of this revision: what was changed in it
    /// 
    /// In the git implementation, the commit message
    /// 
    /// Possibly empty
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Author of this revision
    /// 
    /// In the git implementation, the commit author
    /// 
    /// Possibly empty
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// UTC date/time when this revision was created
    /// 
    /// In the git implementation, the commit date/time
    /// </summary>
    public DateTime DateTime { get; set; }
  }
}
