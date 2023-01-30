// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetMachineObservationStateSelection Service.
  /// </summary>
  public class GetMachineObservationStateSelectionService : GenericCachedService<Lemoine.DTO.GetMachineObservationStateSelection>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetMachineObservationStateSelectionService).FullName);

    #region Constructors
    /// <summary>
    /// GetMachineObservationStateSelection is a cached service.
    /// </summary>
    public GetMachineObservationStateSelectionService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request for GetMachineObservationStateSelection (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Lemoine.DTO.GetMachineObservationStateSelection request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IMachineObservationState> allMachineModes =
          ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindAll();
        
        IList<Lemoine.DTO.MachineObservationStateDTO> allMachineModesDTOList =
          (new Lemoine.DTO.MachineObservationStateDTOAssembler()).Assemble(allMachineModes).ToList();
        
        return allMachineModesDTOList;
      }
    }
    #endregion // Methods
  }
}
