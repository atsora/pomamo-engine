// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.ModelDAO;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Field
{
  /// <summary>
  /// Request class to get the field legends
  /// </summary>
  public sealed class FieldLegends
    : IRequest<IEnumerable<IFieldLegend>>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (FieldLegends).FullName);

    #region Getters / Setters
    /// <summary>
    /// Field (not null)
    /// </summary>
    IField Field { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="field">not null</param>
    public FieldLegends (IField field)
    {
      Debug.Assert (null != field);
      
      this.Field = field;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>List of IFieldLegend</returns>
    public IEnumerable<IFieldLegend> Get ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.Field.FieldLegends"))
        {
          return ModelDAOHelper.DAOFactory.FieldLegendDAO
            .FindAllWithField (this.Field);
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<IFieldLegend>> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey()
    {
      return "Business.FieldLegends." + ((IDataWithId<int>)Field).Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<IFieldLegend>> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<IFieldLegend> data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
