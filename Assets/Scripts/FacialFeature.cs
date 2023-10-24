using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

public class FacialFeature : MonoBehaviour
{
    [SerializeField] ARFaceManager faceManager;
    [SerializeField] List<GameObject> featureIndicators = new List<GameObject>();
    int regionPosNum = 3;
    ARCoreFaceSubsystem faceSubsystem;

    NativeArray<ARCoreFaceRegionData> faceData;

    [SerializeField] List<GameObject> everyfeaturesIndicators = new List<GameObject>();
    int everyPosNum = 468;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < regionPosNum; i++)
        {
            GameObject Indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Indicator.transform.localScale = Vector3.one * 0.02f;
            featureIndicators.Add(Indicator);
            Indicator.SetActive(false);
        }

        //faceManager.facesChanged += OnLocateIndicatorsOnRegionPoses;

        faceSubsystem = (ARCoreFaceSubsystem)faceManager.subsystem;


        for (int i = 0; i < everyfeaturesIndicators.Count; i++)
        {
            GameObject Indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Indicator.transform.localScale = Vector3.one * 0.005f;
            everyfeaturesIndicators.Add(Indicator);
            Indicator.SetActive(false);
        }
        faceManager.facesChanged += OnLocateIndicatorsOnEveryFeatures;
    }

    void OnLocateIndicatorsOnRegionPoses(ARFacesChangedEventArgs args)
    {
        if (args.updated.Count > 0)
        {
            faceSubsystem.GetRegionPoses(args.updated[0].trackableId,Unity.Collections.Allocator.Persistent,ref faceData);

            for(int i = 0; i < faceData.Length; i++)
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

    void OnLocateIndicatorsOnEveryFeatures(ARFacesChangedEventArgs args)
    {
        if (args.updated.Count > 0)
        {
            for (int i = 0; i < args.updated[0].vertices.Length; i++)
            {
                Vector3 vertPos = args.updated[0].vertices[i];
                Vector3 worldVertPos = args.updated[0].transform.TransformPoint(vertPos);

                everyfeaturesIndicators[i].transform.position = worldVertPos;
                everyfeaturesIndicators[i].SetActive(false);
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
}
