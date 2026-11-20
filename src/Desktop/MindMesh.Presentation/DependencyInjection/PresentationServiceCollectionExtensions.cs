using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MindMesh.Presentation.Hotkeys;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Navigation;
using MindMesh.Presentation.Pages.About;
using MindMesh.Presentation.Pages.Editor;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Pages.Editor.Inspector;
using MindMesh.Presentation.Pages.Editor.Library;
using MindMesh.Presentation.Pages.Editor.Nodes;
using MindMesh.Presentation.Pages.Placeholder;
using MindMesh.Presentation.Pages.Settings;
using MindMesh.Presentation.Pages.Welcome;
using MindMesh.Presentation.Platform;
using MindMesh.Presentation.Projects;
using MindMesh.Presentation.Projects.Covers;
using MindMesh.Presentation.Shell;

namespace MindMesh.Presentation.DependencyInjection;

public static class PresentationServiceCollectionExtensions
{
	/// <summary>
	/// Registers view models, navigation and localization.
	/// </summary>
	public static IServiceCollection AddMindMeshPresentation(this IServiceCollection services)
	{
		services.AddSingleton<ILocalizer, Localizer>();

		services.AddSingleton<IPageFactory, PageFactory>();
		services.AddSingleton<INavigationService, NavigationService>();

		services.AddSingleton<SidebarViewModel>();
		services.AddSingleton<TopBarViewModel>();
		services.AddSingleton<MainWindowViewModel>();

		// Editor parts live as long as the application, so panel state survives navigation.
		services.AddSingleton<HotkeyService>();
		services.AddSingleton<EditorWorkspace>();
		services.AddSingleton<ProjectService>();
		services.AddSingleton<ProjectCovers>();
		services.AddSingleton<NewProjectDialogViewModel>();
		services.AddSingleton<NodeText>();
		services.AddSingleton<GraphClipboard>();
		services.AddSingleton<GraphEditorViewModel>();
		services.TryAddSingleton<IFilePickerService, NullFilePickerService>();
		services.TryAddSingleton<IShellService, NullShellService>();
		services.AddSingleton<EditorLayoutViewModel>();
		services.AddSingleton<EditorToolbarViewModel>();
		services.AddSingleton<NodeLibraryViewModel>();
		services.AddSingleton<InspectorViewModel>();
		services.AddSingleton<ExecutionConsoleViewModel>();
		services.AddSingleton<AssistantViewModel>();

		services.AddPage<EditorPageViewModel>(PageKey.Editor, ServiceLifetime.Singleton);
		services.AddPage<WelcomePageViewModel>(PageKey.Welcome, ServiceLifetime.Transient);
		services.AddPage<SettingsPageViewModel>(PageKey.Settings, ServiceLifetime.Transient);
		services.AddPage<AboutPageViewModel>(PageKey.About, ServiceLifetime.Transient);

		services.AddPage<ProjectsPageViewModel>(PageKey.Projects, ServiceLifetime.Transient);
		services.AddPlaceholderPage(PageKey.Templates, phase: 9);
		services.AddPlaceholderPage(PageKey.Marketplace, phase: 9);
		services.AddPlaceholderPage(PageKey.Executions, phase: 5);
		services.AddPlaceholderPage(PageKey.Plugins, phase: 9);

		return services;
	}

	private static void AddPage<TPage>(this IServiceCollection services, PageKey key, ServiceLifetime lifetime)
		where TPage : PageViewModel
	{
		services.Add(new ServiceDescriptor(typeof(TPage), typeof(TPage), lifetime));
		services.AddSingleton(new PageRegistration(key, provider => provider.GetRequiredService<TPage>()));
	}

	private static void AddPlaceholderPage(this IServiceCollection services, PageKey key, int phase)
	{
		services.AddSingleton(new PageRegistration(
			key,
			provider => ActivatorUtilities.CreateInstance<PlaceholderPageViewModel>(provider, key, phase)));
	}
}
