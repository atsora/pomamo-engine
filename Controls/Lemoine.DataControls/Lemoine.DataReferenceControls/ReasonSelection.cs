// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of ReasonSelection.
  /// </summary>
  public partial class ReasonSelection : GenericSelection<IReason>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSelection).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonSelection()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Prepare data for GenericSelection
    /// </summary>
    /// <returns></returns>
    public override IList<IReason> SelectionDataLoad(){
      IList<IReason> reasons;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      
      if(daoFactory == null) // Designer Guard
{
        return null;
      }

      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ReasonSelection.Load"))
      {
        reasons = daoFactory.ReasonDAO.FindAll()
          .OrderBy (x => x)
          .ToList ();
      }
      
      return reasons;
    }
    #endregion // Methods
  }
}
