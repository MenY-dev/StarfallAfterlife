using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class CharacterDetachments : ProfileDictionary<int, Detachment>
    {
        protected override Dictionary<int, Detachment> InternalDictionary { get; } = new()
        {
            { 1732028966, new Detachment(167313035, 167313036, 167313034) },
            { 2121598671, new Detachment(2046575662, 21693133, 21693132, 21693131) },
            { 363684728, new Detachment(1209898114, 2023556878, 2023556877, 2023556876, 1342488378) },
            { 753254433, new Detachment(1456637515, 1877936974, 1877936973, 1243108152, 1243108151) },
            { 1532393843, new Detachment(1532393843, 1586697169, 1586697170, 1586697167, 1044347698, 1044347697) },
            { 1921963548, new Detachment(1441077265, 1441077264, 944967472, 944967471, 944967470) },
            { 943189015, new Detachment(1004217556, 1004217555, 646826790, 646826789, 1726093013) },
            { 553619310, new Detachment(542850873, 1149837458, 746207017, 746207018, 746207016, 426825976) },
            { 1240746291, new Detachment(1184099728, 1184099729, 530857457, 530857458, 530857459, 883897137) },
            { 1630315996, new Detachment(1038479824, 431477231, 431477230, 676955403, 676955402) },
            { 261971758, new Detachment(747240019, 747240018, 232716777, 232716776, 263071933, 263071932) },
            { 651541463, new Detachment(601620115, 133336550, 133336549, 56130197, 1706324904) },
            { 1430680873, new Detachment(310380309, 2082059744, 2082059743, 1789730376, 1789730377, 1789730375) },
            { 1820250578, new Detachment(1582788641, 1582788642, 1582788643, 1582788640, 299059459) },
        };

        public override Detachment this[int id] { get => GetValue(id); set => SetValue(id, value); }

        protected override void SetValue(int key, Detachment value, bool addNew = true)
        {
            GetValue(key)?.Update(value);
        }

        public override void Add(int key, Detachment value) { }

        public override void Add(KeyValuePair<int, Detachment> item) { }

        public override void Clear() { }

        public override bool Remove(int key) => false;

        public override bool Remove(KeyValuePair<int, Detachment> item) => false;
    }
}
