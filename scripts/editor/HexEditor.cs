#if TOOLS || DEBUG
using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Dev-time level design tool for editing hex grid maps.
/// Toggle with F1, switch camera with Tab.
/// </summary>
public partial class HexEditor : Control
{
    private const string MapFilePath = "res://data/maps/hex_map.tres";

    public static HexEditor Instance { get; private set; }

    // Editor state
    private bool _isActive;
    private bool _isTopDownCamera;
    private HexSaveData _selectedHex;
    private Vector2I? _selectedCoords;

    // Camera references
    private Camera3D _gameCamera;
    private Camera3D _editorCamera;
    private Node3D _editorCameraRig;

    // Editor camera settings
    private Vector2 _cameraPanPosition = Vector2.Zero;
    private float _cameraZoom = 30f;
    private const float MinZoom = 10f;
    private const float MaxZoom = 80f;
    private const float PanSpeed = 30f;
    private const float ZoomSpeed = 5f;

    // Current map data being edited
    private HexMapData _mapData;
    private Dictionary<Vector2I, HexSaveData> _hexLookup = new();

    // Visual feedback
    private MeshInstance3D _selectionIndicator;

    // UI references
    private PanelContainer _selectedHexPanel;
    private Label _coordsLabel;
    private OptionButton _stateDropdown;
    private CheckBox _hiddenCheck;
    private SpinBox _woodSpinBox;
    private SpinBox _stoneSpinBox;
    private ItemList _spawnsList;
    private Button _saveButton;
    private Button _loadButton;
    private Button _newHexButton;
    private Button _deleteHexButton;
    private Button _noneButton;
    private Button _treeButton;
    private Button _rockButton;
    private Label _statusLabel;

    // Placement state
    private PackedScene _selectedResourceScene;
    private bool _isUpdatingUI;
    private bool _isClearingResourceSelection;
    private Dictionary<Vector2I, System.Collections.Generic.List<Node3D>> _placedResources = new();

    // Editor modes
    private bool _isNewHexMode;
    private int _selectedSpawnIndex = -1;

    // Hover preview
    private Node3D _placementPreview;

    public bool IsActive => _isActive;
    public HexSaveData SelectedHex => _selectedHex;
    public Vector2I? SelectedCoords => _selectedCoords;

    public override void _Ready()
    {
        Instance = this;
        Visible = false;
        ProcessMode = ProcessModeEnum.Always; // Process even when paused

        SetupEditorCamera();
        SetupSelectionIndicator();
        SetupUI();

        // Initialize with existing map data or create new
        InitializeMapData();
    }

    private void SetupUI()
    {
        // Top bar buttons
        _saveButton = GetNode<Button>("TopBar/HBoxContainer/SaveButton");
        _loadButton = GetNode<Button>("TopBar/HBoxContainer/LoadButton");
        _newHexButton = GetNode<Button>("TopBar/HBoxContainer/NewHexButton");
        _deleteHexButton = GetNode<Button>("TopBar/HBoxContainer/DeleteHexButton");
        _statusLabel = GetNode<Label>("TopBar/HBoxContainer/StatusLabel");

        _saveButton.Pressed += OnSavePressed;
        _loadButton.Pressed += OnLoadPressed;
        _newHexButton.Pressed += OnNewHexPressed;
        _deleteHexButton.Pressed += OnDeleteHexPressed;

        // Selected hex panel
        _selectedHexPanel = GetNode<PanelContainer>("SelectedHexPanel");
        _coordsLabel = GetNode<Label>("SelectedHexPanel/MarginContainer/VBoxContainer/CoordsLabel");
        _stateDropdown = GetNode<OptionButton>("SelectedHexPanel/MarginContainer/VBoxContainer/StateDropdown");
        _hiddenCheck = GetNode<CheckBox>("SelectedHexPanel/MarginContainer/VBoxContainer/HiddenCheck");
        _woodSpinBox = GetNode<SpinBox>("SelectedHexPanel/MarginContainer/VBoxContainer/WoodCostContainer/WoodSpinBox");
        _stoneSpinBox = GetNode<SpinBox>("SelectedHexPanel/MarginContainer/VBoxContainer/StoneCostContainer/StoneSpinBox");
        _spawnsList = GetNode<ItemList>("SelectedHexPanel/MarginContainer/VBoxContainer/SpawnsList");

        _stateDropdown.ItemSelected += OnStateChanged;
        _hiddenCheck.Toggled += OnHiddenToggled;
        _woodSpinBox.ValueChanged += OnWoodCostChanged;
        _stoneSpinBox.ValueChanged += OnStoneCostChanged;
        _spawnsList.ItemSelected += OnSpawnSelected;

        // Resource palette
        _noneButton = GetNode<Button>("ResourcePalette/HBoxContainer/NoneButton");
        _treeButton = GetNode<Button>("ResourcePalette/HBoxContainer/TreeButton");
        _rockButton = GetNode<Button>("ResourcePalette/HBoxContainer/RockButton");

        _noneButton.Toggled += (pressed) => { if (pressed) OnResourceSelected("None"); };
        _treeButton.Toggled += (pressed) => { if (pressed) OnResourceSelected("Tree"); };
        _rockButton.Toggled += (pressed) => { if (pressed) OnResourceSelected("Rock"); };

        // Connect to own signals
        HexSelected += OnHexSelectedUI;
        HexDeselected += OnHexDeselectedUI;
    }

