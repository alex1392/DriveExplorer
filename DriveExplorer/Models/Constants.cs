namespace DriveExplorer.Models {
	public static class ContentTypes {
		public static class Application {
			private const string application = "application";
			public const string json = application + "/json";
			public const string javascript = application + "/javascript";
			public const string xml = application + "/xml";
		}
		public static class Text {
			private const string text = "text";
			public const string plain = text + "/plain";
			public const string xml = text + "/xml";
			public const string html = text + "/html";
		}
	}
}