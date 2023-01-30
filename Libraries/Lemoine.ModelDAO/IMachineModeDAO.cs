// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineMode.
  /// </summary>
  public interface IMachineModeDAO: IGenericUpdateDAO<IMachineMode, int>
  {
    /// <summary>
    /// Find a MachineMode from its translation key or name
    /// </summary>
    /// <param name="translationKeyOrName"></param>
    /// <returns></returns>
    IMachineMode FindByTranslationKeyOrName (string translationKeyOrName);
    
    /// <summary>
    /// Find all auto-sequence machine modes
    /// </summary>
    /// <returns></returns>
    IList<IMachineMode> FindAutoSequence();

    /// <summary>
    /// Find all running machine modes
    /// </summary>
    /// <returns></returns>
    IList<IMachineMode> FindRunning();
  }
}
