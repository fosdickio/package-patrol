using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionalIndicatorGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject samplePackageIndicator, samplePadIndicator;
    void Start()
    {
        samplePackageIndicator= this.transform.Find("IndicatorSample").gameObject;
        samplePackageIndicator.gameObject.SetActive(false);
        samplePadIndicator = Instantiate(samplePackageIndicator, samplePackageIndicator.transform.position, samplePackageIndicator.transform.rotation);
        samplePadIndicator.transform.Find("Pointer").GetComponent<Image>().rectTransform.position = new Vector3(samplePadIndicator.transform.Find("Pointer").GetComponent<Image>().rectTransform.position.x,
            30, samplePadIndicator.transform.Find("Pointer").GetComponent<Image>().rectTransform.position.z);
        samplePadIndicator.transform.Find("Pointer").GetComponent<Image>().sprite= this.transform.Find("indicatorPad").GetComponent<SpriteRenderer>().sprite;
        samplePadIndicator.transform.SetParent(transform, true); // parent = this.transform;
    }
    public static Vector3 WorldToScreenSpace(Vector3 worldPos, Camera cam, RectTransform area)
    {
        Vector3 screenPoint = cam.WorldToScreenPoint(worldPos);
        screenPoint.z = 0;

        Vector2 screenPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPoint, cam, out screenPos))
        {
            return screenPos;
        }

        return screenPoint;
    }
    //List<GameObject> indicators= new List<GameObject>();

    // Update is called once per frame
    public static Vector2 WorldToScreenPointProjected(Camera camera, Vector3 worldPos)
    {
        Vector3 camNormal = camera.transform.forward;
        Vector3 vectorFromCam = worldPos - camera.transform.position;
        float camNormDot = Vector3.Dot(camNormal, vectorFromCam);
        if (camNormDot <= 0)
        {
            // we are behind the camera forward facing plane, project the position in front of the plane
            Vector3 proj = (camNormal * camNormDot * 1.01f);
            worldPos = camera.transform.position + (vectorFromCam - proj);
        }

        return RectTransformUtility.WorldToScreenPoint(camera, worldPos);
    }

    Dictionary<GameObject, GameObject> indicators = new Dictionary<GameObject, GameObject>();

    public void clear()
    {
        foreach(var ind in indicators)
        {
            Destroy(ind.Value);
        }
        indicators = null;
        indicators = new Dictionary<GameObject, GameObject>();
        packages = null;
    }

    /*
    Vector2 x;
    GameObject it;
    */
    void setupIndicators(GameObject[] items, GameObject indicatorToClone, Camera cam, Transform player, bool rotateSprite = true)
    {
        var playerCamPos = WorldToScreenPointProjected(cam, player.position);//(Vector2)cam.WorldToScreenPoint(player.position);

        if (Math.Abs(playerCamPos.x) > 200 || Math.Abs(playerCamPos.y) > 200)
        {
            return;
        }

        foreach (var pkg in items)
        {
            if (pkg == null) { return;  }
            if (!pkg.activeInHierarchy)
            {
                if (indicators.ContainsKey(pkg) && indicators[pkg])
                {
                    indicators[pkg].SetActive(false);
                }
                continue;

            }
            var pkgCamPos = WorldToScreenPointProjected(cam, pkg.transform.position);//cam.WorldToScreenPoint(pkg.transform.position);

            /*
            if (!it)
            {
                it = pkg;
            }
            if(it == pkg)
            {
                x = pkgCamPos;
            }
*/
            var vectorToTarget = playerCamPos - pkgCamPos;


            //var pointPos = WorldToScreenSpace(pkg.transform.position, cam, samplePackageIndicator.GetComponent<RectTransform>());
            GameObject newIndicator;

            if (indicators.ContainsKey(pkg))
            {
                newIndicator = indicators[pkg];
                if (!newIndicator)
                {
                    continue;
                }
                //newIndicator.transform.position = indicatorToClone.transform.position;
                //newIndicator.transform.rotation = indicatorToClone.transform.rotation;
                newIndicator.transform.SetParent(transform, true); // parent = this.transform;
                newIndicator.gameObject.SetActive(true);
            }
            else { 

                newIndicator = Instantiate(indicatorToClone, indicatorToClone.transform.position, indicatorToClone.transform.rotation);
                newIndicator.tag = "indicatorPkg";
                indicators[pkg] = newIndicator;
                var img2 = newIndicator.transform.Find("Pointer").GetComponent<Image>();
                img2.color = pkg.GetComponent<Renderer>().material.color;
            }

            if (pkgCamPos.x > 0 && pkgCamPos.y > 0 && pkgCamPos.x < cam.scaledPixelWidth && pkgCamPos.y < cam.scaledPixelHeight)
            {
                //item is already on the screen
                indicators[pkg].SetActive(false);
                continue;
            }
            else
            {

                indicators[pkg].SetActive(true);
            }
            
            if (vectorToTarget.magnitude < 1000)
            {
                indicators[pkg].SetActive(false);
                continue;
            }


            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            angle -= 90;

            var img = newIndicator.transform.Find("Pointer").GetComponent<Image>();
            var imgParent = indicatorToClone.transform.Find("Pointer").GetComponent<Image>();

            if (!rotateSprite)
            {
                img.transform.rotation = imgParent.transform.rotation;
            }


            float proportion = vectorToTarget.magnitude/ (float)Math.Pow(Math.Pow(cam.scaledPixelHeight,2)+ Math.Pow(cam.scaledPixelWidth, 2),0.5);
            if (proportion > 1)
            {
                proportion = 1;
            }
            else if(proportion < 0.5)
            {
                proportion = 0.5f;
            }
            img.rectTransform.sizeDelta = new Vector2(imgParent.rectTransform.sizeDelta.x* proportion, imgParent.rectTransform.sizeDelta.y* proportion);

            Quaternion newRotation = Quaternion.AngleAxis(angle, Vector3.forward);


            newIndicator.transform.rotation = newRotation;

            //break;
        }
    }

    static int updateCounter = 0;

    string activeCamName = "vcam_Player";
    GameObject[] packages = null;
    void Update()
    {
        if(LevelManager.get()!=null && LevelManager.get().isTutorial)
        {
            return;
        }

        if (packages == null || packages.Length == 0)
        {

            //get all packages, if any
            packages = GameObject.FindGameObjectsWithTag("Package");
        }

        if (activeCamName != GameManagerScript.get().getActiveCameraName())
        {
            activeCamName = GameManagerScript.get().getActiveCameraName();
            // clearIndicators * ()
            LevelManager.get().clearIndicators();
            clear();
        }

        var cam = GameObject.Find("Main Camera").GetComponent<Camera>();



        var player = GameObject.Find(activeCamName).transform;
        setupIndicators(packages, samplePackageIndicator, cam, player);
        //do the same for delivery pads
        var devContainer = GameObject.Find("Delivery Pads").transform;

        GameObject[] devPads = new GameObject[devContainer.childCount];

        int i = 0;
        foreach (Transform padTrans in devContainer)
        {
            if (padTrans.parent == devContainer)
            {
                devPads[i++]=padTrans.gameObject;
            }
        }
        Array.Resize(ref devPads, i);
        setupIndicators(devPads, samplePadIndicator, cam, player, false);

    }

}
