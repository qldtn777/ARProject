//#define UNITY_EDITOR
#define UNITY_ANDROID

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// 목적: ARRay를 발사하여 검출된 Plane의 정보를 받아 그 위치에 Indicator를 위치시킨다.
// 필요속성: Indicator GameObject
// 목적2: 터치입력이 들어오면 Cyberpunk car를 감지된 바닥 위에 위치시킨다.
// 필요속성2: Cyberpunk car
public class ObjectManager : MonoBehaviour
{

    // 필요속성: Indicator GameObject
    public GameObject indicator;
    ARRaycastManager raycastManager;

    // 필요속성2: Cyberpunk car
    public GameObject displayObject;
    public GameObject editorPlane;

    // Start is called before the first frame update
    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        displayObject.SetActive(false);
#if UNITY_EDITOR
        editorPlane.SetActive(true);
#elif UNITY_ANDROID
        editorPlane.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        DetectPlane();
    }

    // 목적: ARRay를 발사하여 검출된 Plane의 정보를 받아 그 위치에 Indicator를 위치시킨다.
    void DetectPlane()
    {

#if UNITY_EDITOR
        if(Input.GetMouseButton(0))
        {
            // 내 스크린 스페이스의 클릭된 지점으로 부터 레이를 발사한다.
            // 1. 스크린을 터치한 좌표
            Vector3 touchPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
            // 2. 스크린의 터치한 좌표를 World Point로
            Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(touchPos);
            // 3. 방향을 설정
            Vector3 direction = (touchWorldPos - transform.position).normalized;
            // 4. 해당 방향으로 레이를 쏜다.
            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(transform.position, direction * 100, Color.red, 0.5f);
                // 5. 레이에 충돌한 오브젝트가 바닥이라면, 바닥의 특정 지점에 displayObject를 위치시킨다.
                if(hit.collider.name.Contains("Plane"))
                {
                    Debug.DrawRay(transform.position, direction * 100, Color.green, 0.5f);

                    displayObject.transform.position = hit.point;
                    displayObject.transform.rotation = hit.transform.rotation;
                    displayObject.SetActive(true);
                }
                else
                {
                    displayObject.SetActive(false);
                }
            }
        }

#elif UNITY_ANDROID
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

        TouchScreen();
#endif
    }

    // 목적2: 터치입력이 들어오면 Cyberpunk car를 감지된 바닥 위에 위치시킨다.
    void TouchScreen()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                // indicator가 plane을 찾으면
                if(indicator.activeSelf)
                {
                    displayObject.transform.position = indicator.transform.position;
                    displayObject.transform.rotation = indicator.transform.rotation;
                    displayObject.SetActive(true);
                }
            }
        }
    }
}
