using System;

namespace urldetector.eladaus
{
	public static class EnumExtension
	{
		public static UrlPart? GetNextPart(this UrlPart currentPart)
		{
			switch (currentPart)
			{
				case UrlPart.FRAGMENT:
					return null;
				case UrlPart.QUERY:
					return UrlPart.FRAGMENT;
				case UrlPart.PATH:
					return UrlPart.QUERY;
				case UrlPart.PORT:
					return UrlPart.PATH;
				case UrlPart.HOST:
					return UrlPart.PORT;
				case UrlPart.USERNAME_PASSWORD:
					return UrlPart.HOST;
				case UrlPart.SCHEME:
					return UrlPart.USERNAME_PASSWORD;
				default:
					throw new ArgumentException($"Unexpected current UrlPart: {currentPart}");
			}
		}
	}
}