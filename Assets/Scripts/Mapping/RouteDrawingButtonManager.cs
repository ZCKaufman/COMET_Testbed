using UnityEngine;
using UnityEngine.UI;


public class RouteDrawingButtonManager : MonoBehaviour
{
    public RouteDrawingManager routeDrawingManager;
    public Button walkButton;
    public Button driveButton;
    public Button deleteButton;
    public Button deleteAllButton;
    public Button ParkingButton;


    private string activeMode = null; // "walk", "drive", or "delete"


    public void OnWalkButtonClicked()
    {
        if (activeMode == "walk")
        {
            routeDrawingManager.ToggleDrawingMode("walk");
            activeMode = null;
        }
        else
        {
            routeDrawingManager.ToggleDrawingMode("walk");
            routeDrawingManager.ToggleDeleteMode(false);
            activeMode = "walk";
        }
        UpdateButtonVisuals();
    }


    public void OnDriveButtonClicked()
    {
        if (activeMode == "drive")
        {
            routeDrawingManager.ToggleDrawingMode("drive");
            activeMode = null;
        }
        else
        {
            routeDrawingManager.ToggleDrawingMode("drive");
            routeDrawingManager.ToggleDeleteMode(false);
            activeMode = "drive";
        }
        UpdateButtonVisuals();
    }


    public void OnDeleteButtonClicked()
    {
        if (activeMode == "delete")
        {
            routeDrawingManager.ToggleDeleteMode(false);
            activeMode = null;
        }
        else
        {
            routeDrawingManager.ToggleDeleteMode(true);
            routeDrawingManager.ToggleDrawingMode(null);
            activeMode = "delete";
        }
        UpdateButtonVisuals();
    }


    private void UpdateButtonVisuals()
    {
        Color activeColor = new Color(0.7f, 0.8f, 1f);
        Color inactiveColor = Color.white;


        walkButton.image.color = (activeMode == "walk") ? activeColor : inactiveColor;
        driveButton.image.color = (activeMode == "drive") ? activeColor : inactiveColor;
        deleteButton.image.color = (activeMode == "delete") ? activeColor : inactiveColor;
    }


    public void OnDeleteAllButtonClicked()
    {
        routeDrawingManager.DeleteAllUserLines();
    }


}
