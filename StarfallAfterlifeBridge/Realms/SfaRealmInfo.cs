using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Realms
{
    public class SfaRealmInfo
    {
        public SfaRealm Realm { get; set; }

        public List<CharacterProgress> Progress { get; } = new();

        public string RealmDirectory { get; set; }

        public string ProgressDirectory => Path.Combine(RealmDirectory, "Progress");

        public void Save()
        {
            Realm?.Save(RealmDirectory);
            SaveProgress();
        }

        public void SaveInfo() => Realm?.SaveInfo(RealmDirectory);

        public void SaveDatabase() => Realm?.SaveDatabase(RealmDirectory);

        public void SaveProgress()
        {
            foreach (var item in Progress)
                item?.Save();
        }

        public void Load()
        {
            Realm?.Load(RealmDirectory);
            LoadProgress();
        }

        public bool LoadInfo() => (Realm ??= new()).LoadInfo(RealmDirectory);

        public bool LoadDatabase() => (Realm ??= new()).LoadDatabase(RealmDirectory);

        public CharacterProgress CreateProgress(int charId)
        {
            return new CharacterProgress
            {
                CharacterId = charId,
                Path = Path.Combine(ProgressDirectory, $"char_{charId}_progress.json"),
            };
        }

        public void LoadProgress()
        {
            try
            {
                if (Directory.Exists(ProgressDirectory) == false)
                    return;

                var progressPaths = Directory.GetFiles(ProgressDirectory, "*.json");

                foreach (var path in progressPaths)
                {
                    var progress = new CharacterProgress() { Path = path };

                    if (progress.Load() == true)
                        Progress.Add(progress);
                }
            }
            catch { }
        }

        public void RemoveProgress(int charId)
        {
            try
            {
                foreach (var item in Progress)
                {
                    if (item.CharacterId == charId)
                        item.RemoveProgressFile();
                }

                Progress.RemoveAll(p => p.CharacterId == charId);
            }
            catch { }
        }

        public void RemoveRealmFiles()
        {
            try
            {
                if (Directory.Exists(RealmDirectory) == true)
                    Directory.Delete(RealmDirectory, true);
            }
            catch { }
        }
    }
}
