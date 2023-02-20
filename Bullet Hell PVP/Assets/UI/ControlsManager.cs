using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

public class ControlsManager : MonoBehaviour
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
                if (_gameControls == null)
                {
                    _gameControls = new GameControls();
                }
                return _gameControls;
            }
        }
    }
    public class RebindsMaster
    {
        public static bool currentlyRebinding;
    }
}
