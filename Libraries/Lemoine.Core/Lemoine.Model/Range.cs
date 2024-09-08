// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Generic range
  /// </summary>
  [Serializable,
   XmlInclude (typeof (UtcDateTimeRange))]
  public class Range<T> : IRange<T>, IEquatable<Range<T>>, ICloneable, IComparable, IComparable<Range<T>>
    where T : struct, IComparable, IComparable<T>, IEquatable<T>
  {
    #region Members
    LowerBound<T> m_lower; // null => -oo
    bool m_lowerInclusive = true;
    UpperBound<T> m_upper; // null => +oo
    bool m_upperInclusive = false;
    bool m_empty = false; // Manuall set to empty
    #endregion // Members

    // disable once StaticFieldInGenericType
    static readonly ILog log = LogManager.GetLogger (typeof (Range<T>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Lower bound of range
    /// </summary>
    [XmlIgnore]
    public LowerBound<T> Lower
    {
      get {
        if (m_empty) {
          log.ErrorFormat ("Lower.get: empty range. StackTrace={0}",
                           System.Environment.StackTrace);
          throw new InvalidOperationException ("empty range");
        }
        return m_lower;
      }
      protected set {
        m_lower = value;
        if (!m_lower.HasValue) {
          m_lowerInclusive = false;
        }
        m_empty = false;
      }
    }

    /// <summary>
    /// Is lower bound inclusive ?
    /// </summary>
    [XmlIgnore]
    public bool LowerInclusive
    {
      get {
        return !m_empty && m_lower.HasValue && m_lowerInclusive;
      }
      protected set {
        m_lowerInclusive = value;
      }
    }

    /// <summary>
    /// Upper bound of range
    /// </summary>
    [XmlIgnore]
    public UpperBound<T> Upper
    {
      get {
        if (m_empty) {
          log.ErrorFormat ("Upper.get: empty range. StackTrace={0}",
            System.Environment.StackTrace);
          throw new InvalidOperationException ("empty range");
        }
        return m_upper;
      }
      protected set {
        m_upper = value;
        m_upperInclusive &= m_upper.HasValue;
        m_empty = false;
      }
    }

    /// <summary>
    /// Is upper bound inclusive ?
    /// </summary>
    [XmlIgnore]
    public bool UpperInclusive
    {
      get {
        return !m_empty && m_upper.HasValue && m_upperInclusive;
      }
      protected set {
        m_upperInclusive = value;
      }
    }

    /// <summary>
    /// Property to serialize the class
    /// </summary>
    [XmlText]
    public string XmlValue
    {
      get { return this.ToString (); }
      set { this.Parse (value); }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Create an empty range
    /// </summary>
    public Range ()
    {
      m_empty = true;
    }

    /// <summary>
    /// Constructor from a string
    /// </summary>
    /// <param name="s"></param>
    public Range (string s)
    {
      Parse (s);
    }

    /// <summary>
    /// Constructor with a default inclusivity "[)"
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    public Range (LowerBound<T> lower, UpperBound<T> upper)
    {
      m_lower = lower;
      m_upper = upper;
      m_empty = false;
      GetCanonical ();
    }

    /// <summary>
    /// Constructor with a default inclusivity "[)"
    /// </summary>
    /// <param name="empty"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="lowerInclusive"></param>
    /// <param name="upperInclusive"></param>
    protected Range (bool empty, LowerBound<T> lower, UpperBound<T> upper, bool lowerInclusive, bool upperInclusive)
    {
      m_empty = empty;
      if (!empty) {
        m_lower = lower;
        m_upper = upper;
        m_lowerInclusive = lowerInclusive;
        m_upperInclusive = upperInclusive;
        GetCanonical ();
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="lowerInclusive"></param>
    /// <param name="upperInclusive"></param>
    public Range (LowerBound<T> lower, UpperBound<T> upper, bool lowerInclusive, bool upperInclusive)
    {
      m_lower = lower;
      m_upper = upper;
      m_lowerInclusive = lowerInclusive;
      m_upperInclusive = upperInclusive;
      m_empty = false;
      GetCanonical ();
    }

    /// <summary>
    /// Constructor
    /// 
    /// inclusivity must be one of the following values:
    /// <item>()</item>
    /// <item>(]</item>
    /// <item>[)</item>
    /// <item>[]</item>
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="inclusivity"></param>
    public Range (LowerBound<T> lower, UpperBound<T> upper, string inclusivity)
    {
      m_lower = lower;
      m_upper = upper;
      ParseInclusivity (inclusivity);
      GetCanonical ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the canonical form of the range
    /// 
    /// Default: do nothing
    /// </summary>
    protected virtual void GetCanonical ()
    { }

    /// <summary>
    /// Parse a string for the constructor
    /// </summary>
    /// <param name="arg"></param>
    protected virtual void Parse (string arg)
    {
      if (null == arg) {
        log.WarnFormat ("Parse: " +
                        "arg is null => fallback: consider it is empty though");
        Debug.Assert (null != arg);
        m_empty = true;
        return;
      }

      string s = arg.Trim (new char[] { '\'', '"', ' ' });

      m_empty = string.IsNullOrEmpty (s) || s.Equals ("empty");
      if (m_empty) {
        log.DebugFormat ("Parse: " +
                         "the Range from string {0} is empty",
                         s);
        return;
      }

      if (s.Length < 3) {
        log.ErrorFormat ("Parse: " +
                         "parameter {0} with not enough characters",
                         s);
        throw new FormatException ("Invalid string length");
      }

      // - Bounds
      string[] bounds = s.Substring (1, s.Length - 2).Split (new char[] { ',' }, 2);
      if (2 != bounds.Length) {
        log.ErrorFormat ("Parse: " +
                         "missing separator bound in parameter {0}",
                         s);
        throw new FormatException ("Missing bound separator");
      }
      else {
        // - lower
        string lowerString = bounds[0].Trim (new char[] { '\'', '"', ' ' });
        if (string.IsNullOrEmpty (lowerString)) {
          m_lower = new LowerBound<T> (null);
        }
        else {
          m_lower = ParseBound (lowerString);
        }
        // - upper
        string upperString = bounds[1].Trim (new char[] { '\'', '"', ' ' });
        if (string.IsNullOrEmpty (upperString)) {
          m_upper = new UpperBound<T> (null);
        }
        else {
          m_upper = ParseBound (upperString);
        }
      }

      // - Inclusivity
      string inclusivity = string.Format ("{0}{1}",
                                          s[0], s[s.Length - 1]);
      ParseInclusivity (inclusivity);
    }

    /// <summary>
    /// Parse a bound
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected virtual T ParseBound (string s)
    {
      log.Error ("ParseBound: not implemented => the constructor from a string argument can't be used");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Parse the inclusivity parameter.
    /// It must one of the following strings:
    /// <item>()</item>
    /// <item>(]</item>
    /// <item>[)</item>
    /// <item>[]</item>
    /// </summary>
    /// <param name="inclusivity"></param>
    void ParseInclusivity (string inclusivity)
    {
      if (string.IsNullOrEmpty (inclusivity)) {
        log.ErrorFormat ("ParseInclusivity: " +
                         "null or empty parameter");
        throw new ArgumentOutOfRangeException ("inclusivity", "NullOrEmpty");
      }
      else if (2 != inclusivity.Length) {
        log.ErrorFormat ("ParseInclusivity: " +
                         "parameter {0} not with 2 characters",
                         inclusivity);
        throw new ArgumentOutOfRangeException ("inclusivity", "Bad length");
      }
      else {
        switch (inclusivity[0]) {
        case '[':
          m_lowerInclusive = true;
          break;
        case '(':
          m_lowerInclusive = false;
          break;
        default:
          log.ErrorFormat ("ParseInclusivity: " +
                           "bad syntax for the lower inclusivity in parameter {0}",
                           inclusivity);
          throw new ArgumentOutOfRangeException ("inclusivity", "Bad format for lower inclusivity");
        }
        switch (inclusivity[1]) {
        case ']':
          m_upperInclusive = true;
          break;
        case ')':
          m_upperInclusive = false;
          break;
        default:
          log.ErrorFormat ("ParseInclusivity: " +
                           "bad syntax for the upper inclusivity in parameter {0}",
                           inclusivity);
          throw new ArgumentOutOfRangeException ("inclusivity", "Bad format for lower inclusivity");
        }
      }
    }

    /// <summary>
    /// Return if a range corresponds to a single point: [x, x]
    /// </summary>
    /// <returns></returns>
    public bool IsPoint ()
    {
      return !m_empty && m_lowerInclusive && m_upperInclusive && m_lower.HasValue && m_upper.HasValue
        && object.Equals (m_lower.Value, m_upper.Value);
    }

    /// <summary>
    /// Check if the range is empty
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty ()
    {
      if (m_empty) {
        log.DebugFormat ("IsEmpty: " +
                         "empty because Empty property is set to true");
        return true;
      }
      else if (m_upper.HasValue && m_lower.HasValue) {
        if (m_upper.Value.CompareTo (m_lower.Value) < 0) {
          log.DebugFormat ("IsEmpty: " +
                           "empty because upper {1} is strictly lesser than lower {0}",
                           m_lower, m_upper);
          return true;
        }
        else if (object.Equals (m_upper.Value, m_lower.Value)
                 && (!m_lowerInclusive || !m_upperInclusive)) {
          log.DebugFormat ("IsEmpty: " +
                           "empty because, lower {0} and upper {1} are equal but one of them is not inclusive",
                           m_lower, m_upper);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Convert a range to a string considering a ToString() method of the bound
    /// </summary>
    /// <param name="convertBoundToString"></param>
    /// <returns></returns>
    public virtual string ToString (Func<T, string> convertBoundToString)
    {
      if (m_empty) {
        return "empty";
      }

      if (m_upper.HasValue && m_lower.HasValue) {
        if (m_upper.Value.CompareTo (m_lower.Value) < 0) {
          // Note: this fixes a problem with PostgreSQL
          // A request in PostgreSQL ends in error if Upper<Lower
          log.WarnFormat ("ToString: " +
                          "the upper bound is before the lower bound " +
                          "=> return empty instead of {0}{1},{2}{3} " +
                          "Stack: {4}",
                          m_lowerInclusive ? "[" : "(",
                          m_lower.HasValue ? convertBoundToString (m_lower.Value) : "",
                          m_upper.HasValue ? convertBoundToString (m_upper.Value) : "",
                          m_upperInclusive ? "]" : ")",
                          System.Environment.StackTrace);
          return "empty";
        }
        else if (object.Equals (m_upper.Value, m_lower.Value)
                 && (!m_lowerInclusive || !m_upperInclusive)) {
          log.WarnFormat ("ToString: " +
                          "lower {1} and upper {2} are equal but one of them is not inclusive " +
                          "=> return empty instead {0}{1},{2}{3} " +
                          "Stack: {4}",
                          m_lowerInclusive ? "[" : "(",
                          m_lower.HasValue ? convertBoundToString (m_lower.Value) : "",
                          m_upper.HasValue ? convertBoundToString (m_upper.Value) : "",
                          m_upperInclusive ? "]" : ")",
                          System.Environment.StackTrace);
          return "empty";
        }
      }

      return string.Format ("{0}{1},{2}{3}",
                            LowerInclusive ? "[" : "(",
                            Lower.HasValue ? convertBoundToString (Lower.Value) : "",
                            Upper.HasValue ? convertBoundToString (Upper.Value) : "",
                            UpperInclusive ? "]" : ")");
    }

    /// <summary>
    /// To string
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return ToString (ConvertBoundToString);
    }

    /// <summary>
    /// Convert a bound to a string
    /// </summary>
    /// <param name="bound"></param>
    /// <returns></returns>
    protected virtual string ConvertBoundToString (T bound)
    {
      return bound.ToString ();
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      if (this.IsEmpty ()) {
        return 0;
      }
      else {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * this.Lower.GetHashCode ();
          hashCode += 1000000009 * this.Upper.GetHashCode ();
          hashCode *= 4;
        }
        if (this.LowerInclusive) {
          hashCode += 1;
        }
        if (this.UpperInclusive) {
          hashCode += 2;
        }
        return hashCode;
      }
    }
    #endregion // Methods

    #region ICloneable implementation
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public object Clone ()
    {
      return this.MemberwiseClone ();
    }
    #endregion // ICloneable implementation

    #region Operators
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (Range<T> other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      Range<T> other = obj as Range<T>;
      return null != other && Range<T>.Equals (this, other);
    }

    /// <summary>
    /// Check the equality
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Equals (Range<T> a, Range<T> b)
    {
      if (object.ReferenceEquals (a, b)) {
        return true;
      }

      if ((null == a) && (null == b)) {
        return true;
      }
      if ((null == a) || (null == b)) {
        return false;
      }

      if (a.IsEmpty () && b.IsEmpty ()) {
        log.DebugFormat ("Equals: " +
                         "because both empty");
        return true;
      }
      else if (a.IsEmpty () || b.IsEmpty ()) {
        log.DebugFormat ("Equals: " +
                         "only one of the two arguments is empty => return false");
        return false;
      }
      else if (object.Equals (a.Lower, b.Lower) && object.Equals (a.Upper, b.Upper)
               && object.Equals (a.LowerInclusive, b.LowerInclusive)
               && object.Equals (a.UpperInclusive, b.UpperInclusive)) {
        return true;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Contains operator
    /// 
    /// Corresponds to Operator @&gt; in PostgreSQL
    /// 
    /// If the operator can't be applied, false is returned
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool ContainsRange (Range<T> other)
    {
      if (this.IsEmpty ()) {
        log.WarnFormat ("ContainsRange: " +
                        "empty, return false");
        return false;
      }
      else if (other.IsEmpty ()) {
        return true;
      }
      else if (!this.Lower.HasValue && !this.Upper.HasValue) { // (,) => true
        return true;
      }
      else if (!this.Lower.HasValue) { // (,... => compare upper only
        Debug.Assert (this.Upper.HasValue);
        if (!other.Upper.HasValue) {
          return false;
        }
        else if (object.Equals (this.Upper.Value, other.Upper.Value)) { // Consider inclusivity
          return !other.UpperInclusive || this.UpperInclusive;
        }
        else if (other.Upper.Value.CompareTo (this.Upper.Value) < 0) {
          return true;
        }
        else {
          return false;
        }
      }
      else if (!this.Upper.HasValue) { // ...,) => compare lower only
        Debug.Assert (this.Lower.HasValue);
        if (!other.Lower.HasValue) {
          return false;
        }
        else if (object.Equals (this.Lower.Value, other.Lower.Value)) { // Consider inclusivity
          return !other.LowerInclusive || this.LowerInclusive;
        }
        else if (this.Lower.Value.CompareTo (other.Lower.Value) < 0) {
          return true;
        }
        else {
          return false;
        }
      }
      else {
        Debug.Assert (this.Lower.HasValue);
        Debug.Assert (this.Upper.HasValue);
        if (!other.Lower.HasValue) {
          return false;
        }
        else if (!other.Upper.HasValue) {
          return false;
        }
        else if (other.Lower.Value.CompareTo (this.Lower.Value) < 0) {
          return false;
        }
        else if (this.Upper.Value.CompareTo (other.Upper.Value) < 0) {
          return false;
        }
        else if (object.Equals (this.Lower.Value, other.Lower.Value)
                 && other.LowerInclusive && !this.LowerInclusive) {
          return false;
        }
        else if (object.Equals (this.Upper.Value, other.Upper.Value)
                 && other.UpperInclusive && !this.UpperInclusive) {
          return false;
        }
        else {
          return true;
        }
      }
    }

    /// <summary>
    /// Contains operator
    /// 
    /// Corresponds to Operator @&gt; in PostgreSQL
    /// 
    /// If the operator can't be applied, false is returned
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool ContainsElement (T element)
    {
      return ContainsRange (new Range<T> (element, element, "[]"));
    }

    /// <summary>
    /// Contains operator
    /// 
    /// Corresponds to Operator @&gt; in PostgreSQL
    /// 
    /// If the operator can't be applied, false is returned
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool ContainsElement (Bound<T> element)
    {
      if (element.HasValue) {
        return ContainsRange (new Range<T> (element.Value, element.Value, "[]"));
      }
      else { // !element.HasValue
        if (element.BoundType == BoundType.Lower) {
          return !this.IsEmpty () && !this.Lower.HasValue;
        }
        else { // BountType.Upper
          Debug.Assert (element.BoundType == BoundType.Upper);
          return !this.IsEmpty () && !this.Upper.HasValue;
        }
      }
    }

    /// <summary>
    /// Overlap operator
    /// 
    /// Corresponds to Operator &amp;&amp; in PostgreSQL
    /// 
    /// If the operator can't be applied, false is returned
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Overlaps (Range<T> other)
    {
      if (this.IsEmpty () || other.IsEmpty ()) {
        log.WarnFormat ("Overlaps: " +
                        "empty, return false. StackTrace: {0}",
                        System.Environment.StackTrace);
        return false;
      }
      else if (!this.Lower.HasValue && !this.Upper.HasValue) { // (,) => true
        return true;
      }
      else if (!this.Lower.HasValue) { // (,... => compare upper only
        Debug.Assert (this.Upper.HasValue);
        if (!other.Lower.HasValue) {
          return true;
        }
        else if (object.Equals (this.Upper.Value, other.Lower.Value)) { // Consider inclusivity
          return other.LowerInclusive && this.UpperInclusive;
        }
        else if (other.Lower.Value.CompareTo (this.Upper.Value) < 0) {
          return true;
        }
        else {
          return false;
        }
      }
      else if (!this.Upper.HasValue) { // ...,) => compare lower only
        Debug.Assert (this.Lower.HasValue);
        if (!other.Upper.HasValue) {
          return true;
        }
        else if (object.Equals (this.Lower.Value, other.Upper.Value)) { // Consider inclusivity
          return other.UpperInclusive && this.LowerInclusive;
        }
        else if (this.Lower.Value.CompareTo (other.Upper.Value) < 0) {
          return true;
        }
        else {
          return false;
        }
      }
      else { // [(...,...)]
        Debug.Assert (this.Upper.HasValue);
        Debug.Assert (this.Lower.HasValue);
        if (other.Upper.HasValue && other.Lower.HasValue) {
          if (object.Equals (other.Lower.Value, this.Upper.Value)) {
            return other.LowerInclusive && this.UpperInclusive;
          }
          else if (object.Equals (other.Upper.Value, this.Lower.Value)) {
            return other.UpperInclusive && this.LowerInclusive;
          }
          else {
            return (other.Lower.Value.CompareTo (this.Upper.Value) < 0)
              && (this.Lower.Value.CompareTo (other.Upper.Value) < 0);
          }
        }
        else { // Reverse it ! In other, there is -oo or +oo
          return other.Overlaps (this);
        }
      }
    }

    /// <summary>
    /// Strictly left operator
    /// 
    /// Corresponds to Operator &lt;&lt; in PostgreSQL
    /// 
    /// If the operator can't be applied, false is returned
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsStrictlyLeftOf (Range<T> other)
    {
      if (this.IsEmpty () || other.IsEmpty ()) {
        log.WarnFormat ("IsStrictlyLeftOf: " +
                        "one of the value is null, return false");
        return false;
      }
      else if (!this.Upper.HasValue) { // +oo
        log.DebugFormat ("IsStrictlyLeftOf: " +
                         "{0} << {1} " +
                         "=>" +
                         "return false because this.Upper=+oo",
                         this, other);
        return false;
      }
      else if (!other.Lower.HasValue) { // -oo
        log.DebugFormat ("IsStrictlyLeftOf: " +
                         "{0} << {1} " +
                         "=> return false because other.Lower=-oo",
                         this, other);
        return false;
      }
      else if (this.Upper.Value.CompareTo (other.Lower.Value) < 0) {
        return true;
      }
      else if (object.Equals (this.Upper.Value, other.Lower.Value)
               && (!this.UpperInclusive || (!other.LowerInclusive))) {
        log.DebugFormat ("IsStrictlyLeftOf: " +
                         "{0} << {1} " +
                         "=> return true because the bounds are equals but one of them is exclusive",
                         this, other);
        return true;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Strictly right operator
    /// 
    /// Corresponds to Operator &gt;&gt; in PostgreSQL
    /// 
    /// If the operator can't be applied, false is returned
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsStrictlyRightOf (Range<T> other)
    {
      if (this.IsEmpty () || other.IsEmpty ()) {
        log.WarnFormat ("IsStrictlyRightOf: " +
                        "one of the value is null, return false");
        return false;
      }
      else if (!other.Upper.HasValue) { // +oo
        log.DebugFormat ("IsStrictlyRightOf: " +
                         "{0} >> {1} " +
                         "=>" +
                         "return false because other.Upper=+oo",
                         this, other);
        return false;
      }
      else if (!this.Lower.HasValue) { // -oo
        log.DebugFormat ("IsStrictlyRightOf: " +
                         "{0} >> {1} " +
                         "=> return false because this.Lower=-oo",
                         this, other);
        return false;
      }
      else if (other.Upper.Value.CompareTo (this.Lower.Value) < 0) {
        return true;
      }
      else if (object.Equals (other.Upper.Value, this.Lower.Value)
               && (!other.UpperInclusive || (!this.LowerInclusive))) {
        log.DebugFormat ("IsStrictlyRightOf: " +
                         "{0} >> {1} " +
                         "=> return true because the bounds are equals but one of them is exclusive",
                         this, other);
        return true;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Is adjacent to operator
    /// 
    /// Corresponds to Operator -|- in PostgreSQL
    /// 
    /// If the operator can't be applied, false is returned
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool IsAdjacentTo (Range<T> other)
    {
      if (this.IsEmpty () || other.IsEmpty ()) {
        log.WarnFormat ("IsAdjacentTo: " +
                        "empty, return false. " +
                        "StackTrace={0}",
                        System.Environment.StackTrace);
        return false;
      }
      else if (this.Upper.HasValue
               && other.Lower.HasValue
               && object.Equals (this.Upper.Value, other.Lower.Value)
               && (this.UpperInclusive || other.LowerInclusive)
               && !(this.UpperInclusive && other.LowerInclusive)) {
        return true;
      }
      else if (this.Lower.HasValue
               && other.Upper.HasValue
               && object.Equals (this.Lower.Value, other.Upper.Value)
               && (this.LowerInclusive || other.UpperInclusive)
               && !(this.LowerInclusive && other.UpperInclusive)) {
        return true;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Union operator
    /// 
    /// This corresponds to + in PostgreSQL
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Range<T> Union (Range<T> other)
    {
      if (other.IsEmpty ()) {
        return (Range<T>)this.Clone ();
      }
      else if (this.IsEmpty ()) {
        return (Range<T>)other.Clone ();
      }
      else if (this.IsAdjacentTo (other)
               || this.Overlaps (other)) { // Ok the union is possible
        Debug.Assert (!this.IsEmpty ());
        Debug.Assert (!other.IsEmpty ());

        // - Consider the lowest value
        LowerBound<T> lower;
        bool lowerInclusive;
        if (!this.Lower.HasValue || !other.Lower.HasValue) {
          lower = new LowerBound<T> (null);
          lowerInclusive = false;
        }
        else if (object.Equals (this.Lower, other.Lower)) {
          lower = this.Lower;
          lowerInclusive = (this.LowerInclusive || other.LowerInclusive);
        }
        else if (this.Lower.Value.CompareTo (other.Lower.Value) < 0) {
          lower = this.Lower;
          lowerInclusive = this.LowerInclusive;
        }
        else {
          Debug.Assert (other.Lower.Value.CompareTo (this.Lower.Value) < 0);
          lower = other.Lower;
          lowerInclusive = other.LowerInclusive;
        }

        // - Consider the uppest value
        UpperBound<T> upper;
        bool upperInclusive;
        if (!this.Upper.HasValue || !other.Upper.HasValue) {
          upper = new UpperBound<T> (null);
          upperInclusive = false;
        }
        else if (object.Equals (this.Upper, other.Upper)) {
          upper = this.Upper;
          upperInclusive = (this.UpperInclusive || other.UpperInclusive);
        }
        else if (this.Upper.Value.CompareTo (other.Upper.Value) < 0) {
          upper = other.Upper;
          upperInclusive = other.UpperInclusive;
        }
        else {
          Debug.Assert (other.Upper.Value.CompareTo (this.Upper.Value) < 0);
          upper = this.Upper;
          upperInclusive = this.UpperInclusive;
        }

        Range<T> newRange = new Range<T> (lower, upper);
        newRange.LowerInclusive = lowerInclusive;
        newRange.UpperInclusive = upperInclusive;
        return newRange;
      }
      else { // Not adjacent and no overlap
        log.ErrorFormat ("Union: " +
                         "operation not possible because {0} and {1} are not overlapping or are not adjacent",
                         this, other);
        throw new ArgumentException ("Union not possible");
      }
    }

    /// <summary>
    /// Intersection operator
    /// 
    /// This corresponds to * in PostgreSQL
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Range<T> Intersects (Range<T> other)
    {
      if (!this.Overlaps (other)) {
        log.DebugFormat ("Intersects: " +
                         "{0} and {1} do not overlap " +
                         "=> return an empty range",
                         this, other);
        return new Range<T> ();
      }
      else {
        Debug.Assert (!this.IsEmpty ());
        Debug.Assert (!other.IsEmpty ());

        // - Consider the lowest value
        LowerBound<T> lower;
        bool lowerInclusive;
        if (!this.Lower.HasValue) {
          lower = other.Lower;
          lowerInclusive = other.LowerInclusive;
        }
        else if (!other.Lower.HasValue) {
          lower = this.Lower;
          lowerInclusive = this.LowerInclusive;
        }
        else if (object.Equals (this.Lower, other.Lower)) {
          lower = this.Lower;
          lowerInclusive = (this.LowerInclusive && other.LowerInclusive);
        }
        else if (this.Lower.Value.CompareTo (other.Lower.Value) < 0) {
          lower = other.Lower;
          lowerInclusive = other.LowerInclusive;
        }
        else {
          Debug.Assert (other.Lower.Value.CompareTo (this.Lower.Value) < 0);
          lower = this.Lower;
          lowerInclusive = this.LowerInclusive;
        }

        UpperBound<T> upper;
        bool upperInclusive;
        if (!this.Upper.HasValue) {
          upper = other.Upper;
          upperInclusive = other.UpperInclusive;
        }
        else if (!other.Upper.HasValue) {
          upper = this.Upper;
          upperInclusive = this.UpperInclusive;
        }
        else if (object.Equals (this.Upper, other.Upper)) {
          upper = this.Upper;
          upperInclusive = (this.UpperInclusive && other.UpperInclusive);
        }
        else if (this.Upper.Value.CompareTo (other.Upper.Value) < 0) {
          upper = this.Upper;
          upperInclusive = this.UpperInclusive;
        }
        else {
          Debug.Assert (other.Upper.Value.CompareTo (this.Upper.Value) < 0);
          upper = other.Upper;
          upperInclusive = other.UpperInclusive;
        }

        Range<T> newRange = new Range<T> (lower, upper);
        newRange.LowerInclusive = lowerInclusive;
        newRange.UpperInclusive = upperInclusive;
        return newRange;
      }
    }
    #endregion // Operators

    #region IComparable implementation
    /// <summary>
    /// Consider first Lower, then Upper
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo (object obj)
    {
      if (obj is Range<T>) {
        Range<T> other = (Range<T>)obj;
        int lowerCompare = this.Lower.CompareTo (other.Lower);
        if (0 != lowerCompare) {
          return lowerCompare;
        }
        else {
          return this.Upper.CompareTo (other.Upper);
        }
      }

      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("obj is not of the same type than this instance", "obj");
    }

    #endregion // IComparable implementation

    #region IComparable<T> implementation
    /// <summary>
    /// Consider first Lower, then Upper
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo (Range<T> other)
    {
      int lowerCompare = this.Lower.CompareTo (other.Lower);
      if (0 != lowerCompare) {
        return lowerCompare;
      }
      else {
        return this.Upper.CompareTo (other.Upper);
      }
    }

    #endregion // IComparable<T> implementation
  }
}
