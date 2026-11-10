using System;
using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.Item
{
    internal static class EnderFlaskLinks
    {
        private const string LinkTag = "ENDER_FLASK_LINK_UUID";
        private static readonly Dictionary<string, Group> groups = new();
        private static bool refreshing;

        [Serializable]
        private sealed class SavedGroup
        {
            public List<TagState> tags;
            public string redirect;
        }

        private sealed class Group
        {
            internal readonly Dictionary<string, TagState> Tags = new();
            internal readonly List<GameItem> Members = new();
            internal GameItem TickOwner;

            internal Group(List<TagState> tags)
            {
                foreach (var tag in tags)
                    Tags.Add(tag.identifier, tag);
            }

            internal Group(GameItem source, string id)
            {
                foreach (var liquid in Liquid.Liquids)
                    AddTag(source, liquid.currentPart);
                AddTag(source, ChemicalHelper.CONTAINER_TEMP_TAG);
                AddTag(source, ChemicalHelper.HEATING_TAG);
                AddTag(source, GeneralHelper.CUSTOM_NAME_TAG);
                AddTag(source, GeneralHelper.CUSTOM_NAME_MODE_TAG);
                Tags.Add(LinkTag, new TagState(LinkTag).Enable().SetString(id));
                Tags.Add(TypeHelper.NOT_FOR_RESALE, new TagState(TypeHelper.NOT_FOR_RESALE).Enable());
            }

            private void AddTag(GameItem source, string key) =>
                Tags.Add(key, source.state.GetTag(key).Clone());

            internal void Add(GameItem item)
            {
                foreach (var pair in Tags)
                {
                    item.state.dict[pair.Key] = pair.Value;
                    item.modifiedState.dict[pair.Key] = pair.Value;
                }
                Members.Add(item);
            }
        }

        private static Group Find(GameItem item)
        {
            if (!EnderFlask.IsFlask(item) || !item.state.dict.TryGetValue(LinkTag, out var tag))
                return null;
            groups.TryGetValue(tag.GetString(), out var group);
            return group;
        }

        internal static bool ShareContents(GameItem first, GameItem second)
        {
            var group = Find(first);
            return group != null && group == Find(second);
        }

        internal static bool CanPlace(GameItem item, GameInventory inventory, List<GameItem> incoming = null)
        {
            var group = Find(item);
            if (group == null || inventory == null)
                return true;
            var machines = GraphUtils.FindParentsType<GameItem>(inventory,
                parent => parent.IsTag("MACHINE"), _ => true);
            if (machines.Count == 0)
                return true;
            foreach (var machine in machines)
                foreach (var member in group.Members)
                    if (member != item && GraphUtils.FindParentEquals(member, machine))
                        return false;
            if (incoming != null)
                foreach (var other in incoming)
                    if (other != item && ShareContents(item, other))
                        return false;
            return true;
        }

        internal static void Bind(GameItem first, GameItem second)
        {
            var filled = WaterHelper.GetTotalVolume(first) > 0 ? first : second;
            var empty = filled == first ? second : first;
            var destination = Find(filled);
            if (destination == null)
            {
                string id = Guid.NewGuid().ToString("N");
                destination = new Group(filled, id);
                groups.Add(id, destination);
                destination.Add(filled);
            }

            var previous = Find(empty);
            if (previous == null)
            {
                destination.Add(empty);
            }
            else
            {
                groups.Remove(previous.Tags[LinkTag].GetString());
                Write(previous.Tags[LinkTag].GetString(), new SavedGroup
                {
                    redirect = destination.Tags[LinkTag].GetString()
                });
                foreach (var member in previous.Members)
                    destination.Add(member);
            }
            foreach (var member in destination.Members)
                member.ValidateShapeState();
            Refresh(filled);
        }

        internal static void Register(GameItem item)
        {
            if (!EnderFlask.IsFlask(item) || !item.state.dict.TryGetValue(LinkTag, out var tag))
                return;
            var group = Load(item, tag.GetString());
            group.Add(item);
            WaterHelper.UpdateContainerGoodType(item);
            WaterFeatureHelper.UpdateWaterFeature(item, WaterFeatureHelper.GetWaterFeature(item).IsExposed());
            SpriteHelper.UpdateWaterContainerSprite(item);
        }

        private static Group Load(GameItem item, string id)
        {
            if (groups.TryGetValue(id, out var group))
                return group;
            string json = ModHelper.GetModData(RefinedChemistry.Instance.Manifest.ID, "ender_flask." + id);
            if (json == null)
            {
                group = new Group(item, id);
            }
            else
            {
                var saved = JsonUtility.FromJson<SavedGroup>(json);
                if (saved.redirect != null)
                    return Load(item, saved.redirect);
                group = new Group(saved.tags);
            }
            groups.Add(id, group);
            return group;
        }

        private static void Write(string id, SavedGroup saved) =>
            ModHelper.SetModData(RefinedChemistry.Instance.Manifest.ID, "ender_flask." + id,
                JsonUtility.ToJson(saved));

        internal static void Save()
        {
            foreach (var pair in groups)
                Write(pair.Key, new SavedGroup { tags = new List<TagState>(pair.Value.Tags.Values) });
        }

        internal static void Remove(GameItem item)
        {
            var group = Find(item);
            if (group == null)
                return;
            group.Members.Remove(item);
        }

        internal static void SelectTickOwners(List<GameItem> items)
        {
            foreach (var group in groups.Values)
                group.TickOwner = null;
            foreach (var item in items)
            {
                var group = Find(item);
                if (group != null && group.TickOwner == null)
                    group.TickOwner = item;
            }
        }

        internal static void Tick(GameItem item, GameInventory inventory, SlotMarker slot,
            Action<GameItem, GameInventory, SlotMarker> tick)
        {
            var group = Find(item);
            if (group == null || group.TickOwner == item)
                tick(item, inventory, slot);
        }

        internal static void Refresh(GameItem source)
        {
            var group = Find(source);
            if (group == null || refreshing)
                return;
            refreshing = true;
            try
            {
                foreach (var member in group.Members)
                {
                    if (member == source)
                        continue;
                    WaterHelper.UpdateContainerGoodType(member);
                    WaterFeatureHelper.UpdateWaterFeature(member, WaterFeatureHelper.GetWaterFeature(member).IsExposed());
                    SpriteHelper.UpdateWaterContainerSprite(member);
                }
            }
            finally
            {
                refreshing = false;
            }
        }

        internal static void Clear() => groups.Clear();

        internal static void Clear(string _) => Clear();
    }
}
