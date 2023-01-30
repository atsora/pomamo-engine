// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.Modification
{
  /// <summary>
  /// Description
  /// </summary>
  public class PendingModificationsService
    : GenericCachedService<PendingModificationsRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PendingModificationsService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public PendingModificationsService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.NoCache)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(PendingModificationsRequestDTO request)
    {
      int revisionId = request.RevisionId;
      if (0 == revisionId) { // Alternative revision id
        revisionId = request.Id;
      }
      
      if ( (0 == revisionId) && (0 == request.ModificationId)) { // Error
        log.ErrorFormat ("Get: " +
                         "no revision or modification ID set");
        return new ErrorDTO ("No revision or modification id set",
                             ErrorStatus.WrongRequestParameter);
      }
      
      if ( (0 < revisionId) && (0 < request.ModificationId)) {
        log.ErrorFormat ("Get: " +
                         "both modification and revision IDs are set");
        return new ErrorDTO ("Both modification and revision IDs are set",
                             ErrorStatus.WrongRequestParameter);
      }
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        long maxId = 0;
        
        if (0 < revisionId) {
          IRevision revision = ModelDAOHelper.DAOFactory.RevisionDAO.FindById(revisionId);
          if (null == revision) {
            log.ErrorFormat ("Get: " +
                             "Revision with {0} does not exist",
                             revisionId);
            return new ErrorDTO ("No revision with id " + revisionId,
                                 ErrorStatus.WrongRequestParameter);
          }
          else { // null != revision
            // To simplify a little bit the process,
            // it is supposed all the modifications of the revision have the same date/time and priority
            // which is normally the case
            IList<IModification> modifications = revision.Modifications.ToList();
            foreach(IModification modification in modifications) {
              if (maxId < ((Lemoine.Collections.IDataWithId<long>)modification).Id) {
                maxId = ((Lemoine.Collections.IDataWithId<long>)modification).Id;
              }
            }
          }
        }
        else { // 0 < modificationId
          Debug.Assert (0 < request.ModificationId);
          maxId = request.ModificationId;
        }
        
        if (maxId == 0) {
          return 0;
        }
        
        var response = new PendingModificationsResponseDTO ();
        response.Number = GetNumberOfRemainingModifications (maxId);
        response.RevisionId = revisionId;
        return response;
      }
    }

    void CreateNewAnalysisStatus (IMachine machine)
    {
      // Note: see Story #171923435
      try {
        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .CreateNewAnalysisStatusNoLimit (machine, false, 0);
      }
      catch (Exception ex) {
        if (ExceptionTest.IsIntegrityConstraintViolation (ex, log)) {
          if (log.IsInfoEnabled) {
            log.Info ($"CreateNewAnalysisStatus: failed with integrity constraint violation for machine {machine.Id}, continue", ex);
          }
        }
        else {
          log.Error ($"CreateNewAnalysisStatus: failed for machine {machine.Id}", ex);
        }
      }
    }

    double GetNumberOfRemainingModifications (long modificationId)
    {
      IGlobalModification globalModification = ModelDAOHelper.DAOFactory.GlobalModificationDAO
        .FindById (modificationId);
      if (null != globalModification) {
        return ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .GetNumberOfRemainingModifications (globalModification, false);
      }

      IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll ();
      foreach (IMachine machine in machines) {
        IMachineModification machineModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (modificationId, machine);
        if (null != machineModification) {
          CreateNewAnalysisStatus (machine);

          return ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetNumberOfRemainingModifications (machineModification, false);
        }
      }

      log.Fatal ($"GetNumberOfRemainingModifications: no modification with Id {modificationId}");
      throw new ArgumentException ("Invalid modification Id", "modificationId");
    }
    #endregion // Methods
  }
}
