// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.WebClient;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using System.Collections.Generic;
using Lemoine.ModelDAO;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of TranslationDAO.
  /// </summary>
  public class TranslationDAO: Lemoine.ModelDAO.ITranslationDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (TranslationDAO).ToString ());
    
    #region ITranslationDAO implementation
    public Lemoine.Model.ITranslation Find(string locale, string translationKey)
    {
      var baseUrl = string.Format ("/Data/Translation/Find/?Locale={0}&Key={1}", locale, translationKey); // Note: /Data/Translation/Find//Key is not valid (Locale is empty)
      return WebServiceHelper.UniqueResult<ITranslation, Translation> (new RequestUrl (baseUrl));
    }
    public System.Collections.Generic.IList<string> GetDistinctTranslationKeys()
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IList<Lemoine.Model.ITranslation> GetTranslationFromKeyAndLocales(string key, System.Collections.Generic.List<string> locales)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericUpdateDAO implementation
    public Lemoine.Model.ITranslation FindByIdAndLock(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericDAO implementation
    public Lemoine.Model.ITranslation FindById(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericUpdateDAO implementation
    public void UpgradeLock(Lemoine.Model.ITranslation entity)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.ITranslation Reload(Lemoine.Model.ITranslation entity)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public virtual bool IsAttachedToSession (Lemoine.Model.ITranslation persistent) => true;
    public System.Collections.Generic.IList<Lemoine.Model.ITranslation> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.ITranslation MakePersistent(Lemoine.Model.ITranslation entity)
    {
      // Do nothing for the moment
      log.ErrorFormat ("MakePersistent: " +
                       "the translation data is not stored in database");
      return entity;
    }
    public void MakeTransient(Lemoine.Model.ITranslation entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.ITranslation entity)
    {
      throw new NotImplementedException();
    }

    public Task<IList<ITranslation>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<ITranslation> MakePersistentAsync (ITranslation entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (ITranslation entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (ITranslation entity)
    {
      throw new NotImplementedException ();
    }

    public Task<ITranslation> FindByIdAsync (int id)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
