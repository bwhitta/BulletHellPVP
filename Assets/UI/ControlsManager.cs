using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsManager
{
    /// <summary>
    /// Contains a static GameControls acessible from GameControlsMaster.GameControls
    /// </summary>
    public class GameControlsMaster
    {
        private static GameControls _gameControls;
        public static GameControls GameControls
        {
            get
            {
                _gameControls ??= new GameControls();
                return _gameControls;
            }
        }
    }
    public class RebindsMaster
    {
        public static bool currentlyRebinding;
    }
    
    public static InputActionMap GetActionMap(string mapName)
    {
        GameControls controls = GameControlsMaster.GameControls;

        //Get the index of the map
        int mapIndex = -1;
        for (int i = 0; i < controls.asset.actionMaps.ToArray().Length; i++)
        {
            if (controls.asset.actionMaps.ToArray()[i].name == mapName)
            {
                mapIndex = i;
                break;
            }
        }

        // Check if an index was found
        if (mapIndex < 0)
        {
            Debug.LogWarning($"Invalid mapName {mapName}");
            return null;
        }

        return controls.asset.actionMaps.ToArray()[mapIndex];
    }
}
