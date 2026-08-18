using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class HelloWorldManager : MonoBehaviour
{
    VisualElement root;
    Button hostButton;
    Button clientButton;
    Button serverButton;
    Button moveButton;
    Label statusLabel;

    void Start()
    {
#if UNITY_SERVER
        NetworkManager.Singleton.StartServer();
#endif
    }

    void OnEnable()
    {
#if UNITY_SERVER

#else
        var document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        hostButton = CreateButton("HostButton", "Host");
        clientButton = CreateButton("ClientButton", "Client");
        serverButton = CreateButton("ServerButton", "Server");
        moveButton = CreateButton("MoveButton", "Move");
        statusLabel = CreateLabel("StatusLabel", "Not Connected");

        root.Clear();
        root.Add(hostButton);
        root.Add(clientButton);
        root.Add(serverButton);
        root.Add(moveButton);
        root.Add(statusLabel);

        hostButton.clicked += OnHostButtonClicked;
        clientButton.clicked += OnClientButtonClicked;
        serverButton.clicked += OnServerButtonClicked;
        moveButton.clicked += SubmitNewPosition;
#endif
    }

    void OnDisable()
    {
#if !UNITY_SERVER
        hostButton.clicked -= OnHostButtonClicked;
        clientButton.clicked -= OnClientButtonClicked;
        serverButton.clicked -= OnServerButtonClicked;
        moveButton.clicked -= SubmitNewPosition;
#endif
    }

    void Update()
    {
#if UNITY_SERVER
        return;
#else
        UpdateUI();
#endif
    }

    void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
    }

    void OnClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
    }

    void OnServerButtonClicked()
    {
        NetworkManager.Singleton.StartServer();
    }

    private Button CreateButton(string name, string text)
    {
        Button button = new Button();
        button.name = name;
        button.text = text;
        button.style.width = 240;
        button.style.backgroundColor = Color.white;
        button.style.color = Color.black;
        button.style.unityFontStyleAndWeight = FontStyle.Bold;
        return button;
    }

    private Label CreateLabel(string name, string content)
    {
        Label label = new Label();
        label.name = name;
        label.text = content;
        label.style.color = Color.black;
        label.style.fontSize = 18;
        return label;
    }

    void UpdateUI()
    {
        if (NetworkManager.Singleton == null)
        {
            SetStartButtons(false);
            SetMoveButton(false);
            SetStatusText("NetworkManager not found");
            return;
        }

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            SetStartButtons(true);
            SetMoveButton(false);
            SetStatusText("Not connected");
        }
        else
        {
            SetStartButtons(false);
            SetMoveButton(true);
            UpdateStatusLabels();
        }
    }

    void SetStartButtons(bool state)
    {
        hostButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        clientButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        serverButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void SetMoveButton(bool state)
    {
        moveButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        if (state)
        {
            moveButton.text = NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change";
        }
    }

    void SetStatusText(string text)
    {
        statusLabel.text = text;
    }

    void UpdateStatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
        string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
        string modeText = "Mode: " + mode;
        SetStatusText($"{transport}\n{modeText}");
    }

    void SubmitNewPosition()
    {
        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            foreach (var uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid);
                var player = playerObject.GetComponent<HelloWorldPlayer>();
                player.Move();
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            var player = playerObject.GetComponent<HelloWorldPlayer>();
            player.Move();
        }
    }
}
