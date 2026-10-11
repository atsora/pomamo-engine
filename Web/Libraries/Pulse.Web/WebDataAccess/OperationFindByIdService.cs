// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.ModelDAO;
using Lemoine.Web;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// OperationFindById Service
  /// </summary>
  public class OperationFindByIdService : GenericCachedService<OperationFindById>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationFindByIdService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationFindByIdService () : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (OperationFindById request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var operation = ModelDAOHelper.DAOFactory.OperationDAO
          .FindById (request.Id);
        if (operation is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetWithoutCache: no operation with id {request.Id}");
          }
          return new ErrorDTO ($"No operation with id {request.Id}",
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          return new OperationDTOAssembler ().Assemble (operation);
        }
      }
    }
  }
}
