// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Web;

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Description of MachineModeCategoryLegendService
  /// </summary>
  public class MachineModeCategoryLegendService
    : GenericCachedService<MachineModeCategoryLegendRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeCategoryLegendService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineModeCategoryLegendService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(MachineModeCategoryLegendRequestDTO request)
    {
      IEnumerable<IMachineMode> machineModes;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.MachineModeCategoryLegend"))
        {
          machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindAll ();
        }
      }

      IEnumerable<MachineModeCategoryId> ids = machineModes
        .Select<IMachineMode, MachineModeCategoryId> (machineMode => machineMode.MachineModeCategory)
        .Distinct ()
        .OrderBy (machineModeCategoryId => (int)machineModeCategoryId);

      var response = new MachineModeCategoryLegendResponseDTO ();
      response.Items = new List<MachineModeCategoryDTO> ();
      foreach (var machineModeCategoryId in ids) {
        response.Items.Add (new MachineModeCategoryDTOAssembler().Assemble (machineModeCategoryId));
      }
      
      return response;
    }
    #endregion // Methods
  }
}
