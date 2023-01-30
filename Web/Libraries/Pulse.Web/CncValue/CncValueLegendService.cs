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
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Web;

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Description of CncValueLegendService
  /// </summary>
  public class CncValueLegendService
    : GenericCachedService<CncValueLegendRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncValueLegendService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CncValueLegendService ()
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
    public override object GetWithoutCache (CncValueLegendRequestDTO request)
    {
      IList<int> machineIds = request.MachineIds;

      if ((machineIds == null) || (machineIds.Count == 0)) {
        return new ErrorDTO ("No machine id was specified", ErrorStatus.WrongRequestParameter);
      }

      return BuildResponseDTO (machineIds);
    }

    object BuildResponseDTO (IList<int> machineIds)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // - Get the list of fields
        HashSet<IField> fields = new HashSet<IField> ();
        foreach (int machineId in machineIds) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.CncValueLegend.Machine")) {
            IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindByIdWithMainMachineModulePerformanceFieldUnit (machineId);
            if (null == monitoredMachine) {
              log.ErrorFormat ("GetWithoutCache: " +
                               "unknown monitored machine with ID {0}" +
                               "=> skip it",
                               machineId);
            }
            else {
              if (null != monitoredMachine.PerformanceField) {
                fields.Add (monitoredMachine.PerformanceField);
              }
            }
          }
        }

        CncValueLegendResponseDTO response = new CncValueLegendResponseDTO ();
        response.Items = new List<CncValueLegendItemDTO> ();
        foreach (var field in fields.Distinct ()) {
          CncValueLegendItemDTO item = new CncValueLegendItemDTO ();
          FieldDTO fieldDto = new FieldDTOAssembler ().Assemble (field);
          item.Field = fieldDto;
          var fieldLegends = Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.Field.FieldLegends (field));
            item.Legends = new List<LegendDTO> ();
          foreach (var color in fieldLegends.Select (l => l.Color).Distinct ()) {
            var legendDto = new LegendDTO ();
            var colorLegends = fieldLegends
              .Where (l => object.Equals (l.Color, color))
              .Select (l => l.Text)
              .Distinct ();
            legendDto.Display = string.Join (" ", colorLegends.ToArray ());
            legendDto.Color = color;
            item.Legends.Add (legendDto);
          }
          response.Items.Add (item);
        }
        return response;
      }
    }
#endregion // Methods
  }
}
