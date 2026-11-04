namespace RefinedChemistry.Item
{
    internal static class LabelerFactory
    {
        internal const string ItemId = "labeler_factory";
        private const string GradeTag = "LABELER_FACTORY_GRADE";

        internal static GameItem Create()
        {
            var item = ItemDirectory.CreateEmptyItem(ItemId)
                .SetSpriteAndShapeFromMod(RefinedChemistry.Instance.Manifest.ID, ItemId);
            item.name = LocalizationHelper.Format(ItemId + "_name");
            item.shortDescription = LocalizationHelper.Format(ItemId + "_desc");
            item.unitValue = 50;
            item.SetGameItemType("TOOL");
            AudioHelper.InitSmallMachine(item);
            item.maySelectSlotItemFunc = (_, _, _) => true;
            item.canSelectSlotItemFunc = (_, _, _) => true;
            item.mayToggleSlotItemFunc = (_, _, _) => true;
            item.canToggleSlotItemFunc = (_, _, _) => true;
            item.onToggleSlotItemFunc = (source, _, _) => source.ModifyTag(GradeTag,
                state => state.Enable().SetInt((GetGradeIndex(source) + 1) % ItemConditionList.conditionChemicalPurity.Count));
            item.mayThisTargetItemFunc = CanRelabel;
            item.canThisTargetItemFunc = CanRelabel;
            item.onThisTargetItemFunc = Relabel;
            return item;
        }

        private static int GetGradeIndex(GameItem item) => item.GetTagReadonly(GradeTag).GetInt();

        internal static ItemCondition GetGrade(GameItem item) =>
            ItemConditionList.conditionChemicalPurity[GetGradeIndex(item)];

        internal static string GetSelectionText(GameItem item) =>
            LocalizationHelper.Format(ItemId + "_grade", GetGrade(item).display);

        internal static bool HasSelectedLabel(GameItem source, GameItem target)
        {
            var feature = ChemicalProductHelper.GetChemicalFeature(target);
            return feature != null &&
                feature.GetCurrentItemCondition().identifier == GetGrade(source).identifier;
        }

        internal static void CreateTooltip(RichTextBuilder builder, GameItem item)
        {
            if (item.identifier == ItemId)
                builder.AddLine(GetSelectionText(item));
        }

        private static bool CanRelabel(GameItem source, GameItem target)
        {
            var feature = ChemicalProductHelper.GetChemicalFeature(target);
            return feature != null && LabelerHelper.IsEditableLabelFeature(feature) &&
                feature.GetCurrentItemCondition().identifier != GetGrade(source).identifier;
        }

        private static void Relabel(GameItem source, GameItem target)
        {
            var feature = ChemicalProductHelper.GetChemicalFeature(target);
            feature.fakeCondition = GetGrade(source);
            feature.SetExposed(false);
            EmporiumAudioPlayer.Instance.audioLabeler.Play();
            AchievementTracker.Instance.CheckLowerLevelsSpecial();
            OverlayHandler.current.UpdateInventoryStateUI();
        }
    }
}
