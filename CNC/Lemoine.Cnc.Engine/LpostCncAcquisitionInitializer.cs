// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD || NET48 || NETCOREAPP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.DataRepository;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Class to get all the <see cref="ICncAcquisition"/> that must be loaded in the lpost
  /// </summary>
  public class LpostCncAcquisitionInitializer: ICncAcquisitionInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (LpostCncAcquisitionInitializer).FullName);

    static readonly string CNC_ACQUISITION_LIST_XML = "CncAcquisitionList.xml";

    readonly bool m_useCoreService;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public LpostCncAcquisitionInitializer (bool useCoreService)
    {
      m_useCoreService = useCoreService;
    }
    #endregion // Constructors

    /// <summary>
    /// Copy the distant cnc resources
    /// </summary>
    public void CopyDistantResources (CancellationToken cancellationToken)
    {
      // Directory for the copy of distant resources
      var resourceDir = new DirectoryInfo (Path.Combine (PulseInfo.LocalConfigurationDirectory, "cnc_resources"));
      if (!resourceDir.Exists) {
        resourceDir.Create ();
      }

      // Synchronization of the resources (from pfrdata -> LPOST)
      if (Lemoine.FileRepository.FileRepoClient.ForceSynchronize ("cnc_resources", resourceDir.FullName, cancellationToken: cancellationToken) ==
          Lemoine.FileRepository.SynchronizationStatus.SYNCHRONIZATION_FAILED) {
        log.Error ("CopyDistantResources: error in the synchronization of the cnc resources, but continue");
      }
    }

    /// <summary>
    /// Get the number of free licenses for CNC ouptut modules
    /// 
    /// Return 0 in case of error
    /// </summary>
    /// <returns></returns>
    public int GetFreeLicenses ()
    {
      try {
        var freeLicenses = CncLicenseManager.FreeLicenses;
        log.Info ($"GetFreeLicenses: {freeLicenses} free licenses for CNC output modules");
        return freeLicenses;
      }
      catch (Exception ex) {
        log.Error ("GetFreeLicenses: error while trying to get the free licenses, but continue", ex);
        return 0;
      }
    }

    /// <summary>
    /// Get the cnc acquisitions that are registered for the current computer
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IEnumerable<ICncAcquisition> GetRegisteredCncAcquisitions (CancellationToken cancellationToken)
    {
      // - Get the list of cncAcquisitions
      log.DebugFormat ("GetRegisteredCncAcquisitions: initialize the cnc acquisitions repository");
      string cncAcquisitionListPath = CNC_ACQUISITION_LIST_XML;
      string cncDirectory = Path.Combine (PulseInfo.LocalConfigurationDirectory, "Cnc");
      if (Directory.Exists (cncDirectory)) {
        cncAcquisitionListPath = Path.Combine (cncDirectory, CNC_ACQUISITION_LIST_XML);
      }
      else {
        cncAcquisitionListPath = Path.Combine (PulseInfo.LocalConfigurationDirectory, CNC_ACQUISITION_LIST_XML);
      }
      var cncAcquisitionsMainFactory = new ListFactory<Lemoine.GDBPersistentClasses.MachineModule> (
        new ListMaker<Lemoine.GDBPersistentClasses.MachineModule> (() => GetMachineModules (cancellationToken)));
      var cncAcquisitionsCopyFactory = new XMLFactory (XmlSourceType.URI, cncAcquisitionListPath);
      var cncAcquisitionsCopyBuilder = new XMLBuilder (cncAcquisitionListPath);
      var cncAcquisitionsRepository = new Repository (
        cncAcquisitionsMainFactory, cncAcquisitionsCopyBuilder, cncAcquisitionsCopyFactory);
      log.Debug ("GetRegisteredCncAcquisitions: get the cnc acquisitions");
      cncAcquisitionsRepository.ForceReadData (TimeSpan.FromSeconds (3), cancellationToken);

      // - Get all the Cnc Acquisition configs
      log.Debug ("GetRegisteredCncAcquisitions: parse the cnc acquisitions");
      ICollection<ICncAcquisition> cncAcquisitions = new HashSet<ICncAcquisition> ();
      foreach (XmlNode child in cncAcquisitionsRepository.Document.DocumentElement.ChildNodes) {
        cancellationToken.ThrowIfCancellationRequested ();
        if (!(child is XmlElement)) {
          continue;
        }
        try {
          XmlSerializer xmlSerializer = new XmlSerializer (typeof (Lemoine.GDBPersistentClasses.MachineModule));
          IMachineModule machineModule;
          using (TextReader reader = new StringReader (child.OuterXml)) {
            machineModule = (IMachineModule)xmlSerializer.Deserialize (reader);
          }
          cncAcquisitions.Add (machineModule.CncAcquisition);
        }
        catch (Exception ex) {
          log.Error ($"GetRegisteredCncAcquisitions: error while trying to deserialize {child.OuterXml}, skip it", ex);
        }
      }

      return cncAcquisitions.Where (a => a.UseCoreService == m_useCoreService);
    }

    IComputer GetLPost ()
    {
      IEnumerable<IComputer> lposts;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Lem_CncService.GetLPost")) {
          lposts =
            ModelDAOHelper.DAOFactory.ComputerDAO.GetLposts ();
        }
      }

      var lpost = lposts.FirstOrDefault (x => x.IsLocal ());
      if (null == lpost) {
        log.Error ("GetLPost: no lpost in database corresponds to this computer");
      }
      return lpost;
    }

    /// <summary>
    /// Get the list of the CNC acquisitions that corresponds to this LPost
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ICollection<Lemoine.GDBPersistentClasses.MachineModule> GetMachineModules (CancellationToken cancellationToken)
    {
      // Note: check a basic connection so that an exception is returned faster
      //       in case the database is down
      ModelDAOHelper.DAOFactory.CheckBasicConnection ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("CncService.GetMachineModules")) {
          var lpost = GetLPost ();
          if (null == lpost) {
            log.Error ("GetMachineModules: no lpost was found, return an empty list");
            return new List<Lemoine.GDBPersistentClasses.MachineModule> ();
          }

          // 3. Get all the CncAcquisition for this lpost
          cancellationToken.ThrowIfCancellationRequested ();
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetMachineModules: get the cnc acquisitions for lpost {0}", lpost.Id);
          }
          IList<ICncAcquisition> cncAcquisitions =
            ModelDAOHelper.DAOFactory.CncAcquisitionDAO
            .FindAllForComputer (lpost);
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetMachineModules: got {0} cnc acquisitions for lpost {1}", cncAcquisitions.Count, lpost.Id);
          }

          // 4. Get all the modules
          ICollection<Lemoine.GDBPersistentClasses.MachineModule> machineModules =
            new HashSet<Lemoine.GDBPersistentClasses.MachineModule> ();
          foreach (ICncAcquisition cncAcquisition in cncAcquisitions) {
            foreach (IMachineModule machineModule in cncAcquisition.MachineModules) {
              cancellationToken.ThrowIfCancellationRequested ();
              if (log.IsDebugEnabled) {
                log.DebugFormat ("GetMachineModules: add machine module of id {0}", machineModule.Id);
              }
              machineModules.Add ((Lemoine.GDBPersistentClasses.MachineModule)machineModule);
            }
          }

          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetMachineModules: got {0} machine modules", machineModules.Count);
          }
          return machineModules;
        }
      }
    }
  }
}

#endif // NETSTANDARD || NET48 || NETCOREAPP
