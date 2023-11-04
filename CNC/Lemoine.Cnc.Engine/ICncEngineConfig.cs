// Copyright (c) 2023 Nicolas Relange

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Configuration of Cnc Engine
  /// </summary>
  public interface ICncEngineConfig
  {
    /// <summary>
    /// Name of the console program to use if the 'useProcess' option is on
    /// 
    /// For example Lem_CncConsole
    /// </summary>
    string ConsoleProgramName { get; }

#if !NET40
    /// <summary>
    /// Name of the cache file for the cnc acquisition list repository
    /// 
    /// If null, consider the default value: CncAcquisitionList.xml
    /// </summary>
    string RepositoryCacheFileName { get; }

    /// <summary>
    /// Filter a cnc acquisition
    /// 
    /// Keep only the cnc acquisitions that return true
    /// </summary>
    /// <param name="cncAcquisition"></param>
    /// <returns></returns>
    bool FilterCncAcquisition (ICncAcquisition cncAcquisition);
#endif // !NET40
  }
}
