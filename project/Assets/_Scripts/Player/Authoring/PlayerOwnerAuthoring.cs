using Project02.Client;
using Project02.Core;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    // Authoring (클라 World, 소유자): 등록, 예측 사본과 이력 소유. RPC는 PlayerNetwork
    public class PlayerOwnerAuthoring : NetworkBehaviour
    {
        public readonly PlayerSimulation Simulation = new();   // 예측 사본
        public readonly TickBuffer<InputPayload> InputHistory = new(BUFFER_SIZE);
        public readonly TickBuffer<PlayerSimulationState> StateHistory = new(BUFFER_SIZE);

        public PlayerNetwork Network => _network;
        public PlayerView View => _view;
        public PlayerCameraView Camera => _camera;
        public SOPlayer Settings => _settings;

        [SerializeField] private PlayerNetwork _network;
        [SerializeField] private PlayerView _view;
        [SerializeField] private PlayerCameraView _camera;
        [SerializeField] private PlayerBodyView _visual;
        [SerializeField] private PlayerProgression _progression;
        [SerializeField] private PlayerLocomotion _locomotion;   // SOMovement
        [SerializeField] private PlayerWeaponView _weapon;           // SOCatalog
        [SerializeField] private SOPlayer _settings;

        private const int BUFFER_SIZE = 1024;

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner;
            if (!enabled) return;

            InputHistory.Clear();
            StateHistory.Clear();

            _camera.Initialize(_settings);
            _visual.Initialize(transform.position, _settings);

            ClientLoop.Instance.Register(this);
        }

        protected override void OnNetworkPostSpawn()
        {
            if (!enabled) return;   // 소유자만

            // 스폰 시 1회 (Authoring). 서버 사본과 같은 값으로 시작해야 첫 예측이 맞는다
            SOLevelTable.Row stats = _progression.Table.Get(_progression.Level.Value);
            PlayerSpawnUtil.Initialize(Simulation, transform.position, _locomotion.Settings, _weapon.Catalog, _network.EquippedWeapon.Value, stats);
        }

        public override void OnNetworkDespawn()
        {
            if (!enabled) return;

            if (ClientLoop.Instance != null)
            {
                ClientLoop.Instance.Unregister(this);
            }

            _camera.Release();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public int DEBUG_ReconcileCount;
        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.UpperRight;

            float width = 300f;
            float height = 20f;
            float paddingRight = 10f;
            float xPos = Screen.width - width - paddingRight;
            float fps = 1.0f / Time.unscaledDeltaTime;

            GUI.Label(new Rect(xPos, 30, width, height), $"reconcile: {DEBUG_ReconcileCount}", style);
            GUI.Label(new Rect(xPos, 70, width, height), $"rtt: {NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId)}ms", style);
            GUI.Label(new Rect(xPos, 90, width, height), $"fps: {fps:F1}", style);
            GUI.Label(new Rect(xPos, 110, width, height), $"energy: {Simulation.State.Stamina.Energy:F1}", style);
        }
#endif
    }
}