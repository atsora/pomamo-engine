// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

using System.Data.Common;
using NHibernate.Type;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Util;
using NHibernate;
using System.Threading.Tasks;
using System.Threading;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// Superclass of single-column nullable types.
  /// </summary>
  /// <remarks>
  /// Maps the Property to a single column that is capable of storing nulls in it. If a .net Struct is
  /// used it will be created with its uninitialized value and then on Update the uninitialized value of
  /// the Struct will be written to the column - not <see langword="null" />. 
  /// </remarks>
  [Serializable]
  public abstract partial class AdvancedNullableType : AbstractType
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AdvancedNullableType));

    readonly SqlType m_sqlType;
    readonly bool m_nullableColumn;

    /// <summary>
    /// Initialize a new instance of the NotNullableType class using a 
    /// <see cref="NHibernate.SqlTypes.SqlType"/>. 
    /// </summary>
    /// <param name="sqlType">The underlying <see cref="NHibernate.SqlTypes.SqlType"/>.</param>
    /// <remarks>This is used when the Property is mapped to a single column.</remarks>
    protected AdvancedNullableType (SqlType sqlType, bool nullableColumn = true)
    {
      m_sqlType = sqlType;
      m_nullableColumn = nullableColumn;
    }

    /// <summary>
    /// When implemented by a class, put the value from the mapped 
    /// Property into to the <see cref="DbCommand"/>.
    /// </summary>
    /// <param name="cmd">The <see cref="DbCommand"/> to put the value into.</param>
    /// <param name="value">The object that contains the value.</param>
    /// <param name="index">The index of the <see cref="DbParameter"/> to start writing the values to.</param>
    /// <param name="session">The session for which the operation is done.</param>
    /// <remarks>
    /// Implementors do not need to handle possibility of null values because this will
    /// only be called from <see cref="NullSafeSet(DbCommand, object, int, ISessionImplementor)"/> after 
    /// it has checked for nulls.
    /// </remarks>
    public abstract void Set (DbCommand cmd, object value, int index, ISessionImplementor session);

    /// <summary>
    /// When implemented by a class, gets the object in the 
    /// <see cref="DbDataReader"/> for the Property.
    /// </summary>
    /// <param name="rs">The <see cref="DbDataReader"/> that contains the value.</param>
    /// <param name="index">The index of the field to get the value from.</param>
    /// <param name="session">The session for which the operation is done.</param>
    /// <returns>An object with the value from the database.</returns>
    public abstract object Get (DbDataReader rs, int index, ISessionImplementor session);

    /// <summary>
    /// When implemented by a class, gets the object in the 
    /// <see cref="DbDataReader"/> for the Property.
    /// </summary>
    /// <param name="rs">The <see cref="DbDataReader"/> that contains the value.</param>
    /// <param name="name">The name of the field to get the value from.</param>
    /// <param name="session">The session for which the operation is done.</param>
    /// <returns>An object with the value from the database.</returns>
    /// <remarks>
    /// Most implementors just call the <see cref="Get(DbDataReader, int, ISessionImplementor)"/> 
    /// overload of this method.
    /// </remarks>
    public virtual object Get (DbDataReader rs, string name, ISessionImplementor session)
    {
      return Get (rs, rs.GetOrdinal (name), session);
    }

    /// <inheritdoc />
    public override string ToLoggableString (object value, ISessionFactoryImplementor factory)
    {
      return value?.ToString ();
    }

    public override void NullSafeSet (DbCommand st, object value, int index, bool[] settable, ISessionImplementor session)
    {
      if (settable[0]) {
        NullSafeSet (st, value, index, session);
      }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This method has been "sealed" because the Types inheriting from <see cref="AdvancedNullableType"/>
    /// do not need to and should not override this method.
    /// </para>
    /// <para>
    /// This method checks to see if value is null, if it is then the value of 
    /// <see cref="DBNull"/> is written to the <see cref="DbCommand"/>.
    /// </para>
    /// <para>
    /// If the value is not null, then the method <see cref="Set(DbCommand, object, int, ISessionImplementor)"/> 
    /// is called and that method is responsible for setting the value.
    /// </para>
    /// </remarks>
    public override void NullSafeSet (DbCommand st, object value, int index, ISessionImplementor session)
    {
      if (m_nullableColumn && (value is null)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"NullSafeSet: binding null to parameter: {index}");
        }

        //Do we check IsNullable?
        // TODO: find out why a certain Parameter would not take a null value...
        // From reading the .NET SDK the default is to NOT accept a null value. 
        // I definitely need to look into this more...
        st.Parameters[index].Value = DBNull.Value;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"binding '{ToLoggableString (value, session.Factory)}' to parameter: {index}");
        }

        Set (st, value, index, session);
      }
    }

    /// <inheritdoc />
    /// <remarks>
    /// This has been sealed because no other class should override it.  This 
    /// method calls <see cref="NullSafeGet(DbDataReader, String, ISessionImplementor)" /> for a single value.  
    /// It only takes the first name from the string[] names parameter - that is a 
    /// safe thing to do because a Nullable Type only has one field.
    /// </remarks>
    public sealed override object NullSafeGet (DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
      return NullSafeGet (rs, names[0], session);
    }

    /// <summary>
    /// Extracts the values of the fields from the DataReader
    /// </summary>
    /// <param name="rs">The DataReader positioned on the correct record</param>
    /// <param name="names">An array of field names.</param>
    /// <param name="session">The session for which the operation is done.</param>
    /// <returns>The value off the field from the DataReader</returns>
    /// <remarks>
    /// In this class this just ends up passing the first name to the NullSafeGet method
    /// that takes a string, not a string[].
    /// 
    /// I don't know why this method is in here - it doesn't look like anybody that inherits
    /// from NotNullableType overrides this...
    /// 
    /// TODO: determine if this is needed
    /// </remarks>
    public virtual object NullSafeGet (DbDataReader rs, string[] names, ISessionImplementor session)
    {
      return NullSafeGet (rs, names[0], session);
    }

    /// <summary>
    /// Gets the value of the field from the <see cref="DbDataReader" />.
    /// </summary>
    /// <param name="rs">The <see cref="DbDataReader" /> positioned on the correct record.</param>
    /// <param name="name">The name of the field to get the value from.</param>
    /// <param name="session">The session for which the operation is done.</param>
    /// <returns>The value of the field.</returns>
    /// <remarks>
    /// <para>
    /// This method checks to see if value is null, if it is then the null is returned
    /// from this method.
    /// </para>
    /// <para>
    /// If the value is not null, then the method <see cref="Get(DbDataReader, Int32, ISessionImplementor)"/> 
    /// is called and that method is responsible for retrieving the value.
    /// </para>
    /// </remarks>
    public virtual object NullSafeGet (DbDataReader rs, string name, ISessionImplementor session)
    {
      int index = rs.GetOrdinal (name);

      if (rs.IsDBNull (index)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"NullSafeGet: returning null as column: {name}");
        }
        // TODO: add a method to NotNullableType.GetNullValue - if we want to
        // use "MAGIC" numbers to indicate null values...
        return null;
      }
      else {
        object val;
        try {
          val = Get (rs, index, session);
        }
        catch (InvalidCastException ice) {
          throw new ADOException (
            string.Format (
              "Could not cast the value in field {0} of type {1} to the Type {2}.  Please check to make sure that the mapping is correct and that your DataProvider supports this Data Type.",
              name, rs[index].GetType ().Name, GetType ().Name), ice);
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"NullSafeGet: returning '{ToLoggableString (val, session.Factory)}' as column: {name}");
        }

        return val;
      }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This implementation forwards the call to <see cref="NullSafeGet(DbDataReader, String, ISessionImplementor)" />.
    /// </para>
    /// <para>
    /// It has been "sealed" because the Types inheriting from <see cref="AdvancedNullableType"/>
    /// do not need to and should not override this method.  All of their implementation
    /// should be in <see cref="NullSafeGet(DbDataReader, String, ISessionImplementor)" />.
    /// </para>
    /// </remarks>
    public sealed override object NullSafeGet (DbDataReader rs, string name, ISessionImplementor session, object owner)
    {
      return NullSafeGet (rs, name, session);
    }

    /// <summary>
    /// Gets the underlying <see cref="NHibernate.SqlTypes.SqlType" /> for 
    /// the column mapped by this <see cref="AdvancedNullableType" />.
    /// </summary>
    /// <value>The underlying <see cref="NHibernate.SqlTypes.SqlType"/>.</value>
    /// <remarks>
    /// This implementation should be suitable for all subclasses unless they need to
    /// do some special things to get the value.  There are no built in <see cref="AdvancedNullableType"/>s
    /// that override this Property.
    /// </remarks>
    public virtual SqlType SqlType
    {
      get { return m_sqlType; }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This implementation forwards the call to <see cref="AdvancedNullableType.SqlType" />.
    /// </para>
    /// <para>
    /// It has been "sealed" because the Types inheriting from <see cref="AdvancedNullableType"/>
    /// do not need to and should not override this method because they map to a single
    /// column.  All of their implementation should be in <see cref="AdvancedNullableType.SqlType" />.
    /// </para>
    /// </remarks>
    public sealed override SqlType[] SqlTypes (IMapping mapping)
    {
      return new[] { OverrideSqlType (mapping, SqlType) };
    }

    /// <summary>
    /// Overrides the sql type.
    /// </summary>
    /// <param name="type">The type to override.</param>
    /// <param name="mapping">The mapping for which to override <paramref name="type"/>.</param>
    /// <returns>The refined types.</returns>
    static SqlType OverrideSqlType (IMapping mapping, SqlType type)
    {
      return mapping != null ? mapping.Dialect.OverrideSqlType (type) : type;
    }

    /// <summary>
    /// Returns the number of columns spanned by this <see cref="AdvancedNullableType"/>
    /// </summary>
    /// <returns>A <see cref="AdvancedNullableType"/> always returns 1.</returns>
    /// <remarks>
    /// This has the hard coding of 1 in there because, by definition of this class, 
    /// a NotNullableType can only map to one column in a table.
    /// </remarks>
    public override sealed int GetColumnSpan (IMapping session)
    {
      return 1;
    }

    /// <summary>
    /// <see cref="AbstractType"/>
    /// </summary>
    /// <param name="old"></param>
    /// <param name="current"></param>
    /// <param name="checkable"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public override bool IsDirty (object old, object current, bool[] checkable, ISessionImplementor session)
    {
      return checkable[0] && IsDirty (old, current, session);
    }

    /// <summary>
    /// <see cref="AbstractType"/>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
    public override bool[] ToColumnNullness (object value, IMapping mapping)
    {
      return value == null ? ArrayHelper.False : ArrayHelper.True;
    }

    #region override of System.Object Members

    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to this
    /// <see cref="AdvancedNullableType"/>.
    /// </summary>
    /// <param name="obj">The <see cref="Object"/> to compare with this NotNullableType.</param>
    /// <returns>true if the SqlType and Name properties are the same.</returns>
    public override bool Equals (object obj)
    {
      /*
       *  Step 1: Perform an == test
       *  Step 2: Instance of check
       *  Step 3: Cast argument
       *  Step 4: For each important field, check to see if they are equal
       *  Step 5: Go back to equals()'s contract and ask yourself if the equals() 
       *  method is reflexive, symmetric, and transitive
       */

      if (this == obj) {
        return true;
      }

      AdvancedNullableType rhsType = obj as AdvancedNullableType;

      if (rhsType == null) {
        return false;
      }

      if (Name.Equals (rhsType.Name) && SqlType.Equals (rhsType.SqlType)) {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Serves as a hash function for the <see cref="AdvancedNullableType"/>, 
    /// suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code that is based on the <see cref="AdvancedNullableType.SqlType"/>'s 
    /// hash code and the <see cref="AbstractType.Name"/>'s hash code.</returns>
    public override int GetHashCode ()
    {
      return (SqlType.GetHashCode () / 2) + (Name.GetHashCode () / 2);
    }

    /// <summary>
    /// Provides a more descriptive string representation by reporting the properties that are important for equality. 
    /// Useful in error messages.
    /// </summary>
    public override string ToString ()
    {
      return $"{base.ToString ()} (SqlType: {SqlType})";
    }

    #endregion

    /// <summary>
    /// <see cref="AbstractType"/>
    /// </summary>
    /// <param name="st"></param>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <param name="settable"></param>
    /// <param name="session"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task NullSafeSetAsync (DbCommand st, object value, int index, bool[] settable, ISessionImplementor session, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) {
        return Task.FromCanceled<object> (cancellationToken);
      }
      try {
        if (settable[0]) {
          return NullSafeSetAsync (st, value, index, session, cancellationToken);
        }

        return Task.CompletedTask;
      }
      catch (Exception ex) {
        return Task.FromException<object> (ex);
      }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This method has been "sealed" because the Types inheriting from <see cref="NullableType"/>
    /// do not need to and should not override this method.
    /// </para>
    /// <para>
    /// This method checks to see if value is null, if it is then the value of 
    /// <see cref="DBNull"/> is written to the <see cref="DbCommand"/>.
    /// </para>
    /// <para>
    /// If the value is not null, then the method <see cref="Set(DbCommand, object, int, ISessionImplementor)"/> 
    /// is called and that method is responsible for setting the value.
    /// </para>
    /// </remarks>
    public sealed override Task NullSafeSetAsync (DbCommand st, object value, int index, ISessionImplementor session, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) {
        return Task.FromCanceled<object> (cancellationToken);
      }
      try {
        NullSafeSet (st, value, index, session);
        return Task.CompletedTask;
      }
      catch (Exception ex) {
        return Task.FromException<object> (ex);
      }
    }

    /// <inheritdoc />
    /// <remarks>
    /// This has been sealed because no other class should override it.  This 
    /// method calls <see cref="NullSafeGet(DbDataReader, String, ISessionImplementor)" /> for a single value.  
    /// It only takes the first name from the string[] names parameter - that is a 
    /// safe thing to do because a Nullable Type only has one field.
    /// </remarks>
    public sealed override Task<object> NullSafeGetAsync (DbDataReader rs, string[] names, ISessionImplementor session, object owner, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) {
        return Task.FromCanceled<object> (cancellationToken);
      }
      try {
        return Task.FromResult<object> (NullSafeGet (rs, names, session, owner));
      }
      catch (Exception ex) {
        return Task.FromException<object> (ex);
      }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// This implementation forwards the call to <see cref="NullSafeGet(DbDataReader, String, ISessionImplementor)" />.
    /// </para>
    /// <para>
    /// It has been "sealed" because the Types inheriting from <see cref="NullableType"/>
    /// do not need to and should not override this method.  All of their implementation
    /// should be in <see cref="NullSafeGet(DbDataReader, String, ISessionImplementor)" />.
    /// </para>
    /// </remarks>
    public sealed override Task<object> NullSafeGetAsync (DbDataReader rs, string name, ISessionImplementor session, object owner, CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested) {
        return Task.FromCanceled<object> (cancellationToken);
      }
      try {
        return Task.FromResult<object> (NullSafeGet (rs, name, session, owner));
      }
      catch (Exception ex) {
        return Task.FromException<object> (ex);
      }
    }

    /// <summary>
    /// <see cref="AbstractType"/>
    /// </summary>
    /// <param name="old"></param>
    /// <param name="current"></param>
    /// <param name="checkable"></param>
    /// <param name="session"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<bool> IsDirtyAsync (object old, object current, bool[] checkable, ISessionImplementor session, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested ();
      return checkable[0] && await (IsDirtyAsync (old, current, session, cancellationToken)).ConfigureAwait (false);
    }
  }
}
