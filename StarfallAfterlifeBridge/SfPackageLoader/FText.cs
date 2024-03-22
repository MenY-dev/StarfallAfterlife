using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public readonly record struct FText(string Text, string Key, string Namespace = null)
    {
        public static explicit operator string(FText text) => text.Text;

        public static implicit operator FText(string text) => new(text, null, null);

        public static implicit operator FText((string, string) text) => new(text.Item1, text.Item2);

        public static implicit operator FText((string, string, string) text) => new(text.Item1, text.Item2, text.Item3);
    }
}
