// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.I18N;

namespace Lemoine.Web.I18N
{
  /// <summary>
  /// Current Day service
  /// </summary>
  public class CatalogService
    : GenericCachedService<CatalogRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CatalogService).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CatalogService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <returns></returns>
    public override object GetWithoutCache (CatalogRequestDTO request)
    {
      var response = new CatalogResponseDTO ();
      if (request.Default is null) {
        response.Value = PulseCatalog.GetString (request.Key);
      }
      else {
        response.Value = PulseCatalog.GetString (request.Key, request.Default);
      }
      return response;
    }
  }
}
