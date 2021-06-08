using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using ZXing.Aztec.Internal;

public class moveEffect : MonoBehaviour
{

    public GameObject PrefabGameObject;
    // Start is called before the first frame update
    void Start()
    {
        Random();
    }

    private void Random()
    {
        for (int i = 0; i < 25; i++)
        {
            CreatItem();
        }
    }

    private void CreatItem()
    {
        RectTransform go = Instantiate(PrefabGameObject).GetComponent<RectTransform>();

        go.transform.parent = this.transform;

        float xTemp = UnityEngine.Random.Range(-900f, 900f);

        float yTemp = UnityEngine.Random.Range(-2124f, -5500f);

        float time = UnityEngine.Random.Range(8f, 20f);

        go.anchoredPosition = new Vector2(xTemp,yTemp);

        go.DOLocalMoveY(-20f, time).SetDelay(0.25f).SetEase(Ease.Linear).OnComplete((() =>
        {
            CreatItem();

            Destroy(go.gameObject);
        }));


    }
  
    // Update is called once per frame
    void Update()
    {
        
    }
}
