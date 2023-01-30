// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Threading.Tasks;

namespace Lemoine.Business.MachineStateTemplate
{
  /// <summary>
  /// Request class to get ...
  /// </summary>
  public sealed class CurrentMachineStateTemplate
    : IRequest<CurrentMachineStateTemplateResponse>
  {
    #region Members
    readonly IMachine m_machine;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CurrentMachineStateTemplate).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public CurrentMachineStateTemplate (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public CurrentMachineStateTemplateResponse Get ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineStateTemplate.Current")) {
          IMachineStateTemplateSlot currentMachineStateTemplateSlot = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
            .FindAt (m_machine, DateTime.UtcNow);
          Debug.Assert (null != currentMachineStateTemplateSlot);
          return new CurrentMachineStateTemplateResponse (currentMachineStateTemplateSlot);
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<CurrentMachineStateTemplateResponse> GetAsync ()
    {
      // TODO: make it asynchronous
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.MachineStateTemplate.Current." + m_machine.Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (CurrentMachineStateTemplateResponse data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<CurrentMachineStateTemplateResponse> data)
    {
      if (null == data.Value) {
        return false;
      }

      if (data.Value.Range is null) {
        log.Fatal ($"IsCacheValid: range is null in {data.Value}");
        return false;
      }

      if (data.Value.Range.IsEmpty ()) {
        return false;
      }

      return data.Value.Range.ContainsElement (DateTime.UtcNow);
    }
    #endregion // IRequest implementation
  }

  /// <summary>
  /// Response of the business request CurrentMachineStateTemplate
  /// </summary>
  public sealed class CurrentMachineStateTemplateResponse
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CurrentMachineStateTemplateResponse).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="slot">should be not null</param>
    internal CurrentMachineStateTemplateResponse (IMachineStateTemplateSlot slot)
    {
      if (null != slot) {
        this.MachineStateTemplate = slot.MachineStateTemplate;
        this.Range = slot.DateTimeRange;
      }
      else {
        log.Error ("No machine state template slot");
        this.MachineStateTemplate = null;
        this.Range = new UtcDateTimeRange ();
      }
    }

    /// <summary>
    /// Associated machine state template
    /// </summary>
    public IMachineStateTemplate MachineStateTemplate { get; private set; }

    /// <summary>
    /// Associated range
    /// </summary>
    public UtcDateTimeRange Range { get; private set; }
  }
}
