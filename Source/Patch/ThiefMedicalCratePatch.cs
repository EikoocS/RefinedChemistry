using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch]
    internal static class ThiefMedicalCratePatch
    {
        private static readonly HashSet<MethodInfo> CrateFactories = new()
        {
            AccessTools.Method(typeof(PreBuiltItemHelper), nameof(PreBuiltItemHelper.LootCrateEvidence)),
            AccessTools.Method(typeof(PreBuiltItemHelper), nameof(PreBuiltItemHelper.LootCrateMedical)),
            AccessTools.Method(typeof(PreBuiltItemHelper), nameof(PreBuiltItemHelper.LootCrateEngineering)),
            AccessTools.Method(typeof(PreBuiltItemHelper), nameof(PreBuiltItemHelper.LootCrateSecurity)),
            AccessTools.Method(typeof(PreBuiltItemHelper), nameof(PreBuiltItemHelper.LootCrateService))
        };

        private static MethodBase TargetMethod() =>
            ModReflect.Target("StoreClientList+<>c", "<CreateThief>b__56_0");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var constructor = AccessTools.Constructor(typeof(Func<GameItem>), [typeof(object), typeof(IntPtr)]);
            var chooseCrate = AccessTools.Method(typeof(ThiefMedicalCratePatch), nameof(ChooseCrate));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo factory &&
                    CrateFactories.Contains(factory))
                {
                    instruction.opcode = OpCodes.Ldnull;
                    instruction.operand = null;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldftn, factory);
                    yield return new CodeInstruction(OpCodes.Newobj, constructor);
                    yield return new CodeInstruction(OpCodes.Call, chooseCrate);
                }
                else
                    yield return instruction;
            }
        }

        private static GameItem ChooseCrate(Func<GameItem> original) =>
            RNG.Roll(1, 6) ? UtoMedicalBoxLoot.Create() : original();
    }
}
