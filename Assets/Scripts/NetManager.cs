using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x0200009E RID: 158
public class NetManager : MonoBehaviour
{
	// Token: 0x14000004 RID: 4
	// (add) Token: 0x06000651 RID: 1617 RVA: 0x00045FC0 File Offset: 0x000441C0
	// (remove) Token: 0x06000652 RID: 1618 RVA: 0x00045FF8 File Offset: 0x000441F8
	
	public event Action<string> GetValueCompleted;

    public event Action<Texture2D> PostPictureCompleted;


	public event Action<Texture2D> PostMp4Completed;

	// Token: 0x14000005 RID: 5
	// (add) Token: 0x06000653 RID: 1619 RVA: 0x00046030 File Offset: 0x00044230
	// (remove) Token: 0x06000654 RID: 1620 RVA: 0x00046068 File Offset: 0x00044268

	public event Action<byte[]> SwapFaceCompletedEvent;

	// Token: 0x06000655 RID: 1621 RVA: 0x000460A0 File Offset: 0x000442A0
	private void Awake()
	{
		bool flag = NetManager.Instance != null;
		if (flag)
		{
			throw new UnityException("已经有了单例，不允许重复赋值");
		}
		NetManager.Instance = this;
	}

	// Token: 0x06000656 RID: 1622 RVA: 0x000460CE File Offset: 0x000442CE
	private void Start()
	{
		//this.Start_UDPReceive(6001);
	}

	// Token: 0x06000657 RID: 1623 RVA: 0x000460E0 File Offset: 0x000442E0
	public void SendMessage(byte[] messageBytes)
	{
		int port = 6000;
		string host = GlobalSettings.ServerIp;
		bool flag = messageBytes == null || messageBytes.Length == 0;
		UnityEngine.Debug.LogError("server ip is " + GlobalSettings.ServerIp);
		if (flag)
		{
			throw new UnityException("没能完整的读取到图片");
		}
		try
		{
			IPAddress ip = IPAddress.Parse(host);
			IPEndPoint ipe = new IPEndPoint(ip, port);
			this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this._clientSocket.Connect(ipe);
			this._clientSocket.Send(messageBytes);
			int count = 0;
			this._thread = new Thread(delegate ()
			{
				int i = 0;
				MemoryStream ms = new MemoryStream();
				for (; ; )
				{
					bool flag2 = this._clientSocket != null;

					
					if (flag2)
					{
						try
						{
							bool flag3 = !this._clientSocket.Connected;
							if (flag3)
							{
								this.CloseMessage(ms);
                                UnityEngine.Debug.LogError("服务器主动断开");
								break;
							}
							byte[] recByte = new byte[2097152];
							int bytes = this._clientSocket.Receive(recByte, recByte.Length, SocketFlags.None);
							i++;
							bool flag4 = bytes > 0;
							if (flag4)
							{
								
								count++;
                                UnityEngine.Debug.LogError("共接收了 " + count + "次容量");
							}
							ms.Write(recByte, 0, bytes);
							bool flag5 = i >= 20;
							if (flag5)
							{
								this.CloseMessage(ms);
								break;
							}
						}
						catch (Exception e2)
						{
							UnityEngine.Debug.LogError("接收错误 " + e2.ToString());
							this.CloseMessage(ms);
							break;
						}
					}
				}
			});
			this._thread.Start();
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError(e.ToString());
			this.CloseMessage(null);
		}
	}

	// Token: 0x06000658 RID: 1624 RVA: 0x000461B8 File Offset: 0x000443B8
	private void CloseMessage(MemoryStream ms)
	{
		bool flag = ms != null;
		if (flag)
		{
			this._receiveData = ms.GetBuffer();
			ms.Dispose();
			ms.Close();
			ms = null;
		}
		this._reveiceIsCompleted = true;
		bool flag2 = this._clientSocket != null;
		if (flag2)
		{
			this._clientSocket.Close();
		}
		this._thread = null;
		this._clientSocket = null;
	}

	// Token: 0x06000659 RID: 1625 RVA: 0x0004621B File Offset: 0x0004441B
	public void SendFaceDataMessage(Texture2D tex)
	{
		this.SendMessage(tex.EncodeToJPG());
	}

	
	public event Action<string> RecevieUDPDataEvent;

	// Token: 0x0600065C RID: 1628 RVA: 0x0004629C File Offset: 0x0004449C
	public void Start_UDPReceive(int recv_port)
	{
		bool flag = this.udp_recv_flag;
		if (!flag)
		{
			this.UDPrecv = new UdpClient(new IPEndPoint(IPAddress.Any, recv_port));
			this.endpoint = new IPEndPoint(IPAddress.Any, 0);
			this.recvThread = new Thread(new ThreadStart(this.RecvThread));
			this.recvThread.IsBackground = true;
			this.recvThread.Start();
			this.udp_recv_flag = true;
		}
	}

