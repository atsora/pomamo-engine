// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IComputer.
  /// </summary>
  public interface IComputerDAO: IGenericUpdateDAO<IComputer, int>
  {
    /// <summary>
    /// Get the Lctr computer
    /// </summary>
    IComputer GetLctr ();

    /// <summary>
    /// Get the Lctr computer asynchronously
    /// </summary>
    System.Threading.Tasks.Task<IComputer> GetLctrAsync ();

    /// <summary>
    /// Try to get the computer that matches the local computer in database
    /// </summary>    
    /// <returns>null if not found or if it could not be determined</returns>
    IComputer GetLocal ();
    
    /// <summary>
    /// Try to get the computer that matches the local computer in database.
    /// 
    /// If the local computer does not exist in the database, create it
    /// </summary>
    /// <returns></returns>
    IComputer GetOrCreateLocal ();
    
    /// <summary>
    ///  Get the Lposts computers
    /// </summary>    
    IList<IComputer> GetLposts ();

    /// <summary>
    ///  Get the Lposts computers asynchronously
    /// </summary>    
    System.Threading.Tasks.Task<IList<IComputer>> GetLpostsAsync ();

    /// <summary>
    ///  Get the cnc computers
    /// </summary>
    IList<IComputer> GetCnc ();

    /// <summary>
    ///  Get the cnc computers asynchronously
    /// </summary>
    System.Threading.Tasks.Task<IList<IComputer>> GetCncAsync ();

    /// <summary>
    ///  Get the web computers
    /// </summary>
    IList<IComputer> GetWeb ();

    /// <summary>
    ///  Get the web computers asynchronously
    /// </summary>
    System.Threading.Tasks.Task<IList<IComputer>> GetWebAsync ();

    /// <summary>
    /// Get the auto-reason server
    /// </summary>
    /// <returns></returns>
    IComputer GetAutoReason ();

    /// <summary>
    /// Get the auto-reason server asynchronously
    /// </summary>
    /// <returns></returns>
    System.Threading.Tasks.Task<IComputer> GetAutoReasonAsync ();
    /// <summary>
    /// Get the alert server
    /// </summary>
    /// <returns></returns>
    IComputer GetAlert ();


    /// <summary>
    /// Get the alert server asynchronously
    /// </summary>
    /// <returns></returns>
    System.Threading.Tasks.Task<IComputer> GetAlertAsync ();

    /// <summary>
    /// Get the synchronization server
    /// </summary>
    /// <returns></returns>
    IComputer GetSynchronization ();

    /// <summary>
    /// Get the synchronization server asynchronously
    /// </summary>
    /// <returns></returns>
    System.Threading.Tasks.Task<IComputer> GetSynchronizationAsync ();
  }
}
