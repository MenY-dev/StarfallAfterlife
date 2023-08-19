using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.MobsEditor
{
    public class MobTagViewModel : ViewModelBase
    {
        public TagNode Tag { get => _tag; set => SetAndRaise(ref _tag, value); }

        public MobFleetViewModel Fleet { get => _fleet; set => SetAndRaise(ref _fleet, value); }
        
        public ObservableCollection<MobTagViewModel> ChildTags { get; } = new();
        
        public bool Added
        {
            get => Fleet?.Info?.Tags.Contains(
                Tag?.GetFullPath(),
                StringComparer.InvariantCultureIgnoreCase) ?? false;
            set
            {
                var fleet = Fleet;
                var tag = Tag.GetFullPath();;

                if (fleet is null || tag is null)
                {
                    RaisePropertyChanged(false, false);
                    return;
                }

                var oldValue = fleet.GetTag(tag);
                fleet.SetTag(tag, value);
                RaisePropertyChanged(oldValue, value);
            }
        }

        private TagNode _tag;
        
        private MobFleetViewModel _fleet;

        public static MobTagViewModel Create(TagNode node, MobFleetViewModel fleet)
        {
            var tag = new MobTagViewModel
            {
                Tag = node,
                Fleet = fleet
            };

            if (node is null)
                return tag;

            foreach (var item in node.ChildNodes)
                tag.ChildTags.Add(Create(item, fleet));

            return tag;
        }
    }
}
