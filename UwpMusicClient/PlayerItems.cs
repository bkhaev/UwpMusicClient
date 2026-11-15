using UwpMusicClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;

namespace UwpMusicClient
{
	/// <summary>
	/// Класс для плеера
	/// </summary>
	internal static class PlayerItems
	{
		public static MediaPlayerElement _mediaPlayerElement;
		public static MediaPlayer _mediaPlayer;
		public static MediaPlaybackList _mediaPlaybackList;

		public static List<(Composition composition, MediaPlaybackItem item)> _query = new List<(Composition composition, MediaPlaybackItem item)>();

		public static void PlayComposition(List<Composition> compositions, int first)
		{
			_mediaPlayer?.Pause();
			_query.Clear();

			MediaPlaybackItem firstItem = null;
			foreach (Composition composition in compositions)
			{
				var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri("http://localhost/uwp_music_client/materials/music/" + composition.Data.NamePath)));
				_mediaPlaybackList.Items.Add(mediaPlaybackItem);

				_query.Add((composition, mediaPlaybackItem));

				if (compositions.IndexOf(composition) == first)
					firstItem = mediaPlaybackItem;
			}

			if (firstItem != null)
				_mediaPlaybackList.StartingItem = firstItem;

			_mediaPlaybackList.ItemOpened += MediaPlaybackList_ItemOpened;
			_mediaPlaybackList.ItemFailed += MediaPlaybackList_ItemFailed;

			_mediaPlaybackList.MaxPlayedItemsToKeepOpen = 3;

			_mediaPlayer = new MediaPlayer();
			_mediaPlayer.Source = _mediaPlaybackList;
			_mediaPlayerElement.SetMediaPlayer(_mediaPlayer);

			_mediaPlayer.Play();
		}

		private static void MediaPlaybackItem_TimedMetadataTracksChanged(MediaPlaybackItem sender, IVectorChangedEventArgs args)
		{
			Debug.WriteLine("Сработал TimedMetadataTracksChanged");
		}

		private static void PlaybackItem_AudioTracksChanged(MediaPlaybackItem sender, IVectorChangedEventArgs args)
		{
			Debug.WriteLine("дорожка изменилась");
		}

		private static void MediaPlaybackList_ItemFailed(MediaPlaybackList sender, MediaPlaybackItemFailedEventArgs args)
		{
			Debug.WriteLine("не удалось проиграть трек");
		}

		private static void MediaPlaybackList_ItemOpened(MediaPlaybackList sender, MediaPlaybackItemOpenedEventArgs args)
		{
			Debug.WriteLine("трек готов к проигрыванию");
		}

		private static void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
		{
			Debug.WriteLine("трек изменился");
		}
	}
}
