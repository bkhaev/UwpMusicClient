using UwpMusicClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static UwpMusicClient.Model.Composition;
using static UwpMusicClient.Pages.HomePage.GenreList;
using static UwpMusicClient.Pages.LibraryPage;

namespace UwpMusicClient.Pages
{
    /// <summary>
    /// Главная страница
    /// </summary>
    public sealed partial class HomePage : Page
    {
		/// <summary>
		/// Подборки композиций от команды
		/// </summary>
		List<Playlist> _sets;

		/// <summary>
		/// Жанры композиций
		/// </summary>
		List<GenreData> _genres;

		/// <summary>
		/// Микс композиций для пользователя (рекомендации)
		/// </summary>
		Playlist _mixSet;

		/// <summary>
		/// Текст песни в модальном окне
		/// </summary>
		string _text;

		/// <summary>
		/// Название песни в модальном окне
		/// </summary>
		string _title;

		public HomePage()
        {
            this.InitializeComponent();

			GetEditorsSets(20, 1);
			GetMix(25, 1);
			GetGenres();

			this.NavigationCacheMode = NavigationCacheMode.Enabled;
		}

		/// <summary>
		/// Процедура получения микса (рекомендации для пользователя)
		/// </summary>
		private void GetMix(int maxResults, int page)
		{
			CommonItems.BFGetData($"user/compositions?part=snippet%2CuserAssociations%2CuserPlaylists&type=mix&maxResults={maxResults}&page={page}", (Playlist playlist) => {
				if (playlist != null && playlist.Kind == "music#compositionListResponse")
				{
					_mixSet = playlist;
					UpdateCompositions();
					LoadCompsPictures();
				}
				else
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");
			});
		}

		/// <summary>
		/// Процедура загрузки обложек подборкам
		/// </summary>
		private async void LoadSetPicture()
		{
			foreach (Playlist item in _sets)
			{
				string content = item.PlaylistData.ImageSymbols;

				if (content != null)
				{
					Task<BitmapImage> task = CommonItems.Base64ToBitmap(content);
					await task;
					item.Picture = task.Result;
				}
			}

			Debug.WriteLine("Картинки наборов загружены");
			UpdateSets();
		}

		/// <summary>
		/// Процедура ререндера композиций на странице
		/// </summary>
		private void UpdateCompositions()
		{
			object cvsCompositions = this.Resources["cvsCompositions"];
			(cvsCompositions as CollectionViewSource).Source = _mixSet.Compositions;
		}

		/// <summary>
		/// Процедура загрузки обложек для композиций на странице
		/// </summary>
		private async void LoadCompsPictures()
		{
			foreach (Composition item in _mixSet.Compositions)
			{
				string content = item.Data.Album?.ImageSymbols;

				if (content != null)
				{
					Task<BitmapImage> task = CommonItems.Base64ToBitmap(content);
					await task;
					item.Picture = task.Result;
				}
			}

			UpdateCompositions();
		}

		/// <summary>
		/// Процедура ререндера подборок
		/// </summary>
		private void UpdateSets()
		{
			object cvsSets = this.Resources["cvsSets"];
			(cvsSets as CollectionViewSource).Source = _sets;
		}

		private void GetEditorsSets(int maxResults, int page)
		{
			CommonItems.BFGetData($"sets?&maxResults={maxResults}&page={page}", (PlaylistList sets) => {
				if (sets != null && sets.Kind == "music#setListResponse")
				{
					_sets = sets.Playlists;
					UpdateSets();
					LoadSetPicture();
				}
				else
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");
			});
		}

		private void GridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			Playlist set = e.ClickedItem as Playlist;
			RequestData data = new RequestData(set.Kind, set.Id);
			NavigationItems.NavView_Navigate("playlist", new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo(), data);
		}

		private void compsListView_ItemClick(object sender, ItemClickEventArgs e)
		{
			PlayerItems._mediaPlaybackList = new MediaPlaybackList();

			Composition composition = e.ClickedItem as Composition;

			int index = _mixSet.Compositions.IndexOf(composition);
			PlayerItems.PlayComposition(_mixSet.Compositions, index);
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

					var playlists = _mixSet.Account.UserPlaylists.Playlists;
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
		/// Процедура для получения жанров композиций
		/// </summary>
		private void GetGenres()
		{
			CommonItems.BFGetData($"genres", (GenreList data) => {
				if (data != null && data.Kind == "music#genreListResponse")
				{
					_genres = data.Genres;
					UpdateGenres();
				}
				else
					CommonItems.Alert("Ошибка", "Данные получены неверно\nИзвиняемся за неудобства!");
			});
		}

		/// <summary>
		/// Процедура ререндера жанров на странице
		/// </summary>
		private void UpdateGenres()
		{
			object cvsGenres = this.Resources["cvsGenres"];
			(cvsGenres as CollectionViewSource).Source = _genres;
		}

		private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
		{
			RequestData data = new RequestData("music#genre", (int)(sender as HyperlinkButton).Tag);
			NavigationItems.NavView_Navigate("playlist", new Windows.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo(), data);
		}

		public class GenreList : BFResponse
		{
			[JsonPropertyName("items")]
			public List<GenreData> Genres { get; set; }

			public class GenreData
			{
				public int Id { get; set; }
				public string Name { get; set; }
			}
		}
	}
}
