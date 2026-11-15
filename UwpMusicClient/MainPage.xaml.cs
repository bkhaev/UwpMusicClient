using UwpMusicClient.Model;
using UwpMusicClient.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static UwpMusicClient.Model.Composition;
using static UwpMusicClient.Pages.LibraryPage;

namespace UwpMusicClient
{
    /// <summary>
    /// Вводная страница после запуска приложения (авторизация)
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter is bool)
			{
				if (!(bool)e.Parameter)
					tryGetLogin();

				base.OnNavigatedTo(e);
			}
			else
				tryGetLogin();
		}

		private void GoBtn_Click(object sender, RoutedEventArgs e)
        {
			tryPostLogin();
		}

		/// <summary>
		/// Процедура авторизации
		/// </summary>
		private void tryPostLogin()
        {
            if (String.IsNullOrWhiteSpace(emailTb.Text))
            {
				CommonItems.Alert("Заполните все поля", "Необходимо ввести логин для входа в приложение.");
				return;
            }
            if (!(new EmailAddressAttribute().IsValid(emailTb.Text)))
            {
				CommonItems.Alert("Неверные поля", "Логин не в правильной форме.");
				return;
			}
			if (String.IsNullOrWhiteSpace(passTb.Password))
			{
				CommonItems.Alert("Заполните все поля", "Необходимо ввести пароль для входа в приложение.");
				return;
			}
			
			Dictionary<string, string> data = new Dictionary<string, string>
			{
				{ "email", emailTb.Text },
				{ "pass", passTb.Password },
			};

			GoBtn.IsEnabled = false;
			GoBtn.Content = "Обработка";

			CommonItems.BFPostData("user/login", data, (UserGetPrivilages userData) =>
			{
				if (userData != null && userData.Code == "201")
				{
					AppSettings.UserRegister(userData.Data.Id, userData.Data.Email, userData.Data.Role);
					this.Frame.Navigate(typeof(AppPage));
				}
				else
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");

				GoBtn.Content = "Войти";
				GoBtn.IsEnabled = true;
			});
		}

		/// <summary>
		/// Процедура аутентификации
		/// </summary>
		private void tryGetLogin()
		{
			GoBtn.IsEnabled = false;
			GoBtn.Content = "Обработка";

			CommonItems.BFGetData("user/login", (UserGetPrivilages userData) =>
			{
				if (userData != null && userData.Code == "201")
				{
					AppSettings.UserRegister(userData.Data.Id, userData.Data.Email, userData.Data.Role);
					this.Frame.Navigate(typeof(AppPage));
				}
				else if (userData != null && userData.Error.Code != "401")
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");

				GoBtn.Content = "Войти";
				GoBtn.IsEnabled = true;
			}, true);
		}

		public class UserGetPrivilages : BFResponse
		{
			public string Code { get; set; }
			public string Message { get; set; }

			[JsonPropertyName("snippet")]
			public DataList Data { get; set; }

			public struct DataList
			{
				[JsonPropertyName("userid")]
				public int Id { get; set; }

				[JsonPropertyName("useremail")]
				public string Email { get; set; }

				[JsonPropertyName("userrole")]
				public char Role { get; set; }
			}
		}
	}
}
