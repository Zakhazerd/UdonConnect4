
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Connect4Button : UdonSharpBehaviour
{
    public int column;
    public Connect4 connect4;
    
    public void AddRed()
    {
        connect4.AddRed(column);
    }
    public void AddYellow()
    {
        connect4.AddYellow(column);
    }
}
