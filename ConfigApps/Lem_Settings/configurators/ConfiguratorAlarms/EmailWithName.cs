// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Text.RegularExpressions;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of EmailWithName.
  /// </summary>
  public class EmailWithName : IComparable
  {
    #region Getters / Setters
    /// <summary>
    /// Email aaa@bbb.ccc
    /// </summary>
    public string Email { get; private set; }
    
    /// <summary>
    /// Name associated to the email (can be null or empty)
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Return true if the name is filled
    /// </summary>
    public bool HasName {
      get { return !String.IsNullOrEmpty(Name); }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="email"></param>
    public EmailWithName(string email)
    {
      Email = email;
      Name = null;
    }
    
    /// <summary>
    /// Implicit conversion from string to EmailWithName
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static implicit operator EmailWithName(string email)
    {
      return new EmailWithName(email);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Short description of the email (return the email OR the name if present)
    /// </summary>
    /// <returns></returns>
    public string toShortString()
    {
      if (String.IsNullOrEmpty(Name)) {
        return Email;
      }
      else {
        return Name;
      }
    }
    
    /// <summary>
    /// Full description of the email (return the email AND the name if present)
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (String.IsNullOrEmpty(Name)) {
        return Email;
      }
      else {
        return Name + " (" + Email + ")";
      }
    }
    
    /// <summary>
    /// Return true if the emails are equal, regardless of the name
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      if (obj == null) {
        return false;
      }

      if (obj is EmailWithName) {
        EmailWithName other = obj as EmailWithName;
        return Email.Equals(other.Email);
      } else if (obj is string) {
        return Email.Equals((string)obj);
      }
      
      return false;
    }
    
    /// <summary>
    /// Hashcode of an EmailWithName
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (Email != null) {
          hashCode += 1000000007 * Email.GetHashCode();
        }

        if (Name != null) {
          hashCode += 1000000009 * Name.GetHashCode();
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// Ordering of EmailWithName: first the elements having a name,
    /// then the elements having no name. Alphabetical order for both
    /// categories.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
      if (obj is string) {
        return Email.CompareTo((string)obj);
      }
      else if (obj is EmailWithName)
      {
        EmailWithName other = (EmailWithName)obj;
        if (HasName) {
          if (other.HasName) {
            return Name.CompareTo(other.Name);
          } else {
            return -1;
          }
        }
        else {
          if (other.HasName) {
            return 1;
          } else {
            return Email.CompareTo(other.Email);
          }
        }
      }
      throw new InvalidOperationException();
    }

    /// <summary>
    /// Return true if the email is valid
    /// </summary>
    /// <returns></returns>
    public bool IsValid()
    {
      if (String.IsNullOrEmpty(Email)) {
        return false;
      }

      Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.\w+)+)$");
      return regex.Match(Email).Success;
    }
    #endregion // Methods
  }
}
