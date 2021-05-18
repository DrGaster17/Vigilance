using System;

using Vigilance.API;

using UnityEngine;
using MapGeneration;
using Grenades;

namespace Vigilance.Utilities
{
    public static class CompCache
    {
        private static string _gameManagerName = "GameManager";

        private static GameObject _gameManager;

        private static Player _ply;
        private static PlayerStats _stats;
        private static CharacterClassManager _ccm;
        private static BanPlayer _ban;
        private static Broadcast _broadcast;
        private static GrenadeManager _gMan;
        private static Inventory _inv;
        private static SeedSynchronizer _seed;

        public static GameObject GameManager => _gameManager == null ? (_gameManager = GameObject.Find(_gameManagerName)) : _gameManager;

        public static Player Player => _ply == null ? (_ply = new Player(ReferenceHub.HostHub)) : _ply;
        public static ReferenceHub ReferenceHub => ReferenceHub.HostHub;
        public static PlayerStats PlayerStats => _stats == null ? (_stats = ReferenceHub.GetComponent<PlayerStats>()) : _stats;
        public static CharacterClassManager CharacterClassManager => _ccm == null ? (_ccm = ReferenceHub.GetComponent<CharacterClassManager>()) : _ccm;
        public static BanPlayer BanPlayer => _ban == null ? (_ban = ReferenceHub.GetComponent<BanPlayer>()) : _ban;
        public static Broadcast Broadcast => _broadcast == null ? (_broadcast = ReferenceHub.GetComponent<Broadcast>()) : _broadcast;
        public static GrenadeManager GrenadeManager => _gMan == null ? (_gMan = ReferenceHub.GetComponent<GrenadeManager>()) : _gMan;
        public static Inventory Inventory => _inv == null ? (_inv = ReferenceHub.GetComponent<Inventory>()) : _inv;
        public static SeedSynchronizer SeedSync => _seed == null ? (_seed = GameManager.GetComponent<SeedSynchronizer>()) : _seed;


        public static void Refresh()
        {
            try
            {
                _gameManager = GameObject.Find(_gameManagerName);

                GameObject host = ReferenceHub.LocalHub.gameObject;

                _ply = new Player(ReferenceHub.HostHub);
                _stats = host.GetComponent<PlayerStats>();
                _ccm = host.GetComponent<CharacterClassManager>();
                _ban = host.GetComponent<BanPlayer>();
                _broadcast = host.GetComponent<Broadcast>();
                _gMan = host.GetComponent<GrenadeManager>();
                _inv = host.GetComponent<Inventory>();
                _seed = host.GetComponent<SeedSynchronizer>();

                GC.Collect();
            }
            catch (Exception e)
            {
                Log.Add("LocalComponents", $"An error occured while refreshing components!\n{e}", LogType.Error);
            }
        }
    }
}
