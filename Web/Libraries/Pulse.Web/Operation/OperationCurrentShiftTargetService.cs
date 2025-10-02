// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Extensions.Business;
using Pulse.Web.CommonResponseDTO;
using System.Threading.Tasks;
using Lemoine.Extensions.Business.Group;
using Lemoine.Business.Operation;
using Lemoine.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class OperationCurrentShiftTargetService
    : GenericAsyncCachedService<OperationCurrentShiftTargetRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationCurrentShiftTargetService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public OperationCurrentShiftTargetService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> Get (OperationCurrentShiftTargetRequestDTO request)
    {
      Debug.Assert (null != request);

      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (groupRequest);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"Get: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var businessRequest = new OperationCurrentShiftTarget (group);
        var operationCurrentShiftTarget = await Lemoine.Business.ServiceProvider
          .GetAsync (businessRequest);
        Debug.Assert (null != operationCurrentShiftTarget);

        var responseDto = new OperationCurrentShiftTargetResponseDTO () {
          DateTime = operationCurrentShiftTarget.DateTime,
          ShiftGoal = operationCurrentShiftTarget.ShiftGoal
        };

        var component = Initialize<IComponent> (operationCurrentShiftTarget.Component, ModelDAOHelper.DAOFactory.ComponentDAO.FindById);
        var operation = Initialize<IOperation> (operationCurrentShiftTarget.Operation, ModelDAOHelper.DAOFactory.OperationDAO.FindById);

        if (null != operation) {
          responseDto.Operation = new OperationDTOAssembler ()
            .AssembleLong (operation);
        }
        if (null != component) {
          responseDto.Component = new ComponentDTOAssembler ()
            .Assemble (component);
        }

        // - Day/shift
        if (operationCurrentShiftTarget.Day.HasValue) {
          responseDto.Day = ConvertDTO.DayToIsoString (operationCurrentShiftTarget.Day.Value);
        }
        if (null != operationCurrentShiftTarget.Shift) {
          responseDto.Shift = new ShiftDTOAssembler ().Assemble (operationCurrentShiftTarget.Shift);
        }
        var range = operationCurrentShiftTarget.Range;
        if (!(range?.IsEmpty () ?? true)) {
          responseDto.Range = range.ToString (x => ConvertDTO.DateTimeUtcToIsoString (x));
        }

        return responseDto;
      }
    }

    /// <summary>
    /// Initialize an object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="v"></param>
    /// <param name="findById"></param>
    /// <returns></returns>
    T Initialize<T> (T v, Func<int, T> findById)
      where T : Lemoine.Collections.IDataWithId<int>
    {
      if (null == v) {
        return v;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (v)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.OperationCurrentShiftTarget.Initialize")) {
            var initialized = findById (v.Id);
            if (null == initialized) {
              log.Error ($"Initialize: object with type {typeof (T)} and id {v.Id} does not exist");
              return default (T);
            }
            else {
              return initialized;
            }
          }
        }
      }
      return v;
    }
  }
}
