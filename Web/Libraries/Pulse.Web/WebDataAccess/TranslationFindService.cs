// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// TranslationFind Service.
  /// </summary>
  public class TranslationFindService : GenericCachedService<Pulse.Web.WebDataAccess.TranslationFind>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TranslationFindService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public TranslationFindService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.TranslationFind request)
    {
      string locale = request.Locale ?? "";
      string key = request.Key;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ITranslation translation = ModelDAOHelper.DAOFactory.TranslationDAO
          .Find (locale, key);
        if (null == translation) {
          return new ErrorDTO (string.Format ("No translation with locale {0} key {1}",
                                              locale, key),
                               ErrorStatus.NotApplicable);
        }
        else {
          return new TranslationDTOAssembler ().Assemble (translation);
        }
      }
    }
    #endregion // Methods
  }
}
