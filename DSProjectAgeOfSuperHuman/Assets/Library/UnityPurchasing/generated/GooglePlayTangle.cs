// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("NYcEJzUIAwwvg02D8ggEBAQABQaESpw0KY5FSrlVS9wI9I3P/kReQz38iaTA7a10sQtTjKMVDkyt/MD74wSuZS2jah6q1T43/kuoy9gBhheJJrnBU1VhbWKudV6LsUvqKralDAoLkYC4xW1T9VUG6qgjgBhVbbtQBukrn89FGNpObCHb8jE/WptcOk5gWZXesVWvxWbrgxhMzKJG0mYu033lx4jj5rYIYKzRJk5iIDuBboI14voW4B5MhsvRi5DvrvswiWq9c0qHBAoFNYcEDweHBAQFwvTmxTHi1c2XzsdjQAoyQaEF5dMcvb6RS6MErJfDs58f5dFU7HD2lFHZijs29Fh3sefUWoeDNxcGDM0fA5TFWxANjO5XNoKWZKKu4gcGBAUE");
        private static int[] order = new int[] { 0,8,6,6,4,10,12,13,10,11,10,12,13,13,14 };
        private static int key = 5;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
