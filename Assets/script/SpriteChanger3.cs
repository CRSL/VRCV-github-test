using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//利用しているインプット一覧
//"next", "prev", "dual", "dual"

//描画用スプライトオブジェクトそれぞれにSprite配列を割り当てており、メモリ効率が悪い。

//指定フォルダのイメージファイルを取得し、テクスチャを生成
//テクスチャからスプライトを作り、スプライトを変更することを目的とする。
//決まったサイズの枠内に入るように、pixelsToUnitsを自動調節する機能を内蔵したい。
//このサイズはインスペクタから設定できるようにする。

public class SpriteChanger3 : MonoBehaviour {

	//書き換え対象とするGameObjectの名前
	public string RightPageSprite = "RightImageSprite"; 
	public int defaultRightPage = 0;
	public string LeftPageSprite = "LeftImageSprite";
	public int defaultLeftPage = 1;
	public string CenterPageSprite ="CenterImageSprite";

	private GameObject tgtObj0;
	private GameObject tgtObj1;
	private GameObject tgtObj2;

	//２ページ表示かどうか、
	private bool isDual = true;

	//最終出力するスプライトを入れるフレームのサイズ
	public float frame_w = 1f;
	public float frame_h = 1f;

	//ページ送り数pagefeed
	public int defaultpfeed = 2;
	private int pfeed;

	//public Sprite pubSprite;
	
	//描画用tex,スプライト破棄できるように先に作る
	private Texture2D tex;
	private Sprite sp;
	
	//画像フォルダを指定・ファイル名・テクスチャ・スプライト格納リスト準備
	public string ImgPath = "ImageFolder";
	private List<string> ImgList = new List<string>();
	private List<Texture2D> TexList = new List<Texture2D> ();
	private List<Sprite> RSprList = new List<Sprite> ();
	private List<Sprite> LSprList = new List<Sprite> ();
	private List<Sprite> CSprList = new List<Sprite> ();
	private int nImages; //画像の枚数

	//フォルダの設定
	void SetImgPath(string path){
		ImgPath = path;
	}
	
	
	//フォルダ内のファイル名を取得・リストに格納
	void GetImgList (string path) {
		string[] files = Directory.GetFiles (path);
		
		foreach (string file in files) {
			//print (file);
			if (file.EndsWith (".jpg") | file.EndsWith (".png") | file.EndsWith (".JPG") | file.EndsWith (".PNG")) {
				ImgList.Add (file);
			}
		}
		ImgList.Sort ();
		
		//ファイル名一覧確認
		foreach (string img in ImgList) {
			print (img);
		}
		nImages = ImgList.Count;
		
		
	}
	
