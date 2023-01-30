// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Collections;
using Lemoine.Model;

namespace Pulse.Web.WebDataAccess.CommonResponseDTO
{
  /// <summary>
  /// SaveResponseDTO for a modification that includes the revision ID
  /// </summary>
  public class SaveModificationResponseDTO: SaveResponseDTO
  {
    /// <summary>
    /// Associated revision response
    /// </summary>
    public SaveResponseDTO Revision { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="modification">not null</param>
    public SaveModificationResponseDTO(IModification modification)
      : base (((IDataWithId<long>)modification).Id)
    {
      Debug.Assert (null != modification);
      
      if (null != modification) {
        if (null != modification.Revision) {
          this.Revision = new SaveResponseDTO (modification.Revision.Id);
        }
      }
    }
  }
}
