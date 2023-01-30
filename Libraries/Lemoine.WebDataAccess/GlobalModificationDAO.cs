// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.WebClient;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of GlobalModificationDAO.
  /// </summary>
  public class GlobalModificationDAO: Lemoine.ModelDAO.IGlobalModificationDAO
  {
    #region IGlobalModificationDAO implementation
    public void UpgradeLock(IGlobalModification entity)
    {
      throw new NotImplementedException();
    }
    public double GetNumberOfRemainingModifications(IGlobalModification globalModification, bool createNewAnalysisStatusBefore = true)
    {
      throw new NotImplementedException();
    }
    public IGlobalModification GetFirstPendingModification()
    {
      throw new NotImplementedException();
    }
    public IGlobalModification GetFirstPendingModification(long lastModificationId, int lastPriority, int minPriority)
    {
      throw new NotImplementedException();
    }
    public IGlobalModification GetFirstPendingGlobalMachineModification(long lastModificationId, int lastPriority, int minPriority)
    {
      throw new NotImplementedException();
    }
    public IEnumerable<IModification> GetNotCompletedSubModifications(IGlobalModification modification)
    {
      throw new NotImplementedException();
    }
    public IEnumerable<IGlobalModification> GetNotCompletedSubGlobalModifications(IGlobalModification modification)
    {
      throw new NotImplementedException();
    }
    public IEnumerable<IMachineModification> GetNotCompletedSubMachineModifications(IGlobalModification modification)
    {
      throw new NotImplementedException();
    }
    public bool HasNotCompletedSubMachineModifications(IGlobalModification modification)
    {
      throw new NotImplementedException();
    }
    public bool HasNotCompletedSubMachineModifications(IGlobalModification modification, IMachine machine, bool createNewAnalysisStatus)
    {
      throw new NotImplementedException();
    }
    public void Delete(AnalysisStatus analysisStatus)
    {
      throw new NotImplementedException();
    }
    public void Delete(AnalysisStatus analysisStatus, DateTime maxCompletionDateTime)
    {
      throw new NotImplementedException();
    }
    public IList<IGlobalModification> GetInErrorStrictlyAfter(int completionOrder)
    {
      throw new NotImplementedException();
    }
    public int GetNumber(AnalysisStatus analysisStatus)
    {
      throw new NotImplementedException();
    }
    public long? GetMaxModificationId ()
    {
      throw new NotImplementedException ();
    }
    public Task<long?> GetMaxModificationIdAsync ()
    {
      throw new NotImplementedException ();
    }
    public IEnumerable<IGlobalModification> FindNotCompletedWithRevision(string application, long minId)
    {
      throw new NotImplementedException();
    }
    public IList<IGlobalModification> FindByRevision(IRevision revision, long minId)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericDAO implementation
    public IGlobalModification FindById(long id)
    {
      return WebServiceHelper.UniqueResult<IGlobalModification, GlobalModification> (new RequestUrl ("/Data/GlobalModification/FindById/" + id));
    }
    #endregion
    #region IBaseGenericDAO implementation
    public IList<IGlobalModification> FindAll()
    {
      throw new NotImplementedException();
    }
    public IGlobalModification MakePersistent(IGlobalModification entity)
    {
      throw new NotImplementedException();
    }
    public void MakeTransient(IGlobalModification entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(IGlobalModification entity)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<IGlobalModification> GetPastPendingModifications (long lastModificationId, int lastPriority, DateTime before, int maxResults, int minPriority)
    {
      throw new NotImplementedException ();
    }

    public Task<IList<IGlobalModification>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IGlobalModification> MakePersistentAsync (IGlobalModification entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IGlobalModification entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IGlobalModification entity)
    {
      throw new NotImplementedException ();
    }

    public Task<long?> GetMaxModificationIdAsync (IDAOSession session)
    {
      throw new NotImplementedException ();
    }

    public async Task<IGlobalModification> FindByIdAsync (long id)
    {
      return await WebServiceHelper.UniqueResultAsync<IGlobalModification, GlobalModification> (new RequestUrl ("/Data/GlobalModification/FindById/" + id));
    }
    #endregion
  }
}
