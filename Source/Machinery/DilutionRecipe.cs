using System;

namespace RefinedChemistry.Machinery
{
    internal sealed class DilutionRecipe
    {
        private const int MinimumOutputVolumeUl = 1001;

        internal int[] SourceParts { get; private set; }
        internal int[] SolventParts { get; private set; }
        internal int SourceVolume { get; private set; }
        internal int SolventVolume { get; private set; }
        internal int TotalVolume => SourceVolume + SolventVolume;
        internal int SelectedVolume { get; private set; }
        internal int TargetConcentration { get; private set; }
        internal decimal ActualConcentration => 1000m * SelectedVolume / TotalVolume;
        internal bool IsExact => Difference == 0;
        private long Difference => Math.Abs(1000L * SelectedVolume - (long)TargetConcentration * TotalVolume);

        internal static string Calculate(int[] source, int[] solvent, int selected, int target,
            int capacity, Func<int[], int, int[]> split, out DilutionRecipe recipe)
        {
            string error = CalculateExact(source, solvent, selected, target, capacity, split, out recipe);
            if (error == "precision")
            {
                recipe = FindClosest(source, solvent, selected, target, capacity, split);
                if (recipe != null)
                    return null;
            }
            return error;
        }

        private static string CalculateExact(int[] source, int[] solvent, int selected, int target,
            int capacity, Func<int[], int, int[]> split, out DilutionRecipe recipe)
        {
            recipe = null;
            if (target < 1 || target > 1000)
                return "range";
            if (selected < 0 || selected >= source.Length || source[selected] <= 0)
                return "select";

            long sourceVolume = Sum(source);
            long solventVolume = Sum(solvent);
            long excess = 1000L * source[selected] - target * sourceVolume;
            if (excess < 0)
                return "concentrate";
            if (capacity < MinimumOutputVolumeUl ||
                sourceVolume + (excess == 0 ? 0 : solventVolume) < MinimumOutputVolumeUl)
                return "minimum_volume";
            int step = 1000 / (int)Gcd(target, 1000);
            int firstExactVolume = (MinimumOutputVolumeUl + step - 1) / step * step;
            if (excess == 0)
            {
                if (firstExactVolume > Math.Min(sourceVolume, capacity))
                    return "precision";
                return Create(source, solvent, selected, target, firstExactVolume, 0, split, out recipe)
                    ? null : "precision";
            }
            if (solventVolume == 0)
                return "solvent.empty";
            if (target * solventVolume <= 1000L * solvent[selected])
                return "solvent.concentration";

            long maxVolume = Math.Min(capacity, sourceVolume + solventVolume);
            long selectedVolume = (long)source[selected] + solvent[selected];
            maxVolume = Math.Min(maxVolume, selectedVolume * 1000 / target);
            if (target < 1000)
                maxVolume = Math.Min(maxVolume,
                    (sourceVolume + solventVolume - selectedVolume) * 1000 / (1000 - target));

            decimal denominator = (decimal)source[selected] * solventVolume -
                (decimal)solvent[selected] * sourceVolume;
            decimal volumeProduct = (decimal)sourceVolume * solventVolume;
            // Each input's largest-remainder allocation changes its selected share by less than 1 μL.
            maxVolume = (long)Math.Min(maxVolume, decimal.Floor(
                (denominator + 2 * solventVolume) * 1000 /
                (target * solventVolume - 1000L * solvent[selected])));
            maxVolume = (long)Math.Min(maxVolume, decimal.Floor(
                (denominator + 2 * sourceVolume) * 1000 / excess));
            for (long total = firstExactVolume; total <= maxVolume; total += step)
            {
                long required = total * target / 1000;
                decimal numerator = required * volumeProduct -
                    (decimal)solvent[selected] * total * sourceVolume;
                decimal lower = decimal.Ceiling((numerator - 2 * volumeProduct) / denominator);
                decimal upper = decimal.Floor((numerator + 2 * volumeProduct) / denominator);
                lower = Math.Max(Math.Max(1, total - solventVolume), lower);
                upper = Math.Min(Math.Min(sourceVolume, total), upper);
                if (lower > upper)
                    continue;
                long first = (long)lower;
                long last = (long)upper;
                long middle = (long)Math.Max(first, Math.Min(last, decimal.Round(numerator / denominator)));
                for (long offset = 0; middle - offset >= first || middle + offset <= last; offset++)
                {
                    long left = middle - offset;
                    if (left >= first && Create(source, solvent, selected, target,
                        (int)left, (int)(total - left), split, out recipe))
                        return null;
                    long right = middle + offset;
                    if (offset > 0 && right <= last && Create(source, solvent, selected, target,
                        (int)right, (int)(total - right), split, out recipe))
                        return null;
                }
            }
            return "precision";
        }

