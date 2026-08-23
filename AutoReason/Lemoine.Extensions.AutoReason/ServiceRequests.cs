// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Requests on the service that is running the auto-reason extensions
  /// </summary>
  internal static class ServiceRequests
  {
    /// <summary>
    /// Get a reference to the service that is running the auto-reason extensions,
    /// creating it if it does not exist yet
    /// </summary>
    /// <param name="log">not null</param>
    /// <returns>not null</returns>
    public static IService GetService (ILog log)
    {
      Debug.Assert (null != log);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("AutoReason.GetService")) {
          var computer = ModelDAOHelper.DAOFactory.ComputerDAO
            .GetOrCreateLocal ();
          if (null == computer) {
            log.Error ("GetService: no local computer known or detected");
            Debug.Assert (null != computer, "Computer is null");
            transaction.Commit ();
            throw new InvalidProgramException ("computer null");
          }
          var program = Lemoine.Info.ProgramInfo.Name;
          if (null == program) {
            log.Error ("GetService: unknown program");
            Debug.Assert (null != program, "Program is null");
            transaction.Commit ();
            throw new InvalidProgramException ("program null");
          }
          var services = ModelDAOHelper.DAOFactory.ServiceDAO
            .FindAll ()
            .Where (s => s.Lemoine && (computer.Id == s.Computer.Id) && program.Equals (s.Program));
          IService service;
          if (services.Any ()) {
            if (1 < services.Count ()) {
              log.Error ("GetService: more than one service matches");
            }
            service = services.First ();
          }
          else {
            service = ModelDAOHelper.ModelFactory.CreateService (computer, "Lemoine AutoReason", program, true);
            ModelDAOHelper.DAOFactory.ServiceDAO.MakePersistent (service);
          }
          transaction.Commit ();
          return service;
        }
      }
    }

    /// <summary>
    /// Create a new revision for the specified service
    /// </summary>
    /// <param name="service">not null</param>
    /// <returns>not null</returns>
    public static IRevision CreateRevision (IService service)
    {
      Debug.Assert (null != service);

      var revision = ModelDAOHelper.ModelFactory
        .CreateRevision ();
      revision.Updater = service;
      revision.IPAddress = Lemoine.Info.ComputerInfo.GetIPAddresses ()
        .First ();
      revision.Application = Lemoine.Info.ProgramInfo.Name;
      ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);
      return revision;
    }
  }
}
