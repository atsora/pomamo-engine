// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.AutoReason
{
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
        using (var transaction = session.BeginTransaction ("AutoReason.Plugin.ConfigRequests.AddTranslation")) {
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
    /// Add the translations of a translation key in locales other than the default one,
    /// if they do not exist yet. An existing translation is never overwritten
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="translations">translation value by locale (for example fr or de). Nullable</param>
    internal static void AddMissingTranslations (string translationKey, IDictionary<string, string> translations)
    {
      Debug.Assert (!string.IsNullOrEmpty (translationKey));

      var validTranslations = translations?
        .Where (t => !string.IsNullOrEmpty (t.Key) && !string.IsNullOrEmpty (t.Value))
        .ToList ();
      if (validTranslations is null || !validTranslations.Any ()) {
        return;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("AutoReason.Plugin.ConfigRequests.AddMissingTranslations")) {
          foreach (var t in validTranslations) {
            var translation = ModelDAOHelper.DAOFactory.TranslationDAO.Find (t.Key, translationKey);
            if (translation is null) {
              if (log.IsInfoEnabled) {
                log.Info ($"AddMissingTranslations: add the translation {t.Value} of {translationKey} for locale {t.Key}");
              }
              translation = ModelDAOHelper.ModelFactory.CreateTranslation (t.Key, translationKey);
              translation.TranslationValue = t.Value;
              ModelDAOHelper.DAOFactory.TranslationDAO.MakePersistent (translation);
            }
          }
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// Add a reason group if it does not exist yet
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="translationValue">not null</param>
    /// <returns></returns>
    internal static IReasonGroup AddReasonGroup (string translationKey, string translationValue)
    {
      Debug.Assert (!string.IsNullOrEmpty (translationKey));
      Debug.Assert (null != translationValue);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("AutoReason.Plugin.ConfigRequests.AddReasonGroup")) {
          var reasonGroup = ModelDAOHelper.DAOFactory.ReasonGroupDAO
        .FindAll ()
        .FirstOrDefault (g => string.Equals (translationKey, g.TranslationKey, StringComparison.InvariantCultureIgnoreCase));
          if (null == reasonGroup) {
            AddTranslation (translationKey, translationValue);

            reasonGroup = ModelDAOHelper.ModelFactory.CreateReasonGroup ();
            reasonGroup.TranslationKey = translationKey;
            ModelDAOHelper.DAOFactory.ReasonGroupDAO
              .MakePersistent (reasonGroup);
          }
          transaction.Commit ();
          return reasonGroup;
        }
      }
    }

    /// <summary>
    /// Add a reason if it does not exist already in the database
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="translationValue">not null if the reason must be created</param>
    /// <param name="translations">translations in other locales (for example fr or de), added if missing, even when the reason already exists. Nullable</param>
    /// <returns></returns>
    public static IReason AddReason (string translationKey, string translationValue, IDictionary<string, string> translations = null)
    {
      Debug.Assert (!string.IsNullOrEmpty (translationKey));

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("AutoReason.Plugin.ConfigRequests.AddReason")) {
          var reason = ModelDAOHelper.DAOFactory.ReasonDAO
        .FindAll ()
        .FirstOrDefault (r => string.Equals (translationKey, r.TranslationKey, StringComparison.InvariantCultureIgnoreCase));
          if (reason is null) {
            Debug.Assert (null != translationValue);
            var reasonGroup = ModelDAO.ModelDAOHelper.DAOFactory.ReasonGroupDAO.FindById ((int)ReasonGroupId.Auto);
            AddTranslation (translationKey, translationValue);
            reason = CreateReason (translationKey, reasonGroup);
          }
          AddMissingTranslations (translationKey, translations);
          transaction.Commit ();
          return reason;
        }
      }
    }

    /// <summary>
    /// Method to create the reason
    /// </summary>
    /// <param name="translationKey">not null or empty</param>
    /// <param name="reasonGroup">not null</param>
    /// <returns></returns>
    internal static IReason CreateReason (string translationKey, IReasonGroup reasonGroup)
    {
      Debug.Assert (!string.IsNullOrEmpty (translationKey));
      Debug.Assert (null != reasonGroup);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("AutoReason.Plugin.ConfigRequests.CreateReason")) {
          var reason = ModelDAOHelper.ModelFactory.CreateReason (reasonGroup);
          reason.TranslationKey = translationKey;
          ModelDAOHelper.DAOFactory.ReasonDAO
            .MakePersistent (reason);
          transaction.Commit ();
          return reason;
        }
      }
    }
  }
}
