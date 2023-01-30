// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Extensions.Web.Responses
{
  /// <summary>
  /// Response DTO for all the services that may return revision
  /// </summary>
  public class NewModificationsDTO
  {
    /// <summary>
    /// Revision
    /// </summary>
    public RevisionDTO Revision { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="revision"></param>
    public NewModificationsDTO (IRevision revision)
    {
      this.Revision = new RevisionDTO (revision);
    }
  }
}
