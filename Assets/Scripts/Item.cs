using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{

    public Image ContentImage;

    private bool _isLeft = false;

    public Sprite DefaultLeftSprite;

    public Sprite UserSprite;

    public Sprite DefultRightSprite;

    public Text Text;

    private float _emojiTextWidth;
    private float _emojiTextHeight;
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetInfo(bool isLeft,string content,bool isUser)
    {
        this._isLeft = isLeft;

        if (isUser)
            this._isLeft = false;


        _emojiTextWidth = Text.rectTransform.sizeDelta.x;
        _emojiTextHeight = Text.rectTransform.sizeDelta.y;

        Vector2 size = ContentImage.rectTransform.sizeDelta;
        ContentImage.rectTransform.anchoredPosition = _isLeft ? new Vector2(-930,0f) : new Vector2(930,0f);
        ContentImage.rectTransform.pivot = _isLeft ? new Vector2(0f,0.5f) : new Vector2(1f, 0.5f);
        Text.text = content;
        ContentSizeFitter csf = null;
        if (Text.preferredWidth <= _emojiTextWidth)
        {
            Text.rectTransform.sizeDelta = new Vector2(Text.preferredWidth , _emojiTextHeight);
           
            ContentImage.rectTransform.sizeDelta = new Vector2(Text.preferredWidth + 200, size.y);

        }
        else
        {
            //Debug.Log("文本长度大于标定长度，所以要折叠，折叠之前的宽度为");
            csf = Text.GetComponent<ContentSizeFitter>();
            csf.enabled = true;
            ContentImage.rectTransform.sizeDelta = new Vector2(_emojiTextWidth + 200, size.y);//*2让其背景比文字多出一截
        }

        if (isUser)
        {
            ContentImage.sprite = UserSprite;
        }
        else
        {
            ContentImage.sprite = _isLeft ? DefaultLeftSprite : DefultRightSprite;
        }
       
       
    }
}
