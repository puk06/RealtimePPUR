using Avalonia.Controls;
using RealtimePPUR.Services;
using RealtimePPUR.ViewModels;

namespace RealtimePPUR.Views;

public partial class StrainGraphWindow : Window
{
    public StrainGraphViewModel ViewModel { get; } = new();

    public StrainGraphWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
        GraphControl.DataContext = ViewModel;
    }

    public void SetValues(StrainList strainList, int firstTime)
    {
        ViewModel.SetValues(strainList, firstTime);
    }

    public void UpdateSongProgress(int time)
    {
        ViewModel.UpdateSongProgress(time);
    }
}
