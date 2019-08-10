///////////////////////////////////////////////////////////
/// Mod: Improved Interior Lighting
/// Author: ShortBeard
/// Version: 1.0.0
/// Description: Creates warmer interior & dungeon lights.
///////////////////////////////////////////////////////////

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Text.RegularExpressions;
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
            public bool FireplacesProduceLight;
            public Color32 FireplaceLightsColor;
            public float FireplaceIntensity;
            public bool FireplaceFlickeringLights;
            public float FireplaceFlickerStrength;
        }


        private string[] fireplaceNames = new string[] { "41117", "41116" }; //Get fireplace in heirarchy by mesh ID. Is there a better way to do this?
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

                    if (interiorModSettings.FireplacesProduceLight == true) {
                        AddLightsToFireplaces(null); //If the game starts or is loaded indoors, add lighting to fireplaces right away
                    }
         
                }
                if (interiorModSettings.FireplacesProduceLight == true) {
                    PlayerEnterExit.OnTransitionInterior += AddLightsToFireplaces; //When player enters an interior, add lights to the fire places
                }           
            }
        }

        private void ApplyShadowSettings(PlayerEnterExit.TransitionEventArgs args) {
            BillboardShadows.ToggleIndoorNPCBillboardShadows(interiorModSettings.NpcShadows); //Technically there are usually no NPC's in dungeon except rescue quests so we toggle this anyway
        }

        /// <summary>
        /// Get all the objects in the interior that should emit light like fireplaces and give them flickering lights
        /// </summary>
        private void AddLightsToFireplaces(PlayerEnterExit.TransitionEventArgs args) {
            GameObject modelsParent = GameManager.Instance.InteriorParent.transform.GetChild(1).GetChild(0).gameObject;
            foreach (Transform child in modelsParent.transform) {
                string objectID = Regex.Match(child.name, @"\d+").Value; //Extract just the ID from the mesh gameobject name
                //If a fireplace is found in the scene, add a light to it
                if (Array.Exists(fireplaceNames, ID => ID == objectID)) {
                    GameObject firePlace = child.gameObject;
                    if (firePlace != null) {
                        GameObject fireplaceLight = new GameObject("FireplaceLight");
                        fireplaceLight.transform.SetParent(firePlace.transform);
                        fireplaceLight.transform.localEulerAngles = new Vector3(0, 0, 0);
                        Light light = fireplaceLight.AddComponent<Light>();
                        SetFireplaceLightPosition(fireplaceLight);
                        //light.color = new Color32(255, 147, 41, 1);
                        light.color = interiorModSettings.FireplaceLightsColor;
                        light.intensity = interiorModSettings.FireplaceIntensity;
                        light.range = 15;
                        light.type = LightType.Spot;
                        light.shadows = LightShadows.Hard;
                        light.shadowStrength = 1f;
                        light.spotAngle = 140;

                        if (interiorModSettings.FireplaceFlickeringLights == true) {
                            AddLightFlicker(fireplaceLight, 3, 4.5f, 0.02f, interiorModSettings.FireplaceFlickerStrength);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adjust the existing lights in the interior, and add a light flicker if we have it enabled
        /// </summary>
        /// <param name="args"></param>
        private void AdjustExistingLights(PlayerEnterExit.TransitionEventArgs args) {
            Debug.Log("Adjust existing lights");
            Light[] dfLights = (Light[])FindObjectsOfType(typeof(Light)); //Get all static NPC's in the scene
            foreach (Light dfLight in dfLights) {
                dfLight.intensity = interiorModSettings.InteriorLightsIntensity;
                dfLight.shadows = LightShadows.Soft;
                dfLight.range = 10;
                dfLight.GetComponent<Light>().color = interiorModSettings.InteriorLightsColor;

                //Add light flickering to the existing light in the interior
                if (interiorModSettings.InteriorFlickeringLights == true) {
                    AddLightFlicker(dfLight.gameObject, 1.5f, 2.5f, 0, interiorModSettings.LightFlickerStrength);
                }

                BillboardShadows.ToggleIndoorNPCBillboardShadows(interiorModSettings.NpcShadows); //Toggle NPC shadows based on settings
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

        /// <summary>
        /// Set the spot light in the fireplace. If the handpainted models mod was found, move the light to a slightly different position so
        /// that it doesn't get caught up in the fireplace mesh geometry.
        /// </summary>
        /// <param name="fireplaceLightObj"></param>
        private void SetFireplaceLightPosition(GameObject fireplaceLightObj) {
            Mod mod = ModManager.Instance.GetMod("Handpainted Models - Main");
            bool modFound = mod != null;
            if (modFound == true) {
                Debug.Log("Handpainted model mod was found");
                fireplaceLightObj.transform.localPosition = new Vector3(0, 0, -0.47f);
            }
            else {
                Debug.Log("Handpainted model mod was not found, putting at regular position.");
                fireplaceLightObj.transform.localPosition = new Vector3(0, 0.08f, -0.15f);
            }
        }
    }
}