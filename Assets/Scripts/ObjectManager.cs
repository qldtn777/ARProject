//#define UNITY_EDITOR
#define UNITY_ANDROID
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

// 목적: ARRay를 발사하여 검출된 Plane의 정보를 받아 그 위치에 Indicator를 위치시킨다.
// 필요속성: Indicator GameObject
// 목적2: 터치입력이 들어오면 Cyberpunk car를 감지된 바닥 위에 위치시킨다.
// 필요속성2: Cyberpunk car
// 목적3: 클릭(터치) 상태라면 오브젝트를 변화량 만큼 Y축으로 회전시킨다.
// 필요속성3: 마우스 이동 변화량 벡터
// 목적4: 포켓폴을 던지고 싶다.
public class ObjectManager : MonoBehaviour
{

    // 필요속성: Indicator GameObject
    public GameObject indicator;
    ARRaycastManager raycastManager;

    // 필요속성2: Cyberpunk car
    public GameObject displayObject;
    public GameObject editorPlane;

    // 필요속성3: 마우스 이전 프레임 벡터, 마우스 이동 변화량 벡터, 회전 속도 조절 변수
    Vector3 prevPos;
    Vector3 deltaPos;
    public float rotationScaleMultiplier = 0.1f;
    Vector3 startPos;
    Vector3 endPos;
    public Transform pokeball;
    public Transform pokeballOriginPos;
    Vector3 originPokeballPos;
    public float throwPowerMultiplier = 0.005f;
    public float pokeballResetTime = 3;

    public Toggle toggle;
    bool toggleFlag = false;

    // Start is called before the first frame update
    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        displayObject.SetActive(false);

        toggle.isOn = false;

#if UNITY_EDITOR
        editorPlane.SetActive(true);
#elif UNITY_ANDROID
        editorPlane.SetActive(false);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
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

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(transform.position, direction * 100, Color.red, 0.5f);
                // 5. 레이에 충돌한 오브젝트가 바닥이라면, 바닥의 특정 지점에 displayObject를 위치시킨다.

                if(!toggleFlag)
                {
                    if (hit.collider.name.Contains("Plane"))
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

            prevPos = Input.mousePosition;
        }
        // 목적3: 클릭(터치) 상태라면 오브젝트를 변화량 만큼 Y축으로 회전시킨다.
        else if (Input.GetMouseButton(0)) 
        {
            if (!toggleFlag)
            {
                deltaPos = (Input.mousePosition - prevPos);

                // y축으로 변화량 만큼 회전시킨다.
                displayObject.transform.Rotate(transform.up, -deltaPos.normalized.x * rotationScaleMultiplier);
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(transform.position, direction * 100, Color.red, 0.5f);

                if (hit.collider.name == "Pokeball" && pokeball != null)
                {
                    pokeball.transform.position = new Vector3(hit.point.x, hit.point.y, pokeball.transform.position.z);
                }
            }
        }
        // 목적4: 포켓볼을 드래그&드랍으로 던지고 싶다.
        else if (Input.GetMouseButtonUp(0))
        {
            if (pokeball == null && !toggleFlag)
                return;

            endPos = Input.mousePosition;
            Vector3 deltaPos = endPos - startPos;
            float throwPower = deltaPos.magnitude;

            // 목적4: 포켓폴을 던지고 싶다.
            pokeball.GetComponent<Rigidbody>().useGravity = true;
            pokeball.GetComponent<Rigidbody>().AddForce(direction * throwPower * throwPowerMultiplier, ForceMode.Impulse);
            Invoke("ResetPokeball", pokeballResetTime);
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

            // 내 스크린 스페이스의 클릭된 지점으로 부터 레이를 발사한다.
            // 1. 스크린을 터치한 좌표
            Vector3 touchPos = new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane);
            // 2. 스크린의 터치한 좌표를 World Point로
            Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(touchPos);
            // 3. 방향을 설정
            Vector3 direction = (touchWorldPos - Camera.main.transform.position).normalized;
            // 4. 해당 방향으로 레이를 쏜다.
            Ray ray = new Ray(Camera.main.transform.position, direction);
            RaycastHit hit;


            if (touch.phase == TouchPhase.Began)
            {
                if (!toggleFlag)
                {
                    // indicator가 plane을 찾으면
                    if (indicator.activeSelf)
                    {
                        displayObject.transform.position = indicator.transform.position;
                        displayObject.transform.rotation = indicator.transform.rotation;
                        displayObject.SetActive(true);
                    }
                }

                startPos = touch.position;
            }
            // 목적3: 클릭(터치) 상태라면 오브젝트를 변화량 만큼 Y축으로 회전시킨다.
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 deltaPos = touch.deltaPosition;
                if (!toggleFlag)
                {
                    // y축으로 변화량 만큼 회전시킨다.
                    displayObject.transform.Rotate(transform.up, -deltaPos.normalized.x * rotationScaleMultiplier);
                }
                else
                {
                    if(Physics.Raycast(ray, out hit, Mathf.Infinity)) 
                    {
                        if(hit.transform.name == "Pokeball" && pokeball != null)
                        {
                            pokeball.transform.position = new Vector3(hit.point.x, hit.point.y, pokeball.transform.position.z);
                        }
                    }
                }
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                if (pokeball == null || !toggleFlag)
                    return;

                endPos = touch.position;
                Vector3 deltaPos = endPos - startPos;
                float throwPower = deltaPos.magnitude;

                // 목적4: 포켓폴을 던지고 싶다.
                pokeball.GetComponent<Rigidbody>().useGravity = true;
                pokeball.GetComponent<Rigidbody>().AddForce(direction * throwPower * throwPowerMultiplier, ForceMode.Impulse);
                Invoke("ResetPokeball", pokeballResetTime);
            }
        }
    }

    void ResetPokeball()
    {
        pokeball.position = pokeballOriginPos.position;
        pokeball.rotation = pokeballOriginPos.rotation;
        pokeball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        pokeball.GetComponent<Rigidbody>().angularVelocity= Vector3.zero;
        pokeball.GetComponent<Rigidbody>().useGravity = false;
    }

    public void OnToggleBtnClk()
    {
        if(toggle.isOn)
        {
            pokeball.gameObject.SetActive(true);
            displayObject.gameObject.SetActive(false);

            toggleFlag = true;
        }
        else
        {
            displayObject.gameObject.SetActive(true);
            pokeball.gameObject.SetActive(false);

            toggleFlag = false;
        }


    }
}
