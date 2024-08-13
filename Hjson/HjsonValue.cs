using System;
using System.IO;

namespace Hjson
{
	/// <summary>Contains functions to load and save in the Hjson format.</summary>
	public static class HjsonValue
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,

			Hex = 1 << 0,
			Inline = 1 << 1,
			InsertNewLine = 1 << 2,
			NoEscape = 1 << 3,
			Multiline = 1 << 4,
			Force = 1 << 5,
			NextNewLine = 1 << 6,
		}

		/// <summary>Loads Hjson/JSON from a file.</summary>
		public static JsonValue Load(string path) => LoadInternal(path, null, null);

		/// <summary>Loads Hjson/JSON from a file, optionally preserving whitespace and comments.</summary>
		public static JsonValue Load(string path, HjsonOptions options) => LoadInternal(path, null, options);

		/// <summary>Loads Hjson/JSON from a stream.</summary>
		public static JsonValue Load(Stream stream) => LoadInternal(stream, null, null);

		/// <summary>Loads Hjson/JSON from a stream, optionally preserving whitespace and comments.</summary>
		public static JsonValue Load(Stream stream, HjsonOptions options) => LoadInternal(stream, null, options);

		/// <summary>Loads Hjson/JSON from a TextReader.</summary>
		public static JsonValue Load(TextReader textReader, IJsonReader jsonReader = null) => LoadInternal(textReader, jsonReader, null);

		/// <summary>Loads Hjson/JSON from a TextReader, optionally preserving whitespace and comments.</summary>
		public static JsonValue Load(TextReader textReader, HjsonOptions options, IJsonReader jsonReader = null) => LoadInternal(textReader, jsonReader, options);

		/// <summary>Loads Hjson/JSON from a TextReader, preserving whitespace and comments.</summary>
		[Obsolete("Use Load", true)]
		public static JsonValue LoadWsc(TextReader textReader) => LoadInternal(textReader, null, new HjsonOptions { KeepWsc = true });

		private static JsonValue LoadInternal(string path, IJsonReader jsonReader, HjsonOptions options)
		{
			if (Path.GetExtension(path).ToLower() == ".json") return JsonValue.Load(path);
			try
			{
				using (var s = File.OpenRead(path))
					return LoadInternal(s, jsonReader, options);
			}
			catch (Exception e) { throw new Exception(e.Message + " (in " + path + ")", e); }
		}

		private static JsonValue LoadInternal(Stream stream, IJsonReader jsonReader, HjsonOptions options)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			return LoadInternal(new StreamReader(stream, true), jsonReader, options);
		}

		private static JsonValue LoadInternal(TextReader textReader, IJsonReader jsonReader, HjsonOptions options)
		{
			if (textReader == null) throw new ArgumentNullException("textReader");
			return new HjsonReader(textReader, jsonReader, options).Read();
		}

		/// <summary>Parses the specified Hjson/JSON string.</summary>
		public static JsonValue Parse(string hjsonString)
		{
			if (hjsonString == null) throw new ArgumentNullException("hjsonString");
			return Load(new StringReader(hjsonString));
		}

		/// <summary>Parses the specified Hjson/JSON string, optionally preserving whitespace and comments.</summary>
		public static JsonValue Parse(string hjsonString, HjsonOptions options)
		{
			if (hjsonString == null) throw new ArgumentNullException("hjsonString");
			return Load(new StringReader(hjsonString), options);
		}

		/// <summary>Saves Hjson to a file.</summary>
		public static void Save(JsonValue json, string path, HjsonOptions options = null)
		{
			if (Path.GetExtension(path).ToLower() == ".json") { json.Save(path, Stringify.Formatted); return; }
			using (var s = File.CreateText(path))
				Save(json, s, options);
		}

		/// <summary>Saves Hjson to a stream.</summary>
		public static void Save(JsonValue json, Stream stream, HjsonOptions options = null)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			Save(json, new StreamWriter(stream), options);
		}

		/// <summary>Saves Hjson to a TextWriter.</summary>
		public static void Save(JsonValue json, TextWriter textWriter, HjsonOptions options = null)
		{
			if (textWriter == null) throw new ArgumentNullException("textWriter");
			new HjsonWriter(options).Save(json, textWriter, 0, false, "", true, true);
			textWriter.Flush();
		}

		internal static bool IsPunctuatorChar(char ch) => ch == '{' || ch == '}' || ch == '[' || ch == ']' || ch == ',' || ch == ':';
	}
}
