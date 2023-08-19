using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class TagNode : ICloneable
    {
        public string Name { get; set; }
        public TagNode ParentNode { get; set; }
        public List<TagNode> ChildNodes { get; protected set; } = new();

        public string GetFullPath()
        {
            var parentPath = ParentNode?.GetFullPath();
            return parentPath is null ? Name : $"{parentPath}.{Name}";
        }

        public override string ToString() => GetFullPath();

        public TagNode AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return null;

            var currentNode = this;

            foreach (var name in tag.Split(
                new char[] { '.', '\\', '/' },
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries))
            {
                var existNode = currentNode.ChildNodes.FirstOrDefault(n =>
                    StringComparer.InvariantCultureIgnoreCase.Compare(n?.Name, name) == 0);

                if (existNode is null)
                {
                    var newNode = new TagNode(){ Name = name };
                    currentNode.AddTag(newNode);
                    currentNode = newNode;
                }
                else
                {
                    currentNode = existNode;
                }
            }

            return currentNode;
        }

        public void AddTag(TagNode tag)
        {
            if (tag is null ||
                GetAllNodes().Contains(tag) ||
                ParentNode == tag)
                return;

            tag.ParentNode = this;
            ChildNodes.Add(tag);
        }

        protected IEnumerable<TagNode> GetAllNodes()
        {
            yield return this;

            foreach (var item in ChildNodes.SelectMany(c => c.GetAllNodes()))
                yield return item;
        }

        object ICloneable.Clone() => Clone();

        public TagNode Clone()
        {
            var clone = new TagNode()
            {
                Name = Name?.ToString(),
            };

            foreach (var child in ChildNodes)
                clone.AddTag(child?.Clone());

            return clone;
        }
    }
}