	//フォルダの変更処理
	void ChangeImgPath(string path){
		SetImgPath (path);
		GetImgList (ImgPath);
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

	//画像の縦横比を維持するためにオブジェクトを変形し、テクスチャを生成、スプライトを返すメソッド
	Sprite setSprite(string path){
		//Destroy (tex);
		tex = ReadTexture(path);
		int PNG_height = tex.height;
		int PNG_width = tex.width;
		print ("tex path " + path);
		print ("tex name " + tex.name);
		
		float wr = (float)PNG_width/frame_w;
		float hr = (float)PNG_height/frame_h;
		float pTU = wr; //pixel to unit
		if(hr > wr){pTU = hr;}
		Rect rect = new Rect(0,0,PNG_width,PNG_height);
		
		Vector2 pivot = new Vector2(0.5f,0.5f);
		//Vector2 pivot = new Vector2(0,0);
		

		//Destroy (sp);
		sp = Sprite.Create(tex,rect,pivot,pTU);
		//Destroy (tex);
		return sp;
	}

	//テクスチャとピボットをうけて収まるようにスプライトを返す。
	Sprite setSprite(Texture2D tex,Vector2 pivot){
		int img_height = tex.height;
		int img_width = tex.width;
		float wr = (float)img_width/frame_w;
		float hr = (float)img_height/frame_h;
		float pTU = wr; //pixel to unit
		if(hr > wr){pTU = hr;} //長い辺がおさまるようにpTUを決める。
		Rect rect = new Rect(0,0,img_width,img_height);
		//pivot = new Vector2 (Mathf.Clamp01 (pivot.x), Mathf.Clamp01 (pivot.y));
		sp = Sprite.Create(tex,rect,pivot,pTU);
		return sp;
	}

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

		private int RightPage;
		private int LeftPage;

	// Use this for initialization
	void Start () {
		//初期ページの設定、基本ページ送り数の設定。
		RightPage = defaultRightPage;
		LeftPage = defaultLeftPage;
		pfeed = defaultpfeed;
			
		tgtObj0 = GameObject.Find(RightPageSprite); //右ページのスプライトオブジェクトの設定
		tgtObj1 = GameObject.Find(LeftPageSprite); //左ページのスプライトオブジェクトの設定
		tgtObj2 = GameObject.Find (CenterPageSprite); //中央ページのスプライトオブジェクトの設定
		
		//フォルダから画像ファイルの一覧を取得
		GetImgList (ImgPath); //List<string> ImgListにファイル一覧が格納される

		//画像が0枚の時
		if(nImages == 0){
			print("no image file");
			Application.Quit();
		}

		//画像ファイル一覧からTexture2Dのリストを取得。
		foreach (string str in ImgList) {
			TexList.Add (ReadTexture (str));
		}

		for(int x=0;x < nImages; x++) {
			RSprList.Add (setSprite (TexList[x],0));
			LSprList.Add (setSprite (TexList[x],1));
			CSprList.Add (setSprite (TexList[x],2));
			print (ImgList[x]);
		}

		TexList.Clear ();

		//ページフィード数が画像枚数を越えないように設定
		if (pfeed >= nImages) {
			pfeed = nImages - 1;
		} else if (pfeed <= 0) {
			pfeed = 1;
		}
		//デフォルトページが画像枚数を超えている場合の設定
		if (LeftPage > nImages-1) {
			LeftPage = nImages - 1;
		}




		tgtObj0.renderer.GetComponent<SpriteRenderer> ().sprite = RSprList[RightPage];
		tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = LSprList[LeftPage];
		tgtObj2.renderer.enabled = false; //デフォルトでは中央は描画されない。
		tgtObj2.renderer.GetComponent<SpriteRenderer> ().sprite = CSprList[RightPage]; //ただしスプライトの設定はされる。


		

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("next")) {
			//左右ともにpfeedずつすすめる。進めてオーバーする部分は%nImagesであまり部分を出す。
			RightPage = (RightPage + pfeed) % nImages;
			LeftPage = (LeftPage + pfeed) % nImages;

			//それぞれに描画。
			tgtObj0.renderer.GetComponent<SpriteRenderer> ().sprite = RSprList[RightPage];
			tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = LSprList[LeftPage];
			tgtObj2.renderer.GetComponent<SpriteRenderer> ().sprite = CSprList[RightPage];

		} else if (Input.GetButtonDown ("prev")) {
			//左右ともにpfeedずつ戻す。0~nImages-1の間に入れる。
			RightPage = (RightPage - pfeed + nImages) % nImages;
			LeftPage = (LeftPage - pfeed + nImages) % nImages;

			//それぞれに描画。
			tgtObj0.renderer.GetComponent<SpriteRenderer> ().sprite = RSprList[RightPage];
			tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = LSprList[LeftPage];
			tgtObj2.renderer.GetComponent<SpriteRenderer> ().sprite = CSprList[RightPage];

		} else if (Input.GetButtonDown ("dual")) {
			if (!isDual) {
				pfeed = defaultpfeed; //初期設定のページ送り数に戻し、
				//表示切り替え
				tgtObj2.renderer.enabled = false;
				tgtObj0.renderer.enabled = true;
				tgtObj1.renderer.enabled = true;
			} else {
				pfeed = 1; //ページ送りを１枚ずつにし、
				//表示切り替え
				tgtObj0.renderer.enabled = false;
				tgtObj1.renderer.enabled = false;
				tgtObj2.renderer.enabled = true;
			}
			isDual = !isDual;
		} else if (Input.GetButtonDown ("swap")) {
			int temp = RightPage;
			RightPage =LeftPage;
			LeftPage = temp;
			tgtObj0.renderer.GetComponent<SpriteRenderer> ().sprite = RSprList[RightPage];
			tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = LSprList[LeftPage];
			tgtObj2.renderer.GetComponent<SpriteRenderer> ().sprite = CSprList[RightPage];
		}
	}
}
