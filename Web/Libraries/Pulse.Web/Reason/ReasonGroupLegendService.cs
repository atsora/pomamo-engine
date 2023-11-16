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
using Lemoine.Extensions.Database;
using Lemoine.Extensions.Web.Services;
using Lemoine.Web;
using Pulse.Extensions.Database;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Description of ReasonGroupLegendService
  /// </summary>
  public class ReasonGroupLegendService
    : GenericAsyncCachedService<ReasonGroupLegendRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonGroupLegendService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public ReasonGroupLegendService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> Get (ReasonGroupLegendRequestDTO request)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: requested");
      }

      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          ReasonGroupLegendResponseDTO response = new ReasonGroupLegendResponseDTO ();
          response.Items = new List<ReasonGroupLegendItemDTO> ();

          var reasonLegendExtensionsRequest = new Lemoine.Business.Extension.GlobalExtensions<IReasonLegendExtension> (x => x.Initialize ());
          var reasons = new HashSet<IReason> ();
          var reasonLegendExtensions = await Lemoine.Business.ServiceProvider
            .GetAsync (reasonLegendExtensionsRequest);
          foreach (var reasonLegendExtension in reasonLegendExtensions) {
            try {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: consider extension {reasonLegendExtension}");
              }
              var newReasons = reasonLegendExtension.GetUsedReasons ().Where (x => null != x);
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: {newReasons.Count ()} reasons for extension {reasonLegendExtension}");
              }
              reasons.UnionWith (newReasons);
            }
            catch (Exception ex1) {
              log.Error ($"Get: exception with extension {reasonLegendExtension} but continue", ex1);
            }
          }
          var reasonGroups = reasons
            .GroupBy (r => r.ReasonGroup)
            .OrderBy (r => r.Key);

          foreach (var groupReasons in reasonGroups) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: add reasonGroup Id={groupReasons.Key.Id}");
            }
            ReasonGroupLegendItemDTO item = response.Items
              .SingleOrDefault (element => string.Equals (groupReasons.Key.Color, element.Color,
                                                          StringComparison.InvariantCultureIgnoreCase));
            string overridenText = Lemoine.Info.ConfigSet
              .LoadAndGet<string> ($"Legend.ReasonGroup.{groupReasons.Key.Color}", "");
            if (item is null) {
              item = new ReasonGroupLegendItemDTO ();
              item.Color = groupReasons.Key.Color;
              if (!string.IsNullOrEmpty (overridenText)) {
                item.Display = overridenText;
              }
              else {
                item.Display = groupReasons.Key.Display;
              }
              item.ReasonGroups = new List<ReasonGroupDTO> ();
              item.ReasonGroups.Add (new ReasonGroupDTOAssembler ().Assemble (groupReasons.Key, groupReasons));
              response.Items.Add (item);
            }
            else { // !(item is null)
              if (string.IsNullOrEmpty (overridenText)) {
                item.Display += ", " + groupReasons.Key.Display;
              }
              item.ReasonGroups.Add (new ReasonGroupDTOAssembler ().Assemble (groupReasons.Key, groupReasons));
            }
          }

          return response;
        }
      }
      catch (Exception ex) {
        log.Error ($"Get: exception", ex);
        throw;
      }
    }
  }
}
