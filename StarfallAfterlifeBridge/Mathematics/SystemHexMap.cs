using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Mathematics
{
    public class SystemHexMap : IEnumerable<bool>
    {
        protected static readonly int[] LengthMap = new int[]
        {
            17, 35, 54, 74, 95, 117, 140, 164, 189,
            215, 242, 270, 299, 329, 360, 392, 425,
            457, 488, 518, 547, 575, 602, 628, 653,
            677, 700, 722, 743, 763, 782, 800, 817
        };

        public const float SystemHexSizeX = 1.5f;
        public const float SystemHexSizeY = 1.732050807568877f;
        public const int HexesCount = 817;

        public int Filling { get; protected set; }

        public BitArray Map { get; }

        public bool this[int index]
        {
            get => GetHex(index);
            set => SetHex(index, value);
        }

        public bool this[int x, int y]
        {
            get => GetHex(x, y);
            set => SetHex(x, y, value);
        }

        public bool this[SystemHex hex]
        {
            get => GetHex(hex);
            set => SetHex(hex, value);
        }

        public SystemHexMap()
        {
            Map = new BitArray(HexesCount);
        }


        public SystemHexMap(bool defaultValue)
        {
            Map = new BitArray(HexesCount, defaultValue);

            if (defaultValue == true)
                Filling = Map.Count;
        }

        public SystemHexMap(Func<SystemHex, bool> predicate)
        {
            Map = new BitArray(HexesCount);

            if (predicate is null)
                return;

            for (int i = 0; i < HexesCount; i++)
                SetHex(i, predicate.Invoke(ArrayIndexToHex(i)));
        }

        public SystemHexMap(Func<int, bool> predicate)
        {
            Map = new BitArray(HexesCount);

            if (predicate is null)
                return;

            for (int i = 0; i < HexesCount; i++)
                SetHex(i, predicate.Invoke(i));
        }

        public SystemHexMap(byte[] data)
        {
            if (data is null || data.Length < 103)
                Map = new BitArray(HexesCount);
            else
                Map = new BitArray(data);

            foreach (var item in Map)
                if ((bool)item == true)
                    Filling++;
        }

        public SystemHexMap(string base64data)
        {
            try
            {
                Map = Base64DataToMap(Convert.FromBase64String(base64data));
            }
            catch
            {
                Map = new BitArray(HexesCount);
            }

            foreach (var item in Map)
                if ((bool)item == true)
                    Filling++;
        }

        public static int HexToArrayIndex(SystemHex hex) => HexToArrayIndex(hex.X, hex.Y);

        public static int HexToArrayIndex(int x, int y)
        {
            if (x < -16 || x > 16 ||
                y < -16 || y > 16 ||
                Math.Abs(x + y) > 16)
                return -1;

            int line = Math.Abs(y - 16);
            int index = 16 + x;

            if (line > 0 && line < 33)
                index += LengthMap[line - 1];

            if (y < 0)
                index += y;

            return index;
        }

        public static bool ArrayIndexToHex(int index, out SystemHex hex)
        {
            hex = new SystemHex();

            if (index < 0 || index >= HexesCount)
                return false;

            int searchResult = Array.BinarySearch(LengthMap, index);
            int line = searchResult < 0 ? ~searchResult : searchResult + 1;

            hex.Y = 16 - line;
            hex.X = index - 16;

            if (line > 0)
                hex.X -= LengthMap[line - 1];

            if (hex.Y < 0)
                hex.X -= hex.Y;

            return true;
        }

        public static SystemHex ArrayIndexToHex(int index)
        {
            if (ArrayIndexToHex(index, out SystemHex hex) == true)
                return hex;

            return SystemHex.Zero;
        }

        public static Vector2 ArrayIndexToSystemPoint(int index) =>
            HexToSystemPoint(ArrayIndexToHex(index));

        public static Vector2 HexToSystemPoint(SystemHex hex)
        {
            return new Vector2
            {
                X = hex.Y * SystemHexSizeX,
                Y = (hex.X + hex.Y * 0.5f) * SystemHexSizeY
            };
        }

        public static Vector2 HexToSystemPoint(int x, int y)
        {
            return new Vector2
            {
                X = y * SystemHexSizeX,
                Y = (x + y * 0.5f) * SystemHexSizeY
            };
        }

        public static SystemHex SystemPointToHex(Vector2 point)
        {
            float yH = point.Y * 0.57735026f;
            float xT = point.X * 0.33333334f;
            float u = (float)(point.X * 0.66666669f);
            float v = yH - xT;
            float w = xT - yH - u;

            int uR = (int)Math.Round(u);
            int vR = (int)Math.Round(v);
            int wR = (int)Math.Round(w);

            float uO = Math.Abs(uR - u);
            float vO = Math.Abs(vR - v);
            float wO = Math.Abs(wR - w);

            if (vO <= wO || vO <= uO)
            {
                if (wO <= uO)
                    uR = -(wR + vR);
            }
            else
                vR = -(uR + wR);

            return new SystemHex(vR, uR);
        }

        protected static BitArray Base64DataToMap(byte[] data)
        {
            var resultMap = new BitArray(HexesCount);

            if (data.Length > 102)
            {
                for (int i = 0; i < resultMap.Count; i++)
                {
                    byte chunk = data[i >> 3];
                    byte mask = (byte)(1 << (7 - (i % 8)));
                    resultMap[i] = (chunk & mask) != 0;
                }
            }

            return resultMap;
        }

        public string ToBase64String()
        {
            byte[] bytes = new byte[103];

            for (int i = 0; i < Map.Count; i++)
            {
                if (Map[i] == true)
                    bytes[i >> 3] |= (byte)(1 << (7 - (i % 8)));
            }

            return Convert.ToBase64String(bytes);
        }

        public bool GetHex(SystemHex hex) => GetHex(hex.X, hex.Y);

        public bool GetHex(int x, int y)
        {
            int index;

            if ((index = HexToArrayIndex(x, y)) > -1)
                return GetHex(index);

            return false;
        }

        public bool GetHex(int index) => index >= 0 && index < HexesCount && Map.Get(index);

        public void SetHex(SystemHex hex, bool value) => SetHex(hex.X, hex.Y, value);

        public void SetHex(int x, int y, bool value)
        {
            int index;

            if ((index = HexToArrayIndex(x, y)) > -1)
                SetHex(index, value);
        }

        public void SetHex(int index, bool value)
        {
            var currentValue = GetHex(index);

            if (value != currentValue)
            {
                Map.Set(index, value);
                Filling += value == true ? 1 : -1;
            }
        }

        public void SetAll(bool value)
        {
            Map.SetAll(value);
            Filling = value == true ? Map.Count : 0;
        }

        public IEnumerable<SystemHex> GetCheckedHexes()
        {
            for (int i = 0; i < Map.Count; i++)
            {
                if (this[i] == true)
                    yield return ArrayIndexToHex(i);
            }
        }

        public IEnumerable<SystemHex> GetUncheckedHexes()
        {
            for (int i = 0; i < Map.Count; i++)
            {
                if (this[i] == false)
                    yield return ArrayIndexToHex(i);
            }
        }

        public IEnumerator<bool> GetEnumerator()
        {
            foreach (var item in Map)
                yield return (bool)item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Map.GetEnumerator();
        }
    }
}
