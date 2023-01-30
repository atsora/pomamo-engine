// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Base ICncModule
  /// </summary>
  public abstract class BaseCncModule: IChecked // Note: no ICncModule to not make it a IDisposable class
  {
    #region Members
    readonly string m_name;
    int m_cncAcquisitionId;
    string m_cncAcquisitionName = "";
    IChecked m_dataHandler;
    #endregion // Members

    /// <summary>
    /// Logger
    /// </summary>
    protected ILog log = LogManager.GetLogger(typeof (BaseCncModule).FullName);

    #region Getters / Setters
    /// <summary>
    /// Cnc Acquisition ID
    /// </summary>
    public virtual int CncAcquisitionId {
      get { return m_cncAcquisitionId; }
      set
      {
        m_cncAcquisitionId = value;
        log = LogManager.GetLogger ($"{m_name}.{value}");
      }
    }
    
    /// <summary>
    /// Cnc Acquisition name
    /// </summary>
    public virtual string CncAcquisitionName {
      get { return m_cncAcquisitionName; }
      set { m_cncAcquisitionName = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Name of the module (for the log)
    /// </summary>
    /// <param name="name">Name of the module</param>
    protected BaseCncModule (string name)
    {
      m_name = name;
      log = LogManager.GetLogger (name);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Implements <see cref="ICncModule" />
    /// </summary>
    /// <param name="dataHandler"></param>
    public virtual void SetDataHandler (IChecked dataHandler)
    {
      m_dataHandler = dataHandler;
    }
    
    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public virtual void SetActive ()
    {
      if (null != m_dataHandler) {
        m_dataHandler.SetActive();
      }
    }

    /// <summary>
    /// Implements <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck()
    {
      if (null != m_dataHandler) {
        m_dataHandler.PauseCheck ();
      }
    }

    /// <summary>
    /// Implements <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck()
    {
      if (null != m_dataHandler) {
        m_dataHandler.ResumeCheck ();
      }
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"CNC module {this.GetType ().FullName}.{this.CncAcquisitionId} [{this.CncAcquisitionName ?? ""}]";
    }
    #endregion // Methods
  }
}