        private static bool Create(int[] source, int[] solvent, int selected, int target,
            int sourceVolume, int solventVolume, Func<int[], int, int[]> split, out DilutionRecipe recipe)
        {
            var candidate = Build(source, solvent, selected, target, sourceVolume, solventVolume, split);
            recipe = candidate.IsExact ? candidate : null;
            return recipe != null;
        }

        private static DilutionRecipe Build(int[] source, int[] solvent, int selected, int target,
            int sourceVolume, int solventVolume, Func<int[], int, int[]> split)
        {
            int[] sourceParts = split(source, sourceVolume);
            int[] solventParts = split(solvent, solventVolume);
            return new DilutionRecipe
            {
                SourceVolume = sourceVolume,
                SolventVolume = solventVolume,
                SourceParts = sourceParts,
                SolventParts = solventParts,
                SelectedVolume = sourceParts[selected] + solventParts[selected],
                TargetConcentration = target
            };
        }

        private static DilutionRecipe FindClosest(int[] source, int[] solvent, int selected,
            int target, int capacity, Func<int[], int, int[]> split)
        {
            long sourceVolume = Sum(source);
            long solventVolume = 1000L * source[selected] == target * sourceVolume ? 0 : Sum(solvent);
            long maxVolume = Math.Min(capacity, sourceVolume + solventVolume);
            long minimumDifference = Gcd(target, 1000);
            DilutionRecipe best = null;
            decimal product = (decimal)sourceVolume * solventVolume;
            decimal denominator = (decimal)source[selected] * solventVolume -
                (decimal)solvent[selected] * sourceVolume;
            for (long total = maxVolume; total >= MinimumOutputVolumeUl; total--)
            {
                if (best != null && (decimal)best.Difference * total <
                    (decimal)minimumDifference * best.TotalVolume)
                    break;
                long remainder = total * target % 1000;
                long closestDifference = remainder == 0 ? 1000 : Math.Min(remainder, 1000 - remainder);
                if (best != null && (decimal)closestDifference * best.TotalVolume > (decimal)best.Difference * total)
                    continue;
                long first = Math.Max(1, total - solventVolume);
                long last = Math.Min(sourceVolume, total);
                if (first > last)
                    continue;
                if (solventVolume == 0)
                {
                    Consider(Build(source, solvent, selected, target, (int)total, 0, split), ref best);
                    continue;
                }

                decimal numerator = total * target / 1000m * product -
                    (decimal)solvent[selected] * total * sourceVolume;
                long middle = (long)Math.Max(first, Math.Min(last, decimal.Round(numerator / denominator)));
                var local = Build(source, solvent, selected, target, (int)middle, (int)(total - middle), split);
                Consider(local, ref best);
                if (local.Difference == closestDifference)
                    continue;
                decimal tolerance = local.Difference / 1000m + 2;
                first = (long)Math.Max(first, decimal.Ceiling((numerator - tolerance * product) / denominator));
                last = (long)Math.Min(last, decimal.Floor((numerator + tolerance * product) / denominator));
                for (long offset = 1; middle - offset >= first || middle + offset <= last; offset++)
                {
                    long left = middle - offset;
                    if (left >= first)
                        Consider(Build(source, solvent, selected, target, (int)left, (int)(total - left), split), ref local);
                    long right = middle + offset;
                    if (right <= last)
                        Consider(Build(source, solvent, selected, target, (int)right, (int)(total - right), split), ref local);
                    if (local.Difference == closestDifference)
                        break;
                }
                Consider(local, ref best);
            }
            return best;
        }

        private static void Consider(DilutionRecipe candidate, ref DilutionRecipe best)
        {
            if (best == null || (decimal)candidate.Difference * best.TotalVolume <
                (decimal)best.Difference * candidate.TotalVolume ||
                ((decimal)candidate.Difference * best.TotalVolume ==
                 (decimal)best.Difference * candidate.TotalVolume && candidate.TotalVolume < best.TotalVolume))
                best = candidate;
        }

        private static long Sum(int[] parts)
        {
            long sum = 0;
            foreach (int part in parts)
                sum += part;
            return sum;
        }

        private static long Gcd(long a, long b)
        {
            while (b != 0)
            {
                long remainder = a % b;
                a = b;
                b = remainder;
            }
            return a;
        }
    }
}
