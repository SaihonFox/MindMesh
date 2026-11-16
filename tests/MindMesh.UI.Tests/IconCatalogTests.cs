using System.Text.RegularExpressions;
using MindMesh.Nodes.BuiltIn;
using MindMesh.Presentation.Common;
using MindMesh.UI.Icons;

namespace MindMesh.UI.Tests;

public partial class IconCatalogTests
{
	[Fact]
	public void EveryIconKey_HasPathData()
	{
		foreach (var key in IconKeys.All)
		{
			Assert.True(IconCatalog.PathData.ContainsKey(key), $"Icon '{key}' has no path data.");
		}
	}

	[Fact]
	public void Catalog_HasNoIconThatIsNeitherAnAppIconNorANodeIcon()
	{
		foreach (var key in IconCatalog.PathData.Keys)
		{
			Assert.True(
				IconKeys.All.Contains(key) || NodeIcons.PathData.ContainsKey(key),
				$"Icon '{key}' belongs to nothing.");
		}
	}

	[Fact]
	public void NodeIconsDoNotShadowAppIcons()
	{
		foreach (var key in NodeIcons.PathData.Keys)
		{
			Assert.DoesNotContain(key, IconKeys.All);
		}
	}

	[Fact]
	public void EveryNodeAndCategoryIcon_Exists()
	{
		var keys = BuiltInExtension.CreateNodes().Select(node => node.IconKey)
			.Concat(BuiltInCategories.All.Select(category => category.IconKey))
			.Distinct()
			.ToList();

		Assert.NotEmpty(keys);

		foreach (var key in keys)
		{
			Assert.True(IconCatalog.PathData.ContainsKey(key), $"Icon '{key}' of a node or category has no path data.");
		}
	}

	[Fact]
	public void TheProjectKindIcon_Exists() => Assert.True(IconCatalog.PathData.ContainsKey("workflow"));

	[Fact]
	public void IconKeys_AreUnique()
	{
		Assert.Equal(IconKeys.All.Count, IconKeys.All.Distinct().Count());
	}

	[Fact]
	public void PathData_StartsWithAMoveCommandAndUsesOnlyPathCharacters()
	{
		foreach (var (key, data) in IconCatalog.PathData)
		{
			Assert.False(string.IsNullOrWhiteSpace(data), $"Icon '{key}' is empty.");
			Assert.Matches(PathCharacters(), data);
			Assert.True(data.StartsWith('M'), $"Icon '{key}' must start with a move command.");
		}
	}

	[Fact]
	public void PathData_HasNoNumbersFarOutsideTheDesignBox()
	{
		// Catches typos such as 112 instead of 12: no number may be far outside the 24 x 24 design box.
		foreach (var (key, data) in IconCatalog.PathData)
		{
			foreach (Match number in NumberPattern().Matches(data))
			{
				var value = double.Parse(number.Value, System.Globalization.CultureInfo.InvariantCulture);
				Assert.True(Math.Abs(value) <= 24.5, $"Icon '{key}' contains the out of range number {number.Value}.");
			}
		}
	}

	[GeneratedRegex(@"^[MmLlHhVvCcSsQqTtAaZz0-9.,\s\-]+$")]
	private static partial Regex PathCharacters();

	[GeneratedRegex(@"-?\d+(?:\.\d+)?")]
	private static partial Regex NumberPattern();
}
