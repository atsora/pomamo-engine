// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IUserShiftAssociationDAO">IUserShiftAssociationDAO</see>
  /// </summary>
  public class UserShiftAssociationDAO
    : SaveOnlyNHibernateDAO<UserShiftAssociation, IUserShiftAssociation, long>
    , IUserShiftAssociationDAO
  {
  }
}
