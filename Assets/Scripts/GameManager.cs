using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    #region 单例模式
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }

    #endregion

    public int xColumn;
    public int yRow;
    public GameObject girdPrefab;

    //填充时间
    public float fillTime;

    #region UI Show

    public Text timeText;

    private float gameTime = 60;

    private bool gameOver;

    public int playerScore;

    public Text playerScoreText;

    private float addScoreTime;

    private float currentScore;

    public Text finalScoreText;

    public GameObject gameOverPanel;
    #endregion

    #region 甜品有关的数据
    //要交换的两个甜品对象
    private GameSweet pressedSweet;
    private GameSweet enteredSweet;

   


    //甜品的种类
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT//标记类型
    }

    //甜品预制体的字典，可以通过甜品的种类来得到对应甜品的游戏物体
    public Dictionary<SweetsType, GameObject> sweetPrefabDict;

    [System.Serializable]
    public struct SweetPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }

    public SweetPrefab[] sweetPrefabs;
   
    //甜品二维数组
    private GameSweet[,] sweets;

    #endregion
    private void Awake()
    {
        _instance = this;
    }
    void Start () {
        //字典的实例化
        sweetPrefabDict = new Dictionary<SweetsType, GameObject>();
        for (int i = 0; i < sweetPrefabs.Length; i++)
        {
            if (!sweetPrefabDict.ContainsKey(sweetPrefabs[i].type))
            {
                sweetPrefabDict.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }
        for(int x = 0; x < xColumn; x++)
		{
            for (int y= 0; y < yRow; y++)
            {
                GameObject chocolate = Instantiate(girdPrefab, CorrectPosition(x,y), Quaternion.identity);
                chocolate.transform.SetParent(this.gameObject.transform);
            }
        }

        sweets = new GameSweet[xColumn, yRow];
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                CreatNewSweet(x, y, SweetsType.EMPTY);
            }
        }

        Destroy(sweets[4, 4].gameObject);
        CreatNewSweet(4, 4, SweetsType.BARRIER);
        Destroy(sweets[4, 3].gameObject);
        CreatNewSweet(4, 3, SweetsType.BARRIER);
        Destroy(sweets[1, 1].gameObject);
        CreatNewSweet(1, 1, SweetsType.BARRIER);
        Destroy(sweets[1, 1].gameObject);
        CreatNewSweet(1, 1, SweetsType.BARRIER);
        Destroy(sweets[7, 1].gameObject);
        CreatNewSweet(7, 1, SweetsType.BARRIER);
        Destroy(sweets[1, 6].gameObject);
        CreatNewSweet(1, 6, SweetsType.BARRIER);
        Destroy(sweets[7, 6].gameObject);
        CreatNewSweet(7, 6, SweetsType.BARRIER);

        StartCoroutine(AllFill());

    }
	
	// Update is called once per frame
	void Update () {
       

        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
        {
            gameTime = 0;
            //显示我们的失败面板
            //播放失败面板的动画
            gameOverPanel.SetActive(true);
           
            finalScoreText.text = playerScore.ToString();
            gameOver = true;
           
        }
        timeText.text = gameTime.ToString("0");


        if (addScoreTime <= 0.05f)
        {
            addScoreTime += Time.deltaTime;
        }
        else
        {
            if (currentScore < playerScore)
            {
                
                currentScore++;
                playerScoreText.text = currentScore.ToString();
                addScoreTime = 0;
            }
        }
        


	}

    /// <summary>
    /// 纠正背景巧克力格子的位置
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector3 CorrectPosition(int x,int y)
    {
        //巧克力块位置的X=GameManager位置的X坐标-大网格长度的一半+行列对应的X坐标
        //巧克力块位置的Y=GameManager位置的Y坐标+大网格长度的一半-行列对应的Y坐标

        return new Vector3(transform.position.x - xColumn / 2f + x, transform.position.y + yRow / 2f - y, 0);

    }

    /// <summary>
    /// 创建一个新的甜品
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameSweet CreatNewSweet(int x,int y,SweetsType type)
    {
        GameObject newSweet=Instantiate(sweetPrefabDict[type], CorrectPosition(x, y), Quaternion.identity);
        newSweet.transform.SetParent(transform);

        sweets[x, y] = newSweet.GetComponent<GameSweet>();
        sweets[x, y].Init(x, y, this, type);

        return sweets[x, y];
    }

    /// <summary>
    /// 全部填充的方法
    /// </summary>
    public IEnumerator AllFill()
    {
        bool needRefill = true;

        while (needRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (Fill())
            {
                yield return new WaitForSeconds(fillTime);
            }

            //清除所有已经匹配好的甜品
            needRefill = ClearAllMatchedSweet();
        }
        
    }

    /// <summary>
    /// 分部填充
    /// </summary>
    /// <returns></returns>
    public bool Fill()
    {
        bool filledNotFinished = false;//判断本次填充是否完成

        for(int y = yRow - 2; y >= 0; y--)
        {
            for(int x = 0; x < xColumn; x++)
            {
                GameSweet sweet = sweets[x, y]; //得到当前元素位置的甜品对象
                if (sweet.CanMove())//如果无法移动，则无法往下填充      
                {
                    GameSweet sweetBelow = sweets[x, y + 1];

                    if (sweetBelow.Type == SweetsType.EMPTY) //垂直填充
                    {
                        Destroy(sweetBelow.gameObject);
                        sweet.MovedComponent.Move(x, y + 1,fillTime);
                        sweets[x, y + 1] = sweet;
                        CreatNewSweet(x, y, SweetsType.EMPTY);
                        filledNotFinished = true;
                    }
                    else                                     //斜向填充
                    {
                        for (int down = -1; down <= 1; down++)
                        {
                            if (down != 0)//排除0正下
                            {
                                int downX = x + down;//当前x坐标，-1 左下，0正下，1右下
                                if (downX >= 0 && downX < xColumn)//排除边界情况 
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];//先从左下方查看
                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canfill = true; //判断垂直填充是否能满足填充需求
                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GameSweet sweetAbove = sweets[downX, aboveY];
                                            if (sweetAbove.CanMove())//上方元素可以移动，用上方元素进行垂直填充即可
                                            {
                                                break;
                                            }
                                            else if (!sweetAbove.CanMove() && sweetAbove.Type != SweetsType.EMPTY)//上方元素不能移动，并且上方元素不是空，无法垂直填充，使用斜向填充
                                            {
                                                canfill = false;
                                            }
                                        }

                                        if (!canfill)
                                        {
                                            Destroy(downSweet.gameObject);
                                            sweet.MovedComponent.Move(downX, y + 1, fillTime);
                                            sweets[downX, y + 1] = sweet;
                                            CreatNewSweet(x, y, SweetsType.EMPTY);
                                            filledNotFinished = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
        }

        //最上排的特殊情况
        for(int x = 0; x < xColumn; x++)
        {
            GameSweet sweet = sweets[x, 0];
            if (sweet.Type == SweetsType.EMPTY)
            {
               GameObject newSweet= Instantiate(sweetPrefabDict[SweetsType.NORMAL], CorrectPosition(x, 0), Quaternion.identity);
                newSweet.transform.SetParent(transform);

                sweets[x, 0] = newSweet.GetComponent<GameSweet>();
                sweets[x, 0].Init(x, -1, this, SweetsType.NORMAL);
                sweets[x, 0].MovedComponent.Move(x, 0,fillTime);
                sweets[x, 0].ColoredComponent.SetColor((ColorSweet.ColorType)Random.Range(0, sweets[x, 0].ColoredComponent.NumColors));
                filledNotFinished = true;
            }
        }

        return filledNotFinished;

    }

   /// <summary>
   /// 甜品是否相邻
   /// </summary>
   /// <param name="sweet1"></param>
   /// <param name="sweet2"></param>
   /// <returns></returns>
    private bool IsFriend(GameSweet sweet1,GameSweet sweet2)
    {
        return (sweet1.X == sweet2.X && (Mathf.Abs(sweet1.Y - sweet2.Y) == 1))
            || (sweet1.Y == sweet2.Y && (Mathf.Abs(sweet1.X - sweet2.X) == 1));

    }

    /// <summary>
    /// 交换甜品的方法
    /// </summary>
    /// <param name="sweet1"></param>
    /// <param name="sweet2"></param>
    private void ExchangeSweets(GameSweet sweet1,GameSweet sweet2)
    {
        if (sweet1.CanMove() && sweet2.CanMove())
        {
            sweets[sweet1.X, sweet1.Y]=sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;

            if (MatchSweets(sweet1, sweet2.X, sweet2.Y) != null||
                MatchSweets(sweet2,sweet1.X,sweet1.Y)!=null||sweet1.Type==SweetsType.RAINBOWCANDY||sweet2.Type==SweetsType.RAINBOWCANDY)
            {

                int tempX = sweet1.X;
                int tempY = sweet1.Y;

                sweet1.MovedComponent.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MovedComponent.Move(tempX, tempY, fillTime);

                if (sweet1.Type == SweetsType.RAINBOWCANDY && sweet1.CanClear() && sweet2.CanClear())
                {
                    ClearColorSweet clearColor = sweet1.GetComponent<ClearColorSweet>();
                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet2.ColoredComponent.Color;
                    }
                    ClearSweet(sweet1.X, sweet1.Y);
                }

                if (sweet2.Type == SweetsType.RAINBOWCANDY && sweet1.CanClear() && sweet2.CanClear())
                {
                    ClearColorSweet clearColor = sweet2.GetComponent<ClearColorSweet>();
                    if (clearColor != null)
                    {
                        clearColor.ClearColor = sweet1.ColoredComponent.Color;
                    }
                    ClearSweet(sweet2.X, sweet2.Y);
                }

                ClearAllMatchedSweet();
                StartCoroutine(AllFill());

                pressedSweet = null;
                enteredSweet = null;
            }
            else
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;

            }

            
        }
    }
   
    #region 玩家对甜品拖拽处理的方法
    public void PressSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        pressedSweet = sweet;
    }

    public void EnterSweet(GameSweet sweet)
    {
        if (gameOver)
        {
            return;
        }
        enteredSweet = sweet;
    }

    public void ReleaseSweet()
    {
        if (gameOver)
        {
            return;
        }
        if (IsFriend(pressedSweet, enteredSweet))
        {
            ExchangeSweets(pressedSweet, enteredSweet);
        }
       
    }
    #endregion

    #region 匹配和清除的方法
    /// <summary>
    /// 匹配方法
    /// </summary>
    /// <param name="sweet"></param>
    /// <param name="newX"></param>
    /// <param name="newY"></param>
    /// <returns></returns>
    public List<GameSweet> MatchSweets(GameSweet sweet, int newX, int newY)
    {
        if (sweet.CanColor())
        {
            ColorSweet.ColorType color = sweet.ColoredComponent.Color;
            List<GameSweet> matchRowSweets = new List<GameSweet>();
            List<GameSweet> matchLineSweets = new List<GameSweet>();
            List<GameSweet> finishedMatchingSweets = new List<GameSweet>();

            //行匹配
            matchRowSweets.Add(sweet);

            //i=0代表往左，i=1代表往右
            for (int i = 0; i <= 1; i++)
            {
                for (int xDistance = 1; xDistance < xColumn; xDistance++)
                {
                    int x;
                    if (i == 0)
                    {
                        x = newX - xDistance;
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if (x < 0 || x >= xColumn)
                    {
                        break;
                    }

                    if (sweets[x, newY].CanColor() && sweets[x, newY].ColoredComponent.Color == color)
                    {
                        matchRowSweets.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchRowSweets.Count >= 3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchRowSweets[i]);
                }
            }

            //L T型匹配
            //检查一下当前行遍历列表中的元素数量是否大于3
            if (matchRowSweets.Count >= 3)
            {
                for (int i = 0; i < matchRowSweets.Count; i++)
                {
                    //行匹配列表中满足匹配条件的每个元素上下依次进行列遍历
                    // 0代表上方 1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int yDistance = 1; yDistance < yRow; yDistance++)
                        {
                            int y;
                            if (j == 0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if (y < 0 || y >= yRow)
                            {
                                break;
                            }

                            if (sweets[matchRowSweets[i].X, y].CanColor() && sweets[matchRowSweets[i].X, y].ColoredComponent.Color == color)
                            {
                                matchLineSweets.Add(sweets[matchRowSweets[i].X, y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (matchLineSweets.Count < 2)
                    {
                        matchLineSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchLineSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchLineSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }

            matchRowSweets.Clear();
            matchLineSweets.Clear();

            matchLineSweets.Add(sweet);

            //列匹配

            //i=0代表往左，i=1代表往右
            for (int i = 0; i <= 1; i++)
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;
                    if (i == 0)
                    {
                        y = newY - yDistance;
                    }
                    else
                    {
                        y = newY + yDistance;
                    }
                    if (y < 0 || y >= yRow)
                    {
                        break;
                    }

                    if (sweets[newX, y].CanColor() && sweets[newX, y].ColoredComponent.Color == color)
                    {
                        matchLineSweets.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchLineSweets[i]);
                }
            }

            //L T型匹配
            //检查一下当前行遍历列表中的元素数量是否大于3
            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    //行匹配列表中满足匹配条件的每个元素上下依次进行列遍历
                    // 0代表上方 1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance = 1; xDistance < xColumn; xDistance++)
                        {
                            int x;
                            if (j == 0)
                            {
                                x = newY - xDistance;
                            }
                            else
                            {
                                x = newY + xDistance;
                            }
                            if (x < 0 || x >= xColumn)
                            {
                                break;
                            }

                            if (sweets[x, matchLineSweets[i].Y].CanColor() && sweets[x, matchLineSweets[i].Y].ColoredComponent.Color == color)
                            {
                                matchRowSweets.Add(sweets[x, matchLineSweets[i].Y]);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (matchRowSweets.Count < 2)
                    {
                        matchRowSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchRowSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchRowSweets[j]);
                        }
                        break;
                    }
                }
            }

            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }
        }

        return null;
    }

    /// <summary>
    /// 清除方法
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool ClearSweet(int x,int y)
    {
        if (sweets[x, y].CanClear() && !sweets[x, y].ClearedComponent.IsClearing)
        {
            sweets[x, y].ClearedComponent.Clear();
            CreatNewSweet(x, y, SweetsType.EMPTY);

            ClearBarrier(x, y);
            return true;
        }
        return false;
    }

    private bool ClearAllMatchedSweet()
    {
        bool needRefill = false;//默认不需要重新填充
        for(int y = 0; y < yRow; y++)
        {
            for(int x = 0; x < xColumn; x++)
            {
                if (sweets[x, y].CanClear())
                {
                    List<GameSweet>matchList= MatchSweets(sweets[x, y], x, y);
                    if (matchList != null)
                    {
                        SweetsType specialSweetsType = SweetsType.COUNT;//决定是否产生特殊甜品

                        GameSweet randomSweet = matchList[Random.Range(0, matchList.Count)];
                        int specialSweetX = randomSweet.X;
                        int specialSweetY = randomSweet.Y;
                        //四个产生行列消除
                        if (matchList.Count == 4)
                        {
                            specialSweetsType = (SweetsType)Random.Range((int)SweetsType.ROW_CLEAR, (int)SweetsType.COLUMN_CLEAR);

                        }
                        //五个产生彩虹糖
                        else if (matchList.Count >= 5)
                        {
                            specialSweetsType = SweetsType.RAINBOWCANDY;
                        }

                        for (int i = 0; i < matchList.Count; i++)
                        {
                            if (ClearSweet(matchList[i].X, matchList[i].Y))
                            {
                                needRefill = true;
                            }
                        }

                        if (specialSweetsType != SweetsType.COUNT)
                        {
                            Destroy(sweets[specialSweetX, specialSweetY]);
                            GameSweet newSweet = CreatNewSweet(specialSweetX, specialSweetY, specialSweetsType);

                            if ((specialSweetsType == SweetsType.ROW_CLEAR || specialSweetsType == SweetsType.COLUMN_CLEAR)
                                &&newSweet.CanColor()&&matchList[0].CanColor())
                            {
                                newSweet.ColoredComponent.SetColor(matchList[0].ColoredComponent.Color);
                            }
                            //加上彩虹糖特殊类型的产生
                            else if (specialSweetsType == SweetsType.RAINBOWCANDY && newSweet.CanColor())
                            {
                                newSweet.ColoredComponent.SetColor(ColorSweet.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }
        return needRefill;
           
    }

    /// <summary>
    /// 清除饼干障碍物
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void ClearBarrier(int x,int y)//x，y坐标是被消除掉的甜品对象的坐标
    {
        for(int friendX = x-1; friendX <= x + 1; friendX++)
        {
            if (friendX != x && friendX >= 0 && friendX < xColumn)
            {
                if(sweets[friendX,y].Type==SweetsType.BARRIER&& sweets[friendX, y].CanClear())
                {
                    sweets[friendX, y].ClearedComponent.Clear();
                    CreatNewSweet(friendX, y, SweetsType.EMPTY);
                }
            }
        }

        for (int friendY = y-1; friendY <= y + 1; friendY++)
        {
            if (friendY != y && friendY >=0 && friendY < yRow)
            {
                if (sweets[x, friendY].Type == SweetsType.BARRIER && sweets[x,friendY].CanClear())
                {
                    sweets[x, friendY].ClearedComponent.Clear();
                    CreatNewSweet(x, friendY, SweetsType.EMPTY);
                }
            }
        }
    }
#endregion

    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }

    public void RePlay()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// 清除行的方法
    /// </summary>
    /// <param name="row"></param>
    public void ClearRow(int row)
    {
        for(int x = 0; x < xColumn; x++)
        {
            ClearSweet(x, row);
        }
    }

    /// <summary>
    /// 清除列的方法
    /// </summary>
    /// <param name="column"></param>
    public void ClearColumn(int column)
    {
        for(int y = 0; y < yRow; y++)
        {
            ClearSweet(column, y);
        }
    }

    /// <summary>
    /// 清除颜色的方法
    /// </summary>
    /// <param name="color"></param>
    public void ClearColor(ColorSweet.ColorType color)
    {
        for(int x = 0; x < xColumn; x++)
        {
            for(int y = 0; y < yRow; y++)
            {
                //交换两个彩虹糖 这里会全部满足，会清除所有颜色的糖果
                if (sweets[x, y].CanColor() && (sweets[x, y].ColoredComponent.Color == color || color == ColorSweet.ColorType.ANY))
                {
                    ClearSweet(x, y);
                }
            }
        }
    }
}
