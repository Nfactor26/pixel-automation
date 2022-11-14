﻿using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

[DataContract]   
public class TestFixture : Document
{   
    /// <summary>
    /// Identifier of the fixture
    /// </summary>
    [DataMember(IsRequired = true, Order = 30)]
    public string FixtureId { get; set; }

    /// <summary>
    /// Display name for the test fixture
    /// </summary>
    [DataMember(IsRequired = true, Order = 40)]
    public string DisplayName { get; set; }

    /// <summary>
    /// Order of execution of fixture amognst other fixtures
    /// </summary>
    [DataMember(IsRequired = true, Order = 50)]
    public int Order { get; set; }

    /// <summary>
    /// Indicates if the fixture is muted. Test case belonging to a muted fixture are skipped.
    /// </summary>
    [DataMember(IsRequired = true, Order = 60)]
    public bool IsMuted { get; set; }

    /// <summary>
    /// Initialization script file for the fixture
    /// </summary>
    [DataMember(IsRequired = true, Order = 70)]
    public string ScriptFile { get; set; }

    /// <summary>
    /// Description of the fixture
    /// </summary>
    [DataMember(IsRequired = false, Order = 80)]
    public string Description { get; set; }

    /// <summary>
    /// Category of the fixutre. Category can be used to filter test fixture for execution.
    /// </summary>
    [DataMember(IsRequired = true, Order = 90)]
    public string Category { get; set; } = "Default";

    /// <summary>
    /// Controls the delay for pre and post run of actors.
    /// </summary>
    [DataMember(IsRequired = true, Order = 100)]
    public int DelayFactor { get; set; } = 3;

    /// <summary>
    /// User defined key value pair that can be associated with test fixture.
    /// This can be used for filtering test fixture during execution.
    /// </summary>
    [DataMember(IsRequired = true, Order = 110)]
    public TagCollection Tags { get; private set; } = new TagCollection();

    /// <summary>
    /// Identifier of the Project
    /// </summary>
    [DataMember(IsRequired = true, Order = 900)]
    public string ProjectId { get; set; }

    /// <summary>
    /// Version of the Project
    /// </summary>
    [DataMember(IsRequired = true, Order = 910)]
    public string ProjectVersion { get; set; }


}
