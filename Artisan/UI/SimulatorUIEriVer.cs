
using Artisan.CraftingLogic.Solvers;
using ECommons.Reflection;
using ImGuiNET;

namespace Artisan.UI;

public static partial class SimulatorUI
{
    internal static class EriVer
    {

        public static void DrawCraftInfo()
        {
            if (SimGS is null && !CustomStatMode)
            {
                ImGui.Text($"Please have a gearset selected from above to use this feature.");
                return;
            }
            DrawSolverCombo();

            if(_selectedSolver?.Clone() is not MacroSolver macro) {
                ImGui.Text($"This feature only works on macro solver.");
                return;
            }
            if(macro.GetFoP("_macro") is not MacroSolverSettings.Macro _macro){
                ImGui.Text($"Unable to read macro.");
                return;
            }
            ImGui.Text($"Ready!");
            int naive = 1;
            int[] counts = {1,1,1};
            int[] complete_counts = {1,1,1};
            bool last_was_poor = false;
            for (int i = _macro.Steps.Count-1; i > 0; --i) {
                if(is_unsupported(_macro, i)){
                    ImGui.Text($"Step {i+1} depends on unsupported condition!");
                    return;
                }
                int states = 1;
                int total = counts[0]; // normal can always hapen
                int complete_total = complete_counts[0]; // normal can always hapen
                if(is_good(_macro, i)){
                    states += 1;
                    total+=counts[1]; // good locks out 1 step
                }
                complete_total+=complete_counts[1];
                if(last_was_poor || is_exc(_macro, i)){
                    states += 1;
                    total+=counts[2]; // exc locks out 2 steps
                }
                complete_total+=complete_counts[2];
                naive *= states;
                last_was_poor = is_poor(_macro, i);
                counts[2] = counts[1];
                counts[1] = counts[0];
                counts[0] = total;
                complete_counts[2] = complete_counts[1];
                complete_counts[1] = complete_counts[0];
                complete_counts[0] = complete_total;
            }
            ImGui.Text($"Total permutations real: {counts[0]}, naive filtered: {naive}, complete: {complete_counts[0]}");
        }

        private static bool is_exc(MacroSolverSettings.Macro macro, int step) => 
                                            MacroSolver.ActionIsQuality(macro.Steps[step].Action)
                                            || macro.Steps[step].ExcludeExcellent
                                            || macro.Options.UpgradeProgressActions&&MacroSolver.ActionIsUpgradeableProgress(macro.Steps[step].Action)
                                            || macro.Options.UpgradeQualityActions&&MacroSolver.ActionIsUpgradeableQuality(macro.Steps[step].Action);
        private static bool is_good(MacroSolverSettings.Macro macro, int step) => 
                                            MacroSolver.ActionIsQuality(macro.Steps[step].Action)
                                            || macro.Steps[step].ExcludeGood
                                            || macro.Options.UpgradeProgressActions&&MacroSolver.ActionIsUpgradeableProgress(macro.Steps[step].Action)
                                            || macro.Options.UpgradeQualityActions&&MacroSolver.ActionIsUpgradeableQuality(macro.Steps[step].Action);
        private static bool is_poor(MacroSolverSettings.Macro macro, int step) => 
                                            MacroSolver.ActionIsQuality(macro.Steps[step].Action)
                                            || macro.Steps[step].ExcludePoor;
        private static bool is_unsupported(MacroSolverSettings.Macro macro, int step) =>
                                            macro.Steps[step].ExcludeCentered
                                            || macro.Steps[step].ExcludeSturdy
                                            || macro.Steps[step].ExcludePliant
                                            || macro.Steps[step].ExcludeMalleable
                                            || macro.Steps[step].ExcludePrimed
                                            || macro.Steps[step].ExcludeGoodOmen;
    }
}