namespace MindMesh.UI.Icons;

/// <summary>
/// Icons of node types and node categories. Their keys are plain text in the node definitions, so extensions can use any of them.
/// </summary>
internal static class NodeIcons
{
	public static IReadOnlyDictionary<string, string> PathData { get; } = new Dictionary<string, string>
	{
		["play"] = "M7 4.5v15l12-7.5z",
		["workflow"] = "M4 5h6v5H4zM14 14h6v5h-6zM10 7.5h2a2 2 0 0 1 2 2V14",
		["webhook"] = "M14.5 6a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0zM8.5 17a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0zM20.5 17a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0zM12 8.5v3.5M10.5 12.5l-3 2.5M13.5 12.5l3 2.5",
		["clock"] = "M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0zM12 7v5l3 2",
		["bolt"] = "M13 3L5 14h6l-1 7 8-11h-6z",
		["branch"] = "M6 4v12M18 8c0 3-2 5-5 5h-2c-3 0-5 1-5 3M8 18a2 2 0 1 1-4 0 2 2 0 0 1 4 0zM20 6a2 2 0 1 1-4 0 2 2 0 0 1 4 0z",
		["switch"] = "M4 7h4l8 10h4M4 17h4l3-4M13 11l3-4h4M18 5l2 2-2 2M18 15l2 2-2 2",
		["repeat"] = "M17 2l3 3-3 3M4 11V9a4 4 0 0 1 4-4h12M7 22l-3-3 3-3M20 13v2a4 4 0 0 1-4 4H4",
		["loop"] = "M21 12a9 9 0 1 1-3-6.7M21 4v5h-5",
		["merge"] = "M6 4v4a5 5 0 0 0 5 5h2a5 5 0 0 1 5 5v2M18 4v4a5 5 0 0 1-5 5",
		["hourglass"] = "M6 3h12M6 21h12M7 3c0 5 5 6 5 9s-5 4-5 9M17 3c0 5-5 6-5 9s5 4 5 9",
		["shield"] = "M12 3l8 3v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6z",
		["stop"] = "M6 6h12v12H6z",
		["globe"] = "M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0zM3 12h18M12 3c3 3 3 15 0 18M12 3c-3 3-3 15 0 18",
		["reply"] = "M9 14L4 9l5-5M4 9h9a7 7 0 0 1 7 7v2",
		["server"] = "M4 4h16v6H4zM4 14h16v6H4zM8 7h.01M8 17h.01",
		["variable"] = "M7 4C4 6 4 18 7 20M17 4c3 2 3 14 0 16M9 9l6 6M15 9l-6 6",
		["shuffle"] = "M16 3h5v5M4 20L21 3M21 16v5h-5M15 15l6 6M4 4l5 5",
		["filter"] = "M3 5h18l-7 8v6l-4 2v-8z",
		["wand"] = "M5 19L17 7M14 4l1-2 1 2 2 1-2 1-1 2-1-2-2-1zM19 11l.7 1.3 1.3.7-1.3.7L19 15l-.7-1.3L17 13l1.3-.7z",
		["function"] = "M16 4h-2a3 3 0 0 0-3 3v10a3 3 0 0 1-3 3H6M8 11h8",
		["download"] = "M12 4v11M7 10l5 5 5-5M5 20h14",
		["upload"] = "M12 20V9M7 14l5-5 5 5M5 4h14",
		["layers"] = "M12 3l9 4.5-9 4.5-9-4.5zM3 12l9 4.5 9-4.5M3 16.5L12 21l9-4.5",
		["check"] = "M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0zM8 12.5l2.7 2.7L16 9.5",
		["sparkles"] = "M12 3l1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8zM19 16l.7 1.8 1.8.7-1.8.7L19 21l-.7-1.8-1.8-.7 1.8-.7z",
		["bot"] = "M5 9h14v10H5zM12 9V5M9 14h.01M15 14h.01M3 13v3M21 13v3",
		["message"] = "M4 5h16v11H9l-5 4z",
		["brain"] = "M9 4a3 3 0 0 0-3 3 3 3 0 0 0-2 5 3 3 0 0 0 2 5 3 3 0 0 0 6 1V5a3 3 0 0 0-3-1zM15 4a3 3 0 0 1 3 3 3 3 0 0 1 2 5 3 3 0 0 1-2 5 3 3 0 0 1-6 1",
		["vector"] = "M5 19L19 5M19 5h-6M19 5v6",
		["book"] = "M5 4h10a3 3 0 0 1 3 3v13H8a3 3 0 0 1-3-3zM5 17a3 3 0 0 1 3-3h10",
		["wrench"] = "M14 7a4 4 0 0 1 5-3l-3 3 1 2 2 1 3-3a4 4 0 0 1-5 5L8 20a2 2 0 0 1-3-3z",
		["file-text"] = "M6 3h8l5 5v13H6zM14 3v5h5M9 13h7M9 17h7",
		["save"] = "M5 4h11l3 3v13H5zM8 4v5h7V4M8 20v-6h8v6",
		["table"] = "M4 5h16v14H4zM4 10h16M10 5v14",
		["code"] = "M8 7l-5 5 5 5M16 7l5 5-5 5M14 4l-4 16",
		["send"] = "M21 3L3 11l7 3 3 7zM10 14l11-11",
		["github"] = "M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0zM9.5 7v10M14.5 8.5v1a3 3 0 0 1-3 3h-2",
		["mail"] = "M4 5h16v14H4zM4 7l8 6 8-6",
		["plug"] = "M9 3v5M15 3v5M6 8h12v4a6 6 0 0 1-12 0zM12 18v3",
	};
}
