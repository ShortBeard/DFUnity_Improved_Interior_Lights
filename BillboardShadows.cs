///////////////////////////////////////////////////////////
/// Mod: Improved Interior Lighting
/// Author: ShortBeard
/// Version: 1.0.0
/// Description: Creates warmer interior & dungeon lights.
///////////////////////////////////////////////////////////
///
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System.Collections;
using UnityEngine;

namespace ImprovedInteriorLighting {

    /// <summary>
    /// This class just contains some static methods to put two-sided shadows on all NPC's or enemies
    /// </summary>
    public class BillboardShadows : MonoBehaviour {

        /// <summary>
        /// Toggles shadow casting on all static NPC's in any given interior
        /// </summary>
        /// <param name="args"></param>
        public static void ToggleIndoorNPCBillboardShadows(bool shadowsEnabled) {
            StaticNPC[] npcBillboards = (StaticNPC[])FindObjectsOfType(typeof(StaticNPC)); //Get all static NPC's in the scene
            foreach (StaticNPC npc in npcBillboards) {
                if (shadowsEnabled == true) {
                    npc.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided; //Give their mesh renderer a shadow
                }
                else {
                    npc.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        /// <summary>
        /// Toggles shadow casting on all enemies in any given dungeon. We have to pass in a MonoBehaviour as an argument
        /// so that we have access to StartCoroutine()
        /// </summary>
        /// <param name="args"></param>
        public static void ToggleDungeonEnemyBillboardShadows(bool shadowsEnabled, MonoBehaviour monoBehaviour) {
            monoBehaviour.StartCoroutine(ApplyShadows(shadowsEnabled));
        }


        /// <summary>
        /// We have to use a coroutine and wait 1 second after loading a saved game (from in-game, not from the menu) because for some reason
        /// Unity won't find DaggerfallEnemy objects immediately after loading, despite them being in the gameobject heirarchy.
        /// Is there a bool somewhere to check when enemies have done loading?
        /// </summary>
        /// <param name="shadowsEnabled"></param>
        /// <returns></returns>
        private static IEnumerator ApplyShadows(bool shadowsEnabled) {
            yield return new WaitForSeconds(1);
            DaggerfallEnemy[] enemyBillboards = (DaggerfallEnemy[])FindObjectsOfType(typeof(DaggerfallEnemy));
            foreach (DaggerfallEnemy enemy in enemyBillboards) {
                if (shadowsEnabled == true) {
                    enemy.transform.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided; //Give their mesh renderer a shadow
                }
                else {
                    enemy.transform.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

        }

    }
}