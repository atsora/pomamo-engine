// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: LGPL-2.1-only

/* -*- c# -*- ******************************************************************
 * Copyright (C) 2009-2023  Lemoine Automation Technologies
 * This library is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation; either version 2.1 of the License, or (at your option) any later version.
 * This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more details.
 * You should have received a copy of the GNU Lesser General Public License along with this library; if not, write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA Also add information on how to contact you by electronic and paper mail.
*/

namespace NHibernate.Dialect
{
  /// <summary>
  /// An SQL dialect for PostgreSQL 8.3 and above, specific for Pomamo
  /// </summary>
  /// <remarks>
  /// <para>
  /// The support for identify columns is removed to keep using the event tables
  /// that share a same sequence.
  /// </para>
  /// </remarks>
  /// <seealso cref="PostgreSQL81Dialect" />
  public class PostgreSQLPulseDialect : PostgreSQL83Dialect
  {
    /// <summary>
    /// PostgreSQL supports Identity column using the "SERIAL" type.
    /// </summary>
    public override bool SupportsIdentityColumns
    {
      get { return false; }
    }
  }
}
