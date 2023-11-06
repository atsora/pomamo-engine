// Copyright (c) 2023 Atsora Solutions

#if NETSTANDARD || NET48 || NETCOREAPP

using Lemoine.CncEngine;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Implementation of <see cref="ICncAcquisitionInitializer"/> returning all the <see cref="Lemoine.Model.ICncAcquisition"/> items in database
  /// </summary>
  public class CncAcquisitionInitializerAll: ICncAcquisitionInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncAcquisitionInitializerAll).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncAcquisitionInitializerAll ()
    {
    }

    /// <summary>
    /// <see cref="ICncAcquisitionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void CopyDistantResources (CancellationToken cancellationToken)
    {
      return;
    }

    /// <summary>
    /// <see cref="ICncAcquisitionInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IEnumerable<Lemoine.Model.ICncAcquisition> GetRegisteredCncAcquisitions (CancellationToken cancellationToken)
    {
      using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        return Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.CncAcquisitionDAO
          .FindAll ();
      }
    }
  }
}

#endif // NETSTANDARD || NET48 || NETCOREAPP
