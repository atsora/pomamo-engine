// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IDayTemplate.
  /// </summary>
  public interface IDayTemplateDAO: IGenericUpdateDAO<IDayTemplate, int>
  {
    /// <summary>
    /// List all IDayTemplate with Eager Mode
    /// </summary>
    /// <returns></returns>
    IList<IDayTemplate> FindAllForConfig();
  }
}
