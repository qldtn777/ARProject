using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// 목적: ARRay를 발사하여 검출된 Plane의 정보를 받아 그 위치에 Indicator를 위치시킨다.
// 필요속성: Indicator GameObject
public class ObjectManager : MonoBehaviour
{

    // 필요속성: Indicator GameObject
    public GameObject indicator;
    ARRaycastManager raycastManager;

    // Start is called before the first frame update
    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        DetectPlane();
    }

    // 목적: ARRay를 발사하여 검출된 Plane의 정보를 받아 그 위치에 Indicator를 위치시킨다.
    void DetectPlane()
    {
        // 스크린 중앙의 위치
        Vector2 screenCenter = new Vector2 (Screen.width * 0.5f, Screen.height * 0.5f);

        // 충돌한 레이의 정보를 담는 변수
        List<ARRaycastHit> hitInfo = new List<ARRaycastHit>();

        // 레이를 발사하여 부딫힌 플레인의 정보를 hitInfo에 저장
        if(raycastManager.Raycast(screenCenter, hitInfo, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
        {
            indicator.SetActive(true);
            indicator.transform.position = hitInfo[0].pose.position;
            indicator.transform.rotation = hitInfo[0].pose.rotation;
        }
        else
        {
            indicator.SetActive(false);
        }

    }
}
