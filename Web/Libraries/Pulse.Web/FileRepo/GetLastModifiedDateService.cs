// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Web;
using Lemoine.Web.FileRepo;

namespace Pulse.Web.FileRepo
{
  /// <summary>
  /// Description of TestService
  /// </summary>
  public class GetLastModifiedDateService
    : GenericNoCacheService<GetLastModifiedDateRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetLastModifiedDateService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public GetLastModifiedDateService ()
      : base ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(GetLastModifiedDateRequestDTO request)
    {
      try {
      return Lemoine.FileRepository.FileRepoClient.Implementation
        .GetLastModifiedDate (request.NSpace, request.Path);
      }
      catch (Lemoine.FileRepository.MissingFileException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetWithoutCache: MissingFileException", ex);
        }
        throw new FileRepoWebException (ex);
      }
      catch (Exception ex) {
        log.Error ("GetWithoutCache: exception", ex);
        throw new FileRepoWebException (ex);
      }
    }
    #endregion // Methods
  }
}
