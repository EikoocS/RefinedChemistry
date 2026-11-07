using System.Collections.Generic;

namespace RefinedChemistry.Machinery
{
    internal static class ChemFactoryOutput
    {
        internal static int Capacity(GameGridInventory inventory, GridShape product)
        {
            var occupied = new List<GridShape> { inventory.inventoryShape };
            foreach (var item in inventory.childItems)
                occupied.Add(item.modifiedShape);
            var candidate = new GridShapeBuilder(product);
            int count = 0;
            while (Place(candidate, occupied, inventory.inventoryShape.width, inventory.inventoryShape.height))
            {
                occupied.Add(candidate.shape.Clone());
                count++;
            }
            return count;
        }

        private static bool Place(GridShapeBuilder candidate, List<GridShape> occupied, int width, int height)
        {
            for (int flip = 0; flip < 2; flip++)
            {
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            candidate.SetTransform(x, y, flip != 0, 4 - rotation);
                            if (!candidate.shape.FullyIntersectsRect(width, height,
                                (pixel, outside) => pixel == 0 || outside == 0))
                                continue;
                            if (occupied.TrueForAll(shape => !candidate.shape.Intersects(shape)))
                                return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
