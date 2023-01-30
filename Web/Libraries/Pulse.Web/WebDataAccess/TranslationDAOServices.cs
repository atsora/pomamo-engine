// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Description of TranslationDAO.
  /// </summary>
  public class TranslationDAOServices: Service
  {
    /// <summary>
    /// Response to GET request for Translation/Find
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Pulse.Web.WebDataAccess.TranslationFind request) {
      return new TranslationFindService().Get (this.GetCacheClient(),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }
  }
}
#endif // NSERVICEKIT
