using HarmonyLib;
using Il2CppRUMBLE.Combat.ShiftStones;
using MelonLoader;
using RumbleModUI;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: MelonInfo(typeof(ShiftedStones.Core), "ShiftedStones", "1.0.0", "Orangenal", null)]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]

namespace ShiftedStones
{
    public class Validation : ValidationParameters
    {
        public override bool DoValidation(string Input)
        {
            string rgbPattern = @"^(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\s(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\s(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$";
            string hexPattern = @"^#?[0-9A-Fa-f]{6}$";

            if (Regex.IsMatch(Input, rgbPattern) || Regex.IsMatch(Input, hexPattern))
            {
                return true;
            }
            return false;
        }
    }

    public class Core : MelonMod
    {
        internal static Mod mod = new Mod();
        public static Material[] originalMaterials = new Material[8];
        public static Material[] clonedMaterials = new Material[8];
        internal static string[] shiftstoneOrder = ["Flow Stone", "Vigor Stone", "Guard Stone", "Stubborn Stone", "Charge Stone", "Volatile Stone", "Surge Stone", "Adamant Stone"];
        internal static List<ShiftStone> loadedStones = [];
        internal static List<MeshRenderer> customStones = [];
        internal static GameObject materialStorage;
        internal static MelonLogger.Instance Logger;

        public override void OnInitializeMelon()
        {
            mod.ModName = Info.Name;
            mod.ModVersion = Info.Version;
            mod.SetFolder("ShiftedStones");
            mod.AddDescription("Description", "", "A mod that lets you recolour shiftstones!", new Tags { IsSummary = true });

            mod.AddToList("Flow", "Vanilla", "Colour of the flow stone\n\nAccepts colours formatted as hex codes (e.g. #FF00CC) or 3 RGB values (e.g. 255 165 0)", new Tags());
            mod.AddToList("Vigor", "Vanilla", "Colour of the vigor stone\n\nAccepts colours formatted as hex codes (e.g. #FF00CC) or 3 RGB values (e.g. 255 165 0)", new Tags());
            mod.AddToList("Guard", "Vanilla", "Colour of the guard stone\n\nAccepts colours formatted as hex codes (e.g. #FF00CC) or 3 RGB values (e.g. 255 165 0)", new Tags());
            mod.AddToList("Stubborn", "Vanilla", "Colour of the stubborn stone\n\nAccepts colours formatted as hex codes (e.g. #FF00CC) or 3 RGB values (e.g. 255 165 0)", new Tags());
            mod.AddToList("Charge", "Vanilla", "Colour of the charge stone\n\nAccepts colours formatted as hex codes (e.g. #FF00CC) or 3 RGB values (e.g. 255 165 0)", new Tags());
            mod.AddToList("Volatile", "Vanilla", "Colour of the volatile stone\n\nAccepts colours formatted as hex codes (e.g. #FF00CC) or 3 RGB values (e.g. 255 165 0)", new Tags());
            mod.AddToList("Surge", "Vanilla", "Colour of the surge stone\n\nAccepts colours formatted as hex codes (e.g. #FF00CC) or 3 RGB values (e.g. 255 165 0)", new Tags());
            mod.AddToList("Adamant", "Vanilla", "Colour of the adamant stone\n\nAccepts colours formatted as hex codes (e.g. #FF00CC) or 3 RGB values (e.g. 255 165 0)", new Tags());

            foreach (ModSetting setting in mod.Settings)
            {
                if (setting.Tags.IsSummary) continue;

                setting.SavedValueChanged += OnSave;
                mod.AddValidation(setting.Name, new Validation());
            }

            mod.GetFromFile();

            UI.instance.UI_Initialized += OnUIInit;

            materialStorage = new GameObject();
            materialStorage.name = "Shiftstone material storage";
            GameObject.DontDestroyOnLoad(materialStorage);

            Logger = LoggerInstance;

            LoggerInstance.Msg("Initialised.");
        }

        private void OnUIInit()
        {
            UI.instance.AddMod(mod);
        }

        private string getCustomShiftstoneObjectParentStone(Renderer renderer)
        {
            string val = null;
            Transform current = renderer.transform;
            while (val == null)
            {
                if (current == null)
                {
                    Logger.Error("Could not find shiftstone");
                    throw new Exception("Cannot find shiftstone name from the given renderer");
                }
                if (shiftstoneOrder.Contains(current.name))
                {
                    val = current.name;
                }
                else
                {
                    current = current.parent;
                }
            }
            return val;
        }

