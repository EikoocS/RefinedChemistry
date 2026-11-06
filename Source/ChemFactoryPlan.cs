using System;
using System.Collections.Generic;
using System.Linq;

namespace RefinedChemistry
{
    internal sealed class ChemFactoryPlan
    {
        internal readonly List<int[]> Products = new();
        internal int[] SourceParts;
        internal int[] AuxiliaryParts;
        internal double MaximumPurity;
        internal int MaximumCount;
        internal int SourceVolume;
        internal int AuxiliaryVolume;

        internal static ChemFactoryPlan Calculate(int[] source, int[] auxiliary, int volume,
            int active, int contaminant, int loss, double? target, int capacity, int quantity)
        {
            var plan = new ChemFactoryPlan();
            int sourceTotal = source.Sum();
            if (sourceTotal == 0)
                return plan;

            plan.MaximumPurity = GetMaximumPurity(source, volume, active, loss);
            int auxiliaryTotal = auxiliary.Sum();
            int sourceVolume = volume;
            if (target.HasValue && target.Value < plan.MaximumPurity)
            {
                if (auxiliaryTotal == 0)
                    return plan;
                decimal concentrationDifference = (decimal)source[active] * auxiliaryTotal -
                    (decimal)auxiliary[active] * sourceTotal;
                if (concentrationDifference <= 0 ||
                    (decimal)target.Value * auxiliaryTotal < (decimal)auxiliary[active] * (100 - loss))
                    return plan;
                decimal requiredActive = Math.Ceiling(volume * (decimal)target.Value / 100);
                if ((double)requiredActive / volume * 100 < target.Value)
                    requiredActive++;
                decimal beforePollution = Math.Ceiling(requiredActive * 100 / (100 - loss));
                sourceVolume = (int)Math.Clamp(Math.Ceiling(
                    (beforePollution * auxiliaryTotal - (decimal)volume * auxiliary[active]) * sourceTotal /
                    concentrationDifference), 1, volume);
            }

            int auxiliaryVolume = volume - sourceVolume;
            plan.MaximumCount = Math.Min(capacity, sourceTotal / sourceVolume);
            if (auxiliaryVolume > 0)
                plan.MaximumCount = Math.Min(plan.MaximumCount, auxiliaryTotal / auxiliaryVolume);
            int count = Math.Clamp(quantity, 0, plan.MaximumCount);
            if (count == 0)
                return plan;

            plan.SourceVolume = sourceVolume * count;
            plan.AuxiliaryVolume = auxiliaryVolume * count;
            plan.SourceParts = Split(source, plan.SourceVolume, active, roundUp: true);
            plan.AuxiliaryParts = Split(auxiliary, plan.AuxiliaryVolume, active, roundUp: true);
            var mixed = new int[source.Length];
            for (int i = 0; i < mixed.Length; i++)
                mixed[i] = plan.SourceParts[i] + plan.AuxiliaryParts[i];
            int totalVolume = volume * count;
            int removed = (int)((long)totalVolume * loss / 100);
            int[] finished = Split(mixed, totalVolume - removed, active, roundUp: false);
            finished[contaminant] += removed;
            int activeTotal = finished[active];
            finished[active] = 0;
            for (int i = 0; i < count; i++)
            {
                int activeAmount = activeTotal / count + (i < activeTotal % count ? 1 : 0);
                int[] parts = WaterHelper.SplitProportionally(finished, volume - activeAmount);
                for (int j = 0; j < parts.Length; j++)
                    finished[j] -= parts[j];
                parts[active] = activeAmount;
                plan.Products.Add(parts);
            }
            return plan;
        }

        internal static double GetMaximumPurity(int[] source, int volume, int active, int loss)
        {
            int maximumActive = (int)((long)volume * source[active] * (100 - loss) / (source.Sum() * 100L));
            return (double)maximumActive / volume * 100;
        }

        private static int[] Split(int[] parts, int volume, int active, bool roundUp)
        {
            if (volume == 0)
                return new int[parts.Length];
            int total = parts.Sum();
            long numerator = (long)parts[active] * volume;
            int activeAmount = (int)((numerator + (roundUp ? total - 1 : 0)) / total);
            var otherParts = (int[])parts.Clone();
            otherParts[active] = 0;
            int[] result = WaterHelper.SplitProportionally(otherParts, volume - activeAmount);
            result[active] = activeAmount;
            return result;
        }
    }
}
