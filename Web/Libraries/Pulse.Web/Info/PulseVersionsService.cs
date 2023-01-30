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

namespace Pulse.Web.Info
{
  /// <summary>
  /// Description of PulseVersionsService
  /// </summary>
  public class PulseVersionsService
    : GenericCachedService<PulseVersionsRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PulseVersionsService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public PulseVersionsService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(PulseVersionsRequestDTO request)
    {
      var response = new PulseVersionsResponseDTO ();
      response.Versions = new Dictionary<string, string> ();
      
      var versions = Lemoine.Info.PulseVersions.Versions;
      foreach (var version in versions) {
        string key = version.Key.ToString ();
        if (key.EndsWith ("Version", StringComparison.InvariantCulture)) {
          key = key.Substring (0, key.Length - "Version".Length);
        }
        response.Versions.Add (key, version.Value.ToString ());
      }

      return response;
    }
    #endregion // Methods
  }
}
