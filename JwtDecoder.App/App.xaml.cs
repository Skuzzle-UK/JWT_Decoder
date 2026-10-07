namespace JwtDecoder.App;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		Services.ThemeService.ApplySaved();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Width/height only apply on desktop (Windows and Mac)
		return new Window(new MainPage())
		{
			Title = "JWT Decoder",
			Width = 1200,
			Height = 780,
			MinimumWidth = 420,
			MinimumHeight = 500
		};
	}
}
