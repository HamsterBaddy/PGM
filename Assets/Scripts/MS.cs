using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MS : MonoBehaviour
{
    public TileBase[] modules;
    public int[] tileWeights;

    public mapCell[][] map;

    public int seed;

    public int mapSize = 64;

    public static MS Instance;

    public Tilemap tilemap;

    public TMP_Text title, seedTxt;
    public TMP_InputField seedInputF, greyWeight;
    public Button genButton, hideButton, exitButton;

    public GameObject loadingScreen;

    public void toggleHide()
    {
        title.gameObject.SetActive(!title.gameObject.activeSelf);
        seedTxt.gameObject.SetActive(!seedTxt.gameObject.activeSelf);
        seedInputF.gameObject.SetActive(!seedInputF.gameObject.activeSelf);
        greyWeight.gameObject.SetActive(!greyWeight.gameObject.activeSelf);
        genButton.gameObject.SetActive(!genButton.gameObject.activeSelf);
        exitButton.gameObject.SetActive(!exitButton.gameObject.activeSelf);
    }

    public void buttonGen()
    {
        GenerateMap();
    }

    public void Exit()
    {
        Application.Quit();
    }


    private void Awake()
    {
        Random.InitState(seed);
    }

    
    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        //Setup
        Instance = this;

        loadingScreen.SetActive(false);

        //Camera.main.aspect = 1;

        //GenerateMap();
    }

    public void GenerateMap()
    {
        StartCoroutine(genMap());
    }

    public IEnumerator genMap()
    {
        seedInputF.enabled = false;
        genButton.enabled = false;
        hideButton.enabled = false;
        greyWeight.enabled = false;

        yield return new WaitForEndOfFrame();
        Debug.Log("DisabledInput");
        yield return new WaitForEndOfFrame();

        loadingScreen.SetActive(true);

        yield return new WaitForEndOfFrame();
        Debug.Log("EnableLoadingScreen");
        yield return new WaitForEndOfFrame();

        int greayW = 16;

        try
        {
            greayW = System.Convert.ToInt32(greyWeight.text);
        }
        catch (System.Exception e)
        {
            char[] chars = greyWeight.text.ToCharArray();

            greayW = 16;

            for (int i = 0; i < chars.Length; i++)
            {
                greayW += Mathf.FloorToInt(Mathf.Pow(-1, i) * chars[i]);
            }
        }

        greayW = Mathf.Abs(greayW);

        MS.Instance.tileWeights[0] = greayW;

        greyWeight.text = greayW.ToString();

        yield return new WaitForEndOfFrame();
        Debug.Log("gotGreyWeight: " + greayW);
        yield return new WaitForEndOfFrame();

        try
        {
            seed = System.Convert.ToInt32(seedInputF.text);
        }
        catch(System.Exception e)
        {
            char[] chars = seedInputF.text.ToCharArray();

            seed = 0;

            for (int i = 0; i < chars.Length; i++)
            {
                seed += Mathf.FloorToInt(Mathf.Pow(-1, i) * chars[i]);
            }
        }

        Random.InitState(seed);

        yield return new WaitForEndOfFrame();
        Debug.Log("gotSeed: " + seed);
        yield return new WaitForEndOfFrame();

        map = new mapCell[mapSize][];

        for (int i = 0; i < map.Length; i++)
        {
            map[i] = new mapCell[map.Length];

            for (int j = 0; j < map[i].Length; j++)
            {
                map[i][j] = new mapCell();
            }
        }

        yield return new WaitForEndOfFrame();
        Debug.Log("SetUpDone");
        yield return new WaitForEndOfFrame();

        int x = Mathf.FloorToInt(Random.value * map.Length);
        int y = Mathf.FloorToInt(Random.value * map.Length);

        map[x][y].assignedTile = 0;
        if(x+1 < map.Length)
            map[x + 1][y].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
        if(x-1 >= 0)
            map[x - 1][y].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
        if(y+1 < map.Length)
            map[x][y + 1].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
        if(y-1 >= 0)
            map[x][y - 1].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });

        //map[Mathf.Abs(x + 1) % map.Length][y].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
        //map[Mathf.Abs(x - 1) % map.Length][y].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
        //map[x][Mathf.Abs(y + 1) % map.Length].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
        //map[x][Mathf.Abs(y - 1) % map.Length].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });

        yield return new WaitForEndOfFrame();
        Debug.Log("InitDone");
        yield return new WaitForEndOfFrame();

        while (!areAllCellsAssigned())
        {
            //Cell Selection
            List<mapCell> cellsWithMinimumThingy = CellsWithMinimumChooseThingy();

            mapCell cell = cellsWithMinimumThingy[Mathf.FloorToInt(Random.value * cellsWithMinimumThingy.Count)];

            bool hasChanged = false;

            for (int i = 0; i < map.Length; i++)
            {
                for (int j = 0; j < map[i].Length; j++)
                {
                    if (map[i][j] == cell)
                    {
                        x = i;
                        y = j;

                        hasChanged = true;

                        break;
                    }
                }

                if (hasChanged)
                    break;
            }

            //Collapse Cell
            int[] weigthedArray = getWeigthedArray(cell.possibleCells);

            cell.assignedTile = weigthedArray[Mathf.FloorToInt(Random.value * weigthedArray.Length)];

            //Propagation
            if (cell.assignedTile == 0)
            {
                //map[Mathf.Abs(x + 1) % map.Length][y].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
                //map[Mathf.Abs(x - 1) % map.Length][y].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
                //map[x][Mathf.Abs(y + 1) % map.Length].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
                //map[x][Mathf.Abs(y - 1) % map.Length].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });

                if (x + 1 < map.Length)
                    map[x + 1][y].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
                if (x - 1 >= 0)
                    map[x - 1][y].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
                if (y + 1 < map.Length)
                    map[x][y + 1].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
                if (y - 1 >= 0)
                    map[x][y - 1].possibleCells.Add(new int[] { 0, MS.Instance.tileWeights[0] });
            }
        }

        //Apply array to Tilemap
        for (int i = -(map.Length / 2); i < map.Length / 2; i++)
        {
            for (int j = -(map[i + (map.Length / 2)].Length / 2); j < map[i + (map.Length / 2)].Length / 2; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), modules[map[i + (map.Length / 2)][j + (map[i + (map.Length / 2)].Length / 2)].assignedTile]);
            }
        }

        loadingScreen.SetActive(false);

        yield return new WaitForEndOfFrame();
        Debug.Log("DisableLoadingScreen");
        yield return new WaitForEndOfFrame();

        seedInputF.enabled = true;
        genButton.enabled = true;
        hideButton.enabled = true;
        greyWeight.enabled = true;

        yield return new WaitForEndOfFrame();
        Debug.Log("DisabledInput");
        yield return new WaitForEndOfFrame();
    }

    public List<mapCell> CellsWithMinimumChooseThingy()
    {
        List<mapCell> result = new List<mapCell>();

        mapCell cell = map[Mathf.FloorToInt(Random.value * map.Length)][Mathf.FloorToInt(Random.value * map.Length)];

        while (cell.assignedTile >= 0)
        {
            cell = map[Mathf.FloorToInt(Random.value * map.Length)][Mathf.FloorToInt(Random.value * map.Length)];
        }

        result.Add(cell);

        for (int i = 0; i < map.Length; i++)
        {
            //Debug.Log(i);
            for (int j = 0; j < map[i].Length; j++)
            {
                if (map[i][j].assignedTile < 0)
                {
                    bool bugbug = false;  //i > 60 && j > 60

                    float IJEntropy = calculateEntropy(map[i][j].possibleCells, bugbug);
                    float OtherEntropy = calculateEntropy(result[0].possibleCells, bugbug);

                    //Debug.Log(i + " | " + j + " | " + IJEntropy);
                    if(bugbug)
                        Debug.Log(i + " | " + j + " | " + string.Join(" ", map[i][j].possibleCells.ConvertAll(ele => string.Join(",", ele.ToList().ConvertAll(elle => elle.ToString()).ToArray())).ToArray()) + " ; " + IJEntropy + " | " + string.Join(" ", result[0].possibleCells.ConvertAll(ele => string.Join(",", ele.ToList().ConvertAll(elle => elle.ToString()).ToArray())).ToArray()) + " ; " + OtherEntropy);


                    if (IJEntropy == OtherEntropy && map[i][j] != result[0])
                    {
                        result.Add(map[i][j]);
                        //Debug.Log(i + " | " + j + " | " + IJEntropy);
                    }
                    else if (IJEntropy < OtherEntropy && map[i][j] != result[0])
                    {
                        result.Clear();

                        result.Add(map[i][j]);

                        //i = 0;

                        //Debug.Log("FINAL: " + i + " | " + j + " | " + IJEntropy);

                        break;
                    }
                }
            }
        }

        return result;
    }

    public float calculateEntropy(List<int[]> possCells, bool bugde)
    {
        int totalWeight = 0;
        float result = 0;

        for (int i = 0; i < possCells.Count; i++)
        {
            totalWeight += possCells[i][1];
        }

        if(bugde)
            Debug.Log("TotalWeight: " + totalWeight);

        for (int i = 0; i < possCells.Count; i++)
        {
            float probability = (((float)possCells[i][1]) / ((float)totalWeight));

            if (bugde)
                Debug.Log("prob: " + probability);

            float entropyPart = probability * Mathf.Log(probability);

            if (bugde)
                Debug.Log("ePart: " + entropyPart);

            result += entropyPart;
        }

        return -result;
    }

    public bool areAllCellsAssigned()
    {
        for (int i = 0; i < map.Length; i++)
        {
            for (int j = 0; j < map[i].Length; j++)
            {
                if (map[i][j].assignedTile < 0)
                    return false;
            }
        }

        return true;
    }

    public int[] getWeigthedArray(List<int[]> ew)
    {
        int[] result;

        //Woops, accidentally removed this totally usefull and needed part at some point in time before releasing — I toally forgot that and can't remember even now xD
        //So I added it again ofcourse
        //Sadly this peace of wonderfull code isn't included in the builds, but atleast it is pushed on GitHub now!
        int totalWeight = 0;
        for (int i = 0; i < ew.Count; i++)
        {
            totalWeight += ew[i][1];
        }
        result = new int[totalWeight];

        List<int> llist = new List<int>();
        for (int i = 0; i < ew.Count; i++)
        {
            for(int j = 0; j < ew[i][1]; j++)
            {
                llist.Add(ew[i][0]);
            }
        }
        result = llist.ToArray();

        return result;
    }


    public class mapCell
    {
        public int assignedTile = -1;
        public List<int[]> possibleCells = new List<int[]>();

        public mapCell()
        {
            possibleCells.Add(new int[] { 1, MS.Instance.tileWeights[1] });
            possibleCells.Add(new int[] { 2, MS.Instance.tileWeights[2] });
            possibleCells.Add(new int[] { 3, MS.Instance.tileWeights[3] });
            possibleCells.Add(new int[] { 4, MS.Instance.tileWeights[4] });
        }

        public class weightedTile
        {
            public int tileID, weight;
        }
    }


}
