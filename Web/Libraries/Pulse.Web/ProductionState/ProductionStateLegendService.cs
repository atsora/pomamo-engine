// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Web;

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Description of ProductionStateLegendService
  /// </summary>
  public class ProductionStateLegendService
    : GenericCachedService<ProductionStateLegendRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProductionStateLegendService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ProductionStateLegendService ()
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
    public override async Task<object> Get (ProductionStateLegendRequestDTO request)
    {
      ProductionStateLegendResponseDTO response = new ProductionStateLegendResponseDTO ();
      response.Items = new List<ProductionStateLegendItemDTO> ();
      IEnumerable<IProductionState> productionStates;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyDeferrableTransaction ("Web.ProductionState.Legend")) {
          productionStates = await ModelDAOHelper.DAOFactory.ProductionStateDAO
            .FindAllAsync ();
          productionStates = productionStates
            .Distinct ()
            .OrderBy (x => x);
        }
      }

      foreach (var productionState in productionStates) {
        ProductionStateLegendItemDTO item = response.Items
          .SingleOrDefault (element => string.Equals (productionState.Color, element.Color,
                                                      StringComparison.InvariantCultureIgnoreCase));
        string overridenText = Lemoine.Info.ConfigSet
          .LoadAndGet<string> ("Legend.ProductionState." + productionState.Color,
                               "");
        if (null == item) {
          item = new ProductionStateLegendItemDTO ();
          item.Color = productionState.Color;
          if (!string.IsNullOrEmpty (overridenText)) {
            item.Display = overridenText;
          }
          else {
            item.Display = productionState.Display;
          }
          item.ProductionStates = new List<ProductionStateDTO> ();
          item.ProductionStates.Add (new ProductionStateDTO (productionState));
          response.Items.Add (item);
        }
        else { // null != item
          if (string.IsNullOrEmpty (overridenText)) {
            item.Display += ", " + productionState.Display;
          }
          item.ProductionStates.Add (new ProductionStateDTO (productionState));
        }
      }

      return response;
    }
    #endregion // Methods
  }
}
