// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

namespace Pulse.Extensions.Web.Graphql
{
  /// <summary>
  /// The authorization policies a field of a GraphQL schema may require
  ///
  /// They are not defined here: they are the very policies the REST services go through,
  /// registered by Lemoine.WebMiddleware.WebMiddlewareServices.AddAuthorization. This class
  /// only names them, so that a typo is a compilation error rather than an
  /// AUTH_POLICY_NOT_FOUND at run time
  ///
  /// A schema declares what a field requires with the Authorize attribute of the GraphQL
  /// library it is written against, which is why the names live here rather than in a type
  /// of that library
  /// </summary>
  public static class GraphqlPolicies
  {
    /// <summary>
    /// What a RequestDTO with no attribute gets: <see cref="AUTHORIZE"/> when the
    /// Authentication.Required configuration is on, <see cref="ANONYMOUS"/> otherwise
    ///
    /// It is the policy a field carries to be open or closed exactly like the REST services
    /// of the same web service
    /// </summary>
    public const string DEFAULT = "default";

    /// <summary>
    /// Requires an authenticated client whose access token has not expired
    /// </summary>
    public const string AUTHORIZE = "authorize";

    /// <summary>
    /// Lets everything through
    ///
    /// A field that must stay open whatever the configuration carries the AllowAnonymous
    /// attribute rather than this policy, the way a RequestDTO does
    /// </summary>
    public const string ANONYMOUS = "anonymous";
  }
}
