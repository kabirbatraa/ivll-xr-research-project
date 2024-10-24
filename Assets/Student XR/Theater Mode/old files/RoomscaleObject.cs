using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomscaleObject : MonoBehaviour
{
    public MeshRenderer passthroughMesh;
    Material defaultMaterial;
    // public Material darkRoomMaterial;
    // public Vector3 dimensions = Vector3.one;
    public bool passthroughWallActive = true;

    private OVRPassthroughLayer passthroughLayer;


    private void Start() {

        // GameObject ovrCameraRig = GameObject.Find("OVRCameraRig");
        // passthroughLayer = ovrCameraRig.GetComponent<OVRPassthroughLayer>();    

        // passthroughLayer.AddSurfaceGeometry(passthroughMesh.gameObject, true);
        // Debug.Log("ROOMSCALE OBJECT");


        defaultMaterial = passthroughMesh.material;
    }

    private void Update() {
        passthroughMesh.material.SetFloat("_InvertedMask", passthroughWallActive ? 1.0f : 0.0f);
    }

    // public void ShowDarkRoomMaterial(bool showDarkRoom)
    // {
    //     passthroughMesh.material = showDarkRoom ? darkRoomMaterial : defaultMaterial;
    // }

    public MeshRenderer GetMeshRenderer() {
        return passthroughMesh;
    }
}
