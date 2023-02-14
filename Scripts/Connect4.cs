
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
public class Connect4 : UdonSharpBehaviour
{
    public PlayerRed redPlayer;
    public PlayerYellow yellowPlayer;
    [UdonSynced]
    public bool inProgress = false;
    [UdonSynced]
    int[] materialArray = new int[42];
    [UdonSynced]
    bool[] activeArray = new bool[42];
    public Material redMaterial;
    public Material yellowMaterial;
    public GameObject[] pieceArray = new GameObject[42];
    public Text winText;
    private void Start()
    {
        
        if (Networking.IsOwner(gameObject))
        {
            for (int i = 0; i < materialArray.Length; i++)
            {
                materialArray[i] = 0;
                activeArray[i] = false;
            }
            RequestSerialization();
        }
        UpdateBoard();

    }
    public void StartPressed()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartGame");
    }
    public void ResetPressed()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetGame");
    }
    public void StartGame()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        redPlayer.HideStart();
        yellowPlayer.HideStart();
        if (!inProgress)
        redPlayer.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "EnableSelection"); //what if they start near the same time
        inProgress = true;
        RequestSerialization();


    }
    public void ResetGame()
    {
        inProgress = false;
        redPlayer.ResetPressed();
        yellowPlayer.ResetPressed();
        winText.text = "";
        if (Networking.IsOwner(gameObject))
        {
            for (int i = 0; i < materialArray.Length; i++)
            {
                materialArray[i] = 0;
                activeArray[i] = false;
            }
            RequestSerialization();
            UpdateBoard();
        }
    }
    public bool CheckDraw()
    {
        if (activeArray[5] == true &&
            activeArray[11] == true &&
            activeArray[17] == true &&
            activeArray[23] == true &&
            activeArray[35] == true &&
            activeArray[41] == true
            )
            return true;
        else
        return false;
    }
    public void AddRed(int column)
    {
        int arrayPosition = 0;
        bool wasValid = true;
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        for(int i = 0; i < 6; i++)
        {
            if (!activeArray[i + 6*column])
            {
                activeArray[i + 6*column] = true;
                materialArray[i + 6 * column] = 1;
                arrayPosition = i + 6 * column;
                break;
            }
            if(i == 5)
            {
                wasValid = false;
            }
        }
        if (wasValid)
        {
            if (CheckWinRed(arrayPosition))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RedWin");
                redPlayer.SelectionButtons();
                UpdateBoard();
                RequestSerialization();

            }
            else if(CheckDraw())
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GameDraw");
                redPlayer.SelectionButtons();
                UpdateBoard();
                RequestSerialization();

            }
            else
            {
                redPlayer.SelectionButtons();
                UpdateBoard();
                yellowPlayer.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "SelectionButtons");
                RequestSerialization();
            }
        }
        
    }
    public override void OnDeserialization()
    {
        UpdateBoard();
    }
    public void AddYellow(int column)
    {
        int arrayPosition = 0;
        bool wasValid = true;
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        for (int i = 0; i < 6; i++)
        {
            if (!activeArray[i + 6 * column])
            {
                activeArray[i + 6 * column] = true;
                materialArray[i + 6 * column] = 2;
                arrayPosition = i + 6 * column;

                break;
            }
            if (i == 5)
            {
                wasValid = false;
            }
        }
        if (wasValid)
        {
            if (CheckWinYellow(arrayPosition))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "YellowWin");
                yellowPlayer.SelectionButtons();
                UpdateBoard();
                RequestSerialization();


            }
            else if(CheckDraw())
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GameDraw");
                yellowPlayer.SelectionButtons();
                UpdateBoard();
                RequestSerialization();


            }
            else
            {
                yellowPlayer.SelectionButtons();
                UpdateBoard();
                redPlayer.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "SelectionButtons");
                RequestSerialization();
            }
        }
    }
    public void UpdateBoard()
    {
        for(int i = 0; i < pieceArray.Length; i++)
        {

            pieceArray[i].SetActive(activeArray[i]);
            if (materialArray[i] == 2)
            {
                pieceArray[i].GetComponent<MeshRenderer>().material = yellowMaterial;
            }
            else pieceArray[i].GetComponent<MeshRenderer>().material = redMaterial;

        }
    }
    public bool CheckWinRed(int placementPosition)
    {
        int count = 0;
        int height = placementPosition % 6;
        //check for vertical
        if(height > 2)
        {
            for(int i = 0; i <4; i++)
            {
                if (materialArray[placementPosition - i] == 1)
                    count++;
                    
            }
            if (count >= 4)
                return true;
            else
                count = 0;
        }
        //check horizontal left
        for(int i = 0; i < 4; i++)
        {
            if (placementPosition - (6 * i) >= 0 && materialArray[placementPosition - (6 * i)] == 1) //check for valid position
            {
                    count++;
            }
            else break;
        }
        //checked horizontal right skipping first piece
        for (int i = 1; i < 4; i++)
        {

            if (placementPosition + (6 * i) <= 41 && materialArray[placementPosition + (6 * i)] == 1) //check for valid position
            {
                count++;
            }
            else break;
        }
        if (count >= 4)
            return true;
        else
            count = 0;


        // Check top-left to bottom-right diagonal. 
       for(int i = 0; i < 4; i++)
        {
            int index = placementPosition - (5 * i);
            if (index >= 0 && index <= 41 && materialArray[index] == 1 && index % 6 >= height)
            {
                count++;
            }
            else break;
                
        }
       for(int i = 1; i <4;i++)
        {
            int index = placementPosition + (5 * i);
            if (index >= 0 && index <= 41 && materialArray[index] == 1 && index % 6 <= height)
            {
                count++;
            }
            else break;
        }
        if (count >= 4)
            return true;
        else
            count = 0;

        // Check bottom-left to top-right diagonal
        for (int i = 0; i < 4; i++)
        {
            int index = placementPosition - (7 * i);
            if (index >= 0 && index <= 41 && materialArray[index] == 1 && index % 6 <= height)
            {
                count++;
            }
            else break;
         }
        for (int i = 1; i < 4; i++)
        {
            int index = placementPosition + (7 * i);
            if (index >= 0 && index <= 41 && materialArray[index] == 1 && index % 6 >= height)
            {
                count++;
            }
            else break;
        }

        if (count >= 4)
            return true;
        else
        return false;

    }
    public bool CheckWinYellow(int placementPosition)
    {
        int count = 0;
        int height = placementPosition % 6;

        //check for vertical
        if (height > 2)
        {
            for (int i = 0; i < 4; i++)
            {
                if (materialArray[placementPosition - i] == 2)
                    count++;

            }
            if (count == 4)
                return true;
            else
                count = 0;
        }
        //check horizontal left
        for (int i = 0; i < 4; i++)
        {
            if (placementPosition - (6 * i) >= 0 && materialArray[placementPosition - (6 * i)] == 2) //check for valid position
            {
                count++;
            }
            else break;
        }
        //checked horizontal right skipping first piece
        for (int i = 1; i < 4; i++)
        {

            if (placementPosition + (6 * i) <= 41 && materialArray[placementPosition + (6 * i)] == 2) //check for valid position
            {
                count++;
            }
            else break;
        }
        if (count >= 4)
            return true;
        else
            count = 0;
        // Check top-left to bottom-right diagonal. 
        for (int i = 0; i < 4; i++)
        {
            int index = placementPosition - (5 * i);
            if (index >= 0 && index <= 41 && materialArray[index] == 2 && index % 6 >= height)
            {
                count++;
            }
            else break;

        }
        for (int i = 1; i < 4; i++)
        {
            int index = placementPosition + (5 * i);
            if (index >= 0 && index <= 41 && materialArray[index] == 2 && index % 6 <= height)
            {
                count++;
            }
            else break;
        }
        if (count >= 4)
            return true;
        else
            count = 0;

        // Check bottom-left to top-right diagonal
        for (int i = 0; i < 4; i++)
        {
            int index = placementPosition - (7 * i);
            if (index >= 0 && index <= 41 && materialArray[index] == 2 && index % 6 <= height)
            {
                count++;
            }
            else break;
        }
        for (int i = 1; i < 4; i++)
        {
            int index = placementPosition + (7 * i);
            if (index >= 0 && index <= 41 && materialArray[index] == 2 && index % 6 >= height)
            {
                count++;
            }
            else break;
        }

        if (count >= 4)
            return true;
        else
            return false;
    }
    
    public void RedWin()
    {
        winText.text = "RED WINS";
    }
    public void YellowWin()
    {
        winText.text = "YELLOW WINS";
    }
    public void GameDraw()
    {
        winText.text = "DRAW";
    }
}
