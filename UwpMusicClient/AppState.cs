using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace UwpMusicClient
{
	internal class AppState
	{
		public Grid Body;
		private string message;
		public string Message {
			get { return message; }
			set
			{
				if (value != "")
				{
					message = value;
					if (Body != null)
						Body.DataContext = this;
				}
			}
		}
	}
}
