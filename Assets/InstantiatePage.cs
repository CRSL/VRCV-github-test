using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class InstantiatePage : MonoBehaviour {

	//インスタンス化するオブジェクト
	public GameObject page;

	//描画用tex,スプライト破棄できるように先に作る
	private Texture2D tex;
	private Sprite sp;

	//画像フォルダを指定・ファイル名・テクスチャ・スプライト格納リスト準備
	public string ImgPath = "ImageFolder";
	private List<string> ImgList = new List<string>();
	private List<Texture2D> TexList = new List<Texture2D> ();
	private List<Sprite> RSprList = new List<Sprite> ();
	private List<Sprite> LSprList = new List<Sprite> ();
	private List<GameObject> GObjList = new List<GameObject> ();

	private int nImages; //画像の枚数
	private int readingPage; //現在の位置(0~nImages)


	//フォルダ内のファイル名前を取得・リストに格納、ファイル数の取得
	void GetImgList (string path) {
		string[] files = Directory.GetFiles (path);
		
		foreach (string file in files) {
			//print (file);
			if (file.EndsWith (".jpg") | file.EndsWith (".png") | file.EndsWith (".JPG") | file.EndsWith (".PNG")) {
				ImgList.Add (file);
			}
		}
		ImgList.Sort ();
		
		//ファイル総数取得
		nImages = ImgList.Count;
		
	}
	
	
	//ファイルストリームを開いて指定ファイルをバイナリ配列で返すメソッド
	byte[] ReadPngFile(string path){
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		BinaryReader bin = new BinaryReader(fileStream);
		byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);
		
		bin.Close();
		
		return values;
	}
	
	//上記のバイナリを返すメソッドを利用し、指定ファイルからテクスチャを生成して返すメソッド
	Texture2D ReadTexture(string path){
		byte[] readBinary = ReadPngFile (path);
		
		Texture2D texture = new Texture2D(0,0);
		texture.LoadImage(readBinary);
		
		return texture;
	}


	//最終出力するスプライトを入れるフレームのサイズ
	public float frame_w = 1f;
	public float frame_h = 1f;

	//テクスチャと左右どちらかか、をうけて収まるようにスプライトを返す。
	//int pos 0,1,2 == R,L,C
	Sprite setSprite(Texture2D tex,int pos){
		int img_height = tex.height;
		int img_width = tex.width;
		float wr = (float)img_width/frame_w;
		float hr = (float)img_height/frame_h;
		float pTU = wr; //pixel to unit
		if(hr > wr){pTU = hr;} //長い辺がおさまるようにpTUを決める。
		Rect rect = new Rect(0,0,img_width,img_height);
		Vector2 pivot = new Vector2 (1f, 0.5f);
		if (pos == 0) {
			pivot.x = 0;
		} else if (pos == 1) {
			pivot.x = 1f;
		} else if (pos == 2) {
			pivot.x = 0.5f;
		} else {
			print ("error");
		}
		//pivot = new Vector2 (Mathf.Clamp01 (pivot.x), Mathf.Clamp01 (pivot.y));
		sp = Sprite.Create(tex,rect,pivot,pTU);
		return sp;
	}

	// Use this for initialization
	void Start () {
		readingPage = 0;

		GetImgList (ImgPath);
		if (nImages == 0) {
			Application.Quit ();
		}


		foreach (string str in ImgList) {
			TexList.Add (ReadTexture (str)); //List<Texture2D> TexListにテクスチャを全部格納する。
		}

		for(int x=0;x < nImages; x++) {
			RSprList.Add (setSprite (TexList[x],0));
			LSprList.Add (setSprite (TexList[x],1));
			print (ImgList[x]);
		}
		
		for (int x = 0; x < nImages; x++) {
			GameObject obj = Instantiate (page, new Vector3 (-0.05f*(float)x/nImages, 0, 0), Quaternion.identity) as GameObject;
			obj.GetComponent<SpriteRenderer> ().sprite = RSprList[x];
			obj.name = "page"+x;
			obj.transform.Rotate (0, 180 + 1 * (float)x / nImages ,0);
			GObjList.Add (obj);
		}
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("next")) {
			if(readingPage == nImages){
				return;
			}
			GObjList[readingPage].transform.Rotate (0,-180,0);
			readingPage++;
		} else if (Input.GetButtonDown ("prev")) {
			if(readingPage == 0){
				return;
			}
			GObjList[readingPage-1].transform.Rotate (0,180,0);
			readingPage--;

		}
	}
}
