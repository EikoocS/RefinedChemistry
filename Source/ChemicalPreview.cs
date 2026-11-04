namespace RefinedChemistry
{
    internal sealed class ChemicalPreview : GameItem
    {
        internal ChemicalPreview(string activeIngredient) : base(null)
        {
            state.EnableTag("CHEM_ITEM_TAG");
            state.EnableTag(ChemicalProductHelper.CHEMICAL_PRODUCT_TAG);
            state.GetTag(ChemicalProductHelper.ACTIVATE_INGREDIENT_TAG).Enable().SetString(activeIngredient);
            itemFeatures.Add(new ItemFeature("CATEGORY_CHEMICAL_PURITY") { identifier = "chemical_purity" });
            modifiedState = state;
        }

        internal (double Purity, string Grade) Evaluate(int[] parts)
        {
            for (int i = 0; i < parts.Length; i++)
                state.GetTag(Liquid.Liquids[i].currentPart).Enable().SetInt(parts[i]);
            ChemicalProductHelper.GradeProduct(this);
            return (ChemicalProductHelper.GetPurity(this),
                ChemicalProductHelper.GetChemicalFeature(this).realCondition.display);
        }

        public override void Validate() => modifiedState = state;

        public override void Destroy()
        {
            state.Clear();
            itemFeatures.Clear();
        }
    }
}
