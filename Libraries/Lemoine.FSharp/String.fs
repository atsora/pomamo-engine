module Lemoine.String

type System.String with
  /// Case insensitive equality for strings
  member s1.iequals(s2: string) =
    System.String.Equals (s1, s2, System.StringComparison.InvariantCultureIgnoreCase)
