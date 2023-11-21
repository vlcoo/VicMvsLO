using System;
using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    public bool wrap;

    private GameObject[] children;
    private Vector3 lastPosition;

    private Camera mainCamera;
    private float[] ppus, halfWidths;
    private Vector2 screenBounds;
    private Vector3[] truePositions, positionsAfterPixelSnap;

    public static BackgroundLoop Instance { get; private set; }

    #region Public Methods

    public void Reposition()
    {
        for (var i = 0; i < children.Length; i++)
        {
            var obj = children[i];
            var difference = transform.position.x - lastPosition.x +
                             (obj.transform.position.x - positionsAfterPixelSnap[i].x);
            var parallaxSpeed = 1 - Mathf.Clamp01(Mathf.Abs(lastPosition.z / obj.transform.position.z));

            if (wrap)
                parallaxSpeed = 1;

            truePositions[i] += difference * parallaxSpeed * Vector3.right;
            obj.transform.position = positionsAfterPixelSnap[i] =
                PixelClamp(truePositions[i], obj.transform.lossyScale, ppus[i]);

            RepositionChildObjects(obj);
        }

        wrap = false;
        lastPosition = transform.position;
    }

    #endregion

    #region Unity Methods

    public void Start()
    {
        Instance = this;

        var t = GameObject.FindGameObjectWithTag("Backgrounds").transform;

        children = new GameObject[t.childCount];
        ppus = new float[t.childCount];
        truePositions = new Vector3[t.childCount];
        positionsAfterPixelSnap = new Vector3[t.childCount];
        halfWidths = new float[t.childCount];

        for (var i = 0; i < t.childCount; i++)
        {
            children[i] = t.GetChild(i).gameObject;
            var sr = children[i].GetComponent<SpriteRenderer>();
            ppus[i] = sr.sprite.pixelsPerUnit;
            halfWidths[i] = sr.bounds.extents.x - 0.00004f;
            positionsAfterPixelSnap[i] = truePositions[i] = children[i].transform.position;
        }

        mainCamera = gameObject.GetComponent<Camera>();
        screenBounds = new Vector2(mainCamera.orthographicSize * mainCamera.aspect, mainCamera.orthographicSize) *
                       3f; // mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
        foreach (var obj in children)
            LoadChildObjects(obj);

        lastPosition = transform.position;
    }

    public void LateUpdate()
    {
        Reposition();
    }

    #endregion

    #region Helper Methods

    private void LoadChildObjects(GameObject obj)
    {
        var objectWidth = halfWidths[Array.IndexOf(children, obj)] * 2f;
        var childsNeeded = (int)Mathf.Ceil(screenBounds.x / objectWidth) + 1;
        var clone = Instantiate(obj);
        for (var i = 0; i <= childsNeeded; i++)
        {
            var c = Instantiate(clone);
            c.transform.SetParent(obj.transform);
            c.transform.position = new Vector3(objectWidth * i, obj.transform.position.y, obj.transform.position.z);
            c.name = obj.name + i;
        }

        Destroy(clone);
        Destroy(obj.GetComponent<SpriteRenderer>());
    }

    private void RepositionChildObjects(GameObject obj)
    {
        if (!obj)
            return;

        var parent = obj.transform;
        if (parent.childCount > 1)
        {
            var firstChild = parent.GetChild(0).gameObject;
            var lastChild = parent.GetChild(parent.childCount - 1).gameObject;
            var halfObjectWidth = halfWidths[Array.IndexOf(children, obj)];
            if (transform.position.x + screenBounds.x > lastChild.transform.position.x + halfObjectWidth)
            {
                firstChild.transform.SetAsLastSibling();
                firstChild.transform.position = new Vector3(lastChild.transform.position.x + halfObjectWidth * 2,
                    lastChild.transform.position.y, lastChild.transform.position.z);
            }
            else if (transform.position.x - screenBounds.x < firstChild.transform.position.x - halfObjectWidth)
            {
                lastChild.transform.SetAsFirstSibling();
                lastChild.transform.position = new Vector3(firstChild.transform.position.x - halfObjectWidth * 2,
                    firstChild.transform.position.y, firstChild.transform.position.z);
            }
        }
    }

    private static Vector3 PixelClamp(Vector3 pos, Vector3 scale, float pixelsPerUnit)
    {
        if (!Settings.Instance.ndsResolution)
            return pos;

        pos *= pixelsPerUnit;
        pos = pos.Divide(scale);

        pos.x = Mathf.CeilToInt(pos.x);
        pos.y = Mathf.CeilToInt(pos.y);
        pos.z = Mathf.CeilToInt(pos.z);

        pos /= pixelsPerUnit;
        pos = pos.Multiply(scale);

        return pos;
    }

    #endregion
}