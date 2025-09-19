// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.WebClient;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// ConfigDAO
  /// </summary>
  public class ConfigDAO: IConfigDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigDAO ()
    {
    }

    #region IConfigDAO implementation

    public void ConfigureSmtpClient (SmtpClient smtpClient)
    {
      throw new NotImplementedException ();
    }
    public virtual bool IsAttachedToSession (Lemoine.Model.IConfig persistent) => true;

    public IList<IConfig> FindAll ()
    {
      return WebServiceHelper.List<IConfig, Config> (new RequestUrl ("/Data/Config/FindAll"));
    }

    public async Task<IList<IConfig>> FindAllAsync ()
    {
      return await WebServiceHelper.ListAsync<IConfig, Config> (new RequestUrl ("/Data/Config/FindAll"));
    }

    public IConfig FindById (int id)
    {
      throw new NotImplementedException ();
    }

    public IConfig FindByIdAndLock (int id)
    {
      throw new NotImplementedException ();
    }

    public Task<IConfig> FindByIdAsync (int id)
    {
      throw new NotImplementedException ();
    }

    public IList<IConfig> FindLike (string filter)
    {
      throw new NotImplementedException ();
    }

    public IConfig GetAnalysisConfig (AnalysisConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public object GetAnalysisConfigValue (AnalysisConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public IConfig GetCalendarConfig (CalendarConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public IConfig GetCncConfig (CncConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public object GetCncConfigValue (CncConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public IConfig GetConfig (string key)
    {
      throw new NotImplementedException ();
    }

    public IConfig GetDataStructureConfig (DataStructureConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public object GetDataStructureConfigValue (DataStructureConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public IConfig GetDbmConfig (DbmConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public object GetDbmConfigValue (DbmConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public IConfig GetOperationExplorerConfig (OperationExplorerConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public object GetOperationExplorerConfigValue (OperationExplorerConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public IConfig GetWebServiceConfig (WebServiceConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public object GetWebServiceConfigValue (WebServiceConfigKey key)
    {
      throw new NotImplementedException ();
    }

    public void Lock (IConfig entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IConfig entity)
    {
      throw new NotImplementedException ();
    }

    public IConfig MakePersistent (IConfig entity)
    {
      throw new NotImplementedException ();
    }

    public Task<IConfig> MakePersistentAsync (IConfig entity)
    {
      throw new NotImplementedException ();
    }

    public void MakeTransient (IConfig entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IConfig entity)
    {
      throw new NotImplementedException ();
    }

    public IConfig Reload (IConfig entity)
    {
      throw new NotImplementedException ();
    }

    public void SetConfig (string key, object v, bool activated)
    {
      throw new NotImplementedException ();
    }

    public void UpgradeLock (IConfig entity)
    {
      throw new NotImplementedException ();
    }
    #endregion // IConfigDAO implementation

  }
}
