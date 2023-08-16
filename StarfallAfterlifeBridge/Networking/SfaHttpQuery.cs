using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StarfallAfterlife.Bridge.Networking
{
    public class SfaHttpQuery : ICollection<SfaHttpQuery.Parameter>
    {
        public string Function { get; protected set; }

        public int Count => Parameters.Count;

        public bool IsReadOnly => true;

        public Parameter this[string i]
        {
            get
            {
                foreach (var item in Parameters)
                    if (item.Key == i)
                        return item;

                return null;
            }
        }

        protected List<Parameter> Parameters { get; } = new List<Parameter>();

        public static SfaHttpQuery Parse(string httpQuery)
        {
            if (httpQuery is null)
                return new SfaHttpQuery();

            return Parse(HttpUtility.ParseQueryString(httpQuery));
        }

        public static SfaHttpQuery Parse(NameValueCollection httpQuery)
        {
            SfaHttpQuery query = new SfaHttpQuery();

            if (httpQuery is null)
                return query;

            foreach (var key in httpQuery.Keys)
            {
                if (string.IsNullOrWhiteSpace(key as string))
                    continue;

                if (key is string stringKey && string.IsNullOrWhiteSpace(stringKey) == false)
                {
                    foreach (var value in httpQuery.GetValues(stringKey))
                    {
                        if (string.IsNullOrWhiteSpace(value))
                            continue;

                        if (stringKey == "func" && query.Function is null)
                            query.Function = value;

                        query.Parameters.Add(new Parameter(stringKey, value));
                    }
                }
            }

            return query;
        }

        public SfaHttpQuery Union(SfaHttpQuery parameters)
        {
            if (parameters is null)
                return this;

            foreach (var item in parameters)
            {
                if (item is null ||
                    string.IsNullOrWhiteSpace(item.Key) == true ||
                    item.Key == "func")
                    continue;

                if (Parameters.Exists(p => p.Key == item.Key))
                    continue;

                Parameters.Add(new Parameter(item.Key, item.Value));

            }

            return this;
        }

        public bool Contains(Parameter item) => Parameters.Contains(item);

        public void CopyTo(Parameter[] array, int arrayIndex) => Parameters.CopyTo(array, arrayIndex);

        public IEnumerable<Parameter> FindAllStartsWith(string key, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            foreach (var item in Parameters)
                if (item?.Key?.StartsWith(key, comparisonType) == true)
                    yield return item;
        }

        public SfaHttpQuery StartsWith(string key, bool trimKeys = false, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            NameValueCollection newParameters = new (Parameters.Count);

            if (string.IsNullOrWhiteSpace(key) == true)
                return Parse(newParameters);

            int keySize = key.Length;

            if (trimKeys == true)
            {

                foreach (var item in Parameters)
                    if (item?.Key?.StartsWith(key, comparisonType) == true)
                        newParameters.Add(item.Key.Substring(keySize), item.Value);
            }
            else
            {
                foreach (var item in Parameters)
                    if (item?.Key?.StartsWith(key, comparisonType) == true)
                        newParameters.Add(item.Key, item.Value);
            }

            return Parse(newParameters);
        }

        public IEnumerator<Parameter> GetEnumerator() => Parameters.GetEnumerator();

        void ICollection<Parameter>.Add(Parameter item) { }

        void ICollection<Parameter>.Clear() { }

        bool ICollection<Parameter>.Remove(Parameter item) => false;

        IEnumerator IEnumerable.GetEnumerator() => Parameters.GetEnumerator();

        public override string ToString()
        {
            string text = string.Empty;

            foreach (var item in Parameters)
            {
                text += $"&{item.Key}={item.Value}";
            }

            return text.TrimStart('&');
        }

        public class Parameter
        {
            public string Key { get; }
            public string Value { get; }

            public Parameter(string key, string value)
            {
                Key = key;
                Value = value;
            }

            public static explicit operator bool?(Parameter parameter)
            {
                string value = parameter?.Value;

                if (value is null)
                    return null;

                if (string.IsNullOrWhiteSpace(value) ||
                    (long)parameter <= 0 ||
                    (double)parameter <= 0)
                    return false;

                return true;
            }

            public static explicit operator int?(Parameter parameter)
            {
                if (int.TryParse(parameter?.Value, out int value))
                    return value;

                return null;
            }

            public static explicit operator long?(Parameter parameter)
            {
                if (long.TryParse(parameter?.Value, out long value))
                    return value;

                return null;
            }

            public static explicit operator float?(Parameter parameter)
            {
                if (float.TryParse(parameter?.Value, out float value))
                    return value;

                return null;
            }

            public static explicit operator double?(Parameter parameter)
            {
                if (double.TryParse(parameter?.Value, out double value))
                    return value;

                return null;
            }

            public static explicit operator string(Parameter parameter)
            {
                return parameter?.Value;
            }

            public override string ToString()
            {
                return Value ?? string.Empty;
            }
        }
    }
}
