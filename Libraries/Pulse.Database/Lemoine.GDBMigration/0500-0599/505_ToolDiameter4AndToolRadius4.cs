// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 505:
  /// </summary>
  [Migration(505)]
  public class ToolDiameter4AndToolRadius4: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof(ToolDiameter4AndToolRadius4).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddVirtualColumn (TableName.TOOL, TableName.TOOL + "diameter4", "numeric", "SELECT round($1.tooldiameter::numeric,4)");
      AddVirtualColumn (TableName.TOOL, TableName.TOOL + "radius4", "numeric", "SELECT round($1.toolradius::numeric,4)");

      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayextract (tablename NAME, pattern TEXT, prefix TEXT DEFAULT '$1.') RETURNS text AS
$BODY$
DECLARE
  _foreigntablepattern TEXT[];
  _foreigntable NAME;
  _foreignpattern TEXT;
BEGIN
  IF pattern='NameOrTranslation' THEN
    RETURN 'trim(nulltoempty(displayfromnametranslationkey (' || prefix || tablename || 'name, ' || prefix || tablename || 'translationkey)))';
  ELSIF pattern='Size' THEN
    RETURN E'\'(\' || ' || displayextract (tablename, 'Diameter4') || E'|| \' \' ||' || displayextract (tablename, 'Radius4') || E'|| \')\'';
  ELSIF pattern SIMILAR TO '\w+' THEN
    RETURN 'trim(nulltoempty(' || prefix || tablename || pattern || '::text))';
  ELSE
    _foreigntablepattern := regexp_matches(pattern, '(\w+)\.(\w+)');
    _foreigntable := _foreigntablepattern[1];
    _foreignpattern := _foreigntablepattern[2];
    RETURN 'nulltoempty((SELECT ' || displayextract (_foreigntable::name, _foreignpattern::text, (_foreigntable || '.')::text) || ' FROM ' || _foreigntable || ' WHERE ' || _foreigntable || 'id=' || prefix || _foreigntable || 'id)::text)';
  END IF;
END;
$BODY$ LANGUAGE 'plpgsql' IMMUTABLE;
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayextract (tablename NAME, pattern TEXT, prefix TEXT DEFAULT '$1.') RETURNS text AS
$BODY$
DECLARE
  _foreigntablepattern TEXT[];
  _foreigntable NAME;
  _foreignpattern TEXT;
BEGIN
  IF pattern='NameOrTranslation' THEN
    RETURN 'trim(nulltoempty(displayfromnametranslationkey (' || prefix || tablename || 'name, ' || prefix || tablename || 'translationkey)))';
  ELSIF pattern='Size' THEN
    RETURN E'\'(\' || ' || displayextract (tablename, 'Diameter') || E'|| \' \' ||' || displayextract (tablename, 'Radius') || E'|| \')\'';
  ELSIF pattern SIMILAR TO '\w+' THEN
    RETURN 'trim(nulltoempty(' || prefix || tablename || pattern || '::text))';
  ELSE
    _foreigntablepattern := regexp_matches(pattern, '(\w+)\.(\w+)');
    _foreigntable := _foreigntablepattern[1];
    _foreignpattern := _foreigntablepattern[2];
    RETURN 'nulltoempty((SELECT ' || displayextract (_foreigntable::name, _foreignpattern::text, (_foreigntable || '.')::text) || ' FROM ' || _foreigntable || ' WHERE ' || _foreigntable || 'id=' || prefix || _foreigntable || 'id)::text)';
  END IF;
END;
$BODY$ LANGUAGE 'plpgsql' IMMUTABLE;
");

      DropVirtualColumn (TableName.TOOL, TableName.TOOL + "diameter4");
      DropVirtualColumn (TableName.TOOL, TableName.TOOL + "radius4");
    }
  }
}
