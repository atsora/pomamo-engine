// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table UserTable
  /// </summary>
  [Serializable]
  public class User : Updater, IUser
  {
    #region Members
    string m_name;
    string m_code;
    string m_externalCode;
    string m_login;
    string m_password;
    IShift m_shift;
    string m_mobileNumber;
    IRole m_role;
    string m_email;
    ICompany m_company;
    TimeSpan? m_disconnectionTime;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (User).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "ExternalCode", "Login", "Code", "Name" }; }
    }

    /// <summary>
    /// User name
    /// </summary>
    [XmlAttribute ("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// User code
    /// </summary>
    [XmlAttribute ("Code")]
    public virtual string Code
    {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// User external code
    /// </summary>
    [XmlAttribute ("ExternalCode")]
    public virtual string ExternalCode
    {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }

    /// <summary>
    /// User login
    /// </summary>
    [XmlAttribute ("Login")]
    public virtual string Login
    {
      get { return m_login; }
      set { m_login = value; }
    }

    /// <summary>
    /// User password
    /// </summary>
    [XmlIgnore]
    public virtual string Password
    {
      get { return m_password; }
      set {
        if (!string.Equals (m_password, value)) {
          if (string.IsNullOrEmpty (value)) {
            m_password = value;
          }
          else {
            m_password = "encrypt:" + value;
          }
        }
      }
    }

    /// <summary>
    /// Shift the user belongs to (optional)
    /// </summary>
    [XmlIgnore]
    public virtual IShift Shift
    {
      get { return m_shift; }
      set { m_shift = value; }
    }

    /// <summary>
    /// Associated shift for Xml Serialization
    /// </summary>
    [XmlElement ("Shift")]
    public virtual Shift XmlSerializationShift
    {
      get { return this.Shift as Shift; }
      set { this.Shift = value; }
    }

    /// <summary>
    /// Mobile number
    /// </summary>
    [XmlElement ("MobileNumber")]
    public virtual string MobileNumber
    {
      get { return m_mobileNumber; }
      set { m_mobileNumber = value; }
    }

    /// <summary>
    /// Associated role
    /// </summary>
    [XmlIgnore]
    public virtual IRole Role
    {
      get { return m_role; }
      set { m_role = value; }
    }

    /// <summary>
    /// Associated role for Xml Serialization
    /// </summary>
    [XmlElement ("Role")]
    public virtual Role XmlSerializationRole
    {
      get { return this.Role as Role; }
      set { this.Role = value; }
    }

    /// <summary>
    /// E-mail address
    /// </summary>
    [XmlAttribute ("EMail")]
    public virtual string EMail
    {
      get { return this.m_email; }
      set { this.m_email = value; }
    }

    /// <summary>
    /// Associated company (if we want to restrict the rights of the user)
    /// Default is null: no restriction
    /// </summary>
    [XmlIgnore]
    public virtual ICompany Company
    {
      get { return this.m_company; }
      set { this.m_company = value; }
    }

    /// <summary>
    /// Associated company for Xml Serialization
    /// </summary>
    [XmlElement ("Company")]
    public virtual Company XmlSerializationCompany
    {
      get { return this.Company as Company; }
      set { this.Company = value; }
    }

    /// <summary>
    /// <see cref="IUser" />
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? DisconnectionTime
    {
      get { return m_disconnectionTime; }
      set { m_disconnectionTime = value; }
    }
    #endregion

    /// <summary>
    /// Default Constructor
    /// </summary>
    public User () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    internal protected User (string login, string password)
    {
      this.Login = login;
      this.Password = password;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
      NHibernateHelper.Unproxy<ICompany> (ref m_company);
      NHibernateHelper.Unproxy<IRole> (ref m_role);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[User {this.Id} Name={this.Name} Code={this.Code} Login={this.Login}]";
      }
      else {
        return $"[User {this.Id}]";
      }
    }

    #region Equals and GetHashCode implementation
    /// <summary>
    /// Equals
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      User other = obj as User;
      if (other == null) {
        return false;
      }

      return this.Id == other.Id;
    }

    /// <summary>
    /// HashCode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        if (m_name != null) {
          hashCode += 1000000007 * m_name.GetHashCode ();
        }

        if (m_code != null) {
          hashCode += 1000000009 * m_code.GetHashCode ();
        }

        if (m_externalCode != null) {
          hashCode += 1000000021 * m_externalCode.GetHashCode ();
        }

        if (m_login != null) {
          hashCode += 1000000033 * m_login.GetHashCode ();
        }

        if (m_password != null) {
          hashCode += 1000000087 * m_password.GetHashCode ();
        }

        if (m_shift != null) {
          hashCode += 1000000093 * m_shift.GetHashCode ();
        }

        if (m_mobileNumber != null) {
          hashCode += 1000000097 * m_mobileNumber.GetHashCode ();
        }

        if (m_role != null) {
          hashCode += 1000000103 * m_role.GetHashCode ();
        }

        if (m_email != null) {
          hashCode += 1000000123 * m_email.GetHashCode ();
        }

        if (m_company != null) {
          hashCode += 1000000127 * m_company.GetHashCode ();
        }
      }
      return hashCode;
    }

    /// <summary>
    /// ==
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator == (User lhs, User rhs)
    {
      if (ReferenceEquals (lhs, rhs)) {
        return true;
      }

      if (ReferenceEquals (lhs, null) || ReferenceEquals (rhs, null)) {
        return false;
      }

      return lhs.Equals (rhs);
    }

    /// <summary>
    /// !=
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator != (User lhs, User rhs)
    {
      return !(lhs == rhs);
    }
    #endregion
  }
}
