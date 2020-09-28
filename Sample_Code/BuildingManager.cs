using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BuildingAnimator))]

public class BuildingManager : MonoBehaviour
{
    public List<APTCamRotationExceptions> AptCamRotationExceptions;

    [SerializeField] private string buildingID;

    [SerializeField] private GameObject floorsContainer;

    [SerializeField] private AptEntityList buildingApts;

    [SerializeField] private AptController currentlySelectedApt;

    [SerializeField] private List<AptController> aptList;

    private bool isBuildingInCylinderMode { get { return BuildFloorAnimator.isBuildingInCylinderMode; } }

    private bool isBuildingInFloorMode { get { return BuildFloorAnimator.isBuildingInFloorMode; } }

    private bool isInited = false;

    private bool isBuildingActive;

    public int CurrentFloor { get { return BuildFloorAnimator.CurrentFloor; } }

    public bool IsFloorScroll { get { return BuildFloorAnimator.isFloorScroll; } }

    public bool IsFilterEnable = false;

    private IEnumerator filter;

    public Camera MainCamera;

    public BuildingAnimator BuildFloorAnimator;

    #region MonoBehavior

    private void Awake()
    {
        BuildFloorAnimator = GetComponent<BuildingAnimator>();
    }

    private void Start()
    {
        StartCoroutine(Wait());
    }

