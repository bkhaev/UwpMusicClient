using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpMusicClient
{
	/// <summary>
	/// Класс хранит настройки приложения, включая сведения о текущем пользователе.
	/// </summary>
	internal static class AppSettings
	{
		public static int UserId { get; private set; }
		public static string UserEmail { get; private set; }
		public static char UserRole { get; private set; }

		public static void UserRegister(int userId, string userEmail, char userRole)
		{
			UserId = userId;
			UserEmail = userEmail;
			UserRole = userRole;
		}
	}
}
