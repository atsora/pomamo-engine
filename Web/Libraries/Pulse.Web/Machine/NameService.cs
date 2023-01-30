// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.Machine
{
  /// <summary>
  /// Service to return the name of a machine or a group
  /// </summary>
  public class NameService
    : GenericCachedService<NameRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NameService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public NameService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (NameRequestDTO request)
    {
      Debug.Assert (null != request);

      if (0 < request.MachineId) {
        return GetByMachine (request, request.MachineId);
      }
      else if (0 < request.Id) {
        return GetByMachine (request, request.Id);
      }
      else {
        return GetByGroup (request);
      }
    }

    object GetByGroup (NameRequestDTO request)
    {
      var groupId = request.GroupId;
      if (string.IsNullOrEmpty (groupId)) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetByGroup: empty group id {0}",
            request.GroupId);
        }
        return new ErrorDTO ("Empty group id", ErrorStatus.WrongRequestParameter);
      }
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (groupRequest);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetByGroup: group with id {0} is not valid",
            request.GroupId);
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }

      var response = new NameResponseDTO (group);
      return response;
    }

    object GetByMachine (NameRequestDTO request, int machineId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (machineId);
        if (null == machine) {
          if (log.IsErrorEnabled) {
            log.ErrorFormat ("GetByMachine: machine with id {0} does not exist", machineId);
          }
          return new ErrorDTO ("Invalid machine id", ErrorStatus.WrongRequestParameter);
        }

        var response = new NameResponseDTO (machine);
        return response;
      }
    }
    #endregion // Methods
  }
}
