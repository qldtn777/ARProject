using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

// 리전 포지션을 찾아서 그 위치에 오브젝트를 위치시킨다.
public class FacialFeature : MonoBehaviour
{
    [SerializeField] ARFaceManager faceManager;
    [SerializeField] List<GameObject> featureIndicators = new List<GameObject>();
    [SerializeField] List<GameObject> everyfeaturesIndicators = new List<GameObject>();
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    int regionPosCnt = 3;
    int everyPosCnt = 468;
    ARCoreFaceSubsystem faceSubsystem;
    NativeArray<ARCoreFaceRegionData> faceData;
    int currentState = 0;
    int maxState = 2;
    
    void Start()
    {
        for(int i = 0; i < regionPosCnt; i++)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.localScale = Vector3.one * 0.02f;
            featureIndicators.Add(indicator);
            indicator.SetActive(false);
        }

        for(int i = 0; i < everyPosCnt; i++)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.localScale = Vector3.one * 0.005f;
            everyfeaturesIndicators.Add(indicator);
            indicator.SetActive(false);
        }

        faceSubsystem = (ARCoreFaceSubsystem)faceManager.subsystem;
        
        faceManager.facesChanged += OnLocateIndicatorsOnRegionPoses; // Region pose 전용
        faceManager.facesChanged += OnLocateIndicatorsOnEveryFeatures; // Total pose 전용

        currentState = maxState;
    }


    public void OnChangeStateBtnClkEvent()
    {
        currentState++;

        if (currentState > maxState)
        {
            SetText("Normal State");

            currentState = 0;
        }
    }

    void SetText(string state)
    {
        textMeshProUGUI.text = state;
    }


    void OnLocateIndicatorsOnRegionPoses(ARFacesChangedEventArgs args)
    {
        if (currentState == 1)
        {
            SetText("Region poses");

            // 업데이트 된 내용이 있다면 특징점에 Sphere를 위치시킨다.
            if (args.updated.Count > 0)
            {
                faceSubsystem.GetRegionPoses(args.updated[0].trackableId, Allocator.Persistent, ref faceData);

                for (int i = 0; i < faceData.Length; i++)
                {
                    featureIndicators[i].transform.position = faceData[i].pose.position;
                    featureIndicators[i].transform.rotation = faceData[i].pose.rotation;
                    featureIndicators[i].SetActive(true);
                }
            }
            else if (args.removed.Count > 0)
            {
                for (int i = 0; i < faceData.Length; i++)
                {
                    featureIndicators[i].SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < faceData.Length; i++)
            {
                featureIndicators[i].SetActive(false);
            }
        }

    }

    void OnLocateIndicatorsOnEveryFeatures(ARFacesChangedEventArgs args)
    {
        if(currentState == 2)
        {
            SetText("Total poses");

            if (args.updated.Count > 0)
            {
                for (int i = 0; i < args.updated[0].vertices.Length; i++)
                {
                    Vector3 vertPos = args.updated[0].vertices[i];
                    Vector3 worldVertPos = args.updated[0].transform.TransformPoint(vertPos);

                    everyfeaturesIndicators[i].transform.position = worldVertPos;
                    everyfeaturesIndicators[i].SetActive(true);
                }
            }
            else if (args.removed.Count > 0)
            {
                for (int i = 0; i < args.updated[0].vertices.Length; i++)
                {
                    everyfeaturesIndicators[i].SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < args.updated[0].vertices.Length; i++)
            {
                everyfeaturesIndicators[i].SetActive(false);
            }
        }
    }
}
