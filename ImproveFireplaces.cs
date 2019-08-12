///////////////////////////////////////////////////////////
/// Mod: Improved Interior Lighting
/// Author: ShortBeard
/// Version: 1.0.2
/// Description: Creates warmer interior & dungeon lights.
///////////////////////////////////////////////////////////

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ImprovedInteriorLighting {

    public class ImproveFireplaces : MonoBehaviour {

        class FireplaceSettings {
            public bool EnabledInInteriors;
            //public bool EnabledInDungeons;
            public Color32 FireplaceLightsColor;
            public float FireplaceIntensity;
            public bool FireplaceFlickeringLights;
            public float FireplaceFlickerStrength;
        }

        private string[] fireplaceNames = new string[] { "41117", "41116" }; //Get fireplace in heirarchy by mesh ID. Is there a better way to do this?
        private static Mod improvedFireplaceMod;
        private static FireplaceSettings fireplaceModSettings;



        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams) {
            GameObject improvedFireplaceObj = new GameObject("improvedFireplaceLighting");
            improvedFireplaceObj.AddComponent<ImproveFireplaces>();
            improvedFireplaceMod = initParams.Mod;
            improvedFireplaceMod.IsReady = true;
        }


        private void Start() {
            fireplaceModSettings = new FireplaceSettings();
            improvedFireplaceMod.GetSettings().Deserialize("FirePlaces", ref fireplaceModSettings);

            //Immediately apply any fireplace lights if we load into an interior or ungeon
            if (GameManager.Instance.IsPlayerInsideBuilding && fireplaceModSettings.EnabledInInteriors /*|| (GameManager.Instance.IsPlayerInsideDungeon && fireplaceModSettings.EnabledInDungeons)*/) {
                AddLightsToFireplaces(null);
            }

            //Set up our events for when we transition into a dungeon or interior
            if (fireplaceModSettings.EnabledInInteriors) {
                PlayerEnterExit.OnTransitionInterior += AddLightsToFireplaces;
            }
            /*
            if (fireplaceModSettings.EnabledInDungeons) {
                PlayerEnterExit.OnTransitionDungeonInterior += AddLightsToFireplaces;
            }
            */

        }

        /// <summary>
        /// Get all the objects in the interior that should emit light like fireplaces
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
                        light.color = fireplaceModSettings.FireplaceLightsColor;
                        light.intensity = fireplaceModSettings.FireplaceIntensity;
                        light.range = 15;
                        light.type = LightType.Spot;
                        light.shadows = LightShadows.Hard;
                        light.shadowStrength = 1f;
                        light.spotAngle = 140;

                        //Add flickering to fireplace is the setting is set to do so
                        if (fireplaceModSettings.FireplaceFlickeringLights == true) {
                            AddLightFlicker(fireplaceLight, 3, 4.5f, 0.02f, fireplaceModSettings.FireplaceFlickerStrength);
                        }
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
