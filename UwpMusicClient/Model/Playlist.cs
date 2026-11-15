using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace UwpMusicClient.Model
{
    public class Playlist : BFResponse
    {
        public int Id { get; set; }


        [JsonPropertyName("snippet")]
        public DataList PlaylistData { get; set; }

        [JsonPropertyName("pageInfo")]
        public PageInfo PlaylistPageInfo { get; set; }

        [JsonPropertyName("items")]
        public List<Composition> Compositions { get; set; }

		public AccountInfo Account { get; set; }

		public struct DataList
        {
            public string Title { get; set; }
            public string Description { get; set; }

            [JsonPropertyName("summary")]
            public int CountOfCompositions { get; set; }

            [JsonPropertyName("picture")]
            public string ImageSymbols { get; set; }
			//public string Type { get; set; }
			//public string Date { get; set; }
        }

        public struct AccountInfo
        {
			[JsonPropertyName("playlists")]
			public PlaylistList UserPlaylists { get; set; }
		}

        public string GetFormattedCountOfCompositions
        {
            get
            {
                int count = (this.PlaylistData.CountOfCompositions == 0) ? this.PlaylistPageInfo.TotalResults : this.PlaylistData.CountOfCompositions;
                int lastDigit = (int)Char.GetNumericValue(count.ToString().Last());
                if (((lastDigit >= 5 && lastDigit <= 9) || lastDigit == 0) || (count >= 10 && count <= 14)) // ..0, ..5-..9, 11-19
                    return count + " треков";
                else if (lastDigit > 1 && lastDigit < 5) // ..2, ..3, ..4
                    return (int)count + " трека";
                else  // ..1
                    return count + " трек";
            }
        }

		public string GetFormattedDuration
		{
			get
			{
				string timetxt = "";
                int sum = this.Compositions.Select(c => c.Data.Duration).ToArray().Sum();
				TimeSpan time = TimeSpan.FromSeconds(sum);
				if (time > TimeSpan.Zero)
				{
					int hours = time.Hours;
					int minutes = time.Minutes;
					int seconds = time.Seconds;

					int lastDigit;

					if (hours > 0)
					{
						//lastDigit = Convert.ToInt32(hours.ToString().Last());
						lastDigit = (int)Char.GetNumericValue(hours.ToString().Last());
						if (((lastDigit >= 5 && lastDigit <= 9) || lastDigit == 0) || (hours >= 10 && hours <= 14)) // 5-20
							timetxt += hours + " часов ";
						else if (lastDigit > 1 && lastDigit < 5) // 2, 3, 4, 22, 23, 24, 32, 33, 34...
							timetxt += (int)hours + " часа ";
						else  // 1, 21, 31, 41...
							timetxt += hours + " час ";
					}

					if (minutes > 0)
					{
						lastDigit = (int)Char.GetNumericValue(minutes.ToString().Last());
						if (((lastDigit >= 5 && lastDigit <= 9) || lastDigit == 0) || (minutes >= 10 && minutes <= 14))
							timetxt += minutes + " минут ";
						else if (lastDigit > 1 && lastDigit < 5)
							timetxt += minutes + " минуты ";
						else
							timetxt += minutes + " минута ";
					}

					if (seconds > 0)
					{
						lastDigit = (int)Char.GetNumericValue(seconds.ToString().Last());
						if (((lastDigit >= 5 && lastDigit <= 9) || lastDigit == 0) || (seconds >= 10 && seconds <= 14))
							timetxt += seconds + " секунд ";
						else if (lastDigit > 1 && lastDigit < 5)
							timetxt += seconds + " секунды ";
						else
							timetxt += seconds + " секунда ";
					}
				}
				else
					timetxt = "0 секунд";

                return timetxt;
			}
		}

		public BitmapImage Picture { get; set; }

    }


	public class PlaylistList : BFResponse
	{
		[JsonPropertyName("pageInfo")]
		public PageInfo PlaylistListInfo { get; set; }

		[JsonPropertyName("items")]
		public List<Playlist> Playlists { get; set; }
	}

}