    #endregion MonoBehavior

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
        InitBuilding();
        BuildFloorAnimator.SetFloorsText();
    }

    public void InitializeAptText()
    {
        foreach (AptController apt in aptList)
        {
            apt.SetOnFloorText();
        }
    }

    public AptController CurrentlySelectedApt
    {
        get
        {
            return currentlySelectedApt;
        }

        set
        {
            currentlySelectedApt = value;
        }
    }

    private void InitBuilding()
    {
        buildingApts = GlobalDataPrivider.Props;

        if (buildingApts == null)
        {
            Debug.LogError("No APTs found");
            return;
        }

        InitBuilding(buildingApts);

        aptList = new List<AptController>(GetComponentsInChildren<AptController>());

        GlobalDataPrivider.UIManager.InitFloorScroll((int)buildingApts.MaxFloor());

        Debug.Log("apt list count " + buildingApts.Count);

        InitializeAptText();
    }

    public void DisableHighlightForAll()
    {
        for (int i = 0; i < aptList.Count; i++)
        {
            aptList[i].ClearDefaulColor();
        }
    }

    public CameraTargetController GetTargetController()
    {
        return GetComponentInChildren<CameraTargetController>();
    }

    public void HighlightWishList()
    {
        DisableHighlightForAll();

        if (GlobalDataPrivider.WishListIds != null)
        {
            foreach (var item in GlobalDataPrivider.WishListIds)
            {
                AptController apt = aptList.Find(x => x.AptEntity.bmbyPropID == item.bmbyPropID);
                apt?.FilterApts();

                Debug.Log("Highlight " + apt?.AptEntity.bmbyPropID);
            }
        }
    }

    private void InitBuilding(AptEntityList aptEntityList)
    {
        foreach (AptController aptController in aptList)
        {
            AptEntity data = aptEntityList.Find(x => x.propNum == aptController.GetAptType());
            aptController.SetupAptController(data);
        }
    }

    public void SetBuildingToCylinderMode()
    {
        if (buildingApts == null)
        {
            Debug.Log("Geting Props");
        }
        else
        {
            if (!isBuildingInCylinderMode)
            {
                if (!isInited)
                {
                    InitBuildingToCylinderMode();
                }

                BuildFloorAnimator.isBuildingInCylinderMode = true;
            }
        }
    }


    public void SetBuildingToHemisphereMode()
    {
        if (CurrentlySelectedApt != null)
        {
            CurrentlySelectedApt.DeselectApt();
        }

        BuildFloorAnimator.isBuildingInCylinderMode = false;
    }

    public void DeselectApt()
    {
        if (CurrentlySelectedApt != null)
        {
            if (!isBuildingInFloorMode)
            {
                CurrentlySelectedApt.DeselectApt();
            }
            else
            {
                CurrentlySelectedApt.DeSelectFloorMesh();
            }
        }
    }

    public void SetAptSelected(AptController aptController)
    {
        CurrentlySelectedApt = aptController;
    }

    public void SetBuildingActive(bool isActive)
    {
        isBuildingActive = isActive;
    }

    public bool IsBuildingActive()
    {
        return isBuildingActive;
    }

    private void InitBuildingToCylinderMode()
    {
        BuildFloorAnimator.EnableFloorMesh();
        isInited = true;
        BuildFloorAnimator.isBuildingInCylinderMode = true;
    }

    public void OutsideInit()
    {
        buildingApts = GlobalDataPrivider.Props;

        if (buildingApts == null)
        {
            Debug.LogError("No APTS FOUND");
        }
        else
        {
            InitBuilding(buildingApts);
        }
    }

    public void SetCylinderLayer()
    {
        List<FloorEntity> floorControllers = new List<FloorEntity>(floorsContainer.GetComponentsInChildren<FloorEntity>());

        if (floorControllers != null)
        {
            floorControllers.ForEach(x => x.gameObject.layer = 13);
        }

        List<AptController> apts = new List<AptController>(floorsContainer.GetComponentsInChildren<AptController>());

        if (apts != null)
        {
            apts.ForEach(x => x.gameObject.layer = 13);
        }
    }

    public void SelectAptByEntity(AptEntity apt, bool doRotate = false)
    {
        AptController tmp = aptList.Find(x => x.GetAptEntity().bmbyPropID == apt.bmbyPropID);

        if (tmp == null)
        {
            Debug.LogError("AptList is Empty");
            return;
        }

        if (isBuildingInCylinderMode)
        {
            if (CurrentlySelectedApt != null)
            {
                CurrentlySelectedApt.DeselectApt();
            }

            CurrentlySelectedApt = tmp;

            CurrentlySelectedApt.SelectRoomInCylinder(doRotate);
        }
        else if (isBuildingInFloorMode)
        {
            if (CurrentlySelectedApt != null)
            {
                if (CurrentlySelectedApt.GetFloor() == tmp.GetFloor())
                {
                    CurrentlySelectedApt = tmp;
                    CurrentlySelectedApt.SelectFloorMesh();
                }
            }
        }
    }

    public void SetBaseLayer()
    {
        List<FloorEntity> floorControllers = new List<FloorEntity>(floorsContainer.GetComponentsInChildren<FloorEntity>());

        if (floorControllers != null)
        {
            floorControllers.ForEach(x => x.gameObject.layer = 0);
        }
    }

    public AptEntityList GetAptList()
    {
        return buildingApts;
    }

    public AptController GetCurrentAptController()
    {
        return CurrentlySelectedApt;
    }

    public AptController GetCurrentAptController(AptEntity entity)
    {
        return aptList.Find(x => x.AptEntity.bmbyPropID == entity.bmbyPropID);
    }

    public string GetBuildingID()
    {
        return buildingID;
    }

    public bool IsBuildingInCylinderMode()
    {
        return isBuildingInCylinderMode;
    }

    public bool IsBuildingInFloorMode()
    {
        return isBuildingInFloorMode;
    }

    #region Filter

    public void FilterApts()
    {
        Debug.Log("Start Couroutine");
        FilteredData.FilterEnable = true;
        filter = StartFilter_Couroutine();
        StartCoroutine(filter);
    }

    public void StopFilterApts()
    {
        FilteredData.FilterEnable = false;

        if (filter != null)
        {
            StopCoroutine(filter);
        }
    }

    private IEnumerator StartFilter_Couroutine()
    {
        IsFilterEnable = true;

        GlobalDataPrivider.PropsFiltered.Clear();
        GlobalDataPrivider.Instance.Debug_PropsFiltered.Clear();

        foreach (AptEntity apt in buildingApts)
        {
            if (FilteredData.Instance.filterApts(apt))
            {
                GlobalDataPrivider.PropsFiltered.Add(apt);
                AptController aptc = aptList.Find(x => x.AptEntity.propNum == apt.propNum);

                if (aptc)
                {
                    aptc.IsFiltered = true;
                    GameObject aGameObject = aptc.gameObject;
                    aGameObject.GetComponent<AptController>().FilterApts();
                }
            }
        }

        yield return null;

        FilteredData.Instance.FilteredAptCount = GlobalDataPrivider.PropsFiltered.Count;
        GlobalDataPrivider.Instance.Debug_PropsFiltered = GlobalDataPrivider.PropsFiltered;

        IsFilterEnable = false;
    }

    #endregion

    #region Floor mode behavior

    public float GetFloorHeight()
    {
        return BuildFloorAnimator.GetCurFloorHeight();
    }

    public void ColapseFloor(int floor)
    {
        BuildFloorAnimator.ClearFloorsTo(floor);
    }

    public void SetBackFromFloorMode()
    {
        BuildFloorAnimator.RestoreBuilding();
    }

    public void ClearFloorsToCurrent()
    {
        if (currentlySelectedApt != null)
        {
            BuildFloorAnimator.ClearFloorsFirs(currentlySelectedApt.AptEntity.floorNum);
        }
    }

    #endregion

    public void ClearApts()
    {
        if (GlobalDataPrivider.WishListIds != null && GlobalDataPrivider.WishListIds.Count > 0)
        {
            foreach (AptEntity apt in GlobalDataPrivider.WishListIds)
            {
                AptController aptc = aptList.Find(x => x.AptEntity.propNum == apt.propNum);

                if (aptc)
                {
                    aptc.GetAptEntity().isInFavorites = false;
                }
            }
        }

        foreach (AptEntity apt in buildingApts)
        {
            AptController aptc = aptList.Find(x => x.AptEntity.propNum == apt.propNum);

            if (aptc)
            {
                GameObject aGameObject = aptc.transform.gameObject;
                aGameObject.GetComponent<AptController>().DeselectApt();
            }
        }
    }

    #region Interior anchors

    [System.Serializable]
    public class OffsetAptText
    {
        public string propNum;

        public Vector3 offset;
    }

    public List<OffsetAptText> OffsetForAptText = new List<OffsetAptText>();

    #endregion
}

[Serializable]
public class APTCamRotationExceptions
{
    public string APTType;
    public float ViewAngle;
}