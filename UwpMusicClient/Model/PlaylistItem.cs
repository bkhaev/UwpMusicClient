using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UwpMusicClient.Model
{
	public class PlaylistItem : BFResponse
	{
		public int Id { get; set; }

		[JsonPropertyName("snippet")]
		public Details ItemDetails { get; set; }

		public struct Details
		{
			public string Date;
			public int UserId;
			public string UserEmail;
			public string Title;
			public Resource Resource;
			public List<Composition.CompositionArtist> artists;
			public int PlaylistId;
		}
	}
}
