using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.Foundation.Collections;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace UwpMusicClient.Model
{
    public class Composition
    {
        public int Id { get; set; }

        [JsonPropertyName("snippet")]
        public DataList Data { get; set; }

		[JsonPropertyName("user associations")]
		public UserAssociations UserAstns { get; set; }

		[JsonPropertyName("playlist")]
		public CompositionPlaylist SourcePlaylist { get; set; }

		public struct DataList
        {
            public string Name { get; set; }
            //public DateTime? Date { get; set; }
            public List<CompositionArtist> Artists { get; set; }
            public Album? Album { get; set; }
            public string Date { get; set; }
            public string Text { get; set; }
            public int Auditions { get; set; }
            public bool Explicit { get; set; }
			public int Duration { get; set; }

			[JsonPropertyName("namepath")]
			public string NamePath { get; set; }
		}

        public struct CompositionArtist
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public struct Album
        {
            public int Id { get; set; }
            public string Name { get; set; }

            [JsonPropertyName("picture")]
            public string ImageSymbols { get; set; }
		}

        public struct UserAssociations
        {
			public bool Liked { get; set; }
            public bool DisLiked { get; set; }
        }

        public struct CompositionPlaylist
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Date { get; set; }
        }

        public BitmapImage Picture { get; set; }

        public string GetDuration
        {
            get
            {
                string timetxt = "";
				TimeSpan time = TimeSpan.FromSeconds(this.Data.Duration);

                if (time.Hours > 0)
                {
					if (time.Minutes > 9)
                    {
						if (time.Seconds > 9)
							timetxt = time.Hours + ":" + time.Minutes + ":" + time.Seconds;
                        else
							timetxt = time.Hours + ":" + time.Minutes + ":" + "0" + time.Seconds;

					}
					else
                    {
						if (time.Seconds > 9)
							timetxt = time.Hours + ":" + "0" + time.Minutes + ":" + time.Seconds;
						else
							timetxt = time.Hours + ":" + "0" + time.Minutes + ":" + "0" + time.Seconds;
					}
				}
                else
                {
                    if (time.Seconds > 9)
						timetxt = time.Minutes + ":" + time.Seconds;
                    else
						timetxt = time.Minutes + ":" + "0" + time.Seconds;
				}

				return timetxt;
			}
        }

        public string GetArtists
        {
            get
            {
                return string.Join(", ", this.Data.Artists.Select(a => a.Name));
			}
        }

		public Visibility IsExplicit
        {
			get
			{
				if (this.Data.Explicit)
					return Visibility.Visible;
				return Visibility.Collapsed;
			}
		}
    }
}
