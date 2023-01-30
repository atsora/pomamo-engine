// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IFieldLegendDAO">IFieldLegendDAO</see>
  /// </summary>
  public class FieldLegendDAO
    : VersionableNHibernateDAO<FieldLegend, IFieldLegend, int>
    , IFieldLegendDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FieldLegendDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      IFieldLegend fieldLegend;

      // StackLight
      {
        var field = ModelDAOHelper.DAOFactory.FieldDAO.FindById ((int)FieldId.StackLight);
        foreach (var s in new string[] { "2", "6", "10", "22", "26", "42", "86", "150", "342", "406", "598", "662" /*, ...*/ }) { // Red
          fieldLegend = ModelDAOHelper.ModelFactory.CreateFieldLegend (field, "Red stack light on or flashing", "#FF0000");
          fieldLegend.StringValue = s;
          InsertDefaultValue (fieldLegend);
        }
        foreach (var s in new string[] { "9", "25", "89", "153", "345", "409", "601", "665"} ){ // Yellow
          fieldLegend = ModelDAOHelper.ModelFactory.CreateFieldLegend (field, "Yellow stack light on or flashing", "#FFFF00");
          fieldLegend.StringValue = s;
          InsertDefaultValue (fieldLegend);
        }
        foreach (var s in new string[] { "37", "101", "165", "357", "421", "613", "677" }) { // Green
          fieldLegend = ModelDAOHelper.ModelFactory.CreateFieldLegend (field, "Green stack light on or flashing", "#008000");
          fieldLegend.StringValue = s;
          InsertDefaultValue (fieldLegend);
        }
        foreach (var s in new string[] { "41", "105", "169", "361", "425", "617", "671" }) { // Yellow + Green
          fieldLegend = ModelDAOHelper.ModelFactory.CreateFieldLegend (field, "Yellow and green stack lights on", "#FFA500");
          fieldLegend.StringValue = s;
          InsertDefaultValue (fieldLegend);
        }
      }
    }

    private void InsertDefaultValue (IFieldLegend fieldLegend)
    {
      Debug.Assert (null != fieldLegend);
      IEnumerable<IFieldLegend> fieldLegends = FindAllWithField (fieldLegend.Field);
      if (!fieldLegend.Range.IsEmpty ()) {
        fieldLegends = fieldLegends.Where (l => l.Range.ContainsRange (fieldLegend.Range));
      }
      if (!string.IsNullOrEmpty (fieldLegend.StringValue)) {
        fieldLegends = fieldLegends.Where (l => string.Equals (fieldLegend.StringValue, l.StringValue));
      }
      if (!fieldLegends.Any ()) { // No existing configuration
        log.InfoFormat ("InsertDefaultValue: " +
                        "add field legend");
        ModelDAOHelper.DAOFactory.FieldLegendDAO.MakePersistent (fieldLegend);
      }
    }
    #endregion // DefaultValues

    /// <summary>
    /// Find all the FieldLegend items for the specified field
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="field">not null</param>
    /// <returns></returns>
    public IList<IFieldLegend> FindAllWithField (IField field)
    {
      Debug.Assert (null != field);
      
      return FindAllWithField (field.Id);
    }
    
    /// <summary>
    /// Find all the FieldLegend items for the specified field
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="fieldId">not 0</param>
    /// <returns></returns>
    public IList<IFieldLegend> FindAllWithField (int fieldId)
    {
      Debug.Assert (0 < fieldId);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<FieldLegend> ()
        .Add (Restrictions.Eq ("Field.Id", fieldId))
        .AddOrder (Order.Asc ("MaxValue"))
        .SetCacheable (true)
        .List<IFieldLegend> ();
    }
    
    /// <summary>
    /// Find all the FieldLegend items for the specified field
    /// and field value
    /// 
    /// Note: the initial request uses a list since the DB does not enforce the rule that a field value
    /// should match at most one FieldLegend MinValue/MaxValue range
    /// 
    /// </summary>
    /// <param name="field"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public IFieldLegend FindWithFieldAndValue (IField field, double fieldValue)
    {
      Debug.Assert (null != field);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities

      // FindWithField is first call to optimize the call to the query cache
      IList<IFieldLegend> allFieldLegends = FindAllWithField (field);
      return allFieldLegends.FirstOrDefault (legend => legend.Range.ContainsElement (fieldValue));
    }
  }
}
