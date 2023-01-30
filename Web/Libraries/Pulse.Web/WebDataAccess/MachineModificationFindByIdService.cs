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
  /// MachineModificationFindById Service.
  /// </summary>
  public class MachineModificationFindByIdService : GenericCachedService<Pulse.Web.WebDataAccess.MachineModificationFindById>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModificationFindByIdService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineModificationFindByIdService () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.MachineModificationFindById request)
    {
      long machineModificationId = request.Id;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (request.MachineId);
        if (null == machine) {
          return new ErrorDTO ("No machine with id " + request.MachineId,
                               ErrorStatus.WrongRequestParameter);
        }
        
        IMachineModification machineModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (machineModificationId, machine);
        if (null == machineModification) {
          return new ErrorDTO ("No machine modification with the specified ID and machine",
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          return new Pulse.Web.WebDataAccess.MachineModificationDTOAssembler ().Assemble (machineModification);
        }
      }
    }
    #endregion // Methods
  }
}
