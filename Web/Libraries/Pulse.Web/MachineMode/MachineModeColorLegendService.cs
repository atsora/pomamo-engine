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
  /// Description of MachineModeColorLegendService
  /// </summary>
  public class MachineModeColorLegendService
    : GenericCachedService<MachineModeColorLegendRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeColorLegendService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineModeColorLegendService ()
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
    public override object GetWithoutCache(MachineModeColorLegendRequestDTO request)
    {
      MachineModeColorLegendResponseDTO response = new MachineModeColorLegendResponseDTO ();
      response.Items = new List<MachineModeColorLegendItemDTO> ();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.MachineModeColorLegend"))
        {
          IList<IMachineMode> machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindAll ();
          IEnumerable<IMachineMode> rootMachineModes = machineModes
            .Where (machineMode => (null == machineMode.Parent));
          foreach (var rootMachineMode in rootMachineModes) {
            AddRootMachineMode (response, machineModes, rootMachineMode);
          }
          foreach (var rootMachineMode in rootMachineModes) {
            AddMachineMode (response, machineModes, rootMachineMode);
          }
        }
      }
      
      return response;
    }

    void AddRootMachineMode (MachineModeColorLegendResponseDTO response, IEnumerable<IMachineMode> machineModes, IMachineMode machineMode)
    {
      MachineModeColorLegendItemDTO item = response.Items
        .SingleOrDefault (element => string.Equals (machineMode.Color, element.Color,
                                                    StringComparison.InvariantCultureIgnoreCase));
      if (null == item) {
        item = new MachineModeColorLegendItemDTO ();
        item.Color = machineMode.Color;
        item.Display = machineMode.Display;
        response.Items.Add (item);
      }
      else { // null != item
        item.Display += ", " + machineMode.Display;
      }
    }
    
    void AddMachineMode (MachineModeColorLegendResponseDTO response, IEnumerable<IMachineMode> machineModes, IMachineMode machineMode)
    {
      if (!response.Items.Any (element => string.Equals (machineMode.Color, element.Color, StringComparison.InvariantCultureIgnoreCase))) {
        var item = new MachineModeColorLegendItemDTO ();
        item.Color = machineMode.Color;
        item.Display = machineMode.Display;
        response.Items.Add (item);
      }
      var children = machineModes.Where (m => object.Equals (machineMode, m.Parent));
      foreach (var child in children) {
        AddMachineMode (response, machineModes, child);
      }
    }
    #endregion // Methods
  }
}
