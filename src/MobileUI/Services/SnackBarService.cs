﻿using CommunityToolkit.Maui.Views;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.Services;

public interface ISnackbarService
{
    Task ShowSnackbar(SnackbarOptions options);
}

public class SnackBarService : ISnackbarService
{
    public async Task ShowSnackbar(SnackbarOptions options)
    {
        var snack = new Snackbar(options);

        await App.Current.MainPage.ShowPopupAsync(snack);
    }
}
