// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Field
{
  /// <summary>
  /// Request class to get the cnc value color
  /// </summary>
  public sealed class FieldValueColor
    : IRequest<string>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (FieldValueColor).FullName);

    #region Getters / Setters
    /// <summary>
    /// Field (not null)
    /// </summary>
    IField Field { get; set; }
    
    /// <summary>
    /// Value
    /// </summary>
    object Value { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncValue">not null</param>
    public FieldValueColor (ICncValue cncValue)
      : this (cncValue.Field, cncValue.Value)
    {
      Debug.Assert (null != cncValue);
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="v"></param>
    public FieldValueColor (IField field, object v)
    {
      Debug.Assert (null != field);
      
      this.Field = field;
      this.Value = v;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>string, empty if no color was set</returns>
    public string Get()
    {
      Debug.Assert (null != this.Field);
      
      log.DebugFormat ("Get: " +
                       "field id {0} value {1}",
                       this.Field.Id, this.Value);
      
      IEnumerable<IFieldLegend> legends = ServiceProvider
        .Get<IEnumerable<IFieldLegend>> (new Lemoine.Business.Field.FieldLegends (this.Field));
      
      // StringValue
      IFieldLegend legend = legends.FirstOrDefault (l => object.Equals (l.Field, this.Field)
                                                    && string.Equals (l.StringValue, this.Value.ToString ()));
      
      // Range
      if (null == legend) {
        if (this.Field.Type.Equals (FieldType.Double)) {
          double doubleValue = (double) this.Value;
          legend = legends.FirstOrDefault (l => object.Equals (l.Field, this.Field) && string.IsNullOrEmpty (l.StringValue) && l.Range.ContainsElement (doubleValue));
        }
        else if (this.Field.Type.Equals (FieldType.Int32)) {
          int intValue = (int) this.Value;
          legend = legends.FirstOrDefault (l => object.Equals (l.Field, this.Field) && string.IsNullOrEmpty (l.StringValue) && l.Range.ContainsElement (intValue));
        }
      }
      
      if (null != legend) {
        return legend.Color;
      }
      else {
        return "";
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey()
    {
      return "Business.FieldValueColor." + ((IDataWithId<int>)Field).Id + "." + Value;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<string> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (string data)
    {
      if (this.Field.Type.Equals (FieldType.Int32) || this.Field.Type.Equals (FieldType.Double)) {
        // Do not keep the cache in case the range is used
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      else {
        return CacheTimeOut.Config.GetTimeSpan ();
      }
    }
    #endregion // IRequest implementation
  }
}
