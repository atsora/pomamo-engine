// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 335:
  /// </summary>
  [Migration(335)]
  public class AddDisplayFunctions: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddDisplayFunctions).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CorrectDatabaseUp ();
      NullFunctionsUp ();
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displaytranslatetable (tablename NAME) RETURNS NAME AS
$BODY$
  SELECT CASE
    WHEN lower(tablename)='user' THEN 'usertable'
    WHEN lower(tablename)='path' THEN 'pathtable'
    WHEN lower(tablename)='line' THEN 'linetable'
    ELSE tablename
    END;
$BODY$ LANGUAGE SQL IMMUTABLE;
");
      DisplayFromNameTranslationUp ();
      DisplayFunctionsUp ();
      TriggerUp ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      TriggerDown ();
      DisplayFunctionsDown ();
      DisplayFromNameTranslationDown ();
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displaytranslatetable (name);
");
      NullFunctionsDown ();
      CorrectDatabaseDown ();
    }
    
    void CorrectDatabaseUp ()
    {
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(\w+)%>', '<%Component.\1%>', 'g')
WHERE displaytable='part';
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(\w+)%>', '<%Operation.\1%>', 'g')
WHERE displaytable='simpleoperation';
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(\w+)%>', '<%Project.\1%>', 'g')
WHERE displaytable='job';
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(\w+)%>', '<%Machine.\1%>', 'g')
WHERE displaytable='monitoredmachine';
");
      
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(Tool|Company|Department|MachineCategory|MachineSubCategory)%>', '<^\1.Display^>', 'g');
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(Category|SubCategory)%>', '<^Machine\1.Display^>', 'g');
");      
    }
    
    void CorrectDatabaseDown ()
    {
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%Component\.(\w+)%>', '<%\1%>', 'g')
WHERE displaytable='part';
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%Operation\.(\w+)%>', '<%\1%>', 'g')
WHERE displaytable='simpleoperation';
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%Project\.(\w+)%>', '<%\1%>', 'g')
WHERE displaytable='job';
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%Machine\.(\w+)%>', '<%\1%>', 'g')
WHERE displaytable='monitoredmachine';
");
      
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(Tool|Company|Department|MachineCategory|MachineSubCategory)\.Display%>', '<%\1%>', 'g');
");
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%Machine(Category|SubCategory)\.Display%>', '<%\1%>', 'g');
");
    }
    
    void NullFunctionsUp ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.equalevennull(
    text1 citext,
    text2 citext)
  RETURNS boolean AS
$BODY$
  SELECT (text1 IS NOT NULL AND text2 IS NOT NULL AND text1=text2) OR (text1 IS NULL AND text2 IS NULL)
$BODY$ LANGUAGE SQL IMMUTABLE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.equalevennull(
    text1 text,
    text2 text)
  RETURNS boolean AS
$BODY$
  SELECT (text1 IS NOT NULL AND text2 IS NOT NULL AND text1=text2) OR (text1 IS NULL AND text2 IS NULL)
