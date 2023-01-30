// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.WebClient;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of MachineModificationDAO.
  /// </summary>
  public class MachineModificationDAO: Lemoine.ModelDAO.IMachineModificationDAO
  {
    #region IMachineModificationDAO implementation
    public long CreateNewAnalysisStatus (IMachine machine, bool serializable, long minModificationId, long limit, out bool limitReached)
    {
      throw new NotImplementedException ();
    }
    public long CreateNewAnalysisStatusNoLimit (IMachine machine, bool serializable, long minModificationId)
    {
      throw new NotImplementedException ();
    }
    public void UpgradeLock(Lemoine.Model.IMachineModification entity)
    {
      throw new NotImplementedException();
    }
    public double GetNumberOfRemainingModifications(Lemoine.Model.IMachineModification machineModification, bool createNewAnalysisStatusBefore = true)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IMachineModification GetFirstPendingModification(Lemoine.Model.IMachine machine)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IMachineModification GetFirstPendingModification(Lemoine.Model.IMachine machine, long lastModificationId, int lastPriority, int minPriority)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IMachineModification GetFirstPendingGlobalMachineModification(Lemoine.Model.IMachine machine, long lastModificationId, int lastPriority, int minPriority)
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IEnumerable<Lemoine.Model.IModification> GetNotCompletedSubModifications(Lemoine.Model.IMachineModification modification)
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IEnumerable<Lemoine.Model.IGlobalModification> GetNotCompletedSubGlobalModifications(Lemoine.Model.IMachineModification modification)
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IEnumerable<Lemoine.Model.IMachineModification> GetNotCompletedSubMachineModifications(Lemoine.Model.IMachineModification modification)
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IEnumerable<Lemoine.Model.IMachineModification> GetNotCompletedSubMachineModifications(Lemoine.Model.IMachine machine, Lemoine.Model.IMachineModification modification)
    {
      throw new NotImplementedException();
    }
    public bool HasNotCompletedSubMachineModifications(Lemoine.Model.IMachineModification modification)
    {
      throw new NotImplementedException();
    }

    public bool HasNotCompletedSubMachineModifications(IMachineModification modification, IMachine machine, bool createNewAnalysisStatus)
    {
      throw new NotImplementedException();
    }

    public bool HasNotCompletedGlobalModifications(Lemoine.Model.IMachineModification modification)
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IList<Lemoine.Model.IMachineModification> GetInErrorStrictlyAfter(int completionOrder)
    {
      throw new NotImplementedException();
    }
    public int GetNumber(Lemoine.Model.AnalysisStatus analysisStatus)
    {
      throw new NotImplementedException();
    }
    public long? GetMaxModificationId (IMachine machine)
    {
      throw new NotImplementedException ();
    }
    public Task<long?> GetMaxModificationIdAsync (IMachine machine)
    {
      throw new NotImplementedException ();
    }
    public IEnumerable<IMachineModification> FindNotCompletedWithRevision(string application, long minId)
    {
      throw new NotImplementedException();
    }
    public IList<IMachineModification> FindByRevision(IRevision revision, long minId)
    {
      throw new NotImplementedException();
    }
    public bool Delete (IMachine machine, AnalysisStatus analysisStatus, IChecked checkedThread, int itemNumberByStep, int maxNumberOfModifications, DateTime? maxRunningDateTime)
    {
      throw new NotImplementedException ();
    }
    public bool Delete (IMachine machine, AnalysisStatus analysisStatus, DateTime maxCompletionDateTime, IChecked checkedThread, int itemNumberByStep, int maxNumberOfModifications, DateTime? maxRunningDateTime)
    {
      throw new NotImplementedException ();
    }
    #endregion
    #region IGenericByMachineDAO implementation
    public Lemoine.Model.IMachineModification FindById(long id, Lemoine.Model.IMachine machine)
    {
      Debug.Assert (null != machine);
      return WebServiceHelper.UniqueResult<IMachineModification, MachineModification> (new RequestUrl ("/Data/MachineModification/FindById/" + id + "/" + machine.Id));
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IMachineModification> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IMachineModification MakePersistent(Lemoine.Model.IMachineModification entity)
    {
      throw new NotImplementedException();
    }
    public void MakeTransient(Lemoine.Model.IMachineModification entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IMachineModification entity)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<IMachineModification> GetPastPendingModifications (IMachine machine, long lastModificationId, int lastPriority, DateTime before, int maxResults, int minPriority)
    {
      throw new NotImplementedException ();
    }

    public Task<IList<IMachineModification>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IMachineModification> MakePersistentAsync (IMachineModification entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IMachineModification entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IMachineModification entity)
    {
      throw new NotImplementedException ();
    }

    public Task<long?> GetMaxModificationIdAsync (IDAOSession session, IMachine machine)
    {
      throw new NotImplementedException ();
    }

    #endregion
  }
}
