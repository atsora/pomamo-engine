// Copyright (c) 2023 Nicolas Relange

#if !NET40

using Lemoine.Core.Log;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Implementation of ICncEngineConfig
  /// </summary>
  public class CncEngineConfig : ICncEngineConfig
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncEngineConfig).FullName);

    readonly bool m_useCoreAcquisition;
    readonly bool m_gpl;

    /// <summary>
    /// <see cref="ICncEngineConfig"/>
    /// </summary>
    public string ConsoleProgramName { get; set; }

    /// <summary>
    /// <see cref="ICncEngineConfig"/>
    /// </summary>
    public string RepositoryCacheFileName { get; set; } = null;

    /// <summary>
    /// <see cref="ICncEngineConfig"/>
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public virtual bool FilterCncAcquisition (ICncAcquisition cncAcquisition) => (cncAcquisition.UseCoreService == m_useCoreAcquisition) && IsLicenseOk (cncAcquisition.License);

    bool IsLicenseOk (CncModuleLicense license) => !(m_gpl ^ license.HasFlag (CncModuleLicense.Gpl));

    /// <summary>
    /// Constructor
    /// </summary>
    public CncEngineConfig (bool useCoreAcquisition, bool gpl)
    {
      m_useCoreAcquisition = useCoreAcquisition;
      m_gpl = gpl;
    }
  }
}
#endif // !NET40