	// Token: 0x0600065D RID: 1629 RVA: 0x00046314 File Offset: 0x00044514
	private void ReceiveCallback(IAsyncResult ar)
	{
		this.recvBuf = this.UDPrecv.EndReceive(ar, ref this.endpoint);
		this.recvdata += Encoding.Default.GetString(this.recvBuf);
		this.old = true;
		this.messageReceive = true;
	}

	// Token: 0x0600065E RID: 1630 RVA: 0x0004636C File Offset: 0x0004456C
	private void RecvThread()
	{
		this.messageReceive = true;
		for (; ; )
		{
			try
			{
				bool flag = this.messageReceive;
				if (flag)
				{
					this.UDPrecv.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
					this.messageReceive = false;
				}
			}
			catch (Exception e)
			{
			}
		}
	}

	// Token: 0x0600065F RID: 1631 RVA: 0x000463D0 File Offset: 0x000445D0
	public string Read_UDPReceive()
	{
		bool flag = this.recvdata != null;
		string result;
		if (flag)
		{
			this.returnstr = string.Copy(this.recvdata);
			bool flag2 = this.old;
			if (flag2)
			{
				this.old = false;
				this.recvdata = "";
				result = this.returnstr;
			}
			else
			{
				result = "";
			}
		}
		else
		{
			result = null;
		}
		return result;
	}

	// Token: 0x06000660 RID: 1632 RVA: 0x00046430 File Offset: 0x00044630
	private void Update()
	{
		bool reveiceIsCompleted = this._reveiceIsCompleted;
		if (reveiceIsCompleted)
		{
			bool flag = this._receiveData != null;
			if (flag)
			{
				UnityEngine.Debug.Log("获取得到的数据长度是： " + this._receiveData.Length);
			}
			bool flag2 = this.SwapFaceCompletedEvent != null;
			if (flag2)
			{
				this.SwapFaceCompletedEvent(this._receiveData);
			}
			this._reveiceIsCompleted = false;
		}
		this._receiveStr = this.Read_UDPReceive();
		bool flag3 = !string.IsNullOrEmpty(this._receiveStr);
		if (flag3)
		{
			bool flag4 = this.RecevieUDPDataEvent != null;
			if (flag4)
			{
				this.RecevieUDPDataEvent(this._receiveStr);
			}
		}
	}

	// Token: 0x06000661 RID: 1633 RVA: 0x000464DB File Offset: 0x000446DB
	public IEnumerator GetValueToServer()
	{
		string netPath = null;
		WWWForm form = new WWWForm();
		
		form.AddField("PicUuid", "GetValue");
		form.AddField("Sited", "菜鸟地网");
		form.AddField("Extension", "jpg");
		
		UnityEngine.Debug.Log(string.Concat(new object[]
		{
			
			"  ServerPictureIp ",
			GlobalSettings.ServerPictureIp
		}));
		UnityWebRequest webRequest = UnityWebRequest.Post(GlobalSettings.ServerGetValueIp, form);
		yield return webRequest.SendWebRequest();
		bool flag2 = webRequest.isNetworkError || !string.IsNullOrEmpty(webRequest.error);
		if (flag2)
		{
			UnityEngine.Debug.Log(webRequest.error);
		}
		else
		{
			netPath = webRequest.downloadHandler.text;
		}

		bool flag3 = netPath == null;


		if (flag3)
		{
			UnityEngine.Debug.LogError("服务器返回错误信息 " + netPath);
			bool flag4 = this.GetValueCompleted != null;
			if (flag4)
			{
				this.GetValueCompleted(null);
			}
		}
		else
		{
			string url = netPath;
			
			bool flag5 = this.GetValueCompleted != null;
			if (flag5)
			{
				this.GetValueCompleted(netPath);
			}
			UnityEngine.Debug.Log(url);
			url = null;
			
		}
		yield break;
	}

    // Token: 0x06000661 RID: 1633 RVA: 0x000464DB File Offset: 0x000446DB
    public IEnumerator PostPictureToServer(Texture2D tex)
    {
        string netPath = null;
        WWWForm form = new WWWForm();
        string uuid = GlobalSettings.CreatUuid();
        bool flag = string.IsNullOrEmpty(uuid);
        if (flag)
        {
            uuid = "null";
        }
        form.AddField("PicUuid", uuid);
        form.AddField("Sited", GlobalSettings.Stie);
        form.AddField("Extension", "jpg");
        byte[] bytes = tex.EncodeToJPG();
        form.AddBinaryData("PicUuidData", bytes, "test", "application/octet-stream");
        UnityEngine.Debug.Log(string.Concat(new object[]
        {
            "bytes length is ",
            bytes.Length,
            "  ServerPictureIp ",
            GlobalSettings.ServerPictureIp
        }));
        UnityWebRequest webRequest = UnityWebRequest.Post(GlobalSettings.ServerPictureIp, form);
        yield return webRequest.SendWebRequest();
        bool flag2 = webRequest.isNetworkError || !string.IsNullOrEmpty(webRequest.error);
        if (flag2)
        {
            UnityEngine.Debug.Log(webRequest.error);
        }
        else
        {
            netPath = webRequest.downloadHandler.text;
        }
        bool flag3 = netPath == null;
        if (flag3)
        {
            UnityEngine.Debug.LogError("服务器返回错误信息 " + netPath);
            bool flag4 = this.PostPictureCompleted != null;
            if (flag4)
            {
                this.PostPictureCompleted(null);
            }
        }
        else
        {
            string url = netPath;
            Texture2D newTex = GlobalSettings.GetQr(url);
            bool flag5 = this.PostPictureCompleted != null;
            if (flag5)
            {
                this.PostPictureCompleted(newTex);
            }
            UnityEngine.Debug.Log(url);
            url = null;
            newTex = null;
        }
        yield break;
    }

