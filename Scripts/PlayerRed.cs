
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;


public class PlayerRed : UdonSharpBehaviour
{
    public Text playerField;
    public Connect4 connectFour;
    [UdonSynced]
    public int playerID = -1;
    public GameObject startButton;
    public UdonBehaviour otherPlayer;
    public GameObject selectionButtons;

    private void Start()
    {
        if(playerID != -1)
        {
            UpdateName();
        }
    }
    public void OnClick()
    {
        if (!connectFour.inProgress)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            playerID = Networking.LocalPlayer.playerId;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateName");
            ShowStart();
            RequestSerialization();
        }
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (playerID == player.playerId)
        {
            playerID = -1;
            playerField.text = "";
            HideStart();
        }
        else if (player.playerId == (int)otherPlayer.GetProgramVariable("playerID") || (int)otherPlayer.GetProgramVariable("playerID") == -1)
        {
            HideStart();
        }
    }
    public void ShowStart()
    {
        if((int)otherPlayer.GetProgramVariable("playerID") > -1)
        {
            startButton.SetActive(true);
            otherPlayer.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ShowStart2");
        }
    }
    public void ShowStart2()
    {
        if ((int)otherPlayer.GetProgramVariable("playerID") > -1)
        {
            startButton.SetActive(true);
        }
    }
    public void HideStart()
    {
        startButton.SetActive(false);
    }
    public void UpdateName()
    {
        playerField.text = Networking.GetOwner(gameObject).displayName;
    }

    public void EnableSelection()
    {
        selectionButtons.SetActive(true);
    }
    public void SelectionButtons()
    {
        selectionButtons.SetActive(!selectionButtons.activeSelf);
    }
    public void ResetPressed()
    {
        playerField.text = "";
        playerID = -1;
        HideStart();
        selectionButtons.SetActive(false);
        RequestSerialization();
    }
}
