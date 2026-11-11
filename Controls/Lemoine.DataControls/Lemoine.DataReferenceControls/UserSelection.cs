// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of UserSelection.
  /// </summary>
  public class UserSelection : GenericSelection<IUser>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UserSelection).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UserSelection ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Prepare data for GenericSelection
    /// </summary>
    /// <returns></returns>
    public override IList<IUser> SelectionDataLoad(){
      IList<IUser> users;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if(daoFactory == null) // Designer Guard
{
        return null;
      }

      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("UserSelection.Load"))
      {
        users = daoFactory.UserDAO.FindAll();
      }
      
      return users;
    }
  }
}
