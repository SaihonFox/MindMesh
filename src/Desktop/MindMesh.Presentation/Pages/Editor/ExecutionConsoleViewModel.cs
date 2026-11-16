using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;

namespace MindMesh.Presentation.Pages.Editor;

/// <summary>
/// Bottom execution console with its tabs. Content arrives with the runtime (Phase 4) and debugger (Phase 5).
/// </summary>
public sealed partial class ExecutionConsoleViewModel : ViewModelBase
{
	[ObservableProperty]
	private ConsoleTabViewModel _selectedTab;

	public ExecutionConsoleViewModel(ILocalizer localizer)
		: base(localizer)
	{
		Tabs =
		[
			new ConsoleTabViewModel(ConsoleTabId.Output, IconKeys.Terminal, localizer),
			new ConsoleTabViewModel(ConsoleTabId.Logs, IconKeys.List, localizer),
			new ConsoleTabViewModel(ConsoleTabId.Errors, IconKeys.Alert, localizer),
			new ConsoleTabViewModel(ConsoleTabId.Variables, IconKeys.Braces, localizer),
			new ConsoleTabViewModel(ConsoleTabId.ExecutionData, IconKeys.Database, localizer),
			new ConsoleTabViewModel(ConsoleTabId.Debugger, IconKeys.Debug, localizer),
		];

		_selectedTab = Tabs[0];
	}

	public IReadOnlyList<ConsoleTabViewModel> Tabs { get; }
}