$BODY$ LANGUAGE SQL IMMUTABLE;
");
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS public.===(citext, citext);");
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS public.===(text, text);");
      Database.ExecuteNonQuery (@"
CREATE OPERATOR public.===(
  PROCEDURE = equalevennull,
  LEFTARG = citext,
  RIGHTARG = citext,
  COMMUTATOR = ===);
");
      Database.ExecuteNonQuery (@"
CREATE OPERATOR public.===(
  PROCEDURE = equalevennull,
  LEFTARG = text,
  RIGHTARG = text,
  COMMUTATOR = ===);
");
      Database.ExecuteNonQuery (@"
      CREATE OR REPLACE FUNCTION nulltoempty (s TEXT) RETURNS text AS
$BODY$
  SELECT CASE WHEN s IS NULL THEN '' ELSE s END;
$BODY$ LANGUAGE SQL IMMUTABLE;
");
    }
    
    void NullFunctionsDown ()
    {
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS nulltoempty(text);");
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS public.===(citext, citext);");
      Database.ExecuteNonQuery (@"DROP OPERATOR IF EXISTS public.===(text, text);");
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS equalevennull(text, text);");
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS equalevennull(citext, citext);");
    }
    
    void DisplayFromNameTranslationUp ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.displayfromnametranslationkeylocale(
    name character varying,
    translationkey_param character varying,
    locale_param character varying)
  RETURNS character varying AS
$BODY$
DECLARE
  display varchar;
BEGIN
  IF ( name is not null ) THEN
    IF name <> '''' AND name <> '' THEN
      RETURN name;
    END IF;
  END IF;
  IF ( translationkey_param is null ) THEN
    RETURN name;
  END IF;
  
  SELECT into display translationvalue FROM translation WHERE translation.translationkey = translationkey_param AND translation.locale = locale_param;
  IF FOUND THEN
    RETURN display;
  END IF;
  SELECT into display translationvalue FROM translation WHERE translation.translationkey = translationkey_param AND translation.locale = substr(locale_param, 0, 2);
  IF FOUND THEN
    RETURN display;
  END IF;
  SELECT into display translationvalue FROM translation WHERE translation.translationkey = translationkey_param AND translation.locale = '';
  IF FOUND THEN
    RETURN display;
  END IF;
  RETURN name;
END;
$BODY$
  LANGUAGE plpgsql STABLE
  COST 100;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION configvalueextract (
    configvalue character varying)
  RETURNS character varying AS
$BODY$
  SELECT regexp_replace (configvalue, '</?\w+/?>', '', 'g');
$BODY$ LANGUAGE SQL IMMUTABLE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayfromnametranslationkey(
    name_param character varying,
    translationkey_param character varying)
  RETURNS character varying AS
$BODY$
  SELECT displayfromnametranslationkeylocale (name_param, translationkey_param, configvalueextract(configvalue))
  FROM config
  WHERE configkey='i18n.locale.default'
  UNION
  SELECT displayfromnametranslationkeylocale (name_param, translationkey_param, '');
$BODY$ LANGUAGE SQL STABLE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayfromnametranslationkeylist() RETURNS SETOF NAME AS
$BODY$
  SELECT unnest(ARRAY['machinemode'::name,'machinestatetemplate'::name,'goaltype'::name,'eventlevel'::name,'role'::name,'machinemonitoringtype'::name,
    'machineobservationstate'::name,'operationtype'::name,'unit'::name,'workorderstatus'::name,'componenttype'::name]) AS tablename;
$BODY$ LANGUAGE SQL IMMUTABLE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayfromnametranslationkeyupdate (tablename NAME) RETURNS void AS
$BODY$
DECLARE
  _displayrequest TEXT;
  _definition TEXT;
  _name NAME;
BEGIN
  _name := 'display';
  _displayrequest := 'SELECT displayfromnametranslationkey($1.' || tablename || 'name::character varying, $1.' || tablename || 'translationkey::character varying)';
  RAISE NOTICE 'Display request: %', _displayrequest;
  _definition = 'CREATE OR REPLACE FUNCTION ' || tablename || _name || '(' || displaytranslatetable(tablename) || ') RETURNS text AS $$ '|| _displayrequest || '$$ LANGUAGE SQL IMMUTABLE;';
  RAISE NOTICE 'Definition for %: %', tablename, _definition;
  EXECUTE _definition;
  _definition = 'CREATE OR REPLACE FUNCTION ' || _name || '(' || displaytranslatetable(tablename) || ') RETURNS text AS $$ '|| _displayrequest || '$$ LANGUAGE SQL IMMUTABLE;';
  EXECUTE _definition;
END;
$BODY$ LANGUAGE 'plpgsql' VOLATILE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayfromnametranslationkeyupdate () RETURNS void AS
$BODY$
BEGIN
  PERFORM displayfromnametranslationkeyupdate (displayfromnametranslationkeylist)
  FROM displayfromnametranslationkeylist ();
END;
$BODY$ LANGUAGE 'plpgsql' VOLATILE;
");
    }
    
    void DisplayFromNameTranslationDown ()
    {
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayfromnametranslationkeyupdate();
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayfromnametranslationkeyupdate(name);
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayfromnametranslationkeylist();
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayfromnametranslationkey (character varying, character varying);
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS configvalueextract (character varying);
");
    }
    
    void DisplayFunctionsUp ()
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
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayrequest (tablename NAME, variant TEXT DEFAULT NULL) RETURNS text AS
$BODY$
  SELECT E'SELECT E\'SELECT \\\'' --'
    || regexp_replace (displaypattern, '<%([\w.]+)%>', E'\' || E\'\\\' || \' || displayextract(\'' || tablename || E'\',\'\\1\') || E\' || \\\'\' || \'', 'g')
    || E'\' || E\'\\\'::text\'' --'
  FROM display
  WHERE display.displaytable=tablename::citext
    AND variant===displayvariant;
$BODY$ LANGUAGE SQL STABLE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayupdate (tablename NAME, variant CITEXT) RETURNS void AS
$BODY$
DECLARE
  _displayrequest TEXT;
  _definition TEXT;
  _name NAME;
BEGIN
  _name := 'display';
  IF variant IS NOT NULL THEN
    _name := _name || '_' || lower(variant);
  END IF;
  _displayrequest := displayrequest(tablename, variant);
  IF _displayrequest IS NULL THEN _displayrequest=''; END IF;
  RAISE NOTICE 'Display request: %', _displayrequest;
  EXECUTE _displayrequest INTO _displayrequest;
  _definition = 'CREATE OR REPLACE FUNCTION ' || tablename || _name || '(' || displaytranslatetable(tablename) || ') RETURNS text AS $$ '|| _displayrequest || '$$ LANGUAGE SQL IMMUTABLE;';
  RAISE NOTICE 'Definition for %, %: %', tablename, variant, _definition;
  EXECUTE _definition;
  _definition = 'CREATE OR REPLACE FUNCTION ' || _name || '(' || displaytranslatetable(tablename) || ') RETURNS text AS $$ '|| _displayrequest || '$$ LANGUAGE SQL IMMUTABLE;';
  EXECUTE _definition;
END;
$BODY$ LANGUAGE 'plpgsql' VOLATILE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayexclude () RETURNS SETOF NAME AS
$BODY$
  SELECT unnest(ARRAY['process'::name,'shopfloordisplay'::name,'buffer'::name]); -- buffer table does not exist yet
$BODY$ LANGUAGE SQL IMMUTABLE ROWS 3;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayupdate (tablename NAME) RETURNS void AS
$BODY$
BEGIN
  PERFORM displayupdate (tablename, displayvariant)
  FROM display
  WHERE displaytable=tablename::citext
    AND lower(displaytable) NOT IN (SELECT * FROM displayexclude ());
END;
$BODY$ LANGUAGE 'plpgsql' VOLATILE;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayupdate () RETURNS void AS
$BODY$
BEGIN
  PERFORM displayupdate (lower(displaytable)::NAME, displayvariant)
  FROM display
  WHERE lower(displaytable) NOT IN (SELECT * FROM displayexclude ());
END;
$BODY$ LANGUAGE 'plpgsql' VOLATILE;
");
    }
    
    void DisplayFunctionsDown ()
    {
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayupdate ();
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayupdate (name);
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayexclude ();
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayupdate (name, citext);
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayrequest(name, text);
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS displayextract(name, text, text);
");
    }
    
    void TriggerUp ()
    {
      Database.ExecuteNonQuery (@"
DROP TRIGGER IF EXISTS display_updater_trigger ON display;
");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION display_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  PERFORM displayupdate (NEW.displaytable::NAME)
  WHERE NEW.displaytable NOT IN (SELECT * from displayexclude ());
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
");
      Database.ExecuteNonQuery (@"
CREATE TRIGGER display_updater_trigger
  AFTER INSERT OR UPDATE
  ON display
  FOR EACH ROW
  EXECUTE PROCEDURE display_updater();
");
    }
    
    void TriggerDown ()
    {
      Database.ExecuteNonQuery (@"
DROP TRIGGER IF EXISTS display_updater_trigger ON display;
");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS public.display_updater();
");
    }
  }
}
