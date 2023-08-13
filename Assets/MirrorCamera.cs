using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MirrorCamera : MonoBehaviour
{
    public RenderTexture RT;
    public RenderTexture targetTexture;
    public Camera mainCamera;
    Plane mirrorPlane;
    Camera newCamera;


    void Start()
    {
        mirrorPlane = new  Plane(transform.up, transform.position);

        var planeNormal = transform.up.normalized;
        var cameraFace = mainCamera.transform.forward.normalized;
        var cameraPos = mainCamera.transform.position;

        var cameraDistance = mirrorPlane.GetDistanceToPoint(cameraPos);
        var mirrorCameraPos = cameraPos - 2 * cameraDistance * planeNormal;

        //logic start from here.
        var mirrorCamera = new GameObject("MirrorCamera");
        mirrorCamera.transform.position = mirrorCameraPos;
        newCamera = mirrorCamera.AddComponent<Camera>();
        newCamera.CopyFrom(mainCamera);

        var angle = Vector3.Dot(cameraFace, planeNormal);
        var touchPoint = GetIntersectWithLineAndPlane(cameraPos, cameraFace, transform.position, planeNormal);

        /*if (angle == 0)
            mirrorCamera.transform.forward = cameraFace;

        else if (angle < 0)
            mirrorCamera.transform.forward = (touchPoint - mirrorCameraPos).normalized;

        else if(angle > 0)
            mirrorCamera.transform.forward = (mirrorCameraPos - touchPoint).normalized;
        */
        //targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        //targetTexture.format = RenderTextureFormat.ARGB32;
        Shader.SetGlobalTexture("_MirrorTexture", targetTexture);

        newCamera.targetTexture = targetTexture;
        //newCamera.enabled = true;
    }

    Vector3 GetIntersectWithLineAndPlane(Vector3 linePoint, Vector3 lineDirect, Vector3 planePoint, Vector3 planeNormal)
    {
        lineDirect.Normalize();
        planeNormal.Normalize();

        var distance = Vector3.Dot(planePoint - linePoint, planeNormal) / Vector3.Dot(lineDirect, planeNormal);
        return distance * lineDirect + linePoint;
    }

    private void OnWillRenderObject()
    {
        //return;
        /*newCamera.clearFlags = mainCamera.clearFlags;
        newCamera.backgroundColor = mainCamera.backgroundColor;
        newCamera.fieldOfView = mainCamera.farClipPlane;
        newCamera.nearClipPlane = mainCamera.nearClipPlane;
        newCamera.fieldOfView = mainCamera.fieldOfView;
        newCamera.aspect = mainCamera.aspect;
        newCamera.orthographicSize = mainCamera.orthographicSize;*/
        newCamera.cullingMask = LayerMask.GetMask("reflection"); //must have different layer with main camera

        var reflectMatrix = CalculateReflectMatrix(transform.up, transform.position - transform.up * 0.001f);
        newCamera.worldToCameraMatrix = mainCamera.worldToCameraMatrix * reflectMatrix;  //use mainCamera worldToCameraMatrix to render a flipped world

        newCamera.projectionMatrix = mainCamera.projectionMatrix;
        var plane = CameraSpacePlane(mainCamera.worldToCameraMatrix, transform.position - transform.up * 0.001f, -transform.up); //-transform.up!!!!!!
        newCamera.projectionMatrix = newCamera.CalculateObliqueMatrix(plane);

        GL.invertCulling = true;
        newCamera.Render();
        GL.invertCulling = false;
    }

    // Update is called once per frame
    void Update()
    {
        return;
        //newCamera.CopyFrom(mainCamera);
        newCamera.clearFlags = mainCamera.clearFlags;
        newCamera.backgroundColor = mainCamera.backgroundColor;
        newCamera.fieldOfView = mainCamera.farClipPlane;
        newCamera.nearClipPlane = mainCamera.nearClipPlane;
        newCamera.fieldOfView = mainCamera.fieldOfView;
        newCamera.aspect = mainCamera.aspect;
        newCamera.orthographicSize = mainCamera.orthographicSize;
        newCamera.cullingMask = mainCamera.cullingMask;

        var reflectMatrix = CalculateReflectMatrix(transform.up, transform.position - transform.up * 0.001f);
        newCamera.worldToCameraMatrix = mainCamera.worldToCameraMatrix * reflectMatrix;  //use mainCamera worldToCameraMatrix to render a flipped world

        newCamera.projectionMatrix = mainCamera.projectionMatrix;
        var plane = CameraSpacePlane(mainCamera.worldToCameraMatrix, transform.position - transform.up * 0.001f, -transform.up); //-transform.up!!!!!!
        newCamera.projectionMatrix = newCamera.CalculateObliqueMatrix(plane);

        GL.invertCulling = true;
        newCamera.Render();
        GL.invertCulling = false;
    }

    Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal)
    {
        Vector3 viewPos = worldToCameraMatrix.MultiplyPoint(pos);
        Vector3 viewNormal = worldToCameraMatrix.MultiplyVector(normal).normalized;
        float w = -Vector3.Dot(viewPos, viewNormal);
        return new Vector4(viewNormal.x, viewNormal.y, viewNormal.z, w);
    }

    Matrix4x4 CalculateReflectMatrix(Vector3 normal, Vector3 positionOnPlane)
    {
        var d = -Vector3.Dot(normal, positionOnPlane);
        var reflectM = new Matrix4x4();
        reflectM.m00 = 1 - 2 * normal.x * normal.x;
        reflectM.m01 = -2 * normal.x * normal.y;
        reflectM.m02 = -2 * normal.x * normal.z;
        reflectM.m03 = -2 * d * normal.x;

        reflectM.m10 = -2 * normal.x * normal.y;
        reflectM.m11 = 1 - 2 * normal.y * normal.y;
        reflectM.m12 = -2 * normal.y * normal.z;
        reflectM.m13 = -2 * d * normal.y;

        reflectM.m20 = -2 * normal.x * normal.z;
        reflectM.m21 = -2 * normal.y * normal.z;
        reflectM.m22 = 1 - 2 * normal.z * normal.z;
        reflectM.m23 = -2 * d * normal.z;

        reflectM.m30 = 0;
        reflectM.m31 = 0;
        reflectM.m32 = 0;
        reflectM.m33 = 1;

        return reflectM;
    }
}
