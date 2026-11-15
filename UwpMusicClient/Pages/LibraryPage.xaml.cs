using UwpMusicClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static System.Net.Mime.MediaTypeNames;

namespace UwpMusicClient.Pages
{
    /// <summary>
    /// Страница с библиотекой пользователя
    /// </summary>
    public sealed partial class LibraryPage : Page
    {
		/// <summary>
		/// Пользовательские плейлисты
		/// </summary>
        List<Playlist> _playlists;

		public LibraryPage()
        {
            this.InitializeComponent();

			GetUserPlaylists(20, 1);

            CommonItems.library = this;

            this.NavigationCacheMode = NavigationCacheMode.Enabled;
		}

		/// <summary>
		/// Процедура получения плейлистов пользователя
		/// </summary>
		private void GetUserPlaylists(int maxResults, int page)
        {
            CommonItems.BFGetData($"user/playlists?&maxResults={maxResults}&page={page}", (PlaylistList playlists) => {
				if (playlists != null && playlists.Kind == "music#playlistListResponse")
				{
					_playlists = playlists.Playlists;
					UpdatePlaylists();
					LoadPlaylistPicture();
				}
				else
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");
			});
        }

		/// <summary>
		/// Процедура ререндера списка плейлистов
		/// </summary>
        private void UpdatePlaylists()
        {
            object cvsPlaylists = this.Resources["cvsPlaylists"];
            (cvsPlaylists as CollectionViewSource).Source = _playlists;
        }

		/// <summary>
		/// Процедура загрузки обложек для плейлистов
		/// </summary>
        private async void LoadPlaylistPicture()
        {
            foreach (Playlist item in _playlists)
            {
                string content = item.PlaylistData.ImageSymbols;

                if (content != null)
                {
                    Task<BitmapImage> task = CommonItems.Base64ToBitmap(content);
                    await task;
                    item.Picture = task.Result;
                }
            }

            Debug.WriteLine("Картинки плейлистов загружены");
            UpdatePlaylists();
        }

		private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Playlist playlist = e.ClickedItem as Playlist;
			RequestData data = new RequestData(playlist.Kind, playlist.Id);
			NavigationItems.NavView_Navigate("playlist", new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo(), data);
		}
	}
}
