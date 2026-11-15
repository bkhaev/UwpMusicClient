using UwpMusicClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static UwpMusicClient.MainPage;

namespace UwpMusicClient.Pages
{
    /// <summary>
    /// Страница настроек аккаунта
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        string _email = AppSettings.UserEmail;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

		private void ExitAppBtn_Click(object sender, RoutedEventArgs e)
		{
            CommonItems.Confirm("Выход из профиля", (ContentDialogResult result) =>
            {
                if (result == ContentDialogResult.Primary)
                {
					CommonItems.BFGetData($"user/logout", (bool result1) => {
						if (result1)
                        {
                            CommonItems.AppLogOut();
                        }
                        else
							CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");
					}, true);
				}
                else if (result == ContentDialogResult.Secondary)
				{
					CoreApplication.Exit();
				}
			}, "Если вы выйдете из профиля, необходимо будет авторизоваться заново.\nВы также можете просто выйти из приложения.", "Выйти из профиля", "Выйти из приложения");
		}
	}
}
