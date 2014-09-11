using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//利用しているインプット一覧
//"next", "prev", "dual"



//指定フォルダのイメージファイルを取得し、テクスチャを生成
//テクスチャからスプライトを作り、スプライトを変更することを目的とする。
//決まったサイズの枠内に入るように、pixelsToUnitsを自動調節する機能を内蔵したい。
//このサイズはインスペクタから設定できるようにする。

public class SpriteChanger2 : MonoBehaviour {

	//書き換え対象とするGameObjectの名前
	public string TargetSprite0 = "RightImageSprite"; 
	public string TargetSprite1 = "LeftImageSprite";
	private GameObject tgtObj0;
	private GameObject tgtObj1;

	//最終出力するスプライトを入れるフレームのサイズ
	public float frame_w = 1f;
	public float frame_h = 1f;

	//ページオフセット
	public int pageoffset = 0;

	//ページ送り数pagefeed
	public int pfeed = 2;

	//public Sprite pubSprite;
	
	//描画用tex,スプライト破棄できるように先に作る
	private Texture2D tex;
	private Sprite sp;
	
	//画像フォルダを指定・ファイル名・テクスチャ・スプライト格納リスト準備
	public string ImgPath = "ImageFolder";
	private List<string> ImgList = new List<string>();
	//private List<Texture> TexList = new List<Texture> ();
	private List<Sprite> SprList = new List<Sprite> ();
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

/*
	//画像の縦横比を維持するためにオブジェクトを変形し、テクスチャを設定するメソッド
	void setTexture(string path){
		//Destroy (tex);
		tex = ReadTexture(path);
		int PNG_height = tex.height;
		int PNG_width = tex.width;
		print ("tex path " + path);
		print ("tex name " + tex.name);

		float wr = (float)PNG_width/frame_w;
		float hr = (float)PNG_height/frame_h;
		float pTU = wr; //pixel to unit
		Rect rect = new Rect(0,0,PNG_width,PNG_height);
		
		Vector2 pivot = new Vector2(0.5f,0.5f);
		//Vector2 pivot = new Vector2(0,0);
		
		if(hr > wr){pTU = hr;}
		//Destroy (sp);
		sp = Sprite.Create(tex,rect,pivot,pTU);
		//new GameObject ("MySprite").AddComponent<SpriteRenderer> ().sprite = sp;
		GetComponent<SpriteRenderer>().sprite = sp;
		print (pTU);
	}
*/	

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

	private int imgct = 0;


	// Use this for initialization
	void Start () {
		//フォルダから画像ファイルの一覧を取得
		GetImgList (ImgPath); //List<string> ImgListにファイル一覧が格納される
		foreach (string str in ImgList) {
						SprList.Add (setSprite (str)); //List<sprite> SprListにスプライトを全部格納する。
				}

		//ページオフセット処理
		if((pageoffset < nImages) && (pageoffset > 0)){
			imgct = imgct + pageoffset;
		}

		//ページフィード数が画像枚数を越えないように設定
		if (pfeed >= nImages) {
			pfeed = nImages - 1;
		} else if (pfeed <= 0) {
			pfeed = 1;
		}
		



		tgtObj0 = GameObject.Find(TargetSprite0); //ターゲットのスプライトの設定
		tgtObj1 = GameObject.Find(TargetSprite1); //ターゲットのスプライトの設定
		tgtObj0.renderer.GetComponent<SpriteRenderer> ().sprite = SprList [imgct];
		tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = SprList [imgct+1];

		

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("next")) {
						imgct = imgct + pfeed;
						if (imgct < 0) { //pfeedに調節を入れたので絶対に成立しないはず。
								imgct = imgct + nImages;
						} else if (imgct >= nImages) {
								imgct = imgct - nImages;
						}
						tgtObj0.renderer.GetComponent<SpriteRenderer> ().sprite = SprList [imgct];

						if (imgct + 1 == nImages) {
								tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = SprList [0];
						} else
								tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = SprList [imgct + 1];


				} else if (Input.GetButtonDown ("prev")) {
						imgct = imgct - pfeed;
						if (imgct < 0) {
								imgct = imgct + nImages;
						} else if (imgct >= nImages) {
								imgct = imgct - nImages; //成立しないはず。
						}
						tgtObj0.renderer.GetComponent<SpriteRenderer> ().sprite = SprList [imgct];
						if (imgct + 1 == nImages) {
								tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = SprList [0];
						} else
								tgtObj1.renderer.GetComponent<SpriteRenderer> ().sprite = SprList [imgct + 1];
				} else if (Input.GetButtonDown ("dual")) {
						if(pfeed==1){pfeed=2;}
						else pfeed=1;
				}
	}
}
