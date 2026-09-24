using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class RoomDisplayManager : MonoBehaviour
{
    [Header("Displayed Info")]
    [SerializeField] TMP_Text roomNameTMPText;
    [SerializeField] TMP_Text playerCountTMPText;
    [SerializeField] TMP_Text mapNameTMPText;

    void Awake()
    {
        if (roomNameTMPText == null)
        {
            roomNameTMPText = GameObject.Find("RoomNameText").GetComponent<TMP_Text>();
        }

        if (playerCountTMPText == null)
        {
            playerCountTMPText = GameObject.Find("PlayerCountText").GetComponent<TMP_Text>();
        }

        if (mapNameTMPText == null)
        {
            mapNameTMPText = GameObject.Find("LevelNameText").GetComponent<TMP_Text>();
        }
    }

    void Start()
    {
        roomNameTMPText.text = "";
        playerCountTMPText.text = "0/0";
        mapNameTMPText.text = "";
    }

    public void UpdateRoomInfo(SessionInfo roomInfo)
    {
        int activePlayerCount = 0;
        int maxPlayerCap = 0;


        playerCountTMPText.text = $"{activePlayerCount}/{maxPlayerCap}";
    }

    public void OnRoomDisplaySelected()
    {
        
    }

    public void OnRoomInfoUpdatedSuccessful()
    {
        
    }
}
