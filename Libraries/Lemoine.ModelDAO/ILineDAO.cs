// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ILine.
  /// </summary>
  public interface ILineDAO: IGenericDAO<ILine, int>
  {
    /// <summary>
    /// Find all the lines that may match a component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<ILine> FindAllByComponent (IComponent component);
  }
}
