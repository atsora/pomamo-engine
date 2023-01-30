// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess.CommonResponseDTO
{
  /// <summary>
  /// Generic 'Save' response DTO
  /// </summary>
  [Api("Return the ID of the saved object")]
  public class SaveResponseDTO
  {
    #region Getters / Setters
    /// <summary>
    /// Id of the created object
    /// </summary>
    public long Id { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    public SaveResponseDTO (int id)
    {
      this.Id = id;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    public SaveResponseDTO (long id)
    {
      this.Id = id;
    }
    #endregion // Constructors
  }
}
