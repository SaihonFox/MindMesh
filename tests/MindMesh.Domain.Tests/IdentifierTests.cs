using MindMesh.Domain.Identifiers;

namespace MindMesh.Domain.Tests;

public class IdentifierTests
{
	[Fact]
	public void ProjectId_New_IsNotEmptyAndUnique()
	{
		var first = ProjectId.New();
		var second = ProjectId.New();

		Assert.False(first.IsEmpty);
		Assert.NotEqual(first, second);
	}

	[Fact]
	public void ProjectId_RoundTripsThroughText()
	{
		var id = ProjectId.New();

		Assert.Equal(id, ProjectId.Parse(id.ToString()));
	}

	[Fact]
	public void ProjectId_Empty_IsEmpty()
	{
		Assert.True(ProjectId.Empty.IsEmpty);
		Assert.True(default(ProjectId).IsEmpty);
	}

	[Fact]
	public void WorkflowId_New_IsNotEmptyAndUnique()
	{
		var first = WorkflowId.New();
		var second = WorkflowId.New();

		Assert.False(first.IsEmpty);
		Assert.NotEqual(first, second);
	}

	[Fact]
	public void WorkflowId_RoundTripsThroughText()
	{
		var id = WorkflowId.New();

		Assert.Equal(id, WorkflowId.Parse(id.ToString()));
	}

	[Fact]
	public void WorkflowId_ToString_UsesHyphenatedLowercaseGuid()
	{
		var guid = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

		Assert.Equal("0f8fad5b-d9cb-469f-a165-70867728950e", new WorkflowId(guid).ToString());
	}

	[Fact]
	public void Parse_WithInvalidText_Throws()
	{
		Assert.Throws<FormatException>(() => ProjectId.Parse("not-a-guid"));
		Assert.Throws<FormatException>(() => WorkflowId.Parse("not-a-guid"));
	}

	[Fact]
	public void IdentifiersOfDifferentKinds_AreDifferentTypes()
	{
		var guid = Guid.NewGuid();

		Assert.NotEqual<object>(new ProjectId(guid), new WorkflowId(guid));
	}
}
