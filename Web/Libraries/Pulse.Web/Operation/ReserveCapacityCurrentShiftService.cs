// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  public class ReserveCapacityCurrentShiftService
    : GenericAsyncCachedService<ReserveCapacityCurrentShiftRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReserveCapacityCurrentShiftService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ReserveCapacityCurrentShiftService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> Get (ReserveCapacityCurrentShiftRequestDTO request)
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
        var businessRequest = new ReserveCapacityCurrentShift (group);
        var reserveCapacityCurrentShift = await Lemoine.Business.ServiceProvider
          .GetAsync (businessRequest);
        Debug.Assert (null != reserveCapacityCurrentShift);

        var responseDto = new ReserveCapacityCurrentShiftResponseDTO () {
          DateTime = reserveCapacityCurrentShift.DateTime,
          NbPieceCurrentShift = reserveCapacityCurrentShift.NbPiecesCurrentShift,
          GoalNowShift = reserveCapacityCurrentShift.GoalCurrentShift,
          ShiftGoal = reserveCapacityCurrentShift.ShiftGoal,
          RemainingCapacity = reserveCapacityCurrentShift.RemainingCapacity,
          ReserveCapacity = reserveCapacityCurrentShift.ReserveCapacity
        };

        var component = Initialize<IComponent> (reserveCapacityCurrentShift.Component, ModelDAOHelper.DAOFactory.ComponentDAO.FindById);
        var operation = Initialize<IOperation> (reserveCapacityCurrentShift.Operation, ModelDAOHelper.DAOFactory.OperationDAO.FindById);

        if (null != operation) {
          responseDto.Operation = new OperationDTOAssembler ()
            .AssembleLong (operation);
        }
        if (null != component) {
          responseDto.Component = new ComponentDTOAssembler ()
            .Assemble (component);
        }

        // - Day/shift
        if (reserveCapacityCurrentShift.Day.HasValue) {
          responseDto.Day = ConvertDTO.DayToIsoString (reserveCapacityCurrentShift.Day.Value);
        }
        if (null != reserveCapacityCurrentShift.Shift) {
          responseDto.Shift = new ShiftDTOAssembler ().Assemble (reserveCapacityCurrentShift.Shift);
        }
        var range = reserveCapacityCurrentShift.Range;
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
          using (var transaction = session.BeginReadOnlyTransaction ("Web.ReserveCapacityCurrentShift.Initialize")) {
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
    #endregion // Methods
  }
}
