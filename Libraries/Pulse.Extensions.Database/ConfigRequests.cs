// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// ConfigRequests
  /// </summary>
  public static class ConfigRequests
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConfigRequests).FullName);

    /// <summary>
    /// Add a translation if it does not exist yet
    /// </summary>
    /// <param name="translationKey"></param>
    /// <param name="translationValue"></param>
    /// <returns></returns>
    internal static ITranslation AddTranslation (string translationKey, string translationValue)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Database.Plugin.ConfigRequests.AddTranslation")) {
          var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find ("", translationKey);
          if (null == translation) {
            translation = ModelDAOHelper.ModelFactory.CreateTranslation ("", translationKey);
            translation.TranslationValue = translationValue;
            ModelDAOHelper.DAOFactory.TranslationDAO.MakePersistent (translation);
          }
          transaction.Commit ();
          return translation;
        }
      }
    }

    /// <summary>
    /// Add a production state if it does not exist already in the database
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="translationValue">not null if the reason must be created</param>
    /// <param name="color">not null if the reason must be created</param>
    /// <returns></returns>
    public static IProductionState AddProductionState (string translationKey, string translationValue, string color)
    {
      Debug.Assert (!string.IsNullOrEmpty (translationKey));

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Database.Plugin.ConfigRequests.AddProductionState")) {
          var productionState = ModelDAOHelper.DAOFactory.ProductionStateDAO
            .FindAll ()
            .FirstOrDefault (r => string.Equals (translationKey, r.TranslationKey, StringComparison.InvariantCultureIgnoreCase));
          if (null == productionState) {
            Debug.Assert (null != translationValue);
            Debug.Assert (null != color);
            AddTranslation (translationKey, translationValue);
            productionState = CreateProductionState (translationKey, color);
          }
          transaction.Commit ();
          return productionState;
        }
      }
    }

    /// <summary>
    /// Method to create the production state
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="color">not null or empty</param>
    /// <returns></returns>
    internal static IProductionState CreateProductionState (string translationKey, string color)
    {
      Debug.Assert (!string.IsNullOrEmpty (translationKey));

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Database.Plugin.ConfigRequests.CreateProductionState")) {
          var reason = ModelDAOHelper.ModelFactory.CreateProductionState (color);
          reason.TranslationKey = translationKey;
          ModelDAOHelper.DAOFactory.ProductionStateDAO
            .MakePersistent (reason);
          transaction.Commit ();
          return reason;
        }
      }
    }
  }
}
