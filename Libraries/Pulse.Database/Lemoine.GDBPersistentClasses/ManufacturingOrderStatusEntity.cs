// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of the manuforderstatus table
  ///
  /// It is not named ManufacturingOrderStatus: the enum <see cref="Lemoine.Model.ManufacturingOrderStatus"/>,
  /// which a manufacturing order carries, already has this name
  /// </summary>
  [Serializable]
  public class ManufacturingOrderStatusEntity : DataWithTranslation, IManufacturingOrderStatus
  {
    int m_id = 0;
    int m_version = 0;
    string m_color = "#000000";

    /// <summary>
    /// <see cref="BaseData"/>
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers => new string[] { "Id", "Name", "TranslationKey" };

    /// <summary>
    /// Id, which is the value of <see cref="Status"/>
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id => m_id;

    /// <summary>
    /// <see cref="IVersionable"/>
    /// </summary>
    [XmlIgnore]
    public virtual int Version => m_version;

    /// <summary>
    /// <see cref="IManufacturingOrderStatus"/>
    /// </summary>
    [XmlIgnore]
    public virtual ManufacturingOrderStatus Status => (ManufacturingOrderStatus)m_id;

    /// <summary>
    /// <see cref="IManufacturingOrderStatus"/>
    /// </summary>
    [XmlAttribute ("Color")]
    public virtual string Color => m_color;

    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    protected ManufacturingOrderStatusEntity ()
    { }

    /// <summary>
    /// <see cref="ISerializableModel"/>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here
    }
  }
}
