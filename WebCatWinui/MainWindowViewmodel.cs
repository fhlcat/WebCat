using CommunityToolkit.Mvvm.ComponentModel;

namespace WebCatWinui;

public partial class MainViewmodel : ObservableObject
{
    [ObservableProperty] public partial string? Query { get; set; }

    [ObservableProperty] public partial string Endpoint { get; set; } = "https://api.deepseek.com";

    [ObservableProperty] public partial string? ApiKey { get; set; }

    [ObservableProperty] public partial string? Model { get; set; }

    [ObservableProperty] public partial double Temperature { get; set; } = 0.7;

    [ObservableProperty] public partial double Interval { get; set; } = 1000;

    [ObservableProperty] public partial bool IsHeadless { get; set; } = true;
}