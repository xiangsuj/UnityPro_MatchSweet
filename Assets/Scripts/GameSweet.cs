using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSweet : MonoBehaviour {

    #region sweet data
    private int x;
    private int y;
    private GameManager.SweetsType type;

    public int X
    {
        get
        {
            return x;
        }
        
        set
        {
            if (CanMove())
            {
                x = value;
            }
           
        }
    }

    public int Y
    {
        get
        {
            return y;
        }

        set
        {
            if (CanMove())
            {
                y = value;
            }
           
        }
    }
 
    public GameManager.SweetsType Type
    {
        get
        {
            return type;
        }

        
    }
    #endregion

    #region sweet UI
    [HideInInspector]
    public GameManager gameManager;

    //move 安全校验，若物体身上没有MoveSweet组件，则不会进行移动
    private MovedSweet movedComponent;
    public MovedSweet MovedComponent
    {
        get
        {
            return movedComponent;
        }

        
    }
    public bool CanMove()
    {
        return movedComponent != null;
    }

    //color 安全校验，若物体身上没有ColorSweet组件，则不会进行移动
    private ColorSweet coloredComponent;
    public ColorSweet ColoredComponent
    {
        get
        {
            return coloredComponent;
        }


    }
    public bool CanColor()
    {
        return coloredComponent != null;
    }

    //clear校验
    private ClearedSweet clearedComponent;
    public ClearedSweet ClearedComponent
    {
        get
        {
            return clearedComponent;
        }

        
    }
    public bool CanClear()
    {
        return clearedComponent != null;
    }
    

    
    #endregion

    private void Awake()
    {
        movedComponent = GetComponent<MovedSweet>();
        coloredComponent = GetComponent<ColorSweet>();
        clearedComponent = GetComponent<ClearedSweet>();
    }
   
   
    public void Init(int _x,int _y,GameManager _gameManager,GameManager.SweetsType _type)
    {
        x = _x;
        y = _y;
        gameManager = _gameManager;
        type = _type;
    }

    private void OnMouseEnter()
    {
        gameManager.EnterSweet(this);
    }

    private void OnMouseDown()
    {
        gameManager.PressSweet(this);
    }

    private void OnMouseUp()
    {
        gameManager.ReleaseSweet();
    }
}
