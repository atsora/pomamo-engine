// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  ///  DAO interface for IComponentMachineAssociation.
  /// </summary>
  public interface IComponentMachineAssociationDAO 
    : IGenericByMachineDAO<IComponentMachineAssociation, long>
  {
    /// <summary>
    /// Find all the component machine associations for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<IComponentMachineAssociation> FindAllWithComponent (IComponent component);
  }
}
