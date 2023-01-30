// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.WebClient;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Lemoine.ModelDAO;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of ModificationDAO.
  /// </summary>
  public class ModificationDAO: Lemoine.ModelDAO.IModificationDAO
  {
    readonly ILog log = LogManager.GetLogger(typeof (ModificationDAO).FullName);

    #region IModificationDAO implementation
    public void Lock(IModification entity)
    {
      throw new NotImplementedException();
    }
    public void UpgradeLock(IModification entity)
    {
      throw new NotImplementedException();
    }
    public void MakePersistent(IModification modification)
    {
      throw new NotImplementedException();
    }
    public IEnumerable<IModification> FindAll()
    {
      throw new NotImplementedException();
    }
    public IModification FindById(long id)
    {
      throw new NotImplementedException();
    }
    public double GetNumberOfRemainingModifications(long modificationId, bool createNewAnalysisStatusBefore = true)
    {
      var requestUrl = new RequestUrl ("/Modification/Pending/?ModificationId=" + modificationId);
      string url = requestUrl
        .AddBase (WebServiceHelper.Url)
        .Add ("format", "json")
        .ToString ();
      WebRequest request = WebRequest
        .Create (url);
      string json;
      using (WebResponse response = request.GetResponse ())
      {
        Stream stream = response.GetResponseStream ();
        using (var streamReader = new StreamReader (stream)) {
          json = streamReader.ReadToEnd ();
        }
      }
      
      if (json.StartsWith ("{\"ErrorMessage\"", StringComparison.InvariantCulture)) {
        log.ErrorFormat ("GetNumberOfRemainingModifications: " +
                         "GetNumberOfRemainingModifications request failed with {0}",
                         json);
        throw new Exception ("GetNumberOfRemainingModifications error");
      }
      
      var result = JsonConvert.DeserializeObject<PendingModifications> (json);
      
      return result.Number;
    }
    public double GetCompletion(IModification modification)
    {
      throw new NotImplementedException();
    }
    public long GetNextCompletionOrder()
    {
      throw new NotImplementedException();
    }
    public int GetNumber(AnalysisStatus analysisStatus)
    {
      throw new NotImplementedException();
    }
    public IEnumerable<IModification> FindNotCompletedWithRevision(string application, long minId)
    {
      throw new NotImplementedException();
    }
    public IEnumerable<IModification> FindByRevision(IRevision revision, long minId)
    {
      throw new NotImplementedException();
    }
    public long? GetMaxModificationId()
    {
      throw new NotImplementedException();
    }
    public Task<long?> GetMaxModificationIdAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<long?> GetMaxModificationIdAsync (IDAOSession session)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
