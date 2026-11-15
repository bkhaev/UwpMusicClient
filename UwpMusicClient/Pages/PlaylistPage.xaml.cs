using UwpMusicClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Media.Protection.PlayReady;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static UwpMusicClient.MainPage;
using static UwpMusicClient.Model.Composition;
using static UwpMusicClient.Pages.LibraryPage;
using static System.Net.Mime.MediaTypeNames;

namespace UwpMusicClient.Pages
{
    /// <summary>
    /// Страница плейлиста
    /// </summary>
    public sealed partial class PlaylistPage : Page
    {

		/// <summary>
		/// Текущий плейлист
		/// </summary>
        Playlist _playlist;

		/// <summary>
		/// Текст песни в модальном окне
		/// </summary>
		string _text;

		/// <summary>
		/// Название песни в модальном окне
		/// </summary>
		string _title;

		public PlaylistPage()
        {
            this.InitializeComponent();
		}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is RequestData)
            {
				var data = (RequestData)e.Parameter;
				GetPlaylist(data.Kind, data.Id, 25, 1);

				base.OnNavigatedTo(e);
            }
            else
            {
                Debug.WriteLine("OnNavigatedTo: ID плейлиста не был передан");
            }
        }

		/// <summary>
		/// Процедура получения плейлиста
		/// </summary>
        private void GetPlaylist(string kind, int id, int maxResults, int page)
        {
			string url = "";
			if (kind == "music#playlist")
				url = $"playlists/open?part=snippet%2CuserAssociations%2CuserPlaylists&id={id}&maxResults={maxResults}&page={page}";
			else if (kind == "music#set")
				url = $"sets/open?part=snippet%2CuserAssociations%2CuserPlaylists&id={id}&maxResults={maxResults}&page={page}";
			else if (kind == "music#album")
				url = $"albums/open?part=snippet%2CuserAssociations%2CuserPlaylists&id={id}&maxResults={maxResults}&page={page}";
			else if (kind == "music#genre")
				url = $"genres/open?part=snippet%2CuserAssociations%2CuserPlaylists&id={id}&maxResults={maxResults}&page={page}";
			else
			{
				CommonItems.Alert("Ошибка", "Неизвестный тип запроса\nИзвиняемся за неудобства!");
				return;
			}

			CommonItems.BFGetData(url, (Playlist playlist) => {
				if (playlist != null && (playlist.Kind == "music#playlist" || playlist.Kind == "music#set" || playlist.Kind == "music#album" || playlist.Kind == "music#genre"))
				{
					_playlist = playlist;
					playlistHeaderSp.DataContext = _playlist;
					UpdateCompositions();
					LoadCompsPictures();
				}
				else
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");
			});
		}

		/// <summary>
		/// Процедура ререндера списка композиций текущего плейлиста
		/// </summary>
        private void UpdateCompositions()
        {
            object cvsCompositions = this.Resources["cvsCompositions"];
            (cvsCompositions as CollectionViewSource).Source = _playlist.Compositions;
        }

		/// <summary>
		/// Процедура загрузки обложек композиций
		/// </summary>
		private async void LoadCompsPictures()
		{
            BitmapImage playlistPreview = null;

			if (_playlist.Kind == "music#album")
			{
				Task<BitmapImage> task = CommonItems.Base64ToBitmap(_playlist.PlaylistData.ImageSymbols);
				await task;
				previewImg.Source = task.Result;
			}
			else
			{
				foreach (Composition item in _playlist.Compositions)
				{
					string content = item.Data.Album?.ImageSymbols;

					if (content != null)
					{
						Task<BitmapImage> task = CommonItems.Base64ToBitmap(content);
						await task;
						item.Picture = task.Result;

						if (playlistPreview == null)
						{
							playlistPreview = task.Result;
						}
					}
				}

				Debug.WriteLine("Картинки для треков загружены");

				if (playlistPreview != null && _playlist.Kind == "music#playlist")
				{
					previewImg.Source = playlistPreview;
				}
				else if (_playlist.Kind == "music#set")
				{
					Task<BitmapImage> task = CommonItems.Base64ToBitmap(_playlist.PlaylistData.ImageSymbols);
					await task;
					previewImg.Source = task.Result;
				}
			}

			UpdateCompositions();
		}

		private void ListView_ItemClick(object sender, ItemClickEventArgs e)
		{
            PlayerItems._mediaPlaybackList = new MediaPlaybackList();
			Composition composition = e.ClickedItem as Composition;
            PlayComposition(composition);
		}

		private void _mediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Процедура проигрывания композиции
		/// </summary>
		public void PlayComposition(Composition composition)
		{
			int index = _playlist.Compositions.IndexOf(composition);
			PlayerItems.PlayComposition(_playlist.Compositions, index);
		}

		private void compsListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			var orig = e.OriginalSource as DependencyObject;

			while (orig != null && orig != compsListView)
			{
				var lv = orig as ListViewItem;
				if (lv != null)
				{
					var composition = lv.Content as Composition;

					MenuFlyout menu = this.Resources["compositionMenu"] as MenuFlyout;

					menu.Items.Clear();

					////////////////

					BitmapIcon icon = new BitmapIcon();

					if (composition.UserAstns.Liked)
						icon.UriSource = new Uri("ms-appx:///images/liked_icon.png");
					else
						icon.UriSource = new Uri("ms-appx:///images/like_icon.png");

					var option = new MenuFlyoutItem()
					{
						Text = composition.UserAstns.Liked ? "Не нравится" : "Нравится",
						Icon = icon,
						Tag = "isLiked",
					};
					option.Click += (s, re) =>
					{
						SwitchLike(composition);
					};

					menu.Items.Add(option);

					////////////////

					MenuFlyoutSubItem userPlaylists = new MenuFlyoutSubItem()
					{
						Text = "Добавить в плейлист",
						Icon = new SymbolIcon(Symbol.Library),
						Tag = "playlists",
					};

					var playlists = _playlist.Account.UserPlaylists.Playlists;
					if (playlists.Count > 0)
					{
						foreach (var playlist in playlists)
						{
							option = new MenuFlyoutItem
							{
								Tag = playlist.Id,
								Text = playlist.PlaylistData.Title,
								Name = (string)userPlaylists.Tag,
							};
							option.Click += (s, re) =>
							{
								AddToPlaylist(composition.Id, playlist.Id);
							};

							userPlaylists.Items.Add(option);
						}
					}
					else
						userPlaylists.IsEnabled = false;

					menu.Items.Add(userPlaylists);

					////////////////

					if (AppPage.PreviousPage == typeof(LibraryPage))
					{
						option = new MenuFlyoutItem()
						{
							Text = "Удалить из плейлиста",
							Tag = "remove",
						};
						option.Click += (s, re) =>
						{
							DeleteFromPlaylist(composition.SourcePlaylist.Id);
						};

						menu.Items.Add(option);
					}

					////////////////

					menu.Items.Add(new MenuFlyoutSeparator());

					option = new MenuFlyoutItem()
					{
						Text = "Показать текст",
						Icon = new SymbolIcon(Symbol.FontColor),
						Tag = "text",
					};

					if (String.IsNullOrWhiteSpace(composition.Data.Text))
						option.IsEnabled = false;
					else
					{
						option.Click += (s, re) =>
						{
							_text = composition.Data.Text;
							_title = composition.Data.Name;
							Bindings.Update();
							FlyoutBase.ShowAttachedFlyout(MainSp);
						};
					}

					menu.Items.Add(option);

					////////////////

					if (_playlist.Kind != "music#album")
					{
						option = new MenuFlyoutItem()
						{
							Text = "Перейти в альбом",
							Tag = "album",
						};

						if (composition.Data.Album?.Name == null)
							option.IsEnabled = false;
						else
						{
							option.Click += (s, re) =>
							{
								RequestData data = new RequestData("music#album", (int)(composition.Data.Album?.Id));
								NavigationItems.NavView_Navigate("playlist", new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo(), data);
							};
						}

						menu.Items.Add(option);
					}

					////////////////

					MenuFlyoutSubItem compArtists = new MenuFlyoutSubItem()
					{
						Text = "Перейти к исполнителю",
						Icon = new SymbolIcon(Symbol.OtherUser),
						Tag = "artists",
					};

					var artists = composition.Data.Artists;
					if (artists.Count > 0)
					{
						foreach (var artist in artists)
						{
							option = new MenuFlyoutItem
							{
								Tag = artist.Id,
								Text = artist.Name,
								Name = (string)compArtists.Tag,
							};
							option.Click += (s, re) =>
							{
								CommonItems.Alert("artists", "id = " + artist.Id);
							};

							compArtists.Items.Add(option);
						}
					}
					else
						compArtists.IsEnabled = false;

					menu.Items.Add(compArtists);

					////////////////

					break;
				}

				orig = VisualTreeHelper.GetParent(orig);
			}
		}

		/// <summary>
		/// Процедура для добавления/удаления пометки "Нравится" для композиции
		/// </summary>
		private void SwitchLike(Composition composition)
		{
			var res = new Resource() { Kind = "music#composition", Id = composition.Id };
			var data = new PostPlaylistItems()
			{
				ResourceId = res,
			};

			CommonItems.BFPostData($"user/playlistitems/favorites", data, (PlaylistItem item) =>
			{
				if (item != null && item.Kind == "music#playlistItem")
				{
					bool liked;

					if (item.Id == 0)
					{
						Debug.WriteLine("Композиция удалена из понравившихся");
						liked = false;
					}
					else
					{
						Debug.WriteLine("Композиция добавлена в понравившееся");
						liked = true;
					}

					var update = new UserAssociations() { Liked = liked };
					composition.UserAstns = update;

					CommonItems.UpdateLibrary();
				}
				else
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");
			});
		}

		/// <summary>
		/// Процедура для добавления композиции в плейлист пользователя
		/// </summary>
		private void AddToPlaylist(int compositionId, int playlistId)
		{
			var res = new Resource() { Kind = "music#composition", Id = compositionId };
			var data = new PostPlaylistItems()
			{
				Id = playlistId,
				ResourceId = res,
			};

			CommonItems.BFPostData($"user/playlistitems", data, (PlaylistItem item) =>
			{
				if (item != null && item.Kind == "music#playlistItem")
				{
					if (item.Id == 0)
					{
						Debug.WriteLine("Композиция уже была добавлена в плейлист");
					}
					else
					{
						Debug.WriteLine("Композиция добавлена в плейлист");
					}

					CommonItems.UpdateLibrary();
				}
				else
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");
			});
		}

		/// <summary>
		/// Процедура для удаления композиции из плейлиста пользователя
		/// </summary>
		private void DeleteFromPlaylist(int id)
		{
			CommonItems.BFDeleteData($"user/playlistitems?id={id}", (bool result) =>
			{
				if (result == true)
				{
					Debug.WriteLine("Композиция удалена из плейлиста");
					CommonItems.UpdateLibrary();
				}
				else
				{
					Debug.WriteLine("Композиция не была удалена из плейлиста");
				}
			});
		}
	}


	public class PostPlaylistItems
	{
		public int Id { get; set; }

		public Resource ResourceId { get; set; }
	}

}
