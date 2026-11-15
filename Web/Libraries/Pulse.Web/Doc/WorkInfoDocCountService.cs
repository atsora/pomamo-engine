// Copyright (C) 2026 Atsora Solutions

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Business.Operation;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.ModelDAO;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Number of distinct documents of an operation and/or of a component (or part),
  /// for example to show a document icon only when there is at least one document
  ///
  /// Operation: the documents of its active revision. Component: the documents of the
  /// component, a part sharing the id of its component. A document associated to both is
  /// counted once, an unknown id counts 0
  ///
  /// Only the associations are counted, the document store is not requested.
  /// The response is cached for a short time (<see cref="CacheTimeOut.CurrentLong"/>)
  /// </summary>
  public class WorkInfoDocCountService
    : GenericAsyncCachedService<WorkInfoDocCountRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WorkInfoDocCountService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkInfoDocCountService ()
      : base (CacheTimeOut.CurrentLong)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (WorkInfoDocCountRequestDTO request)
    {
      Debug.Assert (null != request);

      if (!request.OperationId.HasValue && !request.ComponentId.HasValue) {
        if (log.IsErrorEnabled) {
          log.Error ("Get: neither OperationId nor ComponentId is set");
        }
        return new ErrorDTO ("OperationId or ComponentId is required", ErrorStatus.WrongRequestParameter);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int? operationRevisionId = null;
        if (request.OperationId.HasValue) {
          var operation = await ModelDAOHelper.DAOFactory.OperationDAO
            .FindByIdAsync (request.OperationId.Value);
          if (operation is null) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: unknown operation id {request.OperationId}");
            }
          }
          else {
            operationRevisionId = (await OperationRevisions.GetActiveAsync (operation)).Id;
          }
        }

        var docCount = await ModelDAOHelper.DAOFactory.DocDAO
          .CountDistinctAsync (request.ComponentId, operationRevisionId);
        return new WorkInfoDocCountResponseDTO {
          DocCount = docCount
        };
      }
    }
  }
}
