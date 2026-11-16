using System.Text.Json.Serialization;
using MindMesh.Application.Settings;

namespace MindMesh.Infrastructure.Settings;

/// <summary>
/// Source-generated JSON metadata for the settings file (no reflection, trimming friendly).
/// </summary>
[JsonSourceGenerationOptions(
	WriteIndented = true,
	PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UseStringEnumConverter = true,
	ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
	AllowTrailingCommas = true)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext;
