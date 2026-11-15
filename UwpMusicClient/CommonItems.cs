using UwpMusicClient.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation.Collections;
using static System.Net.Mime.MediaTypeNames;
using System.IO.Pipes;
using static System.Net.WebRequestMethods;
using UwpMusicClient.Model;
using Windows.UI.Xaml.Shapes;
using System.Net;
using System.Text.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using static UwpMusicClient.MainPage;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using System.Net.Http.Headers;
using System.Net.Http;

namespace UwpMusicClient
{
	/// <summary>
	/// Общий класс для приложения: Endpoint Сервера, HTTP-клиент, окна подтверждения
	/// </summary>
    internal static class CommonItems
    {
		public static string EndPoint = "http://localhost/1/uwp_music_client/api/";

		public static LibraryPage library;

		private static ContentDialog ActiveDialog;

		private static AppPage _app;

		public static void AppRegister(AppPage app)
		{
			if (_app == null)
			{
				_app = app;
			}
		}

		/// <summary>
		/// Процедура выходит из приложения. Окно Windows закрывается.
		/// </summary>
		public static void AppLogOut()
		{
			_app.Frame.Navigate(typeof(MainPage), false, new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
		}

		/// <summary>
		/// Функция для получения данных от сервера
		/// </summary>
		public static void BFGetData<T>(string url, Action<T> callback, bool isSafe = false)
        {
            try
            {
				using (WebClient webClient = new WebClient())
				{
					webClient.DownloadStringCompleted += (sender, e) =>
					{
						if (callback.GetType().GetGenericArguments()[0] == typeof(bool) && String.IsNullOrEmpty(e.Result))
						{
							callback((T)Convert.ChangeType(true, Type.GetTypeCode(typeof(T))));
						}
						else if (String.IsNullOrEmpty(e.Result))
						{
							callback(default);
						}
						else
						{
							var options = new JsonSerializerOptions
							{
								PropertyNameCaseInsensitive = true
							};
							var response = JsonSerializer.Deserialize<T>(e.Result, options);

							if ((response as BFResponse).Error != null && !isSafe)
							{
								(response as BFResponse).Error.DisplayErrorDialog();
								callback(default);
							}
							else
								callback(response);
						}
					};

					//webClient.BaseAddress = EndPoint;
					webClient.DownloadStringAsync(new Uri(EndPoint + url));
				}
			}
			catch (WebException e)
			{
				Alert("Ошибка " + e.Status, e.Message);
				callback(default);
			}
		}

		/// <summary>
		/// Функция для отправки данных на сервер
		/// </summary>
		public static void BFPostData<T, K>(string url, K data, Action<T> callback)
		{
			try
			{
				using (WebClient webClient = new WebClient())
				{
					//webClient.Credentials = new NetworkCredential(emailTb.Text, passTb.Password);
					webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

					webClient.UploadStringCompleted += (sender, e) =>
					{
						var options = new JsonSerializerOptions
						{
							PropertyNameCaseInsensitive = true
						};
						var response = JsonSerializer.Deserialize<T>(e.Result, options);

						if ((response as BFResponse).Error != null)
						{
							(response as BFResponse).Error.DisplayErrorDialog();
							callback(default);
						}
						else
							callback(response);
					};

					Dictionary<string, K> snippet = new Dictionary<string, K>()
					{
						{ "snippet", data }
					};
					webClient.UploadStringAsync(new Uri(EndPoint + url), JsonSerializer.Serialize(snippet));
				}
			}
			catch (WebException e)
			{
				Alert("Ошибка " + e.Status, e.Message);
				callback(default);
			}
		}

		/// <summary>
		/// Функция для удаления данных на сервере
		/// </summary>
		public static async void BFDeleteData(string url, Action<bool> callback)
		{
			try
			{
				using (HttpClient httpClient = new HttpClient())
				{
					httpClient.BaseAddress = new Uri(EndPoint);
					httpClient.DefaultRequestHeaders.Accept.Clear();
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.DeleteAsync(url);

					if (response.IsSuccessStatusCode)
					{
						var result = await response.Content.ReadAsStringAsync();

						if (String.IsNullOrEmpty(result))
							callback(true);
						else
						{
							var options = new JsonSerializerOptions
							{
								PropertyNameCaseInsensitive = true
							};
							var data = JsonSerializer.Deserialize<BFResponse>(result, options);

							if (data.Error != null)
							{
								data.Error.DisplayErrorDialog();
							}

							callback(false);
						}
					}
					else
					{
						Alert("Ошибка " + response.StatusCode);
						callback(false);
					}
				}
			}
			catch (WebException e)
			{
				Alert("Ошибка " + e.Status, e.Message);
				callback(default);
			}
		}

		public static async void Alert(string title, string content = null, bool isPrimary = false)
        {
			if (ActiveDialog != null && !isPrimary)
			{
				return;
			}
			else if (ActiveDialog != null && isPrimary)
			{
				ActiveDialog.Hide();
			}

			ContentDialog errorDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Ок",
            };
			ActiveDialog = errorDialog;
			ActiveDialog.Closed += Dialog_Closed;

			ContentDialogResult result = await errorDialog.ShowAsync();
        }

		public static async void Confirm(string title, Action<ContentDialogResult> callback, string content = null, string primaryBtn = null, string secondaryBtn = null, string closeBtn = "Отмена")
		{
			if (ActiveDialog != null)
			{
				callback(ContentDialogResult.None);
				return;
			}

			ContentDialog errorDialog = new ContentDialog
			{
				Title = title,
				Content = content,
				CloseButtonText = closeBtn,
				PrimaryButtonText = primaryBtn,
				SecondaryButtonText = secondaryBtn,
			};
			ActiveDialog = errorDialog;
			ActiveDialog.Closed += Dialog_Closed;

			var result = await errorDialog.ShowAsync();

			callback(result);
		}

		private static void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
		{
			ActiveDialog = null;
		}

		/// <summary>
		/// Функция преобразования изображения из строки (base64) в объект Bitmap
		/// </summary>
		public static async Task<BitmapImage> Base64ToBitmap(string imageSymbols)
        {
            try
            {
                byte[] ib = Convert.FromBase64String(imageSymbols);

                using (Stream ms = new MemoryStream(ib))
                {
                    using (IRandomAccessStream fileStream = ms.AsRandomAccessStream())
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.DecodePixelHeight = 150;
                        bi.DecodePixelWidth = 150;

                        await bi.SetSourceAsync(fileStream);
                        return bi;
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

		public static void UpdateLibrary()
		{
			if (library != null)
			{
				Debug.WriteLine("Библиотека обновлена");
				library.NavigationCacheMode = NavigationCacheMode.Disabled;
			}
		}
    }

    public class BFResponse
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("error")]
        public Error Error { get; set; } = null;
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<ErrorList> Errors { get; set; }

        public struct ErrorList
        {
            public string message;
            public string domain;
            public string reason;
        }

        public void DisplayErrorDialog()
        {
			CommonItems.Alert("Ошибка " + this.Code, this.Message);
        }
    }

	/// <summary>
	/// Класс для передачи данных между страницами.
	/// </summary>
	public class RequestData
	{
		public string Kind;
		public int Id;

		public RequestData(string kind, int id)
		{
			this.Kind = kind;
			this.Id = id;
		}
	}

	public struct PageInfo
	{
		public int TotalResults { get; set; }
		public int ResultsPerPage { get; set; }
	}

	public class Resource
	{
		public string Kind { get; set; }

		public int Id { get; set; }
	}
}
