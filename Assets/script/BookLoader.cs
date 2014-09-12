using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//利用しているインプット一覧
//"next", "prev", "dual", "swap"

//アタッチしたスプライトに対し変更処理を行う。

//指定フォルダのイメージファイルを取得し、テクスチャを生成
//テクスチャからスプライトを作り、スプライトを変更することを目的とする。
//決まったサイズの枠内に入るように、pixelsToUnitsを自動調節する機能を内蔵したい。
//このサイズはインスペクタから設定できるようにする。

public class BookLoader : MonoBehaviour {
	
	//最終出力するスプライトを入れるフレームのサイズ
	public float frame_w = 1f;
	public float frame_h = 1f;
	
	//ページオフセット、このスプライトが何枚目か、を意味する。
	public int pageoffset = 0;
	
	//これが右のページならTrueなBool
	public bool isRight = true;
	private Vector2 SpPivot;
	
	//現在左右スワップが行われているかどうか
	private bool isSwapping = false;
	
	//ページ送り数pagefeed
	public int pfeed = 1;
	private int defaultpfeed;
	
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
		
		//ファイル名一覧確認
		//foreach (string img in ImgList) {
		//	print (img);
		//}
		
		//ファイル総数取得
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


		//width height setting
		//float RC = (float)PNG_height/(float)PNG_width;
		//print (RC);
		//print (transform.localScale.magnitude);
		//if(PNG_width > PNG_height){
		//	transform.localScale = new Vector3(1,0.1f,RC);
		//}else if(RC != 0){
		//	transform.localScale = new Vector3 (1/RC,0.1f,1);
		//}
		//
		//renderer.material.SetTexture (0,tex);
		//renderer.material.mainTexture.wrapMode = TextureWrapMode.Clamp;


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
		tex = ReadTexture(path);
		int PNG_height = tex.height;
		int PNG_width = tex.width;
		
		//debug用、テクスチャ名表示
		//print ("tex path " + path);
		//print ("tex name " + tex.name);
		
		float wr = (float)PNG_width/frame_w;
		float hr = (float)PNG_height/frame_h;
		float pTU = wr; //pixel to unit
		if(hr > wr){pTU = hr;} //長い辺がおさまるようにpTUを決める。
		Rect rect = new Rect(0,0,PNG_width,PNG_height);
		
		//スプライトのPivotをページごとに変える。
		//左ページなら(1,0.5), 右ページなら(0,0.5)になるはず
		//通常は右ページがスプライト0、左ページがスプライト1
		//一応左右判定用の変数を用意しておく。(isRight)
		//右ページ用スプライトは右ページにしか使われないのでPivotはStart()内でどちらかに設定しておく。
		
		sp = Sprite.Create(tex,rect,SpPivot,pTU);
		
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
		pivot = new Vector2 (Mathf.Clamp01 (pivot.x), Mathf.Clamp01 (pivot.y));
		sp = Sprite.Create(tex,rect,pivot,pTU);
		return sp;
	}
	
	
	private int imgct = 0;
	
	
	// Use this for initialization
	void Start () {
		//フォルダから画像ファイルの一覧を取得
		GetImgList (ImgPath); //List<string> ImgListにファイル一覧が格納される
		//nImagesも利用可能。
		
		defaultpfeed = pfeed;
		
		//左右ページどちらかを判定し、スプライトのPivotを適切に設定する。
		//左ページなら(1,0.5), 右ページなら(0,0.5)になるはず
		//通常は右ページがスプライト0、左ページがスプライト1
		//一応左右判定用の変数を用意しておく。(isRight)
		//右ページ用スプライトは右ページにしか使われないのでSpPivotはStart()内でどちらかに設定しておく。
		if (isRight) {
			SpPivot = new Vector2 (0, 0.5f);
		} else {
			SpPivot = new Vector2(1, 0.5f);
		}
		
		
		foreach (string str in ImgList) {
			SprList.Add (setSprite (str)); //List<sprite> SprListにスプライトを全部格納する。SpPivotを使うのでその設定をしてから。
		}
		
		//ページオフセット処理・nImagesを使う都合上、ファイル一覧取得後。
		if((pageoffset < nImages) && (pageoffset > 0)){
			imgct = imgct + pageoffset;
		}
		
		
		//スプライトに１枚目のテクスチャを貼る。
		GetComponent<SpriteRenderer> ().sprite = SprList [imgct];
		
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetButtonDown ("next")) { //"next"ボタンが押された時。
			imgct = imgct + pfeed; //ページ送り
			if (imgct < 0) { //異常事態だが一応対応。pfeedに負の値などの場合か。
				imgct = nImages - 1; //最終ページを表示する。
			} else if (imgct >= nImages) { //最終ページを越えた場合、
				imgct = imgct - nImages; //最初に戻る。
			}
			this.GetComponent<SpriteRenderer> ().sprite = SprList [imgct];
			
		} else if (Input.GetButtonDown ("prev")) {
			imgct = imgct - pfeed;
			if (imgct < 0) {
				imgct = imgct + nImages;
			} else if (imgct >= nImages) {
				imgct = 0;
			}
			this.GetComponent<SpriteRenderer> ().sprite = SprList [imgct];
		} else if (Input.GetButtonDown ("dual")) { //2画面表示なら2枚ずつ送る。1画面表示なら1枚ずつ送る。
			if (pfeed == 1) {
				pfeed = defaultpfeed; //一応、デフォルトのn画面表示と1画面表示の切り替えという形をとる。
			} else {
				pfeed = 1;
			}
		} else if (Input.GetButtonDown ("swap")) {
			if(isSwapping ^ isRight){
				imgct++;
				if(imgct>=nImages){imgct = imgct - nImages;}
			} else {
				imgct--;
				if(imgct<0){imgct = imgct + nImages;}
			}
			isSwapping = !isSwapping;
			this.GetComponent<SpriteRenderer> ().sprite = SprList [imgct];
		}
		
		/*
		if(Input.GetButtonDown("next")){
			imgct = imgct + pfeed;
			if(imgct<0){imgct=ImgList.Count-1;}
			else if(imgct>=ImgList.Count){imgct=0;}
			setTexture (ImgList [imgct]);
			
		}else if(Input.GetButtonDown("prev")){
			imgct = imgct - pfeed;
			if(imgct<0){imgct=ImgList.Count-1;}
			else if(imgct>=ImgList.Count){imgct=0;}
			setTexture (ImgList [imgct]);
			
		} else if (Input.GetButtonDown ("dual")) {
			if(pfeed==1){pfeed=2;}
			else pfeed=1;
		}
		*/
	}
}
