///////////////////////////////////////////////////////////
/// Mod: Improved Interior Lighting
/// Author: ShortBeard
/// Version: 1.0.1
/// Description: Creates warmer interior & dungeon lights.
///////////////////////////////////////////////////////////

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;

namespace ImprovedInteriorLighting {

    public class ImproveDungeonLighting : MonoBehaviour {

        private class DungeonSettings {
            public bool Enabled;
            public bool EnemyShadows;
            public Color32 DungeonLightsColor;
            public float DungeonLightsIntensity;
            public bool FlickeringLights;
            public bool NpcShadows;
            public float LightFlickerStrength;
        }



        // Use this for initialization
        private const int LIGHT_OBJECT_ARCHIVE = 210; //All light objects seem to exist within this archive
        private static Mod improvedDungeonLightMod;
        private static DungeonSettings dungeonModSettings;



        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams) {
            GameObject improvedDungeonLightingObj = new GameObject("improvedDungeonLighting");
            improvedDungeonLightingObj.AddComponent<ImproveDungeonLighting>();
            improvedDungeonLightMod = initParams.Mod;
            improvedDungeonLightMod.IsReady = true;
        }



        private void Start() {
            dungeonModSettings = new DungeonSettings();

            Mod mod = ModManager.Instance.GetMod("Handpainted Models - Main");
            bool handPaintedModFound = mod != null;
            improvedDungeonLightMod.GetSettings().Deserialize("Dungeons", ref dungeonModSettings);
            if (dungeonModSettings.Enabled == true) {
                if (GameManager.Instance.IsPlayerInsideDungeon) {
                    ApplyShadowSettings(null); //Apply our shadow settings
                    RemoveVanillaLightSources(null); //If the game starts or is loaded indoors, apply the shadows right away

                    //Check to see if the hand painted mod exists - if it does we don't have to add new lights, only adjust exisitng ones.
                    if (handPaintedModFound == true) {
                        AdjustExistingLightSources(null);
                    }
                    else {
                        AddImprovedLighting(null);
                    }
                }

                PlayerEnterExit.OnTransitionDungeonInterior += RemoveVanillaLightSources; //Remove all the daggerfall vanilla light sources in dungeons.
                PlayerEnterExit.OnTransitionDungeonInterior += ApplyShadowSettings;

                //If the hainted painted mod is found, adjust that mods existing lights instead of creating new ones
                if (handPaintedModFound == true) {
                    PlayerEnterExit.OnTransitionDungeonInterior += AdjustExistingLightSources;
                }
                else {
                    PlayerEnterExit.OnTransitionDungeonInterior += AddImprovedLighting;
                }
            }
        }

        private void ApplyShadowSettings(PlayerEnterExit.TransitionEventArgs args) {
            BillboardShadows.ToggleIndoorNPCBillboardShadows(dungeonModSettings.EnemyShadows); //Technically there are usually no NPC's in dungeon except rescue quests so we toggle this anyway
            BillboardShadows.ToggleDungeonEnemyBillboardShadows(dungeonModSettings.EnemyShadows, this); //Toggle enemy billboard shadows
        }


        /// <summary>
        /// DFUnity in an attempt to stay true to the original does light optimization, so multiple "light objects" may be represented by only one light source in the same proximity.
        /// Before we add new lights to everything, we need to removed the optimized ones first.
        /// </summary>
        private void RemoveVanillaLightSources(PlayerEnterExit.TransitionEventArgs args) {
            DungeonLightHandler[] dfLights = (DungeonLightHandler[])FindObjectsOfType(typeof(DungeonLightHandler)); //Get all dungeon lights in the scene
            for (int i = 0; i < dfLights.Length; i++) {
                Destroy(dfLights[i].gameObject);
            }
        }


        /// <summary>
        /// If the user is running the Handpainted Models mod - adjust the lights instead of completely replacing them.
        /// </summary>
        private void AdjustExistingLightSources(PlayerEnterExit.TransitionEventArgs args) {
            Light[] dfLights = (Light[])FindObjectsOfType(typeof(Light)); //Get all dungeon lights in the scene
            foreach (Light dfLight in dfLights) {
                //We don't want the torch to be changed here
                if (dfLight.gameObject.name != "Torch") {

                    dfLight.color = dungeonModSettings.DungeonLightsColor;
                    dfLight.intensity = dungeonModSettings.DungeonLightsIntensity;
                    dfLight.range = 10;
                    dfLight.type = LightType.Point;
                    dfLight.shadows = LightShadows.Soft;
                    dfLight.shadowStrength = 1f;

                    //Add flickering to regular light source
                    if (dungeonModSettings.FlickeringLights == true) {
                        AddLightFlicker(dfLight.gameObject, 1.5f, 2.5f, 0, dungeonModSettings.LightFlickerStrength);
                    }
                }
            }
        }





        /// <summary>
        /// Find all bill boards that match the archive that stores all light emitting object billboard. (Is there a better way to find these?)
        /// Then after finding them, spawn a new light source on them.
        /// This runs when hand painted models mod isn't being used.
        /// </summary>
        /// <param name="args"></param>
        private void AddImprovedLighting(PlayerEnterExit.TransitionEventArgs args) {
            DaggerfallBillboard[] lightBillboards = (DaggerfallBillboard[])FindObjectsOfType(typeof(DaggerfallBillboard)); //Get all "light emitting objects" in the dungeon
            foreach (DaggerfallBillboard billBoard in lightBillboards) {
                if (billBoard.Summary.Archive == LIGHT_OBJECT_ARCHIVE) {
                    GameObject newLightObject = new GameObject("ImprovedDungeonLight");
                    newLightObject.transform.SetParent(billBoard.transform);
                    newLightObject.transform.localPosition = new Vector3(0, 0, 0); //Put the new improved light child object at the parents root position

                    //Add a new light onto the dungeon light object
                    Light improvedLight = newLightObject.AddComponent<Light>();
                    improvedLight.intensity = dungeonModSettings.DungeonLightsIntensity;
                    improvedLight.color = dungeonModSettings.DungeonLightsColor;
                    improvedLight.range = 10;
                    improvedLight.type = LightType.Point;
                    improvedLight.shadows = LightShadows.Soft;
                    improvedLight.shadowStrength = 1f;

                    //Add flickering light if the mod settings allow for it
                    if (dungeonModSettings.FlickeringLights == true) {
                        AddLightFlicker(newLightObject, 1.5f, 2.5f, 0, dungeonModSettings.LightFlickerStrength);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a light flickering script to object with the new light source on it
        /// </summary>
        /// <param name="lightObject"></param>
        private void AddLightFlicker(GameObject lightObject, float maxReduction, float maxIncrease, float rateDamping, float strength) {
            LightFlicker lightFlicker = lightObject.AddComponent<LightFlicker>();
            lightFlicker.MaxReduction = maxReduction;
            lightFlicker.MaxIncrease = maxIncrease;
            lightFlicker.RateDamping = rateDamping;
            lightFlicker.Strength = strength;
        }
    }
}