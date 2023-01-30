// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;

namespace Pulse.Extensions.Web.Responses
{
  /// <summary>
  /// Return a revision (a set of modifications were recorded in the system)
  /// </summary>
  public class RevisionDTO
  {
    /// <summary>
    /// Revision id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="revision">not null</param>
    public RevisionDTO (IRevision revision)
    {
      Debug.Assert (null != revision);
      
      if (null != revision) {
        this.Id = revision.Id;
      }
    }
  }
}
