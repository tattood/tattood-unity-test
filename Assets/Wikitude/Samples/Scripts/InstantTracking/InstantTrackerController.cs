using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Wikitude;
using System.IO;
using System;
using System.Text;

public class InstantTrackerController : SampleController 
{
	public GameObject ButtonDock;
	public GameObject InitializationControls;
	public Text HeightLabel;
	public Text ScaleLabel;

    //public Material mat;
    private Transform model;
    private Gyroscope gyro;

	public InstantTracker Tracker;
    public GameObject Trackable;
    public Text x_t, y_t, z_t, modelPlacementText;

    public List<Button> Buttons;
	public List<GameObject> Models;

	public Image ActivityIndicator;

	public Color EnabledColor = new Color(0.2f, 0.75f, 0.2f, 0.8f);
	public Color DisabledColor = new Color(1.0f, 0.2f, 0.2f, 0.8f);

	private float _currentDeviceHeightAboveGround = 1.0f;

	private MoveController _moveController;
	private GridRenderer _gridRenderer;

	private HashSet<GameObject> _activeModels = new HashSet<GameObject>();
	private InstantTrackingState _currentState = InstantTrackingState.Initializing;
	private bool _isTracking = false;

	public HashSet<GameObject> ActiveModels {
		get { 
			return _activeModels;
		}
	}
    //public GameObject initialModel;
    //void Start()
    //{
    //    Quaternion initalModelRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-Camera.main.transform.forward, Vector3.up), Vector3.up);
    //    initialModel.transform.rotation = initalModelRotation;
    //}
	private void Awake() {
		Application.targetFrameRate = 60;

		_moveController = GetComponent<MoveController>();
		_gridRenderer = GetComponent<GridRenderer>();
        if (Input.gyro != null)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
        }
    }
    private void Start()
    {
        OnHeightValueChanged(2f);
    }
    float yRotation, xRotation, zRotation;
    Quaternion lookRotation;
    float rebalanceFreq;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        rebalanceFreq += Time.deltaTime;
        if (rebalanceFreq == 0.05f)
        {
            rebalanceFreq = 0;
            if (Input.gyro != null)
            {
                Debug.Log("x: " + Input.gyro.attitude.eulerAngles.x + " y: " + Input.gyro.attitude.eulerAngles.y + " z: " + Input.gyro.attitude.eulerAngles.z);

                yRotation = Input.gyro.attitude.eulerAngles.y;
                xRotation = (Input.gyro.attitude.eulerAngles.x);
                //zRotation = Input.gyro.attitude.eulerAngles.z;

                x_t.text = "" + xRotation;
                //y_t.text = "" + yRotation;
                //z_t.text = "" + zRotation;
                //Trackable.transform.eulerAngles = new Vector3(xRotation, 0, 0);
                //Trackable.transform.localRotation = new Quaternion(0, Input.gyro.attitude.x, 0, 0);
                Trackable.transform.rotation = Quaternion.Euler(new Vector3(xRotation, yRotation, 0));
                //lookRotation = Quaternion.LookRotation(-Camera.main.transform.forward);
                //lookRotation.z = 0; 
                //Trackable.transform.rotation = lookRotation;
                y_t.text = "" + modelIndex;
            }
        }
        
    }
    Quaternion modelRotation;
    bool placedModel= true;
    void LateUpdate()
    {
        //placeWithTouch();

    }
    
    public void placeWithTouch()
    {
        if (_isTracking && !placedModel)
        {
            if (Input.touchCount != 0 || Input.GetMouseButtonDown(0))
            {
                GameObject modelPrefab = Models[modelIndex];
                model = Instantiate(modelPrefab).transform;
                //model.GetComponentInChildren<MeshRenderer>().material = mat;
                _activeModels.Add(model.gameObject);
                var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane p = new Plane(Vector3.up, Vector3.zero);
                float enter;
                if (p.Raycast(cameraRay, out enter))
                {
                    model.position = cameraRay.GetPoint(enter);
                    placedModel = true;
                    modelPlacementText.text = "Model Placed";
                }
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.5f, 1.5f);
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(-0.2f, 0.1f);
                z_t.text = "" + model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale;
            }


        }
    }

    #region UI Events
    public void OnInitializeButtonClicked() {
		Tracker.SetState(InstantTrackingState.Tracking);
        placedModel = false;
        _currentDeviceHeightAboveGround = 2;
        HeightLabel.text = string.Format("{0:0.##} m", _currentDeviceHeightAboveGround);
        Tracker.DeviceHeightAboveGround = _currentDeviceHeightAboveGround;
        placeToMiddle();
    }
    public Material mat;

    public void changeMaterial(string imagePath)
    {
        byte[] decodedBytes = Convert.FromBase64String(imagePath);
        Texture2D imageTexture = new Texture2D(Screen.width, Screen.height);
        imageTexture.LoadImage(decodedBytes);
        mat.mainTexture = imageTexture;
        mat.mainTexture.wrapMode = TextureWrapMode.Clamp;
    }
    public void placeToMiddle()
    {
        GameObject modelPrefab = Models[modelIndex];
        model = Instantiate(modelPrefab).transform;
        model.gameObject.GetComponent<MeshRenderer>().material = mat;
        _activeModels.Add(model.gameObject);
        model.position = Vector3.zero;
        placedModel = true;
        modelPlacementText.text = "Model Placed";
        model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.5f, 1.5f);
        model.gameObject.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(-0.2f, -0.2f);
        z_t.text = "" + model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale;
        ImageProcessing.IP(model.gameObject);
        
    }
    float modelScale;

    public void OnHeightValueChanged(float newHeightValue) {
		_currentDeviceHeightAboveGround = newHeightValue;
		HeightLabel.text = string.Format("{0:0.##} m", _currentDeviceHeightAboveGround);
		Tracker.DeviceHeightAboveGround = _currentDeviceHeightAboveGround;
	}
    GameObject modelPrfb;
    public void OnScaleChanged(float newScaleValue)
    {
        if (model != null)
        {
            y_t.text = "Scale: " + newScaleValue;
            modelScale = newScaleValue;
            if (modelScale == 2 && modelIndex != 0)
            {

                Vector3 pos = model.transform.position;
                foreach (var model in _activeModels)
                {
                    Destroy(model);
                }
                modelIndex = 0;
                _activeModels.Clear();
                modelPrfb = Models[modelIndex];
                model = Instantiate(modelPrfb).transform;
                _activeModels.Add(model.gameObject);
                model.position = pos;

                z_t.text = "Model changed to Model " + modelIndex;
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.5f, 1.5f);
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(-0.2f, -0.2f);
                z_t.text = "" + model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale;
            }
            else if (modelScale == 3 && modelIndex != 1)
            {

                Vector3 pos = model.transform.position;
                foreach (var model in _activeModels)
                {
                    Destroy(model);
                }
                modelIndex = 1;
                _activeModels.Clear();
                modelPrfb = Models[modelIndex];
                model = Instantiate(modelPrfb).transform;
                _activeModels.Add(model.gameObject);
                model.position = pos;

                z_t.text = "Model changed to Model " + modelIndex;

                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.5f, 1.5f);
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(-0.2f, -0.2f);
                z_t.text = "" + model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale;
            }
            else if (modelScale == 4 && modelIndex != 2)
            {

                Vector3 pos = model.transform.position;
                foreach (var model in _activeModels)
                {
                    Destroy(model);
                }
                modelIndex = 2;
                _activeModels.Clear();
                modelPrfb = Models[modelIndex];
                model = Instantiate(modelPrfb).transform;
                _activeModels.Add(model.gameObject);
                model.position = pos;
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.5f, 1.5f);
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(-0.2f, -0.2f);
                z_t.text = "Model changed to Model " + modelIndex;
            }
            else if (modelScale == 5 && modelIndex != 3)
            {
                Vector3 pos = model.transform.position;
                foreach (var model in _activeModels)
                {
                    Destroy(model);
                }
                modelIndex = 3;
                _activeModels.Clear();
                modelPrfb = Models[modelIndex];
                model = Instantiate(modelPrfb).transform;
                _activeModels.Add(model.gameObject);
                model.position = pos;
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.5f, 1.5f);
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(-0.2f, -0.2f);
                z_t.text = "Model changed to Model " + modelIndex;
            }
            else if (modelScale == 6 && modelIndex != 4)
            {
                Vector3 pos = model.transform.position;
                foreach (var model in _activeModels)
                {
                    Destroy(model);
                }
                modelIndex = 4;
                _activeModels.Clear();
                modelPrfb = Models[modelIndex];
                model = Instantiate(modelPrfb).transform;
                _activeModels.Add(model.gameObject);
                model.position = pos;
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1.5f, 1.5f);
                model.gameObject.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(-0.2f, -0.2f);
                z_t.text = "Model changed to Model " + modelIndex;
            }

        }
        
    }
    public void ScaleModel(string numToScale)
    {
        float result;
        float.TryParse(numToScale, out result);
        model.localScale =  result * Vector3.one;
    }
    int modelIndex = 0;
    public void OnSelectModel()
    {
        if (_isTracking)
        {
            // Create object
            GameObject modelPrefab = Models[modelIndex];
            model = Instantiate(modelPrefab).transform;
            //model.GetComponentInChildren<MeshRenderer>().material = mat;
            _activeModels.Add(model.gameObject);
            // Set model position at touch position
            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane p = new Plane(Vector3.up, Vector3.zero);
            float enter;
            if (p.Raycast(cameraRay, out enter))
            {
                model.position = cameraRay.GetPoint(enter);
            }

            // Set model orientation to face toward the camera
            Quaternion modelRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-Camera.main.transform.forward, Vector3.up), Vector3.up);
            model.rotation = modelRotation;

            _moveController.SetMoveObject(model);
        }
    }
    public void OnBeginDrag () {
		if (_isTracking) {
			// Create object
			GameObject modelPrefab = Models[modelIndex];
			model = Instantiate(modelPrefab).transform;
            //model.GetComponentInChildren<MeshRenderer>().material = mat;
            _activeModels.Add(model.gameObject);
			// Set model position at touch position
			var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			Plane p = new Plane(Vector3.up, Vector3.zero);
			float enter;
			if (p.Raycast(cameraRay, out enter)) {
				model.position = cameraRay.GetPoint(enter);
			}

			// Set model orientation to face toward the camera
			Quaternion modelRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-Camera.main.transform.forward, Vector3.up), Vector3.up);
			model.rotation = modelRotation;

			_moveController.SetMoveObject(model);
		}
	}

	public override void OnBackButtonClicked() {
		if (_currentState == InstantTrackingState.Initializing) {
			base.OnBackButtonClicked();
            placedModel = false;
		} else {
			Tracker.SetState(InstantTrackingState.Initializing);
		}
	}
	#endregion

	#region Tracker Events
	public void OnEnterFieldOfVision(string target) {
		SetSceneActive(true);
	}

	public void OnExitFieldOfVision(string target) {
		SetSceneActive(false);
	}

	private void SetSceneActive(bool active) {
		foreach (var button in Buttons) {
			button.interactable = active;
		}

		foreach (var model in _activeModels) {
			model.SetActive(active);
		}

		ActivityIndicator.color = active ? EnabledColor : DisabledColor;
		
		_gridRenderer.enabled = active;
		_isTracking = active;
	}

	public void OnStateChanged(InstantTrackingState newState) {
		Tracker.DeviceHeightAboveGround = _currentDeviceHeightAboveGround;
		_currentState = newState;
		if (newState == InstantTrackingState.Tracking) {
			InitializationControls.SetActive(false);
			ButtonDock.SetActive(true);
           // _gridRenderer.enabled = false;
		} else {
			foreach (var model in _activeModels) {
				Destroy(model);
			}
			_activeModels.Clear();

			InitializationControls.SetActive(true);
			ButtonDock.SetActive(false);

        }

        _gridRenderer.enabled = true;
    }
	#endregion
}
