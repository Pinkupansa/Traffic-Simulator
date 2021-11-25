using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[System.Serializable]
public enum EditMode {PlaceRoad, PlaceVehicle, PlaceDestination}
public class SimulationEditor : MonoBehaviour
{
    [SerializeField] private EditMode editMode;
    [SerializeField] private Vehicle[] vehicles;
    [SerializeField] private Slider globalMaxSpeedSlider, securityDistanceSlider, carMaxSpeedSlider, reactionTimeSlider, sightRadiusSlider, imprudenceSlider;
    [SerializeField] private GameObject carParametersMenu, globalParametersMenu;
    [SerializeField] private GameObject endFlagPrefab;
    [SerializeField] private Text globalMaxSpeedText, securityDistanceText, carMaxSpeedText, reactionTimeText, sightRadiusText, imprudenceText;
    private GameObject objectInHand;
    private GameObject currentDestinationFlag;
    private Vehicle currentVehicle;
    [SerializeField] private float spaceDilatation;
    private void Start()
    {
        UpdateData();
    }
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && !CarManager.current.started)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(Input.GetButtonDown("Fire1"))
            {
                OnLeftClick(mousePos);
            }
            if(Input.GetButtonDown("Fire2"))
            {
                OnRightClick(mousePos);
            }
        }
    }
    private void OnLeftClick(Vector2 mousePos)
    {
        switch(editMode)
        {
            case EditMode.PlaceRoad:
                PathManager.current.CreatePoint(mousePos);
                
                break;
            case EditMode.PlaceDestination:
                if(currentDestinationFlag != null)
                {
                    Destroy(currentDestinationFlag);
                }
                currentDestinationFlag = Instantiate(endFlagPrefab, mousePos, Quaternion.identity);

                break;
            case EditMode.PlaceVehicle:
                if(currentDestinationFlag == null)
                {
                    CarManager.current.CreateCar(PathManager.current.road.GetPointByID(PathManager.current.road.GetClosestPointID(mousePos)).position, currentVehicle.data, currentVehicle.sprite, false);
                }
                else
                {
                    currentVehicle.data.destination = currentDestinationFlag.transform.position;
                    CarManager.current.CreateCar(PathManager.current.road.GetPointByID(PathManager.current.road.GetClosestPointID(mousePos)).position, currentVehicle.data, currentVehicle.sprite, true);
                }
                
                break;
        }
    }
    private void OnRightClick(Vector2 mousePos)
    {
        switch (editMode)
        {
            case EditMode.PlaceRoad:
                PathManager.current.SetLink(mousePos);
                break;
            case EditMode.PlaceDestination:
                break;
            case EditMode.PlaceVehicle:
                break;
        }
    }
    public void EraseAll()
    {
        SceneManager.LoadSceneAsync(0);
    }
    public void SetEditMode(int mode)
    {
        editMode = (EditMode)mode;
    }
    public void SetVehicle(int index)
    {
        HideUnhideCarParameters(true);
        currentVehicle = Instantiate(vehicles[index]);
        carMaxSpeedSlider.value = currentVehicle.data.maxCarSpeed;
        reactionTimeSlider.value = currentVehicle.data.reactionTime;
        sightRadiusSlider.value = currentVehicle.data.sightRadius;
        imprudenceSlider.value = currentVehicle.data.imprudence;
    }
    public void UpdateData()
    {
        CarManager.current.maxSpeed = globalMaxSpeedSlider.value;
        globalMaxSpeedText.text = string.Format("{0} km/h", (int)(globalMaxSpeedSlider.value * 3.6f * spaceDilatation));
        CarManager.current.securityDistance = securityDistanceSlider.value;
        securityDistanceText.text = string.Format("{0} m", (int)securityDistanceSlider.value * spaceDilatation);
        if (currentVehicle != null)
        {
            
            currentVehicle.data.maxCarSpeed = carMaxSpeedSlider.value;
            carMaxSpeedText.text = string.Format("{0} km/h", (int)carMaxSpeedSlider.value * 3.6f * spaceDilatation);
            currentVehicle.data.reactionTime = reactionTimeSlider.value;
            reactionTimeText.text = string.Format("{0} s", Mathf.Round(reactionTimeSlider.value*100f)/100f);
            currentVehicle.data.sightRadius = sightRadiusSlider.value;
            sightRadiusText.text = string.Format("{0} m", (int)sightRadiusSlider.value * spaceDilatation);
            currentVehicle.data.imprudence = imprudenceSlider.value;
            imprudenceText.text = string.Format("{0}", (int)Mathf.Round(imprudenceSlider.value * 10f)/10f);
            
        }
        
    }
    public void HideUnhideCarParameters(bool show)
    {
        carParametersMenu.SetActive(show);
    }
}

