using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PhotonPun = Photon.Pun;
using PhotonRealtime = Photon.Realtime;

public class GUIManager : MonoBehaviour
{


    [SerializeField]
    private GameObject menuPanel;

    [SerializeField]
    private GameObject lobbyPanel;

    [SerializeField]
    private GameObject consolePanel;

    [SerializeField]
    private GameObject[] adminButtons;

    [SerializeField]
    private GameObject[] studentButtons;

    [SerializeField]
    private GameObject[] cameraButtons;



    // we need the instance so that we can default to StudentMode from StreamlineManager when a room is joined
    public static GUIManager Instance;
    private void Awake() {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }




    public void OnExitRoomButtonPressed() {
        SampleController.Instance.Log("OnExitRoomButtonPressed");
        ResetControlPanel();
    }


    public void ResetControlPanel() {
        // OnStudentMode(); // reset main panel to student view so that the next room we join, we are in student mode by default
        DisplayLobbyPanel();
        consolePanel.SetActive(true);
    }

    public void OnStudentMode() {
        // display only the student buttons
        SetAdminButtonsActive(false);
        SetCameraButtonsActive(false);
        SetStudentButtonsActive(true);

        // disable the Console object
        consolePanel.SetActive(false);
        SetGroupNumber(1);
    }

    public void OnAdminMode() {
        // show the admin buttons
        SetCameraButtonsActive(false);
        SetStudentButtonsActive(false);
        SetAdminButtonsActive(true);

        // enable the console object
        consolePanel.SetActive(true);
        SetGroupNumber(1);
    }

    public void OnCameraMode() {
        // only show the camera buttons
        SetStudentButtonsActive(false);
        SetAdminButtonsActive(false);
        SetCameraButtonsActive(true);

        // disable the console object
        consolePanel.SetActive(false);
        SetGroupNumber(0);
    }

    private void SetStudentButtonsActive(bool active) {
        foreach (GameObject b in studentButtons) {
            b.SetActive(active);
        }
    }
    private void SetAdminButtonsActive(bool active) {
        foreach (GameObject b in adminButtons) {
            b.SetActive(active);
        }
    }
    private void SetCameraButtonsActive(bool active) {
        foreach (GameObject b in cameraButtons) {
            b.SetActive(active);
        }
    }



    public void DisplayMenuPanel()
    {
        menuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
    }

    public void DisplayLobbyPanel()
    {
        lobbyPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    



    private void SetGroupNumber(int groupNumber) {
        PhotonRealtime.Player LocalPlayer = Photon.Pun.PhotonNetwork.LocalPlayer;
        LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "groupNumber", groupNumber } });
        // also set locally for faster updates
        LocalPlayer.CustomProperties["groupNumber"] = groupNumber;
    }


}