    public override void _Input(InputEvent @event)
    {
        // F1 toggles editor
        if (@event.IsActionPressed("toggle_hex_editor"))
        {
            ToggleEditor();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (!_isActive) return;

        // Tab toggles camera mode
        if (@event.IsActionPressed("toggle_editor_camera"))
        {
            ToggleCameraMode();
            GetViewport().SetInputAsHandled();
            return;
        }

        // Escape deselects hex and clears resource placement
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (_isNewHexMode)
            {
                ExitNewHexMode();
            }
            else if (_selectedResourceScene != null)
            {
                ClearResourceSelection();
            }
            else
            {
                DeselectHex();
            }
            GetViewport().SetInputAsHandled();
            return;
        }

        // Delete key removes selected spawn
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Delete)
        {
            DeleteSelectedSpawn();
            GetViewport().SetInputAsHandled();
            return;
        }

        // Mouse click for selection (only if not clicking on UI)
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && !IsMouseOverUI())
            {
                HandleLeftClick(mouseButton.Position);
                GetViewport().SetInputAsHandled();
            }
        }

        // Mouse wheel for zoom (in top-down mode)
        if (_isTopDownCamera && @event is InputEventMouseButton scrollEvent)
        {
            if (scrollEvent.ButtonIndex == MouseButton.WheelUp)
            {
                _cameraZoom = Mathf.Max(MinZoom, _cameraZoom - ZoomSpeed);
                UpdateEditorCamera();
            }
            else if (scrollEvent.ButtonIndex == MouseButton.WheelDown)
            {
                _cameraZoom = Mathf.Min(MaxZoom, _cameraZoom + ZoomSpeed);
                UpdateEditorCamera();
            }
        }

        // Mouse motion for placement preview
        if (@event is InputEventMouseMotion mouseMotion && _selectedResourceScene != null)
        {
            UpdatePlacementPreview(mouseMotion.Position);
        }
    }

    public override void _Process(double delta)
    {
        if (!_isActive || !_isTopDownCamera) return;

        // WASD/Arrow pan in top-down mode
        Vector2 panInput = Vector2.Zero;
        if (Input.IsActionPressed("move_left") || Input.IsActionPressed("ui_left"))
            panInput.X -= 1;
        if (Input.IsActionPressed("move_right") || Input.IsActionPressed("ui_right"))
            panInput.X += 1;
        if (Input.IsActionPressed("move_forward") || Input.IsActionPressed("ui_up"))
            panInput.Y -= 1;
        if (Input.IsActionPressed("move_back") || Input.IsActionPressed("ui_down"))
            panInput.Y += 1;

        if (panInput != Vector2.Zero)
        {
            _cameraPanPosition += panInput.Normalized() * PanSpeed * (float)delta;
            UpdateEditorCamera();
        }
    }

    private void ToggleEditor()
    {
        _isActive = !_isActive;
        Visible = _isActive;

        if (_isActive)
        {
            // Pause game
            GetTree().Paused = true;

            // Release mouse
            Input.MouseMode = Input.MouseModeEnum.Visible;

            // Find game camera
            FindGameCamera();

            // Show all hexes (editor mode shows everything)
            ShowAllHexes(true);

            // Show cost labels on all hexes
            ShowAllCostLabels(true);

            // Spawn preview resources for all hexes
            SpawnAllPreviewResources();

            // Ensure None is selected
            ClearResourceSelection();

            GD.Print("[HexEditor] Activated");
        }
        else
        {
            // Unpause game
            GetTree().Paused = false;

            // Restore mouse capture
            Input.MouseMode = Input.MouseModeEnum.Captured;

            // Switch back to game camera
            if (_isTopDownCamera)
            {
                ToggleCameraMode();
            }

            // Hide cost labels
            ShowAllCostLabels(false);

            // Restore hex visibility to game state
            ShowAllHexes(false);

            // Clear preview resources
            ClearAllPreviewResources();

            // Clear selections and modes
            ClearResourceSelection();
            DeselectHex();
            if (_isNewHexMode) ExitNewHexMode();

            GD.Print("[HexEditor] Deactivated");
        }
    }

    /// <summary>
    /// Show or hide all hexes regardless of game state.
    /// </summary>
    private void ShowAllHexes(bool showAll)
    {
        if (HexGridManager.Instance == null) return;

        foreach (var (coords, tile, visual) in HexGridManager.Instance.GetAllHexes())
        {
            if (visual == null) continue;

            if (showAll)
            {
                // Force show all hexes in editor
                visual.ShowImmediate();
            }
            else
            {
                // Restore game visibility - only show unlocked and unlockable hexes
                if (tile.State == HexState.Unlocked)
                {
                    visual.ShowImmediate();
                }
                else if (!visual.StartHidden && HexGridManager.Instance.CanUnlock(coords))
                {
                    visual.ShowImmediate();
                }
                else
                {
                    // Hide hexes that shouldn't be visible in game mode
                    visual.Scale = Vector3.Zero;
                }
            }
        }
    }

    /// <summary>
    /// Spawn preview resources for all hexes from map data.
    /// </summary>
    private void SpawnAllPreviewResources()
    {
        ClearAllPreviewResources();

        foreach (var (coords, hexData) in _hexLookup)
        {
            if (hexData.Spawns == null || hexData.Spawns.Count == 0) continue;

            var hexCenter = HexCoordinates.HexToWorld(coords);

            foreach (var spawn in hexData.Spawns)
            {
                if (spawn?.ResourceScene == null) continue;

                var resource = spawn.ResourceScene.Instantiate<Node3D>();
                resource.Position = hexCenter + new Vector3(spawn.LocalOffset.X, 0, spawn.LocalOffset.Y);
                resource.RotationDegrees = new Vector3(0, spawn.RotationY, 0);
                resource.Name = $"EditorPreview_{coords}";
                GetTree().CurrentScene.AddChild(resource);

                if (!_placedResources.ContainsKey(coords))
                {
                    _placedResources[coords] = new System.Collections.Generic.List<Node3D>();
                }
                _placedResources[coords].Add(resource);
            }
        }

        int totalPreviews = _placedResources.Values.Sum(list => list.Count);
        GD.Print($"[HexEditor] Spawned {totalPreviews} preview resources");
    }

    /// <summary>
    /// Clear all preview resources.
    /// </summary>
    private void ClearAllPreviewResources()
    {
        foreach (var list in _placedResources.Values)
        {
            foreach (var node in list)
            {
                node?.QueueFree();
            }
        }
        _placedResources.Clear();
    }

    /// <summary>
    /// Show or hide cost labels on all hexes.
    /// </summary>
    private void ShowAllCostLabels(bool show)
    {
        if (HexGridManager.Instance == null) return;

        foreach (var (coords, tile, visual) in HexGridManager.Instance.GetAllHexes())
        {
            if (visual == null) continue;

            if (show)
            {
                visual.UpdateCostLabel(tile);
            }
            visual.ShowCostLabel(show);
        }
    }

    private void ToggleCameraMode()
    {
        _isTopDownCamera = !_isTopDownCamera;

        if (_isTopDownCamera)
        {
            // Switch to editor camera
            if (_gameCamera != null)
                _gameCamera.Current = false;
            if (_editorCamera != null)
                _editorCamera.Current = true;

            // Center on selected hex or origin
            if (_selectedCoords.HasValue)
            {
                var worldPos = HexCoordinates.HexToWorld(_selectedCoords.Value);
                _cameraPanPosition = new Vector2(worldPos.X, worldPos.Z);
            }
            else
            {
                _cameraPanPosition = Vector2.Zero;
            }
            UpdateEditorCamera();

            GD.Print("[HexEditor] Top-down camera");
        }
        else
        {
            // Switch back to game camera
            if (_editorCamera != null)
                _editorCamera.Current = false;
            if (_gameCamera != null)
                _gameCamera.Current = true;

            GD.Print("[HexEditor] Game camera");
        }
    }

    private void FindGameCamera()
    {
        if (_gameCamera != null) return;

        // Find player's camera
        var player = GameManager.Instance?.CurrentPlayer;
        if (player != null)
        {
            _gameCamera = player.GetNodeOrNull<Camera3D>("CameraArm/Camera3D");
        }
    }

    private void SetupEditorCamera()
    {
        // Create camera rig
        _editorCameraRig = new Node3D();
        _editorCameraRig.Name = "EditorCameraRig";

        // Create orthographic camera looking down
        _editorCamera = new Camera3D();
        _editorCamera.Name = "EditorCamera";
        _editorCamera.Projection = Camera3D.ProjectionType.Orthogonal;
        _editorCamera.Size = _cameraZoom;
        _editorCamera.Near = 0.1f;
        _editorCamera.Far = 200f;
        _editorCamera.Current = false;

        // Position camera looking straight down
        _editorCamera.Position = new Vector3(0, 50, 0);
        _editorCamera.RotationDegrees = new Vector3(-90, 0, 0);

        _editorCameraRig.AddChild(_editorCamera);

        // Add to scene tree (will be added when editor activates)
        CallDeferred(nameof(AddCameraToScene));
    }

    private void AddCameraToScene()
    {
        var mainScene = GetTree().CurrentScene;
        if (mainScene != null && _editorCameraRig.GetParent() == null)
        {
            mainScene.AddChild(_editorCameraRig);
        }
    }

    private void UpdateEditorCamera()
    {
        if (_editorCamera == null || _editorCameraRig == null) return;

        _editorCameraRig.Position = new Vector3(_cameraPanPosition.X, 0, _cameraPanPosition.Y);
        _editorCamera.Size = _cameraZoom;
    }

    private void SetupSelectionIndicator()
    {
        _selectionIndicator = new MeshInstance3D();
        _selectionIndicator.Name = "SelectionIndicator";

        // Create a hex-shaped selection ring
        var mesh = new TorusMesh();
        mesh.InnerRadius = HexCoordinates.HexSize * 0.85f;
        mesh.OuterRadius = HexCoordinates.HexSize * 0.95f;
        mesh.Rings = 6;
        mesh.RingSegments = 6;
        _selectionIndicator.Mesh = mesh;

        // Yellow selection material
        var material = new StandardMaterial3D();
        material.AlbedoColor = new Color(1f, 0.9f, 0.2f);
        material.EmissionEnabled = true;
        material.Emission = new Color(1f, 0.9f, 0.2f);
        material.EmissionEnergyMultiplier = 2f;
        _selectionIndicator.MaterialOverride = material;

        _selectionIndicator.Visible = false;
        _selectionIndicator.Position = new Vector3(0, 0.1f, 0);
        _selectionIndicator.RotationDegrees = new Vector3(0, 30, 0); // Align with pointy-top hex

        CallDeferred(nameof(AddSelectionIndicatorToScene));
    }

    private void AddSelectionIndicatorToScene()
    {
        var mainScene = GetTree().CurrentScene;
        if (mainScene != null && _selectionIndicator.GetParent() == null)
        {
            mainScene.AddChild(_selectionIndicator);
        }
    }

    /// <summary>
    /// Check if the mouse is currently over any UI control.
    /// </summary>
    private bool IsMouseOverUI()
    {
        // Check if mouse is over the top bar, selected hex panel, or resource palette
        var mousePos = GetViewport().GetMousePosition();

        if (_selectedHexPanel.Visible && _selectedHexPanel.GetGlobalRect().HasPoint(mousePos))
            return true;

        var topBar = GetNode<PanelContainer>("TopBar");
        if (topBar.GetGlobalRect().HasPoint(mousePos))
            return true;

        var palette = GetNode<PanelContainer>("ResourcePalette");
        if (palette.GetGlobalRect().HasPoint(mousePos))
            return true;

        return false;
    }

    private void HandleLeftClick(Vector2 screenPos)
    {
        var camera = _isTopDownCamera ? _editorCamera : _gameCamera;
        if (camera == null) return;

        // Raycast from camera
        var from = camera.ProjectRayOrigin(screenPos);
        var to = from + camera.ProjectRayNormal(screenPos) * 1000f;

        var spaceState = camera.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            var hitPos = (Vector3)result["position"];
            var hexCoords = HexCoordinates.WorldToHex(hitPos);

            // New hex mode - create hex at click position
            if (_isNewHexMode)
            {
                CreateNewHex(hexCoords);
                return;
            }

            // If a resource is selected for placement, place it
            if (_selectedResourceScene != null)
            {
                PlaceResource(hexCoords, hitPos);
            }
            // If a spawn is selected and clicking in same hex, reposition it
            else if (_selectedSpawnIndex >= 0 && _selectedCoords == hexCoords)
            {
                RepositionSelectedSpawn(hitPos);
            }
            else
            {
                SelectHex(hexCoords);
            }
        }
        else
        {
            // Click on empty space
            if (_isNewHexMode)
            {
                // In new hex mode, try to create hex at approximate position
                // This requires estimating hex coords from the click - use ground plane intersection
                var groundY = 0f;
                var rayDir = camera.ProjectRayNormal(screenPos);
                var rayOrigin = camera.ProjectRayOrigin(screenPos);
                if (Mathf.Abs(rayDir.Y) > 0.001f)
                {
                    var t = (groundY - rayOrigin.Y) / rayDir.Y;
                    var groundPos = rayOrigin + rayDir * t;
                    var hexCoords = HexCoordinates.WorldToHex(groundPos);
                    CreateNewHex(hexCoords);
                }
                return;
            }
            DeselectHex();
        }
    }

    /// <summary>
    /// Place a resource at the specified world position within a hex.
    /// </summary>
    private void PlaceResource(Vector2I hexCoords, Vector3 worldPos)
    {
        // Make sure hex exists
        if (HexGridManager.Instance?.GetTile(hexCoords) == null)
        {
            GD.Print($"[HexEditor] Cannot place resource - hex {hexCoords} doesn't exist");
            return;
        }

        // Calculate local offset from hex center
        var hexCenter = HexCoordinates.HexToWorld(hexCoords);
        var localOffset = new Vector2(worldPos.X - hexCenter.X, worldPos.Z - hexCenter.Z);

        // Create spawn point data and mark for embedding
        var spawnPoint = new ResourceSpawnPoint();
        spawnPoint.ResourceScene = _selectedResourceScene;
        spawnPoint.LocalOffset = localOffset;
        spawnPoint.RotationY = 0f;
        spawnPoint.ResourceLocalToScene = true;

        // Get or create hex save data
        if (!_hexLookup.TryGetValue(hexCoords, out var hexData))
        {
            hexData = new HexSaveData(hexCoords);
            hexData.ResourceLocalToScene = true;
            _hexLookup[hexCoords] = hexData;
            _mapData.Hexes.Add(hexData);
            GD.Print($"[HexEditor] Created new hex data for {hexCoords}");
        }

        // Ensure Spawns array exists
        if (hexData.Spawns == null)
        {
            hexData.Spawns = new Godot.Collections.Array<ResourceSpawnPoint>();
        }

        hexData.Spawns.Add(spawnPoint);

        GD.Print($"[HexEditor] Hex {hexCoords} now has {hexData.Spawns.Count} spawns");

        // Instantiate the resource visually (preview)
        var resource = _selectedResourceScene.Instantiate<Node3D>();
        resource.Position = worldPos;
        resource.Name = $"EditorPlaced_{hexCoords}_{hexData.Spawns.Count}";
        GetTree().CurrentScene.AddChild(resource);

        // Track placed resources for cleanup
        if (!_placedResources.ContainsKey(hexCoords))
        {
            _placedResources[hexCoords] = new System.Collections.Generic.List<Node3D>();
        }
        _placedResources[hexCoords].Add(resource);

        GD.Print($"[HexEditor] Placed resource at {hexCoords} offset ({localOffset.X:F1}, {localOffset.Y:F1})");

        // Update spawns list if this hex is selected
        if (_selectedCoords == hexCoords)
        {
            UpdateHexPanel();
        }
    }

    private void SelectHex(Vector2I coords)
    {
        _selectedCoords = coords;
        _selectedSpawnIndex = -1; // Reset spawn selection

        // Look up hex data
        if (_hexLookup.TryGetValue(coords, out var hexData))
        {
            _selectedHex = hexData;
        }
        else
        {
            // Hex exists in grid but not in map data yet - create new entry
            _selectedHex = new HexSaveData(coords);
            if (HexGridManager.Instance?.GetTile(coords) != null)
            {
                _hexLookup[coords] = _selectedHex;
                _mapData.Hexes.Add(_selectedHex);
            }
            else
            {
                _selectedHex = null;
                _selectedCoords = null;
                return;
            }
        }

        // Update selection indicator
        var worldPos = HexCoordinates.HexToWorld(coords);
        _selectionIndicator.Position = new Vector3(worldPos.X, 0.1f, worldPos.Z);
        _selectionIndicator.Visible = true;

        GD.Print($"[HexEditor] Selected hex {coords}");

        // Emit signal for UI to update
        EmitSignal(SignalName.HexSelected, coords);
    }

    private void DeselectHex()
    {
        _selectedHex = null;
        _selectedCoords = null;
        _selectionIndicator.Visible = false;

        EmitSignal(SignalName.HexDeselected);
    }

    private void InitializeMapData()
    {
        if (ResourceLoader.Exists(MapFilePath))
        {
            _mapData = GD.Load<HexMapData>(MapFilePath);
        }

        if (_mapData == null)
        {
            _mapData = new HexMapData();
        }

        // Build lookup table
        _hexLookup.Clear();
        foreach (var hex in _mapData.Hexes)
        {
            _hexLookup[hex.Coordinates] = hex;
        }
    }

    /// <summary>
    /// Get the current map data being edited.
    /// </summary>
    public HexMapData GetMapData() => _mapData;

    /// <summary>
    /// Update a hex's data in the map.
    /// </summary>
    public void UpdateHexData(Vector2I coords, HexSaveData data)
    {
        _hexLookup[coords] = data;

        // Update in array if exists, otherwise add
        for (int i = 0; i < _mapData.Hexes.Count; i++)
        {
            if (_mapData.Hexes[i].Coordinates == coords)
            {
                _mapData.Hexes[i] = data;
                return;
            }
        }
        _mapData.Hexes.Add(data);
    }

    // UI Event Handlers
    private void OnHexSelectedUI(Vector2I coords)
    {
        _selectedHexPanel.Visible = true;
        UpdateHexPanel();
    }

    private void OnHexDeselectedUI()
    {
        _selectedHexPanel.Visible = false;
    }

    private void UpdateHexPanel()
    {
        if (_selectedHex == null) return;

        _isUpdatingUI = true;

        _coordsLabel.Text = $"Coords: ({_selectedHex.Coordinates.X}, {_selectedHex.Coordinates.Y})";
        _stateDropdown.Selected = (int)_selectedHex.InitialState;
        _hiddenCheck.ButtonPressed = _selectedHex.StartHidden;
        _woodSpinBox.Value = _selectedHex.UnlockCostWood;
        _stoneSpinBox.Value = _selectedHex.UnlockCostStone;

        // Update spawns list
        _spawnsList.Clear();
        if (_selectedHex.Spawns != null)
        {
            foreach (var spawn in _selectedHex.Spawns)
            {
                string sceneName = spawn.ResourceScene?.ResourcePath.GetFile().GetBaseName() ?? "Unknown";
                _spawnsList.AddItem($"{sceneName} ({spawn.LocalOffset.X:F1}, {spawn.LocalOffset.Y:F1})");
            }
        }

        _isUpdatingUI = false;
    }

    private void OnStateChanged(long index)
    {
        if (_isUpdatingUI || _selectedHex == null) return;
        _selectedHex.InitialState = (HexInitialState)index;
        GD.Print($"[HexEditor] State changed to {_selectedHex.InitialState}");
    }

    private void OnHiddenToggled(bool pressed)
    {
        if (_isUpdatingUI || _selectedHex == null) return;
        _selectedHex.StartHidden = pressed;
        GD.Print($"[HexEditor] StartHidden changed to {pressed}");
    }

    private void OnWoodCostChanged(double value)
    {
        if (_isUpdatingUI || _selectedHex == null) return;
        _selectedHex.UnlockCostWood = (int)value;
    }

    private void OnStoneCostChanged(double value)
    {
        if (_isUpdatingUI || _selectedHex == null) return;
        _selectedHex.UnlockCostStone = (int)value;
    }

    private void OnResourceSelected(string resourceType)
    {
        // Skip callback when programmatically clearing selection
        if (_isClearingResourceSelection) return;

        // ButtonGroup handles mutual exclusion, just set the scene
        if (resourceType == "Tree")
        {
            _selectedResourceScene = ResourceSpawnManager.Instance?.TreeScene;
        }
        else if (resourceType == "Rock")
        {
            _selectedResourceScene = ResourceSpawnManager.Instance?.RockScene;
        }
        else
        {
            _selectedResourceScene = null;
        }

        GD.Print($"[HexEditor] Selected resource: {resourceType}");
    }

    /// <summary>
    /// Clear resource placement mode by selecting "None".
    /// </summary>
    private void ClearResourceSelection()
    {
        _selectedResourceScene = null;

        // Destroy placement preview
        DestroyPlacementPreview();

        // Select None button (ButtonGroup handles deselecting others)
        _isClearingResourceSelection = true;
        _noneButton.ButtonPressed = true;
        _isClearingResourceSelection = false;
    }

    private void OnSavePressed()
    {
        SaveMapData();
    }

    private void OnLoadPressed()
    {
        LoadMapData();
    }

    private void OnNewHexPressed()
    {
        EnterNewHexMode();
    }

    private void OnDeleteHexPressed()
    {
        if (_selectedCoords.HasValue && _selectedHex != null)
        {
            // Remove from lookup and array
            _hexLookup.Remove(_selectedCoords.Value);
            for (int i = _mapData.Hexes.Count - 1; i >= 0; i--)
            {
                if (_mapData.Hexes[i].Coordinates == _selectedCoords.Value)
                {
                    _mapData.Hexes.RemoveAt(i);
                    break;
                }
            }
            GD.Print($"[HexEditor] Deleted hex {_selectedCoords.Value}");
            DeselectHex();
        }
    }

    private void SaveMapData()
    {
        // Ensure we have all hexes from the grid in our map data
        SyncGridToMapData();

        // Count spawns for debug
        int totalSpawns = 0;

        // Mark all nested resources as local to scene for proper embedding
        foreach (var hex in _mapData.Hexes)
        {
            hex.ResourceLocalToScene = true;
            if (hex.Spawns != null && hex.Spawns.Count > 0)
            {
                totalSpawns += hex.Spawns.Count;
                GD.Print($"[HexEditor] Hex {hex.Coordinates} has {hex.Spawns.Count} spawns");
                foreach (var spawn in hex.Spawns)
                {
                    spawn.ResourceLocalToScene = true;
                    GD.Print($"  - {spawn.ResourceScene?.ResourcePath ?? "NULL"} at {spawn.LocalOffset}");
                }
            }
        }

        GD.Print($"[HexEditor] Saving {_mapData.Hexes.Count} hexes with {totalSpawns} total spawns");

        var error = ResourceSaver.Save(_mapData, MapFilePath, ResourceSaver.SaverFlags.ChangePath);
        if (error == Error.Ok)
        {
            _statusLabel.Text = $"Saved {_mapData.Hexes.Count} hexes, {totalSpawns} spawns";
            GD.Print($"[HexEditor] Saved to {MapFilePath}");
        }
        else
        {
            _statusLabel.Text = $"Save failed: {error}";
            GD.PrintErr($"[HexEditor] Failed to save: {error}");
        }
    }

    private void LoadMapData()
    {
        // Clear and reload
        ClearAllPreviewResources();
        InitializeMapData();

        // Respawn all preview resources
        SpawnAllPreviewResources();

        // Update cost labels
        ShowAllCostLabels(true);

        DeselectHex();

        int totalSpawns = _hexLookup.Values.Sum(h => h.Spawns?.Count ?? 0);
        _statusLabel.Text = $"Loaded {_mapData.Hexes.Count} hexes, {totalSpawns} spawns";
        GD.Print($"[HexEditor] Loaded {_mapData.Hexes.Count} hexes, {totalSpawns} spawns from {MapFilePath}");
    }

    /// <summary>
    /// Sync all hexes from the current grid to map data.
    /// This ensures hexes that haven't been explicitly edited are still saved.
    /// </summary>
    private void SyncGridToMapData()
    {
        if (HexGridManager.Instance == null)
        {
            GD.PrintErr("[HexEditor] HexGridManager.Instance is null!");
            return;
        }

        int addedCount = 0;
        int totalFromGrid = 0;

        // Add all hexes from the grid that aren't already in our map data
        foreach (var (coords, tile, visual) in HexGridManager.Instance.GetAllHexes())
        {
            totalFromGrid++;

            if (!_hexLookup.ContainsKey(coords))
            {
                // Create save data from runtime tile
                var hexData = new HexSaveData(coords);
                hexData.InitialState = tile.State == HexState.Unlocked ? HexInitialState.Unlocked : HexInitialState.Locked;
                hexData.UnlockCostWood = tile.GetCost(ResourceType.Wood);
                hexData.UnlockCostStone = tile.GetCost(ResourceType.Stone);
                hexData.StartHidden = visual?.StartHidden ?? false;
                hexData.Spawns = new Godot.Collections.Array<ResourceSpawnPoint>();

                _hexLookup[coords] = hexData;
                _mapData.Hexes.Add(hexData);
                addedCount++;
            }
        }

        GD.Print($"[HexEditor] Grid has {totalFromGrid} hexes, added {addedCount} new, total in map: {_mapData.Hexes.Count}");
    }

    /// <summary>
    /// Handle spawn selection from the list.
    /// </summary>
    private void OnSpawnSelected(long index)
    {
        _selectedSpawnIndex = (int)index;
        _statusLabel.Text = $"Spawn selected - Click in hex to reposition, Delete to remove";
        GD.Print($"[HexEditor] Selected spawn index {_selectedSpawnIndex}");
    }

    /// <summary>
    /// Reposition the selected spawn to a new position within the hex.
    /// </summary>
    private void RepositionSelectedSpawn(Vector3 worldPos)
    {
        if (_selectedHex == null || _selectedSpawnIndex < 0) return;
        if (_selectedHex.Spawns == null || _selectedSpawnIndex >= _selectedHex.Spawns.Count) return;
        if (!_selectedCoords.HasValue) return;

        var coords = _selectedCoords.Value;
        var hexCenter = HexCoordinates.HexToWorld(coords);
        var newOffset = new Vector2(worldPos.X - hexCenter.X, worldPos.Z - hexCenter.Z);

        // Update spawn data
        var spawn = _selectedHex.Spawns[_selectedSpawnIndex];
        spawn.LocalOffset = newOffset;

        // Update preview node position
        if (_placedResources.TryGetValue(coords, out var resourceList) && _selectedSpawnIndex < resourceList.Count)
        {
            var node = resourceList[_selectedSpawnIndex];
            if (node != null)
            {
                node.Position = worldPos;
            }
        }

        GD.Print($"[HexEditor] Repositioned spawn to ({newOffset.X:F1}, {newOffset.Y:F1})");
        _statusLabel.Text = $"Spawn moved to ({newOffset.X:F1}, {newOffset.Y:F1})";

        // Update UI
        UpdateHexPanel();
    }

    /// <summary>
    /// Delete the currently selected spawn from the hex.
    /// </summary>
    private void DeleteSelectedSpawn()
    {
        if (_selectedHex == null || _selectedSpawnIndex < 0) return;
        if (_selectedHex.Spawns == null || _selectedSpawnIndex >= _selectedHex.Spawns.Count) return;

        var coords = _selectedCoords.Value;

        // Remove the preview node
        if (_placedResources.TryGetValue(coords, out var resourceList) && _selectedSpawnIndex < resourceList.Count)
        {
            var node = resourceList[_selectedSpawnIndex];
            node?.QueueFree();
            resourceList.RemoveAt(_selectedSpawnIndex);
        }

        // Remove from spawn data
        _selectedHex.Spawns.RemoveAt(_selectedSpawnIndex);
        _selectedSpawnIndex = -1;

        GD.Print($"[HexEditor] Deleted spawn from hex {coords}");

        // Update UI
        UpdateHexPanel();
    }

    /// <summary>
    /// Enter new hex creation mode.
    /// </summary>
    private void EnterNewHexMode()
    {
        _isNewHexMode = true;
        ClearResourceSelection();
        _statusLabel.Text = "NEW HEX MODE: Click to add hex (Esc to cancel)";
        GD.Print("[HexEditor] Entered new hex mode");
    }

    /// <summary>
    /// Exit new hex creation mode.
    /// </summary>
    private void ExitNewHexMode()
    {
        _isNewHexMode = false;
        _statusLabel.Text = "HEX EDITOR [F1 to close, Tab for top-down]";
        GD.Print("[HexEditor] Exited new hex mode");
    }

    /// <summary>
    /// Create a new hex at the specified coordinates.
    /// </summary>
    private void CreateNewHex(Vector2I coords)
    {
        // Check if hex already exists
        if (HexGridManager.Instance?.GetTile(coords) != null)
        {
            GD.Print($"[HexEditor] Hex {coords} already exists");
            return;
        }

        // Create hex data
        var hexData = new HexSaveData(coords);
        hexData.ResourceLocalToScene = true;
        hexData.Spawns = new Godot.Collections.Array<ResourceSpawnPoint>();
        _hexLookup[coords] = hexData;
        _mapData.Hexes.Add(hexData);

        // Create visual in grid manager (need to expose this or work around)
        // For now, just add to map data - will appear after save/reload
        GD.Print($"[HexEditor] Created new hex at {coords} (save and reload to see it)");
        _statusLabel.Text = $"Created hex {coords} - Save to apply";

        ExitNewHexMode();
    }

    /// <summary>
    /// Update the placement preview position.
    /// </summary>
    private void UpdatePlacementPreview(Vector2 screenPos)
    {
        if (_selectedResourceScene == null)
        {
            DestroyPlacementPreview();
            return;
        }

        var camera = _isTopDownCamera ? _editorCamera : _gameCamera;
        if (camera == null) return;

        // Raycast to find position
        var from = camera.ProjectRayOrigin(screenPos);
        var to = from + camera.ProjectRayNormal(screenPos) * 1000f;

        var spaceState = camera.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            var hitPos = (Vector3)result["position"];

            // Create preview if needed
            if (_placementPreview == null)
            {
                _placementPreview = _selectedResourceScene.Instantiate<Node3D>();
                _placementPreview.Name = "PlacementPreview";
                GetTree().CurrentScene.AddChild(_placementPreview);

                // Make it semi-transparent
                SetPreviewTransparency(_placementPreview, 0.5f);
            }

            _placementPreview.Position = hitPos;
            _placementPreview.Visible = true;
        }
        else
        {
            if (_placementPreview != null)
            {
                _placementPreview.Visible = false;
            }
        }
    }

    /// <summary>
    /// Set transparency on a node and its children.
    /// </summary>
    private void SetPreviewTransparency(Node3D node, float alpha)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is MeshInstance3D meshInstance)
            {
                var material = meshInstance.GetActiveMaterial(0);
                if (material is StandardMaterial3D stdMat)
                {
                    var newMat = (StandardMaterial3D)stdMat.Duplicate();
                    newMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                    newMat.AlbedoColor = new Color(newMat.AlbedoColor.R, newMat.AlbedoColor.G, newMat.AlbedoColor.B, alpha);
                    meshInstance.MaterialOverride = newMat;
                }
            }
            if (child is Node3D child3D)
            {
                SetPreviewTransparency(child3D, alpha);
            }
        }
    }

    /// <summary>
    /// Destroy the placement preview.
    /// </summary>
    private void DestroyPlacementPreview()
    {
        if (_placementPreview != null)
        {
            _placementPreview.QueueFree();
            _placementPreview = null;
        }
    }

    // Signals
    [Signal] public delegate void HexSelectedEventHandler(Vector2I coords);
    [Signal] public delegate void HexDeselectedEventHandler();
}
#endif
