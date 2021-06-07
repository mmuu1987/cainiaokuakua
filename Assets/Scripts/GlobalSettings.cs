using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using Random = System.Random;


// Token: 0x0200009A RID: 154
public static class GlobalSettings
{
	// Token: 0x06000631 RID: 1585 RVA: 0x000451F0 File Offset: 0x000433F0
	public static bool FileIsUsed(string fileFullName)
	{
		bool result = false;
		bool flag = !File.Exists(fileFullName);
		if (flag)
		{
			result = false;
		}
		else
		{
			FileStream fileStream = null;
			try
			{
				fileStream = File.Open(fileFullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException ioEx)
			{
				result = true;
			}
			catch (Exception ex)
			{
				result = true;
			}
			finally
			{
				bool flag2 = fileStream != null;
				if (flag2)
				{
					fileStream.Close();
				}
			}
		}
		return result;
	}

	// Token: 0x06000632 RID: 1586 RVA: 0x0004527C File Offset: 0x0004347C
	public static void ReadXml()
	{
		XmlDocument doc = new XmlDocument();
		string xmlPath = Application.streamingAssetsPath + "/Setting.xml";
		bool flag = !File.Exists(xmlPath);
		if (flag)
		{
			//Log.Error("Common", "没有找到XML文件");
		}
		doc.Load(xmlPath);
		XmlNode selectSingleNode = doc.SelectSingleNode("CommonTag");
		bool flag2 = selectSingleNode != null;
		if (flag2)
		{//
			XmlNodeList nodeList = selectSingleNode.ChildNodes;
			foreach (object obj in nodeList)
			{
				XmlNode item = (XmlNode)obj;
				bool flag3 = item.Name == "LOG_LEVENL";
				if (flag3)
				{
					GlobalSettings.LOG_LEVENL = int.Parse(item.InnerText);
				}
				else
				{
					bool flag4 = item.Name == "IsDeletedPicture";
					if (flag4)
					{
						bool flag5 = item.InnerText == "false";
						if (flag5)
						{
							GlobalSettings.IsDeletedPicture = false;
						}
						else
						{
							GlobalSettings.IsDeletedPicture = true;
						}
					}
					else
					{
						bool flag6 = item.Name == "IP";
						if (flag6)
						{
							GlobalSettings.ServerIp = item.InnerText;
						}
						else
						{
							bool flag7 = item.Name == "Brightness";
							if (flag7)
							{
								GlobalSettings.Brightness = float.Parse(item.InnerText);
							}
							else
							{
								bool flag8 = item.Name == "Saturation";
								if (flag8)
								{
									GlobalSettings.Saturation = float.Parse(item.InnerText);
								}
								else
								{
									bool flag9 = item.Name == "Contrast";
									if (flag9)
									{
										GlobalSettings.Contrast = float.Parse(item.InnerText);
									}
									else
									{
										bool flag10 = item.Name == "IsDebug";
										if (flag10)
										{
											Debug.Log(item.InnerText);
											GlobalSettings.IsDebug = (item.InnerText == "true");
										}
										else
										{
											bool flag11 = item.Name == "RoleInfos";
											if (flag11)
											{
												foreach (object obj2 in item.ChildNodes)
												{
													XmlElement element = (XmlElement)obj2;
													
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06000633 RID: 1587 RVA: 0x00045664 File Offset: 0x00043864
	public static void SaveXml()
	{
		string localPath = Application.streamingAssetsPath + "/Setting.xml";
		XmlDocument xml = new XmlDocument();
		XmlDeclaration xmldecl = xml.CreateXmlDeclaration("1.0", "UTF-8", "");
		xml.AppendChild(xmldecl);
		XmlElement root = xml.CreateElement("CommonTag");
		XmlElement info = xml.CreateElement("LOG_LEVENL");
		info.InnerText = GlobalSettings.LOG_LEVENL.ToString();
		root.AppendChild(info);
		XmlElement temp = xml.CreateElement("Brightness");
		temp.InnerText = GlobalSettings.Brightness.ToString();
		root.AppendChild(temp);
		XmlElement temp2 = xml.CreateElement("Saturation");
		temp2.InnerText = GlobalSettings.Saturation.ToString();
		root.AppendChild(temp2);
		XmlElement temp3 = xml.CreateElement("Contrast");
		temp3.InnerText = GlobalSettings.Contrast.ToString();
		root.AppendChild(temp3);
		XmlElement temp4 = xml.CreateElement("IsDebug");
		temp4.InnerText = GlobalSettings.IsDebug.ToString();
		root.AppendChild(temp4);
		XmlElement temp5 = xml.CreateElement("IP");
		temp5.InnerText = GlobalSettings.ServerIp.ToString();
		root.AppendChild(temp5);
		XmlElement roleInfos = xml.CreateElement("RoleInfos");
		int i = 0;
        root.AppendChild(roleInfos);
		xml.AppendChild(root);
		xml.Save(localPath);
	}

	// Token: 0x06000634 RID: 1588 RVA: 0x000459C4 File Offset: 0x00043BC4
	

	// Token: 0x06000635 RID: 1589 RVA: 0x00045A30 File Offset: 0x00043C30
	

	// Token: 0x06000636 RID: 1590 RVA: 0x00045AE8 File Offset: 0x00043CE8
	public static string CreatUuid()
	{
		string uuid = null;
		Random random = new Random();
		for (int i = 1; i <= 8; i++)
		{
			uuid += GlobalSettings.CharsLetter[random.Next(GlobalSettings.CharsLetter.Length)].ToString();
		}
		return uuid;
	}

	// Token: 0x06000637 RID: 1591 RVA: 0x00045B3D File Offset: 0x00043D3D
	public static IEnumerator WaitTime(float time, Action action)
	{
		yield return new WaitForSeconds(time);
		bool flag = action != null;
		if (flag)
		{
			action();
		}
		yield break;
	}

	// Token: 0x06000638 RID: 1592 RVA: 0x00045B54 File Offset: 0x00043D54
	public static Texture2D GetQr(string result)
	{
		Texture2D tex = new Texture2D(256, 256);
		BarcodeWriter writer = new BarcodeWriter
		{
			Format = BarcodeFormat.QR_CODE,
			Options = new QrCodeEncodingOptions
			{
				Height = tex.height,
				Width = tex.width
			}
		};
		Color32[] temp = writer.Write(result);
		tex.SetPixels32(temp);
		tex.Apply();
		return tex;
	}

	// Token: 0x06000639 RID: 1593 RVA: 0x00045BC6 File Offset: 0x00043DC6
	public static IEnumerator WaitEndFarme(Action callBack)
	{
		yield return null;
		bool flag = callBack != null;
		if (flag)
		{
			callBack();
		}
		yield break;
	}

	/// <summary>
	/// 判断字符串是否是IP地址格式
	/// </summary>
	/// <param name="ipAddress"></param>
	/// <returns></returns>
    public static bool ValidateIPAddress(string ipAddress)
    {
        Regex validipregex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
        return (ipAddress != "" && validipregex.IsMatch(ipAddress.Trim())) ? true : false;
    }

	// Token: 0x0600063A RID: 1594 RVA: 0x00045BD8 File Offset: 0x00043DD8
	public static void InitArg()
	{
		string av = PlayerPrefs.GetString("AV");
		string tv = PlayerPrefs.GetString("TV");
		string iso = PlayerPrefs.GetString("ISO");
		bool flag = !string.IsNullOrEmpty(av);
		if (flag)
		{
			GlobalSettings.AV = uint.Parse(av);
		}
		bool flag2 = !string.IsNullOrEmpty(tv);
		if (flag2)
		{
			GlobalSettings.TV = uint.Parse(tv);
		}
		bool flag3 = !string.IsNullOrEmpty(iso);
		if (flag3)
		{
			GlobalSettings.ISO = uint.Parse(iso);
		}
	}

	// Token: 0x0600063B RID: 1595
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);

	// Token: 0x0600063C RID: 1596
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetForegroundWindow();

	// Token: 0x0600063D RID: 1597
	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

	// Token: 0x0600063E RID: 1598
	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

	// Token: 0x0600063F RID: 1599 RVA: 0x00045C59 File Offset: 0x00043E59
	public static void OnClickMinimize()
	{
		GlobalSettings.ShowWindow(GlobalSettings.GetForegroundWindow(), 2);
	}

	// Token: 0x06000640 RID: 1600 RVA: 0x00045C68 File Offset: 0x00043E68
	public static void OnClickMaximize()
	{
		GlobalSettings.ShowWindow(GlobalSettings.GetForegroundWindow(), 3);
	}

	// Token: 0x06000641 RID: 1601 RVA: 0x00045C77 File Offset: 0x00043E77
	public static void OnClickRestore()
	{
		GlobalSettings.ShowWindow(GlobalSettings.GetForegroundWindow(), 1);
	}

	// Token: 0x06000642 RID: 1602 RVA: 0x00045C86 File Offset: 0x00043E86
	public static void TopWindows()
	{
		GlobalSettings.SetWindowPos(GlobalSettings.GetForegroundWindow(), -1, 0, 0, 0, 0, 3);
	}

	// Token: 0x06000643 RID: 1603
	[DllImport("User32.dll")]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	// Token: 0x06000644 RID: 1604
	[DllImport("User32.dll")]
	public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

	// Token: 0x04000675 RID: 1653
	public const float ZoomMagnification = 1.6f;

	// Token: 0x04000676 RID: 1654
	public static int PictureWidth = 2400;

	// Token: 0x04000677 RID: 1655
	public static int PictureHeight = 1600;

	// Token: 0x04000678 RID: 1656
	public static float WidthScale = 0.5f;

	// Token: 0x04000679 RID: 1657
	public static float HeightScale = 1f;

	// Token: 0x0400067A RID: 1658
	public static uint AV = 40U;

	// Token: 0x0400067B RID: 1659
	public static uint TV = 109U;

	// Token: 0x0400067C RID: 1660
	public static uint ISO = 104U;

	// Token: 0x0400067D RID: 1661
	private static char[] CharsLetter = new char[]
	{
		'1',
		'2',
		'3',
		'4',
		'5',
		'6',
		'7',
		'8',
		'9',
		'0'
	};

	// Token: 0x0400067E RID: 1662
	public static string ServerPictureIp = "http://www.syyj.tglfair.com/Webpage/VoiceOffice/MyScreenshots.aspx";

    public static string ServerSetValueIp = "http://www.syyj.tglfair.com/Webpage/SetValue.aspx";

    public static string ServerGetValueIp = "http://www.syyj.tglfair.com/Webpage/GetValue.aspx";

	public static string ServerMP4Ip = "http://www.syyj.tglfair.com/Webpage/VoiceOffice/ServerGetMp4.aspx";

	public static string GetMp4 = "http://www.syyj.tglfair.com/Webpage/VoiceOffice/ClientGetMp4.aspx";

	// Token: 0x0400067F RID: 1663
	public static bool IsDeletedPicture = true;

	// Token: 0x04000680 RID: 1664
	public static float Brightness = 1f;

	// Token: 0x04000681 RID: 1665
	public static float Saturation = 1f;

	// Token: 0x04000682 RID: 1666
	public static float Contrast = 1f;

	// Token: 0x04000683 RID: 1667
	public static bool IsDebug = true;

	// Token: 0x04000684 RID: 1668
	public static string ServerIp = "";

	// Token: 0x04000685 RID: 1669


	// Token: 0x04000686 RID: 1670
	public static string Stie = "阿里巴巴菜鸟地网";

	// Token: 0x04000687 RID: 1671
	public static bool IsOutLog = true;

	// Token: 0x04000688 RID: 1672
	private const uint SWP_SHOWWINDOW = 64U;

	// Token: 0x04000689 RID: 1673
	private const int GWL_STYLE = -16;

	// Token: 0x0400068A RID: 1674
	private const int WS_BORDER = 1;

	// Token: 0x0400068B RID: 1675
	private const int SW_SHOWMINIMIZED = 2;

	// Token: 0x0400068C RID: 1676
	private const int SW_SHOWMAXIMIZED = 3;

	// Token: 0x0400068D RID: 1677
	private const int SW_SHOWRESTORE = 1;

	// Token: 0x0400068E RID: 1678
	public const int REPORT_LEVENL = 2;

	// Token: 0x0400068F RID: 1679
	public static int LOG_LEVENL = 3;
}
