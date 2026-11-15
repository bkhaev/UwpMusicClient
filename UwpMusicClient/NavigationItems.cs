using UwpMusicClient.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace UwpMusicClient
{
	/// <summary>
	/// Класс для маршрутизации / навигации по приложению
	/// </summary>
	internal static class NavigationItems
	{
		public static readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
		{
			("home", typeof(HomePage)),
			("library", typeof(LibraryPage)),
		};

		public static NavigationView navigation;
		public static Frame navigationFrame;

		/// <summary>
		/// Производит переход между страницами
		/// </summary>
		/// <param name="navItemTag">Тег элемента панели навигации, соответствующий элементу коллекции _pages</param>
		public static void NavView_Navigate(string navItemTag, Windows.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo, RequestData data = null)
		{
			Type _page = null;
			if (navItemTag == "settings")
			{
				_page = typeof(SettingsPage);
			}
			else if (navItemTag == "playlist")
			{
				navigationFrame.Navigate(typeof(PlaylistPage), data, transitionInfo);
				return;
			}
			else
			{
				var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
				_page = item.Page;
			}

			var preNavPageType = navigationFrame.CurrentSourcePageType;

			if (!(_page is null) && !Type.Equals(preNavPageType, _page))
			{
				navigationFrame.Navigate(_page, null, transitionInfo);
			}
		}
	}
}
