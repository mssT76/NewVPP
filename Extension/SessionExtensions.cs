using Newtonsoft.Json;

namespace NewVPP.Extension
{
    public static class SessionExtensions
    {
		public static void SetObjectAsJson(this ISession session, object value, string key)
		{
			session.SetString(key, JsonConvert.SerializeObject(value));
		}

		public static T GetObjectFromJson<T>(this ISession session, string key)
		{
			var value = session.GetString(key);
			return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
		}
	}
}
