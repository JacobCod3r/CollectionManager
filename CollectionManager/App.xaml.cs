using CollectionManager.State;

namespace CollectionManager;

public partial class App : Application
{
    public static AppState State { get; private set; } = null!;

    public App()
    {
        InitializeComponent();

        State = new AppState();
        MainPage = new AppShell();
    }
}