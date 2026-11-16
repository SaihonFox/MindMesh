using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Application.Settings;

namespace MindMesh.Presentation.Pages.Editor;

/// <summary>
/// Visibility and size of the editor panels. Every change is stored in the user settings.
/// </summary>
public sealed partial class EditorLayoutViewModel : ObservableObject
{
	private readonly ISettingsService _settings;

	[ObservableProperty]
	private bool _isNodeLibraryVisible;

	[ObservableProperty]
	private bool _isInspectorVisible;

	[ObservableProperty]
	private bool _isConsoleVisible;

	[ObservableProperty]
	private bool _isAssistantVisible;

	[ObservableProperty]
	private double _nodeLibraryWidth;

	[ObservableProperty]
	private double _inspectorWidth;

	[ObservableProperty]
	private double _assistantWidth;

	[ObservableProperty]
	private double _consoleHeight;

	public EditorLayoutViewModel(ISettingsService settings)
	{
		_settings = settings;

		var layout = settings.Current.Layout;

		_isNodeLibraryVisible = layout.IsNodeLibraryVisible;
		_isInspectorVisible = layout.IsInspectorVisible;
		_isConsoleVisible = layout.IsConsoleVisible;
		_isAssistantVisible = layout.IsAssistantVisible;
		_nodeLibraryWidth = layout.NodeLibraryWidth;
		_inspectorWidth = layout.InspectorWidth;
		_assistantWidth = layout.AssistantWidth;
		_consoleHeight = layout.ConsoleHeight;
	}

	[RelayCommand]
	private void ToggleNodeLibrary() => IsNodeLibraryVisible = !IsNodeLibraryVisible;

	[RelayCommand]
	private void ToggleInspector() => IsInspectorVisible = !IsInspectorVisible;

	[RelayCommand]
	private void ToggleConsole() => IsConsoleVisible = !IsConsoleVisible;

	[RelayCommand]
	private void ToggleAssistant() => IsAssistantVisible = !IsAssistantVisible;

	/// <summary>Restores default panel sizes and visibility.</summary>
	[RelayCommand]
	private void ResetLayout()
	{
		IsNodeLibraryVisible = true;
		IsInspectorVisible = true;
		IsConsoleVisible = true;
		IsAssistantVisible = false;
		NodeLibraryWidth = LayoutSettings.DefaultNodeLibraryWidth;
		InspectorWidth = LayoutSettings.DefaultInspectorWidth;
		AssistantWidth = LayoutSettings.DefaultAssistantWidth;
		ConsoleHeight = LayoutSettings.DefaultConsoleHeight;
	}

	partial void OnIsNodeLibraryVisibleChanged(bool value) =>
		Store(layout => layout with { IsNodeLibraryVisible = value });

	partial void OnIsInspectorVisibleChanged(bool value) =>
		Store(layout => layout with { IsInspectorVisible = value });

	partial void OnIsConsoleVisibleChanged(bool value) =>
		Store(layout => layout with { IsConsoleVisible = value });

	partial void OnIsAssistantVisibleChanged(bool value) =>
		Store(layout => layout with { IsAssistantVisible = value });

	partial void OnNodeLibraryWidthChanged(double value) =>
		Store(layout => layout with { NodeLibraryWidth = value });

	partial void OnInspectorWidthChanged(double value) =>
		Store(layout => layout with { InspectorWidth = value });

	partial void OnAssistantWidthChanged(double value) =>
		Store(layout => layout with { AssistantWidth = value });

	partial void OnConsoleHeightChanged(double value) =>
		Store(layout => layout with { ConsoleHeight = value });

	private void Store(Func<LayoutSettings, LayoutSettings> change) =>
		_settings.Update(settings => settings with { Layout = change(settings.Layout) });
}
