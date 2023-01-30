// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Web.Config
{
  /// <summary>
  /// Current Day service
  /// </summary>
  public class ConfigFromKeyService
    : GenericCachedService<ConfigFromKeyRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ConfigFromKeyService).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ConfigFromKeyService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <returns></returns>
    public override object GetWithoutCache (ConfigFromKeyRequestDTO request)
    {
      var response = new ConfigFromKeyResponseDTO ();
      response.Value = Lemoine.Info.ConfigSet.Get<object> (request.Key);
      return response;
    }
    #endregion // Methods
  }
}