        private void OnSave(object sender = null, EventArgs e = null)
        {
            if (loadedStones.Count == 0 || !mod.GetUIStatus()) return;
            string name = ((ModSetting)sender).Name;
            var arr = loadedStones.Where(s => s.StoneName == name + " Stone").ToArray();
            string colour = ((ValueChange<string>)e).Value;

            //if (colour.ToLower() == "vanilla" && customStones.Count() > 0)
            //{
            //    foreach (MeshRenderer renderer in customStones.Where(r => r.transform.parent.parent.name == name + "Stone"))
            //    {
            //        MelonLogger.Msg(renderer.gameObject.name);
            //        renderer.material = originalMaterials[shiftstoneOrder.IndexOf(name + " Stone")];
            //    }
            //}
            foreach (MeshRenderer renderer in customStones.Where(r => getCustomShiftstoneObjectParentStone(r) == name + "Stone"))
            {
                if (colour.ToLower() == "vanilla" && customStones.Count > 0)
                {
                    renderer.material = originalMaterials[shiftstoneOrder.IndexOf(name + " Stone")];
                }
                else // There was a check here for if the material was the vanilla but it didn't work for some reason and it doesn't really matter
                {
                    renderer.material = clonedMaterials[shiftstoneOrder.IndexOf(name + " Stone")];
                    Material material = renderer.material;
                    ShiftstonePatch.setColours(colour, ref material);
                }
            }
            for (int i = 0; i < arr.Count(); i++)
            {
                if (arr[i] == null) continue;
                if (arr.Contains(arr[i]))
                {
                    MeshRenderer renderer = arr[i].transform.GetChild(0).GetComponent<MeshRenderer>();
                    Material material = renderer.material;
                    if (colour.ToLower() == "vanilla")
                    {
                        //if (customStones.Count > 0)
                        //{
                        //    foreach (MeshRenderer rend in customStones.Where(r => r.transform.parent == renderer.transform))
                        //    {
                        //        MelonLogger.Msg("FOUND HIM! SICK 'EM!");
                        //        rend.material = originalMaterials[shiftstoneOrder.IndexOf(name + " Stone")];
                        //    }
                        //}
                        //else
                        //{
                            renderer.material = originalMaterials[shiftstoneOrder.IndexOf(name + " Stone")];
                        //}
                        continue;
                    }
                    ShiftstonePatch.setColours(colour, ref material);
                }
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            loadedStones = [];
            customStones = [];
        }
    }

    [HarmonyPatch(typeof(ShiftStone), "OnFetchFromPool")]
    internal static class ShiftstonePatch {
        internal static void setColours(string colourStr, ref Material material)
        {
            Color colour;
            colourStr = colourStr.Replace("#","");
            colourStr = colourStr.Replace("0x", "");
            if (colourStr.Contains(" "))
            {
                string[] parts = colourStr.Split(' ');
                if (parts.Length == 3 &&
                    byte.TryParse(parts[0], out byte rByte) &&
                    byte.TryParse(parts[1], out byte gByte) &&
                    byte.TryParse(parts[2], out byte bByte))
                {
                    colour = new Color(rByte / 255f, gByte / 255f, bByte / 255f);
                }
                else
                {
                    Core.Logger.Error("Somehow not a valid colour");
                    throw new Exception("Provided string is not in a valid format.");
                }
            }
            else if (colourStr.Length == 6 &&
                int.TryParse(colourStr, System.Globalization.NumberStyles.HexNumber, null, out int hex))
            {
                float r = ((hex >> 16) & 0xFF) / 255f;
                float g = ((hex >> 8) & 0xFF) / 255f;
                float b = (hex & 0xFF) / 255f;
                colour = new Color(r, g, b);
            }
            else
            {
                Core.Logger.Error("Somehow not a valid colour");
                throw new Exception("Provided string is not in a valid format.");
            }

            material.SetColor("_Core_Color", colour);
            material.SetColor("_Edge_Color", colour);

            Color.RGBToHSV(colour, out float hue, out float sat, out float val);

            material.SetColor("_Base_Color", Color.HSVToRGB(hue, sat*3/4, val*3/4));
            material.SetColor("_Shadow_Color", Color.HSVToRGB(hue, sat * 1 / 2, val * 1 / 2));
        }

        [HarmonyBefore(["ShifstoneModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null:Custom Shiftstone Models"])]
        private static void Postfix(ref ShiftStone __instance)
        {
            int index = Core.shiftstoneOrder.IndexOf(__instance.StoneName);
            MeshRenderer renderer = __instance.transform.GetChild(0).GetComponent<MeshRenderer>();

            if (Core.originalMaterials[index] == null)
            {
                Core.originalMaterials[index] = new Material(renderer.material);
                GameObject materialCopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                materialCopy.GetComponent<MeshRenderer>().material = Core.originalMaterials[index];
                materialCopy.name = $"Material Copy";
                materialCopy.transform.parent = Core.materialStorage.transform;
                materialCopy.transform.position = new(10000f, 10000f, 10000f);
            }

            if (Core.clonedMaterials[index] == null)
            {
                Core.clonedMaterials[index] = new Material(renderer.material);
                GameObject materialCopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                materialCopy.GetComponent<MeshRenderer>().material = Core.clonedMaterials[index];
                materialCopy.name = $"Material Copy";
                materialCopy.transform.parent = Core.materialStorage.transform;
                materialCopy.transform.position = new(10000f, 10000f, 10000f);
                if (((string)Core.mod.Settings[index + 1].SavedValue).ToLower() != "vanilla")
                    setColours((string)Core.mod.Settings[index + 1].SavedValue, ref Core.clonedMaterials[index]);
            }

            renderer.material = (((string)Core.mod.Settings[index + 1].SavedValue).ToLower() != "vanilla" ? Core.clonedMaterials : Core.originalMaterials)[index];

            if (!Core.loadedStones.Contains(__instance))
                Core.loadedStones.Add(__instance);
        }

        [HarmonyAfter(["ShifstoneModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null:Custom Shiftstone Models"])]
        [HarmonyPostfix]
        private static void AddCustomModelRenderers(ref ShiftStone __instance)
        {
            if (__instance.transform.childCount > 2) {
                var renderers = __instance.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    if (renderer.gameObject.name.Contains("%"))
                    {
                        Core.customStones.Add(renderer);
                    }
                }
            }
        }
    }
}