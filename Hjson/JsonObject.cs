using System;
using System.Collections;
using System.Collections.Generic;

namespace Hjson
{
	using JsonPair = KeyValuePair<string, JsonValue>;

	/// <summary>Implements an object value.</summary>
	public class JsonObject: JsonValue, IDictionary<string, JsonValue>, ICollection<JsonPair>
	{
		private readonly Dictionary<string, JsonValue> map;

		/// <summary>Initializes a new instance of this class.</summary>
		/// <remarks>You can also initialize an object using the C# add syntax: new JsonObject { { "key", "value" }, ... }</remarks>
		public JsonObject(params JsonPair[] items)
		{
			this.map = new Dictionary<string, JsonValue>();
			if (items != null) this.AddRange(items);
		}

		/// <summary>Initializes a new instance of this class.</summary>
		/// <remarks>You can also initialize an object using the C# add syntax: new JsonObject { { "key", "value" }, ... }</remarks>
		public JsonObject(IEnumerable<JsonPair> items)
		{
			if (items == null) throw new ArgumentNullException("items");
			this.map = new Dictionary<string, JsonValue>();
			this.AddRange(items);
		}

		/// <summary>Gets the count of the contained items.</summary>
		public override int Count => this.map.Count;

		IEnumerator<JsonPair> IEnumerable<JsonPair>.GetEnumerator() => this.map.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.map.GetEnumerator();

		/// <summary>Gets or sets the value for the specified key.</summary>
		public sealed override JsonValue this[string key]
		{
			get
			{
				this.map.TryGetValue(key, out var value);
				return value;
			}
			set => this.map[key] = value;
		}

		/// <summary>The type of this value.</summary>
		public override JsonType JsonType => JsonType.Object;

		/// <summary>Gets the keys of this object.</summary>
		public ICollection<string> Keys => this.map.Keys;

		/// <summary>Gets the values of this object.</summary>
		public ICollection<JsonValue> Values => this.map.Values;

		/// <summary>Adds a new item.</summary>
		/// <remarks>You can also initialize an object using the C# add syntax: new JsonObject { { "key", "value" }, ... }</remarks>
		public void Add(string key, JsonValue value)
		{
			if (key == null) throw new ArgumentNullException("key");
			this.map[key] = value; // json allows duplicate keys
		}

		/// <summary>Adds a new item.</summary>
		public void Add(JsonPair pair) => this.Add(pair.Key, pair.Value);

		/// <summary>Adds a range of items.</summary>
		public void AddRange(IEnumerable<JsonPair> items)
		{
			if (items == null) throw new ArgumentNullException("items");
			foreach (var pair in items) this.Add(pair);
		}

		/// <summary>Clears the object.</summary>
		public void Clear() => this.map.Clear();

		bool ICollection<JsonPair>.Contains(JsonPair item) => (this.map as ICollection<JsonPair>).Contains(item);

		bool ICollection<JsonPair>.Remove(JsonPair item) => (this.map as ICollection<JsonPair>).Remove(item);

		/// <summary>Determines whether the array contains a specific key.</summary>
		public override bool ContainsKey(string key)
		{
			if (key == null) throw new ArgumentNullException("key");
			return this.map.ContainsKey(key);
		}

		/// <summary>Copies the elements to an System.Array, starting at a particular System.Array index.</summary>
		public void CopyTo(JsonPair[] array, int arrayIndex) => (this.map as ICollection<JsonPair>).CopyTo(array, arrayIndex);

		/// <summary>Removes the item with the specified key.</summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
		public bool Remove(string key)
		{
			if (key == null) throw new ArgumentNullException("key");
			return this.map.Remove(key);
		}

		bool ICollection<JsonPair>.IsReadOnly => false;

		/// <summary>Gets the value associated with the specified key.</summary>
		public bool TryGetValue(string key, out JsonValue value) => this.map.TryGetValue(key, out value);

		/// <summary>Gets the value associated with the specified key.</summary>
		public bool TryGetValue<T>(string key, out T value)
		{
			value = default;

			var obj = default(object);
			if (this.map.TryGetValue(key, out var json) && (obj = Convert.ChangeType(json.ToValue(), typeof(T))) != null)
			{
				value = (T)obj;
				return true;
			}

			return false;
		}

		public bool TryGetValue1<T>(string key, ref T value)
		{
			var obj = default(object);
			if (this.map.TryGetValue(key, out var json) && (obj = Convert.ChangeType(json.ToValue(), typeof(T))) != null)
			{
				value = (T)obj;
				return true;
			}

			return false;
		}

		public bool TryGetValue2<T>(string key, ref T value) where T: IJsonConvertible<T>
		{
			if (this.map.TryGetValue(key, out var json) && T.TryDeserialize(json, out value))
			{
				return true;
			}
			return false;
		}

		public bool TryGetValue3<T>(string key, ref T value) where T : unmanaged, Enum
		{
			if (this.map.TryGetValue(key, out var json))
			{
				var enum_value = json.Qs();
				if (Enum.TryParse<T>(enum_value, true, out value))
				{
					return true;
				}
			}
			return false;
		}

		void ICollection<JsonPair>.Add(JsonPair item) => this.Add(item);

		void ICollection<JsonPair>.Clear() => this.Clear();

		void ICollection<JsonPair>.CopyTo(JsonPair[] array, int arrayIndex) => this.CopyTo(array, arrayIndex);

		int ICollection<JsonPair>.Count => this.Count;
	}
}
