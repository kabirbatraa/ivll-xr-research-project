using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

using PhotonPun = Photon.Pun;
using PhotonRealtime = Photon.Realtime;
using System;

public class StreamTheaterModeData : MonoBehaviour, IPunObservable
{

    // singleton structure for easy access
    public static StreamTheaterModeData Instance;
    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }


    

    [NonSerialized]
    public int currentSkyboxIndex = 0;

    public List<Material> skyboxList = new List<Material>();

    // go to next skybox
    public static void ChangeSkybox() {
        Debug.Log("going to next");
        Instance.currentSkyboxIndex++;
        if (Instance.currentSkyboxIndex >= Instance.skyboxList.Count) Instance.currentSkyboxIndex = 0;
    }

    // go to previous skybox
    public static void PreviousSkybox() {
        Debug.Log("going to previous skybox");
        Instance.currentSkyboxIndex--;
        if (Instance.currentSkyboxIndex < 0) Instance.currentSkyboxIndex = Instance.skyboxList.Count-1;
    }

    public static Material GetSkyboxMaterial() {
        if (Instance.skyboxList.Count == 0) {
            Debug.Log("ERROR: skyboxList is empty!");
            return null;
        }
        else {
            return Instance.skyboxList[Instance.currentSkyboxIndex];
        }
    }


    // static functions to call the functions on the instance
    // will also instantiate the instance as a new photon object if it doesnt already exist
    public static void ToggleTheaterMode() {
        if (Instance != null) {
            Instance.ToggleTheaterModeOnInstance();
        }
        else {
            GameObject newStreamerObject = PhotonNetwork.Instantiate("StreamTheaterModeData", Vector3.zero, Quaternion.identity);
            newStreamerObject.GetComponent<StreamTheaterModeData>().ToggleTheaterModeOnInstance();
        }

    }

    public static void ResetTheaterMode() {
        if (Instance != null) {
            Instance.ResetTheaterModeOnInstance();
        }
        else {
            GameObject newStreamerObject = PhotonNetwork.Instantiate("StreamTheaterModeData", Vector3.zero, Quaternion.identity);
            newStreamerObject.GetComponent<StreamTheaterModeData>().ToggleTheaterModeOnInstance();
        }
    }



    private PhotonView photonView;


    // variables to manage how much the ceiling has opened and how much the walls have dropped
    // these variables are updated by button presses on the instructor side, or they are set by OnPhotonSerializeView
    [NonSerialized]
    public float wallLoweredPercentage = 0.0f;
    [NonSerialized]
    public float ceilingRemovedPercentage = 0.0f;

    // ceilingVisible will be a custom server property since it doesn't have to be streamed constantly
    [NonSerialized]
    public bool ceilingVisible = true;


    private bool theaterModePaused = false;

    private Coroutine latestCoroutine = null;



    // functions for instructor gui to easily control the walls and ceiling dropping
    private bool hasBeenReset = true; // reset by default
    private void ToggleTheaterModeOnInstance() {
        TakeOwnershipOfStreamer();
        
        if (hasBeenReset) {
            // safely assume that everything is 0, can simply start theater mode
            TriggerTheaterMode();

            // dont forget to set hasBeenReset
            hasBeenReset = false;
            return;
        }

        // otherwise, its either paused or playing (or finished)
        // can just flip the paused state
        theaterModePaused = !theaterModePaused;


    }

    // reset function
    private void ResetTheaterModeOnInstance() {
        TakeOwnershipOfStreamer();

        // stop the coroutines
        if (latestCoroutine != null)
            StopCoroutine(latestCoroutine);
        
        // make sure to unpause
        theaterModePaused = false;
        
        // reset the values

        ceilingVisible = true;
        ceilingRemovedPercentage = 0.0f;
        wallLoweredPercentage = 0.0f;

        hasBeenReset = true;
        
    }

    private void TakeOwnershipOfStreamer() {
        gameObject.GetPhotonView().TransferOwnership(PhotonNetwork.LocalPlayer);
    }

    // // pause function
    // private void PauseTheaterMode() {
    //     theaterModePaused = true;
    // }

    // // continue function
    // private void ContinueTheaterMode() {
    //     theaterModePaused = false;
    // }

    

    // start the open theater procedure
    private void TriggerTheaterMode() {
        ResetTheaterMode();
        latestCoroutine = StartCoroutine(RemoveCeiling());
        // if paused, ResetTheaterMode will make sure to unpause
    }

    // coroutines for opening the theater mode
    IEnumerator RemoveCeiling() {

        // start position of walls are stored in originalWalls list
        float timeElapsed = 0;
        float moveDuration = 4.0f;

        while (timeElapsed < moveDuration) {

            if (theaterModePaused) {
                // then skip to the next frame instead of continuing the animation
                yield return null;
                continue;
            }

            ceilingRemovedPercentage = timeElapsed / moveDuration;

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        ceilingVisible = false;
        latestCoroutine = StartCoroutine(LowerTheWalls());
    }

    IEnumerator LowerTheWalls() {

        // start position of walls are stored in originalWalls list
        float timeElapsed = 0;
        float moveDuration = 8.0f;

        while (timeElapsed < moveDuration) {

            if (theaterModePaused) {
                // then skip to the next frame instead of continuing the animation
                yield return null;
                continue;
            }

            wallLoweredPercentage = timeElapsed / moveDuration;

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        latestCoroutine = null;
    }




    // public GameObject head;

    // get the photon view of the gameobject
    private void Start() {
        photonView = GetComponent<PhotonView>();
        // photonView.isMine tells us if this object was instantiated locally or not
        // use this to not render the cube if it is of the self

        // if (photonView.IsMine) {
        //     // foreach (Transform child in transform) {
        //     //     child.gameObject.SetActive(false);
        //     // }
        //     head.SetActive(false);
        // }
    }

    private void Update() {

        // if (photonView.IsMine) {

        //     Transform localHead = UserHeadPositionTrackerManager.Instance.localHeadTransform;

        //     gameObject.transform.position = localHead.position;
        //     gameObject.transform.eulerAngles = localHead.eulerAngles;

        // }


        // update the custom property based variables (read and write)
        if (photonView.IsMine) {
            // this is the instructor, so update the bool value

            string key = "ceilingVisible";
            bool value = ceilingVisible;

            var newCustomProperty = new ExitGames.Client.Photon.Hashtable { { key, value } };
            // update on server
            PhotonNetwork.CurrentRoom.SetCustomProperties(newCustomProperty);

            // update locally because server will update local cached hashmap with delay
            PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties[key] = value;


            
            key = "currentSkyboxIndex";
            int valueInt = currentSkyboxIndex;

            newCustomProperty = new ExitGames.Client.Photon.Hashtable { { key, valueInt } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(newCustomProperty);
            PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties[key] = valueInt;
        }
        else {
            // this is a student, so read the bool value
            string key = "ceilingVisible";

            // if the custom property doesnt exist, then assume true
            if (PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key)) {
                ceilingVisible = (bool)PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties[key];
            }
            else {
                ceilingVisible = true;
            }


            key = "currentSkyboxIndex";
            // if the custom property doesnt exist, then assume 0
            if (PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key)) {
                currentSkyboxIndex = (int)PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties[key];
            }
            else {
                currentSkyboxIndex = 0;
            }

        }



    }

    // this function is called every time this object tries to update itself
    // if this object was instantiated locally (it is the local head), then it only sends data
    // otherwise, this object must have been instantiated elsewhere (it is someone elses head), so it recieves data
    // streaming happens many times per second (very fast)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

        // Debug.Log("Stream Theater Mode Data's OnPhotonSerializeView");

        // if writing, then this object was instantiated locally (it is the local head)
        if (stream.IsWriting) {

            // Debug.Log("writing values for wallLoweredPercentage etc");


            // write the head data of the head transform located in the UserHeadPositionTrackerManager
                // this script has only 1 instance, and we passed in the "center eye anchor" aka the position of the head

            // Transform localHead = UserHeadPositionTrackerManager.Instance.localHeadTransform;

            // stream.SendNext(localHead.position);
            // stream.SendNext(localHead.eulerAngles);


            stream.SendNext(wallLoweredPercentage);
            stream.SendNext(ceilingRemovedPercentage);

            // Debug.Log($"writing wallLoweredPercentage: {wallLoweredPercentage}");
            // Debug.Log($"writing ceilingRemovedPercentage: {ceilingRemovedPercentage}");

        }

        // if reading, this object must have been instantiated elsewhere (it is someone elses head)
        else {


            // by recieving data on someone elses head, we can move around this current prefab itself
                // if this prefab has a child object with an actual mesh, then we will see that mesh correspond to the head
                // of another user
            
            // gameObject.transform.position = (Vector3)stream.ReceiveNext();
            // gameObject.transform.eulerAngles = (Vector3)stream.ReceiveNext();

            wallLoweredPercentage = (float)stream.ReceiveNext();
            ceilingRemovedPercentage = (float)stream.ReceiveNext();
            Debug.Log($"reading wallLoweredPercentage: {wallLoweredPercentage}");
            Debug.Log($"reading ceilingRemovedPercentage: {ceilingRemovedPercentage}");

        }

    }
}