	public IEnumerator PostMP4ToServer(string mp4Path)
	{

		UnityEngine.Debug.Log("提交服务器的mp4路径是： " + mp4Path);
		byte[] bytes;
		if (File.Exists(mp4Path))
        {
			 bytes = File.ReadAllBytes(mp4Path);
		}
		else
        {
			yield break;
        }

		if (bytes == null) yield break;

		

		string netPath = null;
		WWWForm form = new WWWForm();
		string uuid = GlobalSettings.CreatUuid();
		bool flag = string.IsNullOrEmpty(uuid);
		if (flag)
		{
			uuid = "null";
		}
		form.AddField("PicUuid", uuid);
		form.AddField("Sited", GlobalSettings.Stie);
		form.AddField("Extension", "mp4");
		
		form.AddBinaryData("PicUuidData", bytes, "test", "application/octet-stream");
		UnityEngine.Debug.Log(string.Concat(new object[]
		{
			"bytes length is ",
			bytes.Length,
			"  ServerPictureIp ",
			GlobalSettings.ServerMP4Ip
		}));
		UnityWebRequest webRequest = UnityWebRequest.Post(GlobalSettings.ServerMP4Ip, form);
		yield return webRequest.SendWebRequest();
		bool flag2 = webRequest.isNetworkError || !string.IsNullOrEmpty(webRequest.error);
		if (flag2)
		{
			UnityEngine.Debug.Log(webRequest.error);
		}
		else
		{
			netPath = webRequest.downloadHandler.text;
		}
		bool flag3 = netPath == null;
		if (flag3)
		{
			UnityEngine.Debug.LogError("服务器返回错误信息 " + netPath);
			bool flag4 = this.PostMp4Completed != null;
			if (flag4)
			{
				this.PostMp4Completed(null);
			}
		}
		else
		{
			string url = GlobalSettings.GetMp4+ "?html=" + netPath;
			
			Texture2D newTex = GlobalSettings.GetQr(url);
			bool flag5 = this.PostMp4Completed != null;
			if (flag5)
			{
				this.PostMp4Completed(newTex);
			}
			UnityEngine.Debug.Log(url);
			
		
		}
		yield break;
	}

	// Token: 0x06000662 RID: 1634 RVA: 0x000464F4 File Offset: 0x000446F4
	private void OnDestroy()
	{
		UnityEngine.Debug.Log("Del this server");
		bool flag = this._clientSocket != null;
		if (flag)
		{
			this._clientSocket.Close();
		}
		this._clientSocket = null;
		bool flag2 = this._thread != null;
		UDPrecv.Close();
		UDPrecv.Dispose();
		
		if (flag2)
		{
			this._thread.Abort();
		}
	}

	// Token: 0x040006A1 RID: 1697
	public static NetManager Instance;

	// Token: 0x040006A2 RID: 1698
	private Thread _thread;

	// Token: 0x040006A4 RID: 1700
	private Socket _clientSocket;

	// Token: 0x040006A5 RID: 1701
	private byte[] _receiveData;

	// Token: 0x040006A7 RID: 1703
	private bool _reveiceIsCompleted = false;

	// Token: 0x040006A8 RID: 1704
	private bool udp_send_flag = false;

	// Token: 0x040006A9 RID: 1705
	private bool udp_recv_flag = false;

	// Token: 0x040006AA RID: 1706
	private Thread thrRecv;

	// Token: 0x040006AB RID: 1707
	private UdpClient UDPrecv;

	// Token: 0x040006AC RID: 1708
	private IPEndPoint endpoint;

	// Token: 0x040006AD RID: 1709
	private byte[] recvBuf;

	// Token: 0x040006AE RID: 1710
	private Thread recvThread;

	// Token: 0x040006AF RID: 1711
	private bool old = false;

	// Token: 0x040006B0 RID: 1712
	private string returnstr;

	// Token: 0x040006B1 RID: 1713
	private string recvdata;

	// Token: 0x040006B2 RID: 1714
	private bool messageReceive;

	// Token: 0x040006B3 RID: 1715
	private string _receiveStr;
}
