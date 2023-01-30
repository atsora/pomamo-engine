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
  public class ListDirectoriesInDirectoryService
    : GenericNoCacheService<ListDirectoriesInDirectoryRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ListDirectoriesInDirectoryService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ListDirectoriesInDirectoryService ()
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
    public override object GetWithoutCache(ListDirectoriesInDirectoryRequestDTO request)
    {
      try {
      return Lemoine.FileRepository.FileRepoClient.Implementation
        .ListDirectoriesInDirectory (request.NSpace, "");
      }
      catch (Exception ex) {
        log.Error ("GetWithoutCache: exception", ex);
        throw new FileRepoWebException (ex);
      }
    }
    #endregion // Methods
  }
}
