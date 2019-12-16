using System;

namespace urldetector.legacy.eladaus
{
	public static class LegacyEnumExtension
	{
		public static LegacyUrlPart? GetNextPart(this LegacyUrlPart currentPart)
		{
			switch (currentPart)
			{
				case LegacyUrlPart.FRAGMENT:
					return null;
				case LegacyUrlPart.QUERY:
					return LegacyUrlPart.FRAGMENT;
				case LegacyUrlPart.PATH:
					return LegacyUrlPart.QUERY;
				case LegacyUrlPart.PORT:
					return LegacyUrlPart.PATH;
				case LegacyUrlPart.HOST:
					return LegacyUrlPart.PORT;
				case LegacyUrlPart.USERNAME_PASSWORD:
					return LegacyUrlPart.HOST;
				case LegacyUrlPart.SCHEME:
					return LegacyUrlPart.USERNAME_PASSWORD;
				default:
					throw new ArgumentException($"Unexpected current UrlPart: {currentPart}");
			}
		}
	}
}