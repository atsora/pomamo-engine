// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IProductionStateDAO">IProductionStateDAO</see>
  /// </summary>
  public class ProductionStateDAO
    : VersionableNHibernateDAO<ProductionState, IProductionState, int>
    , IProductionStateDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProductionStateDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      { // Production
        var productionState = new ProductionState ((int)ProductionStateId.Production, "ProductionStateProduction", "#008000"); // Green
        InsertDefaultValue (productionState);
      }
      { // NoProduction
        var productionState = new ProductionState ((int)ProductionStateId.NoProduction, "ProductionStateNoProduction", "#FFFF00"); // Yellow
        InsertDefaultValue (productionState);
      }
      {
        var productionState = new ProductionState ((int)ProductionStateId.UnplannedStop, "ProductionStateUnplanned", "#AA0909"); // Red
        InsertDefaultValue (productionState);
      }
      {
        var productionState = new ProductionState ((int)ProductionStateId.PlannedStop, "ProductionStatePlanned", "#FFFF00"); // Yellow
        InsertDefaultValue (productionState);
      }
      {
        var productionState = new ProductionState ((int)ProductionStateId.Unscheduled, "ProductionStateUnscheduled", "#F3F3F3"); // Very light grey
        InsertDefaultValue (productionState);
      }
      {
        var productionState = new ProductionState ((int)ProductionStateId.Setup, "ProductionStateSetup", "#3498DB"); // Blue
        InsertDefaultValue (productionState);
      }
      {
        var productionState = new ProductionState ((int)ProductionStateId.Unknown, "ProductionStateUnknown", "#808080"); // Grey
        InsertDefaultValue (productionState);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="productionState">not null</param>
    private void InsertDefaultValue (IProductionState productionState)
    {
      Debug.Assert (null != productionState);

      try {
        IProductionState existingProductionState = FindById (productionState.Id);
        if (null == existingProductionState) { // the config does not exist => create it
          log.InfoFormat ("InsertDefaultValue: " +
                          "add id={0} translationKey={1}",
                          productionState.Id, productionState.TranslationKey);
          // Use a raw SQL Command, else the Id is reset
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
            command.CommandText = $@"INSERT INTO ProductionState (productionstateid, productionstatetranslationkey, productionstatecolor)
VALUES ({productionState.Id}, '{productionState.TranslationKey}', '{productionState.Color}')";
            command.ExecuteNonQuery ();
          }
          ModelDAOHelper.DAOFactory.FlushData ();
        }
      }
      catch (Exception ex) {
        log.Error ($"InsertDefaultValue: inserting new pdocution state {productionState} failed", ex);
      }
    }
    #endregion // DefaultValues

    /// <summary>
    /// Get all the items sorted by their Id
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IProductionState> FindAllSortedById ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ProductionState> ()
        .AddOrder (Order.Asc ("Id"))
        .SetCacheable (true)
        .List<IProductionState> ();
    }
  }
}
