// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: LGPL-2.1-only

/* -*- c# -*- ******************************************************************
 * Copyright (C) 2009-2023  Lemoine Automation Technologies
 * This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation; either version 2.1 of the License, or (at your option) any later version.
 * This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more details.
 * You should have received a copy of the GNU Lesser General Public License along with this library; if not, write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA Also add information on how to contact you by electronic and paper mail.
*/

using System;
using System.Collections.Generic;
using System.Linq;

using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Criterion
{
  /// <summary>
  /// The base class for an <see cref="ICriterion"/> that compares a single Property
  /// to a value.
  /// </summary>
  [Serializable]
  public class FunctionExpression : AbstractCriterion
  {
    private readonly string m_propertyName;
    readonly string m_functionName;
    private readonly IType m_type;
    private readonly object m_value;
    private bool m_ignoreCase;
    private readonly string m_op;

    /// <summary>
    /// Initialize a new instance of the <see cref="SimpleExpression" /> class for a named
    /// Property and its value using a function that returns TRUE or FALSE
    /// </summary>
    /// <param name="propertyName">The name of the Property in the class.</param>
    /// <param name="functionName">The name of the function to apply on the property</param>
    public FunctionExpression(string propertyName, string functionName)
      : this (propertyName, functionName, NHibernateUtil.Boolean, true, "=")
    {
    }
    
    /// <summary>
    /// Initialize a new instance of the <see cref="SimpleExpression" /> class for a named
    /// Property and its value using the default operator "="
    /// </summary>
    /// <param name="propertyName">The name of the Property in the class.</param>
    /// <param name="functionName">The name of the function to apply on the property</param>
    /// <param name="type">Type of value</param>
    /// <param name="value">The value for the Property.</param>
    public FunctionExpression(string propertyName, string functionName, IType type, object value)
      : this (propertyName, functionName, type, value, "=")
    {
    }
    
    /// <summary>
    /// Initialize a new instance of the <see cref="SimpleExpression" /> class for a named
    /// Property and its value.
    /// </summary>
    /// <param name="propertyName">The name of the Property in the class.</param>
    /// <param name="functionName">The name of the function to apply on the property</param>
    /// <param name="type">Type of value</param>
    /// <param name="value">The value for the Property.</param>
    /// <param name="op">The SQL operation.</param>
    public FunctionExpression(string propertyName, string functionName, IType type, object value, string op)
    {
      this.m_propertyName = propertyName;
      m_functionName = functionName;
      m_type = type;
      this.m_value = value;
      this.m_op = op;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="functionName"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="op"></param>
    /// <param name="ignoreCase"></param>
    public FunctionExpression(string propertyName, string functionName, IType type, object value, string op, bool ignoreCase)
      : this(propertyName, functionName, type, value, op)
    {
      this.m_ignoreCase = ignoreCase;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public FunctionExpression IgnoreCase()
    {
      m_ignoreCase = true;
      return this;
    }

    /// <summary>
    /// Gets the named Property for the Expression.
    /// </summary>
    /// <value>A string that is the name of the Property.</value>
    public string PropertyName
    {
      get { return m_propertyName; }
    }

    /// <summary>
    /// Gets the function name property to apply
    /// </summary>
    public string FunctionName
    {
      get { return m_functionName; }
    }
    
    /// <summary>
    /// Gets the Value for the Expression.
    /// </summary>
    /// <value>An object that is the value for the Expression.</value>
    public object Value
    {
      get { return m_value; }
    }

    /// <summary>
    /// Converts the SimpleExpression to a <see cref="SqlString"/>.
    /// </summary>
    /// <returns>A SqlString that contains a valid Sql fragment.</returns>
    public override SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery)
    {
      SqlString[] columnNames =
        GetColumnNamesForSimpleExpression(
          m_propertyName,
          criteriaQuery,
          criteria,
          this,
          m_value);

      TypedValue typedValue = new TypedValue (m_type, m_value);
      Parameter[] parameters = criteriaQuery.NewQueryParameter(typedValue).ToArray();
      
      if (m_ignoreCase)
      {
        if (columnNames.Length != 1)
        {
          throw new HibernateException(
            "case insensitive expression may only be applied to single-column properties: " +
            m_propertyName);
        }
        
        return new SqlString(
          criteriaQuery.Factory.Dialect.LowercaseFunction,
          StringHelper.OpenParen,
          columnNames[0],
          StringHelper.ClosedParen,
          Op,
          parameters.Single());
      }
      else
      {
        SqlStringBuilder sqlBuilder = new SqlStringBuilder(4 * columnNames.Length);
        var columnNullness = typedValue.Type.ToColumnNullness(typedValue.Value, criteriaQuery.Factory);

        if (columnNullness.Length != columnNames.Length)
        {
          throw new AssertionFailure("Column nullness length doesn't match number of columns.");
        }
        
        for (int i = 0; i < columnNames.Length; i++)
        {
          if (i > 0)
          {
            sqlBuilder.Add(" and ");
          }
          
          if (columnNullness[i])
          {
            sqlBuilder.Add(string.Format ("{0}({1})",
                                          m_functionName,
                                          columnNames[i]))
              .Add(Op)
              .Add(parameters[i]);
          }
          else
          {
            sqlBuilder.Add(columnNames[i])
              .Add(" is null ");
          }
        }
        return sqlBuilder.ToSqlString();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="criteriaQuery"></param>
    /// <returns></returns>
    public override TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
    {
      var typedValues = new List<TypedValue>();

      typedValues.Add(GetParameterTypedValue(criteria, criteriaQuery));

      return typedValues.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="criteriaQuery"></param>
    /// <returns></returns>
    public TypedValue GetParameterTypedValue(ICriteria criteria, ICriteriaQuery criteriaQuery)
    {
      object icvalue = m_ignoreCase ? m_value.ToString().ToLower() : m_value;
      
      return criteriaQuery.GetTypedValue(criteria, m_propertyName, icvalue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override IProjection[] GetProjections()
    {
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return (object)m_propertyName + Op + ValueToStrings();
    }

    /// <summary>
    /// Get the Sql operator to use for the specific
    /// subclass of <see cref="SimpleExpression"/>.
    /// </summary>
    protected virtual string Op
    {
      get { return m_op; }
    }

    private static readonly System.Type[] CallToStringTypes = new[]
    {
      typeof(DateTime),
      typeof(string),
    };

    private string ValueToStrings()
    {
      if (m_value == null)
      {
        return "null";
      }
      var type = m_value.GetType();
      if (type.IsPrimitive || CallToStringTypes.Any(t => t.IsAssignableFrom(type)))
      {
        return m_value.ToString();
      }

      return ObjectHelpers.IdentityToString(m_value);
    }
    
    #region Lemoine extensions
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="criteriaQuery"></param>
    /// <param name="criteria"></param>
    /// <param name="criterion"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public SqlString[] GetColumnNamesForSimpleExpression(
      string propertyName,
      ICriteriaQuery criteriaQuery,
      ICriteria criteria,
      ICriterion criterion,
      object value)
    {
      return GetColumnNamesUsingPropertyName(
        criteriaQuery,
        criteria,
        propertyName,
        value,
        criterion);
    }
    
    internal SqlString[] GetColumnNamesUsingProjection(IProjection projection, ICriteriaQuery criteriaQuery, ICriteria criteria)
    {
      SqlString sqlString = projection.ToSqlString(criteria,
                                                   criteriaQuery.GetIndexForAlias(),
                                                   criteriaQuery);
      return new SqlString[]
      {
        SqlStringHelper.RemoveAsAliasesFromSql(sqlString)
      };
    }
    
    private SqlString[] GetColumnNamesUsingPropertyName(
      ICriteriaQuery criteriaQuery,
      ICriteria criteria,
      string propertyName,
      object value,
      ICriterion critertion)
    {
      string[] columnNames = criteriaQuery.GetColumnsUsingProjection(criteria, propertyName);

      if (value != null && !(value is System.Type) && !m_type.ReturnedClass.IsInstanceOfType(value))
      {
        throw new QueryException(string.Format(
          "Type mismatch in {0}: {1} expected type {2}, actual type {3}",
          critertion.GetType(), propertyName, m_type.ReturnedClass, value.GetType()));
      }

      IType propertyType = criteriaQuery.GetTypeUsingProjection(criteria, propertyName);
      if (propertyType.IsCollectionType)
      {
        throw new QueryException(string.Format(
          "cannot use collection property ({0}.{1}) directly in a criterion,"
          + " use ICriteria.CreateCriteria instead",
          criteriaQuery.GetEntityName(criteria), propertyName));
      }
      return Array.ConvertAll<string, SqlString>(columnNames, delegate(string col)
                                                 {
                                                   return new SqlString(col);
                                                 });
    }
    #endregion // Lemoine extensions
  }
}
