///////////////////////////////////////////////////////////
/// Mod: Improved Interior Lighting
/// Author: ShortBeard
/// Version: 1.0.2
/// Description: Creates warmer interior & dungeon lights.
///////////////////////////////////////////////////////////

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;

public class ImprovePlayerTorch : MonoBehaviour {

    private class TorchSettings {
        public bool PlayerTorchChanged;
        public Color32 PlayerTorchColor;
        //public bool PlayerTorchFlicker;
        //public float PlayerTorchFlickerStrength;
    }

    private static TorchSettings torchModSettings;
    private static Mod improvedPlayerTorchMod;


    [Invoke(StateManager.StateTypes.Game, 0)]
    public static void Init(InitParams initParams) {
        GameObject improvedPlayerTorchObj = new GameObject("improvedPlayerTorch");
        improvedPlayerTorchObj.AddComponent<ImprovePlayerTorch>();
        improvedPlayerTorchMod = initParams.Mod;
        improvedPlayerTorchMod.IsReady = true;
    }



    // Use this for initialization
    void Start() {
        torchModSettings = new TorchSettings();
        improvedPlayerTorchMod.GetSettings().Deserialize("Torch", ref torchModSettings);
        //Apply any torch settings changes if we have to
        if (torchModSettings.PlayerTorchChanged) {
            AdjustPlayerTorch();
        }
    }

    /// <summary>
    /// This just calls the coroutine to look for the torch after entering a dungeon
    /// </summary>
    /// <param name="args"></param>
    private void AdjustPlayerTorch() {
        //Change the lighting on the player torch if the settings are set to do so
        //We have to get the inactive gameobject using PlayerAdvanced as a reference point since the Torch object can start in an inactive state.
        GameObject torchObject = GameObject.Find("PlayerAdvanced").transform.GetChild(0).GetChild(1).gameObject;
        if (torchObject != null) {
            Light torchLight = torchObject.GetComponent<Light>();
            torchLight.color = torchModSettings.PlayerTorchColor;

            //Might later on add torch flickering, but would first have to do more testing on how it interacts with the existing in-game torch.
            /*
            if (torchModSettings.PlayerTorchFlicker == true && torchObject.GetComponent<LightFlicker>() == null) {
                AddLightFlicker(torchObject, 0.5f, 1.5f, 0, torchModSettings.PlayerTorchFlickerStrength);
            }
            */
        }
    }
}
