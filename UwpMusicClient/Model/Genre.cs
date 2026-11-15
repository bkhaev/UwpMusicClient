using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpMusicClient.Model
{
	internal class Genre : BFResponse
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<Composition> Compositions { get; set; }
	}
}
