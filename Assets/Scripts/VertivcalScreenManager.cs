using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class VertivcalScreenManager : MonoBehaviour
{

    public GameObject PrefabGameObject;

    public RectTransform Grip;

    public List<string> InfoList = new List<string>();

    private Queue<Item> _qurereItems = new Queue<Item>();

    private bool isLeft = true;

    public Camera CapCamera;

    public RawImage CaptrueImage;

    public Text NameText;

    public Text NumberText;

    public Text DescriptionText;

    public RawImage QRawImage;

    private int _month = 6;

    private int _day = 5;

    private int _count = 0;


    private bool _isReceiveInfo = false;
    // Start is called before the first frame update
    void Start()
    {
        List<string> strs = new List<string>();
        foreach (string s in InfoList)
        {
           
            strs.Add(s.Replace(" ", "\u3000"));
        }

        InfoList = strs;

        NetManager.Instance.GetValueCompleted+= GetValueCompleted;

        NetManager.Instance.PostPictureCompleted += Instance_PostPictureCompleted1;

        StartCoroutine(GetInfo());

        Screen.SetResolution(2160,3840,true);
       
        _count = PlayerPrefs.GetInt("Person");

        if (_count == 0) _count = 1000;


    }

    private Coroutine _coroutine;

    private Coroutine _showImg;
    /// <summary>
    /// 提交服务器图片数据完成，并生成了二维码
    /// </summary>
    /// <param name="obj"></param>
    private void Instance_PostPictureCompleted1(Texture2D obj)
    {
        AddItetm(true, DescriptionText.text);


        if(_coroutine!=null)StopCoroutine(_coroutine);
        _coroutine= StartCoroutine(GlobalSettings.WaitTime(5f, (() =>
        {
            _isReceiveInfo = true;
            QRawImage.texture = obj;
            CaptrueImage.gameObject.SetActive(true);
            if(_showImg!=null)StopCoroutine(_showImg);
            _showImg =StartCoroutine(GlobalSettings.WaitTime(30f, (() =>
            {
                CaptrueImage.gameObject.SetActive(false);
                _isReceiveInfo = false;
            })));
        })));

     
    }

    /// <summary>
    /// 从服务器获取信息
    /// </summary>
    /// <param name="obj"></param>
    private void GetValueCompleted(string obj)
    {
      
        if (!obj.Contains("!*_*!")) {

            Debug.Log("没有在服务器获取到数据");
            return;//如果数据不符合规范

        }

        if(_showImg!=null)StopCoroutine(_showImg);
        if(_coroutine!=null)StopCoroutine(_coroutine);

        CaptrueImage.gameObject.SetActive(false);
        _isReceiveInfo = false;


        string[] strs = obj.Split(new [] { "!*_*!" }, StringSplitOptions.None);

        string netName = strs[0];

        string description = strs[1];

        _count++;

        PlayerPrefs.SetInt("Person",_count);

        NameText.text = netName;

        DescriptionText.text = description;

        NumberText.text = _count.ToString();

       
        StartCoroutine(MakePhoto());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public IEnumerator MakePhoto()
    {
        yield return new WaitForEndOfFrame();
        int resWidth = 2160;
        int resHeight = 3840;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);

        {
            this.CapCamera.targetTexture = rt;
            this.CapCamera.Render();
            this.CapCamera.targetTexture = null;
        }
        RenderTexture prevActiveTex = RenderTexture.active;
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0f, 0f, (float)resWidth, (float)resHeight), 0, 0);
        screenShot.Apply();
        RenderTexture.active = prevActiveTex;
        Object.Destroy(rt);
        //base.StartCoroutine(NetManager.Instance.PostPictureToServer(screenShot));
       
        CaptrueImage.texture = screenShot;

       StartCoroutine( NetManager.Instance.PostPictureToServer(screenShot));





    }
    /// <summary>
    /// 获取天数，月份，来计算人的个数，虚拟人的数量
    /// </summary>
    /// <returns></returns>
    private int HandleTime()
    {
        int month = DateTime.Now.Month;

        int v1 = month - _month;
        if (v1 <= 0) v1 = 0;

        int day = DateTime.Now.Day;

        int v2 = day - _day;

        if (v2 <= 0) v2 = 0;

        int value = (v1 * 30 + v2) * 10000;

        return value;
    }
    private IEnumerator GetInfo()
    {
        while (true)
        {

            yield return new WaitForSeconds(2f);

            StartCoroutine(NetManager.Instance.GetValueToServer());

            if (!_isReceiveInfo)
            {
                AddItetm(false, null);
            }
            else
            {
                yield return null;
            }
           
        }
    }
    private void AddItetm(bool isUser,string content)
    {
        Item item = Instantiate(PrefabGameObject).GetComponent<Item>();

        item.transform.parent = Grip;

        string str = InfoList[Random.Range(0, InfoList.Count)];


        if (isUser)
        {
            item.SetInfo(true, content, true);
        }
        else
        {
            item.SetInfo(true, str, false);
        }

       

      

        _qurereItems.Enqueue(item);

        if (_qurereItems.Count > 10)
        {
            Item temp = _qurereItems.Dequeue();

            Destroy(temp.gameObject);
        }
    }



#if UNITY_EDITOR_WIN

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "test"))
        {
            StartCoroutine(NetManager.Instance.GetValueToServer());
        }
    }
#endif
}
