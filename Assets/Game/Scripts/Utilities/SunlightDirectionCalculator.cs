using UnityEngine;
namespace Game.Utilities
{
	public static class SunlightDirectionCalculator
	{
		public static Vector3 GetSunlightDirection(float latitude, float progressOfDay, float progressOfYear)
		{
			var latitudeRad = latitude * Mathf.Deg2Rad;
			var declination = Mathf.Asin(Mathf.Sin(23.44f * Mathf.Deg2Rad) *
				Mathf.Sin(2 * Mathf.PI * (progressOfYear + 0.25f)));
			var hourAngle = 2 * Mathf.PI * progressOfDay;
			var altitude = Mathf.Asin(Mathf.Sin(latitudeRad) * Mathf.Sin(declination) +
				Mathf.Cos(latitudeRad) * Mathf.Cos(declination) * Mathf.Cos(hourAngle));
			var azimuth = Mathf.Atan2(-Mathf.Sin(hourAngle),
				-Mathf.Cos(hourAngle) * Mathf.Sin(latitudeRad) + Mathf.Tan(declination) * Mathf.Cos(latitudeRad));
			return new(Mathf.Sin(azimuth) * Mathf.Cos(altitude),
				Mathf.Sin(altitude),
				Mathf.Cos(azimuth) * Mathf.Cos(altitude));
		}
	}
}
