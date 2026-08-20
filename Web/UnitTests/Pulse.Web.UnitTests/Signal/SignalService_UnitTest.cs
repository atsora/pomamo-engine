// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Business.Group.Impl;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Extensions.Web;
using Pulse.Web.Signal;

namespace Pulse.Web.UnitTests.Signal
{
  /// <summary>
  /// Group extension for the tests, that defines a single static group with all the machines
  /// </summary>
  public class TestGroupExtension : IGroupExtension
  {
    /// <summary>
    /// Id of the group that is returned by this extension
    /// </summary>
    public static readonly string GROUP_ID = "ALL";

    /// <summary>
    /// <see cref="Lemoine.Extensions.IExtension"/>
    /// </summary>
    public bool UniqueInstance => true;

    /// <summary>
    /// <see cref="IGroupExtension"/>
    /// </summary>
    public bool Initialize () => true;

    /// <summary>
    /// <see cref="IGroupExtension"/>
    /// </summary>
    public string GroupCategoryName => "Test";

    /// <summary>
    /// <see cref="IGroupExtension"/>
    /// </summary>
    public double GroupCategorySortPriority => 0.0;

    /// <summary>
    /// <see cref="IGroupExtension"/>
    /// </summary>
    public bool OmitGroupCategory => false;

    /// <summary>
    /// <see cref="IGroupExtension"/>
    /// </summary>
    public bool OmitInMachineSelection => true;

    /// <summary>
    /// <see cref="IGroupExtension"/>
    /// </summary>
    public IEnumerable<IGroup> Groups => new List<IGroup> { CreateGroup () };

    /// <summary>
    /// <see cref="IGroupExtension"/>
    /// </summary>
    public GroupIdExtensionMatch GetGroupIdExtensionMatch (string groupId) =>
      GROUP_ID.Equals (groupId?.Trim (), StringComparison.InvariantCultureIgnoreCase)
        ? GroupIdExtensionMatch.Yes
        : GroupIdExtensionMatch.No;

    /// <summary>
    /// <see cref="IGroupExtension"/>
    /// </summary>
    public IGroup GetGroup (string groupId) => this.GetGroupFromGroups (groupId);

    IGroup CreateGroup ()
    {
      var machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll ();
      return new GroupFromMachineList (GROUP_ID, "All", "Test", machines, 0.0, false, false);
    }
  }

  /// <summary>
  /// Signal extension for the tests, that returns one signal for the group
  /// and one additional signal when a role is set
  /// </summary>
  public class TestSignalExtension : ISignalExtension
  {
    /// <summary>
    /// Background color of the group signal
    /// </summary>
    public static readonly string GROUP_COLOR = "#FFFFFF";

    /// <summary>
    /// Background color of the role signal
    /// </summary>
    public static readonly string ROLE_COLOR = "#000000";

    /// <summary>
    /// <see cref="Lemoine.Extensions.IExtension"/>
    /// </summary>
    public bool UniqueInstance => true;

    /// <summary>
    /// <see cref="ISignalExtension"/>
    /// </summary>
    public bool Initialize () => true;

    /// <summary>
    /// <see cref="ISignalExtension"/>
    /// </summary>
    public IList<Pulse.Extensions.Web.Signal> GetSignals (IGroup group, IRole role)
    {
      var signals = new List<Pulse.Extensions.Web.Signal> {
        new Pulse.Extensions.Web.Signal ($"Group {group.Id}", GROUP_COLOR)
      };
      if (role is not null) {
        signals.Add (new Pulse.Extensions.Web.Signal ($"Role {role.WebAppKey}", ROLE_COLOR));
      }
      return signals;
    }
  }

