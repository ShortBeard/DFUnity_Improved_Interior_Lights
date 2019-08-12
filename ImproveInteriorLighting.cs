///////////////////////////////////////////////////////////
/// Mod: Improved Interior Lighting
/// Author: ShortBeard
/// Version: 1.0.2
/// Description: Creates warmer interior & dungeon lights.
///////////////////////////////////////////////////////////

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;

namespace ImprovedInteriorLighting {

    public class ImproveInteriorLighting : MonoBehaviour {

        class Settings {
            public bool Enabled;
            public bool NpcShadows;
            public Color32 InteriorLightsColor;
            public float InteriorLightsIntensity;
            public bool InteriorFlickeringLights;
            public float LightFlickerStrength;
        }


        private static Mod improvedInteriorLightMod;
        private static Settings interiorModSettings;

        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams) {
            GameObject improvedInteriorLightingObj = new GameObject("improvedInteriorLightingObj");
            improvedInteriorLightingObj.AddComponent<ImproveInteriorLighting>();
            improvedInteriorLightMod = initParams.Mod;
            improvedInteriorLightMod.IsReady = true;
        }


        private void Start() {
            interiorModSettings = new Settings();
            improvedInteriorLightMod.GetSettings().Deserialize("Interiors", ref interiorModSettings);
            if (interiorModSettings.Enabled == true) {
                PlayerEnterExit.OnTransitionInterior += AdjustExistingLights; //When player enters an interior, add a small amount of flickering to existing lights
                PlayerEnterExit.OnTransitionInterior += ApplyShadowSettings; //When player enters an interior, add a small amount of flickering to existing lights
                if (GameManager.Instance.IsPlayerInsideBuilding) {
                    ApplyShadowSettings(null);
                    AdjustExistingLights(null); //Adjust existing lights. We should do this before adding lights to fireplaces.         
                }
            }
        }

        private void ApplyShadowSettings(PlayerEnterExit.TransitionEventArgs args) {
            BillboardShadows.ToggleIndoorNPCBillboardShadows(interiorModSettings.NpcShadows); //Technically there are usually no NPC's in dungeon except rescue quests so we toggle this anyway
        }


        /// <summary>
        /// Adjust the existing lights in the interior, and add a light flicker if we have it enabled
        /// </summary>
        /// <param name="args"></param>
        private void AdjustExistingLights(PlayerEnterExit.TransitionEventArgs args) {
            Debug.Log("Adjust existing lights");
            Light[] dfLights = (Light[])FindObjectsOfType(typeof(Light)); //Get all static NPC's in the scene
            foreach (Light dfLight in dfLights) {
                //We don't want to adjust the torch when we enter the interior
                if (dfLight.gameObject.name != "Torch") {
                    dfLight.intensity = interiorModSettings.InteriorLightsIntensity;
                    dfLight.shadows = LightShadows.Soft;
                    dfLight.range = 10;
                    dfLight.GetComponent<Light>().color = interiorModSettings.InteriorLightsColor;

                    //Add light flickering to the existing light in the interior, but not to the player torch
                    if (interiorModSettings.InteriorFlickeringLights) {
                        AddLightFlicker(dfLight.gameObject, 1.5f, 2.5f, 0, interiorModSettings.LightFlickerStrength);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a light flickering script to object with the new light source on it
        /// If strength is not passed in as a paramter, game will read mod settings for the strength value.
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