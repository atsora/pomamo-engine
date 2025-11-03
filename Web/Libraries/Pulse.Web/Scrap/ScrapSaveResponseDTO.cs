// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
using Pulse.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Reponse DTO for ReasonSave service
  /// </summary>
  [Api ("ReasonSave Response DTO")]
  public class ScrapSaveResponseDTO
  {
    /// <summary>
    /// Revision
    /// </summary>
    public RevisionDTO Revision { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="revision"></param>
    public ScrapSaveResponseDTO (IRevision revision)
    {
      this.Revision = new RevisionDTO (revision);
    }
  }
}