  /// <summary>
  /// Unit tests for <see cref="SignalService"/>
  /// </summary>
  public class SignalService_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (SignalService_UnitTest).FullName);

    readonly SignalService m_service = new SignalService ();

    /// <summary>
    /// Constructor
    /// </summary>
    public SignalService_UnitTest ()
    { }

    /// <summary>
    /// Test no signal is returned when no signal extension is active
    /// </summary>
    [Test]
    public async Task TestNoSignalExtension ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add<TestGroupExtension> ();
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var request = new SignalRequestDTO {
            GroupId = TestGroupExtension.GROUP_ID
          };

          var response = await m_service.Get (request) as SignalResponseDTO;

          Assert.That (response, Is.Not.Null);
          Assert.That (response.Messages, Is.Empty);
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the signals of the extensions are returned with their contrast color
    /// </summary>
    [Test]
    public async Task TestSignalsWithRole ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add<TestGroupExtension> ();
          Lemoine.Extensions.ExtensionManager.Add<TestSignalExtension> ();
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var request = new SignalRequestDTO {
            GroupId = TestGroupExtension.GROUP_ID,
            RoleKey = "manager"
          };

          var response = await m_service.Get (request) as SignalResponseDTO;

          Assert.That (response, Is.Not.Null);
          Assert.That (response.Messages, Has.Count.EqualTo (2));
          var groupMessage = response.Messages[0];
          var roleMessage = response.Messages[1];
          Assert.Multiple (() => {
            Assert.That (groupMessage.Message, Is.EqualTo ($"Group {TestGroupExtension.GROUP_ID}"));
            Assert.That (groupMessage.BgColor, Is.EqualTo (TestSignalExtension.GROUP_COLOR));
            Assert.That (groupMessage.FgColor, Is.EqualTo ("#000000"));
            // Note: the role web app key is stored in a case insensitive column
            Assert.That (roleMessage.Message, Is.EqualTo ("Role manager").IgnoreCase);
            Assert.That (roleMessage.BgColor, Is.EqualTo (TestSignalExtension.ROLE_COLOR));
            Assert.That (roleMessage.FgColor, Is.EqualTo ("#FFFFFF"));
          });
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test no role is transmitted to the extensions when no role key is set
    /// </summary>
    [Test]
    public async Task TestSignalsWithoutRole ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add<TestGroupExtension> ();
          Lemoine.Extensions.ExtensionManager.Add<TestSignalExtension> ();
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var request = new SignalRequestDTO {
            GroupId = TestGroupExtension.GROUP_ID
          };

          var response = await m_service.Get (request) as SignalResponseDTO;

          Assert.That (response, Is.Not.Null);
          Assert.That (response.Messages, Has.Count.EqualTo (1));
          Assert.That (response.Messages[0].Message, Is.EqualTo ($"Group {TestGroupExtension.GROUP_ID}"));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test an error is returned when the group does not exist
    /// </summary>
    [Test]
    public async Task TestUnknownGroup ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add<TestGroupExtension> ();
          Lemoine.Extensions.ExtensionManager.Add<TestSignalExtension> ();
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var request = new SignalRequestDTO {
            GroupId = "UnknownGroup"
          };

          var response = await m_service.Get (request) as ErrorDTO;

          Assert.That (response, Is.Not.Null);
          Assert.That (response.Status, Is.EqualTo (ErrorStatus.WrongRequestParameter));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test an error is returned when the role key does not match any role
    /// </summary>
    [Test]
    public async Task TestUnknownRole ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Extensions.ExtensionManager.Add<TestGroupExtension> ();
          Lemoine.Extensions.ExtensionManager.Add<TestSignalExtension> ();
          Lemoine.Extensions.ExtensionManager.Activate (false);
          Lemoine.Extensions.ExtensionManager.Load ();

          var request = new SignalRequestDTO {
            GroupId = TestGroupExtension.GROUP_ID,
            RoleKey = "UnknownRole"
          };

          var response = await m_service.Get (request) as ErrorDTO;

          Assert.That (response, Is.Not.Null);
          Assert.That (response.Status, Is.EqualTo (ErrorStatus.WrongRequestParameter));
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          transaction.Rollback ();
        }
      }
    }
  }
}
