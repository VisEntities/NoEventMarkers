/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using Newtonsoft.Json;
using System.Reflection;

namespace Oxide.Plugins
{
    [Info("No Event Markers", "VisEntities", "1.2.1")]
    [Description("Removes map markers for events such as patrol helicopters, hackable crates, and cargo ships.")]
    public class NoEventMarkers : RustPlugin
    {
        #region Fields

        private static NoEventMarkers _plugin;
        private static Configuration _config;
        private static readonly FieldInfo _patrolHelicopterMapMarkerFieldInfo = typeof(PatrolHelicopter).GetField("mapMarkerInstance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo _travellingVendorMapMarkerFieldInfo = typeof(TravellingVendor).GetField("mapMarkerInstance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Disable Patrol Helicopter Marker")]
            public bool DisablePatrolHelicopterMarker { get; set; }

            [JsonProperty("Disable Hackable Locked Crate Marker")]
            public bool DisableHackableLockedCrateMarker { get; set; }

            [JsonProperty("Disable Cargo Ship Marker")]
            public bool DisableCargoShipMarker { get; set; }

            [JsonProperty("Disable Chinook Helicopter Marker")]
            public bool DisableChinookHelicopterMarker { get; set; }

            [JsonProperty("Disable Explosion Marker")]
            public bool DisableExplosionMarker { get; set; }

            [JsonProperty("Disable Travelling Vendor Marker")]
            public bool DisableTravellingVendorMarker { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            if (string.Compare(_config.Version, "1.1.0") < 0)
            {
                _config.DisableChinookHelicopterMarker = defaultConfig.DisableChinookHelicopterMarker;
                _config.DisableExplosionMarker = defaultConfig.DisableExplosionMarker;
            }

            if (string.Compare(_config.Version, "1.2.0") < 0)
            {
                _config.DisableTravellingVendorMarker = defaultConfig.DisableTravellingVendorMarker;
            }

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                DisablePatrolHelicopterMarker = true,
                DisableHackableLockedCrateMarker = true,
                DisableCargoShipMarker = true,
                DisableChinookHelicopterMarker = true,
                DisableExplosionMarker = true,
                DisableTravellingVendorMarker = true
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private void OnServerInitialized(bool isStartup)
        {
            foreach (BaseNetworkable entity in BaseNetworkable.serverEntities)
            {
                if (entity == null)
                    continue;

                RemoveMapMarker(entity);
            }
        }

        private void OnEntitySpawned(BaseEntity entity)
        {
            if (entity == null)
                return;

            RemoveMapMarker(entity);
        }

        #endregion Oxide Hooks

        #region Map Marker Removal

        private void RemoveMapMarker(BaseNetworkable entity)
        {
            if (entity is PatrolHelicopter && _config.DisablePatrolHelicopterMarker)
            {
                PatrolHelicopter patrolHelicopter = entity as PatrolHelicopter;
                if (patrolHelicopter != null)
                {
                    BaseEntity mapMarkerInstance = (BaseEntity)_patrolHelicopterMapMarkerFieldInfo.GetValue(patrolHelicopter);
                    if (mapMarkerInstance != null)
                    {
                        mapMarkerInstance.Kill();
                    }
                }
            }
            else if (entity is CargoShip && _config.DisableCargoShipMarker)
            {
                CargoShip cargoShip = entity as CargoShip;
                if (cargoShip != null && cargoShip.mapMarkerInstance != null)
                {
                    cargoShip.mapMarkerInstance.Kill();
                }
            }
            else if (entity is HackableLockedCrate && _config.DisableHackableLockedCrateMarker)
            {
                HackableLockedCrate hackableLockedCrate = entity as HackableLockedCrate;
                if (hackableLockedCrate != null && hackableLockedCrate.mapMarkerInstance != null)
                {
                    hackableLockedCrate.mapMarkerInstance.Kill();
                }
            }
            else if (entity is CH47Helicopter && _config.DisableChinookHelicopterMarker)
            {
                CH47Helicopter chinookHelicopter = entity as CH47Helicopter;
                if (chinookHelicopter != null && chinookHelicopter.mapMarkerInstance != null)
                {
                    chinookHelicopter.mapMarkerInstance.Kill();
                }
            }
            else if (entity is MapMarkerExplosion && _config.DisableExplosionMarker)
            {
                entity.Kill();
            }
            else if (entity is TravellingVendor && _config.DisableTravellingVendorMarker)
            {
                TravellingVendor travellingVendor = entity as TravellingVendor;
                if (travellingVendor != null)
                {
                    BaseEntity mapMarkerInstance = (BaseEntity)_travellingVendorMapMarkerFieldInfo.GetValue(travellingVendor);
                    if (mapMarkerInstance != null)
                    {
                        mapMarkerInstance.Kill();
                    }
                }
            }
        }

        #endregion Map Marker Removal
    }
}