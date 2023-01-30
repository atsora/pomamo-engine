// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for IsoFileDTO.
  /// </summary>
  public class IsoFileDTO
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public IsoFileDTO () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="isoFile"></param>
    internal protected IsoFileDTO (Lemoine.Model.IIsoFile isoFile)
    {
      this.Id = ((Lemoine.Collections.IDataWithId<int>)isoFile).Id;
      this.Display = isoFile.Display;
    }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
  }
}
