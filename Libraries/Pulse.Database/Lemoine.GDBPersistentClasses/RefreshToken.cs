// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table RefreshToken
  /// </summary>
  public class RefreshToken
    : IRefreshToken
    , IVersionable
    , IComparable, IComparable<IRefreshToken>
    , IEquatable<IRefreshToken>
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IUser m_user;
    DateTime m_creation = DateTime.UtcNow;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (RefreshToken).FullName);

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual IUser User => m_user;

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual string Token { get; set; }

    /// <summary>
    /// Creation date/time of the token in UTC
    /// </summary>
    public virtual DateTime Creation => m_creation;

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual DateTime Expiration { get; set; }

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual DateTime? Revoked { get; set; }

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual string OAuth2Name { get; set; }

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual string OAuth2AuthenticationName { get; set; }

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual string OAuth2RefreshToken { get; set; }

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual int Id => m_id;

    /// <summary>
    /// <see cref="IRefreshToken"/>
    /// </summary>
    public virtual int Version => m_version;

    #region Constructors
    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected RefreshToken ()
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public RefreshToken (IUser user, TimeSpan expiresIn)
    {
      m_user = user;
      var utcNow = DateTime.UtcNow;
      m_creation = utcNow;
      this.Expiration = utcNow.Add (expiresIn);
      this.Token = CreateRefreshToken ();
    }
    #endregion // Constructors

    string CreateRefreshToken ()
    {
#if NET6_0_OR_GREATER
      var randomNumber = RandomNumberGenerator.GetBytes (32);
      return Convert.ToBase64String (randomNumber);
#else // !NET6_0_OR_GREATER
      var randomNumber = new byte[32];
      using (var generator = new RNGCryptoServiceProvider ()) {
        generator.GetBytes (randomNumber);
        return Convert.ToBase64String (randomNumber);
      }
#endif // !NET6_0_OR_GREATER
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (obj is RefreshToken) {
        var other = (IRefreshToken)obj;
        return CompareTo (other);
      }

      log.Error ($"CompareTo: object {obj} of invalid type");
      throw new ArgumentException ("object is not a IRefreshToken");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IRefreshToken other)
    {
      return this.Expiration.CompareTo (other.Expiration);
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[RefreshToken {this.Id} User={this.User.Id}]";
      }
      else {
        return $"[RefreshToken {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IRefreshToken other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }
      if (obj is null) {
        return false;
      }

      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      IRefreshToken other = obj as RefreshToken;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return this.Id.Equals (other.Id)
          && this.Version.Equals (other.Version);
      }
      return false;
    }

    /// <summary>
    /// Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * this.Token.GetHashCode ();
        }
        return hashCode;
      }
    }
  }
}
