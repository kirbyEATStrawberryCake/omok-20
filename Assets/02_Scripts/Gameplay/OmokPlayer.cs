using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public OmokBoard omokBoard;
    public GameObject stonePrefab;
    public GameObject curser;

    public const int WHITE = 1;
    public const int BLACK = 2;
    public int playerStone = WHITE;

    void Start()
    {
        omokBoard = GameObject.Find("OmokBoard").GetComponent<OmokBoard>();
    }

    void Update()
    {
        if (!GameManager.Instance().isMyTurn || GameManager.Instance().isGameOver)
        {
            curser.SetActive(false);
            return;
        }

        curser.SetActive(true);
        MovePosition();
    }

    public void MovePosition()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (transform.position.x < 8)
            {
                transform.position += new Vector3(0.5f, 0, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (transform.position.x > 0)
            {
                transform.position += new Vector3(-0.5f, 0, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (transform.position.y < 0)
            {
                transform.position += new Vector3(0, 0.5f, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (transform.position.y > -8)
            {
                transform.position += new Vector3(0, -0.5f, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            int _x = Mathf.Abs((int)(transform.position.x / 0.5f));
            int _y = Mathf.Abs((int)(transform.position.y / 0.5f));
            Debug.Log("X : " + _x + " , " + "Y : " + _y);

            if (GameManager.Instance().isMyTurn && omokBoard.StoneSet(_x, _y, playerStone))
            {
                GameObject _stone = Instantiate(stonePrefab, transform.position, Quaternion.identity);
                _stone.transform.parent = omokBoard.transform;

                if (omokBoard.CheckWin(playerStone))
                {
                    Debug.Log("");
                }
            }
        }
    }
}