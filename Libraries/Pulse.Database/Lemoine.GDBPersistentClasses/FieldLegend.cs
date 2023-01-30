// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table FieldLegend
  /// 
  /// This is a new table.
  /// 
  /// This new table lists for some fields some ranges of values that can be considered as a threshold.
  ///
  /// In an graphical application, a given legend may be displayed with a given color for example.
  /// </summary>
  [Serializable]
  public class FieldLegend: IFieldLegend, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IField m_field;
    string m_stringValue = null;
    double? m_minValue = null;
    double? m_maxValue = null;
    string m_text;
    string m_color;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (FieldLegend).FullName);

    #region Getters / Setters
    /// <summary>
    /// FieldLegend Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// FieldLegend Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated field
    /// </summary>
    public virtual IField Field {
      get { return m_field; }
    }
    
    /// <summary>
    /// String value to match
    /// 
    /// This may be null, if not applicable
    /// </summary>
    public virtual string StringValue {
      get { return m_stringValue; }
      set { m_stringValue = value; }
    }
    
    /// <summary>
    /// Minimum range value
    /// 
    /// If null, no low bound
    /// </summary>
    public virtual double? MinValue { // public for the configuration
      get { return m_minValue; }
      set { m_minValue = value; }
    }
    
    /// <summary>
    /// Maximum range value
    /// 
    /// if null, no high bound
    /// </summary>
    public virtual double? MaxValue { // public for the configuration
      get { return m_maxValue; }
      set { m_maxValue = value; }
    }
    
    /// <summary>
    /// Applicable range
    /// </summary>
    public virtual Range<double> Range {
      get { return new Range<double> (new LowerBound<double> (this.MinValue), new UpperBound<double> (this.MaxValue)); }
      set
      {
        this.MinValue = value.Lower.NullableValue;
        this.MaxValue = value.Upper.NullableValue;
      }
    }
    
    /// <summary>
    /// Associated text to the legend
    /// 
    /// It can't be null
    /// </summary>
    public virtual string Text {
      get { return m_text; }
      set
      {
        if (null == value) {
          log.ErrorFormat ("Text.set: " +
                           "this property can't be null");
          throw new ArgumentNullException ("Text");
        }
        m_text = value;
      }
    }
    
    /// <summary>
    /// Associated color to the legend
    /// 
    /// It can't be null
    /// </summary>
    public virtual string Color {
      get { return m_color; }
      set
      {
        if (null == value) {
          log.ErrorFormat ("Color.set: " +
                           "this property can't be null");
          throw new ArgumentNullException ("Color");
        }
        m_color = value;
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected FieldLegend ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="field"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    public FieldLegend (IField field, string text, string color)
    {
      if (null == field) {
        log.ErrorFormat ("Cstr: " +
                         "field can't be null");
        throw new ArgumentNullException ("field");
      }
      m_field = field;
      this.Text = text;
      this.Color = color;
    }
    #endregion // Constructors
  }
}
