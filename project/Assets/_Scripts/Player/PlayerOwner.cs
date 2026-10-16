using Project02.Client;
using Project02.Core;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerOwner : NetworkBehaviour
    {
        public PlayerView View => _view;
        public PlayerController Controller => _controller;

        public readonly TickBuffer<InputPayload> InputHistory = new(BUFFER_SIZE);
        public readonly TickBuffer<PlayerSimulationState> StateHistory = new(BUFFER_SIZE);

        public StatePayload PendingState;
        public bool HasPendingState;
        public int LatestServerTick = -1;

        [SerializeField] private PlayerController _controller;
        [SerializeField] private PlayerView _view;

        private const int BUFFER_SIZE = 1024;

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner;
            if (!enabled)
            {
                return;
            }

            InputHistory.Clear();
            StateHistory.Clear();

            _controller.PlayerInput.Initialize();
            _controller.PlayerInput.Active();
            _controller.PlayerInput.Enable();
            _controller.PlayerCamera.Initialize(_controller.PlayerSettings);
            _controller.PlayerVisual.Initialize(transform.position, _controller.PlayerSettings);

            ClientSimulation.Instance.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            if (!enabled)
            {
                return;
            }

            ClientSimulation.Instance?.Unregister(this);

            _controller.PlayerInput.Inactive();
            _controller.PlayerInput.Disable();
            _controller.PlayerCamera.Release();
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
        public void StateRPC(StatePayload payload)
        {
            if (payload.State.Tick <= LatestServerTick) return;   // 늦게 온 옛 상태 버림
            LatestServerTick = payload.State.Tick;

            PendingState = payload;
            HasPendingState = true;
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
            GUI.Label(new Rect(xPos, 110, width, height), $"energy: {_controller.PlayerLocomotion.Energy:F1}", style);
            GUI.Label(new Rect(xPos, 130, width, height), $"yaw: {_controller.PlayerCamera.Yaw:F1}", style);
        }
#endif
    }
}