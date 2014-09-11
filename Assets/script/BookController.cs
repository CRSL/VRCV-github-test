using UnityEngine;
using System.Collections;

//利用しているインプット一覧
//"dual", "LHorizontal", "LVertical", "ZoomAxis", "RVertical"

public class BookController : MonoBehaviour {
	
	//左右ページの親要素を移動・回転させるためのスクリプト
	//あとでlook at を用いてカメラの方向を向くようにしたい。いらないかも？
	
	//左スティックで上下左右、トリガーでカメラとの距離を調節する。
	//右スティック上下で本の傾きを調節
	//左右スワップと2枚目の非表示切り替えも付けたい。

	//private GameObject Lpanel;
	//private GameObject Rpanel;
	private GameObject Lsprite;



	public float sen_x = 1f;
	public float x_min = -1f;
	public float x_max = 1f;

	public float sen_y = 1f;
	public float y_min = -1f;
	public float y_max = 1f;

	public float sen_z = 1f;
	public float near = 0.15f;
	private float near_clip = 0.001f;
	public float far = 10f;
	private float far_clip = 100f;
	private float minimum_range = 3f;

	public float sen_xRot = 20f;
	public float xRot_min = -80f;
	public float xRot_max = 80f;


	public bool x_inv = false;
	public bool y_inv = false;
	public bool z_inv = false;

	private Vector3 posAxis = new Vector3(0,0,0);
	//private Vector3 angAxis = new Vector3(0,0,0);
	//private float cDis;
	//private GameObject MCam;
	
	

	// Use this for initialization
	void Start () {
		//invert処理
		if(x_inv){sen_x = sen_x * -1;}
		if(y_inv){sen_y = sen_y * -1;}
		if(z_inv){sen_z = sen_z * -1;}

		//本の可動範囲がマイナス位置に来ないように調節
		if(near < near_clip){near = near_clip;}
		else if(near > far_clip){near = far_clip - minimum_range;}
		if (far < (near + minimum_range)) {far = near + minimum_range;}
		else if (far > far_clip) {far = far_clip;}

		//Lpanel = GameObject.Find ("LeftBaseQuad");
		//Rpanel = GameObject.Find ("RightBaseQuad");
		Lsprite = GameObject.Find ("LeftImageSprite");

	//	MCam = GameObject.Find("OVRCameraController");
	}
	
	// Update is called once per frame
	void Update () {
		//左スティックで平行移動
		//移動距離はカメラからの距離に比例 dX = z*sin(dθ)/cos(dθ) dθ=inputx*pi/180
		//トリガーで奥行き移動
		//移動距離は画角の変化が一定になるように
		//現在の画角によって変化量がかわる θ = 2arctan(1/2z), dz = 1/2tan((θ+dθ)/2) - 1/2tan(θ/2)

		float dtheta_x = Input.GetAxis ("LHorizontal") * sen_x * Mathf.PI / 180;
		float dtheta_y = Input.GetAxis ("LVertical") * sen_y * Mathf.PI / 180;
		float dtheta_z = Input.GetAxis ("ZoomAxis") * sen_z * Mathf.PI / 180;

		float posz = transform.localPosition.z;
		float theta = 2f * Mathf.Atan (0.5f * posz);

		float dx = posz * Mathf.Sin (dtheta_x) / Mathf.Cos (dtheta_x);
		float dy = posz * Mathf.Sin (dtheta_y) / Mathf.Cos (dtheta_y);
		float dz = 0.5f * (Mathf.Tan (0.5f * (theta + dtheta_z)) - Mathf.Tan (0.5f * theta));

		posAxis.Set (dx, dy, dz);
		transform.Translate (posAxis);

		//移動範囲の制限
		posAxis = transform.localPosition; //posAxis再利用
		transform.localPosition = new Vector3(Mathf.Clamp (posAxis.x, x_min, x_max), Mathf.Clamp (posAxis.y, y_min, y_max), Mathf.Clamp (posAxis.z, near, far));


		//cDis = Vector3.Distance(MCam.transform.position, transform.position) * 0.05f;
		//posAxis.Set(Input.GetAxis("Left Horizontal") * sen_x * cDis * cDis , Input.GetAxis("Left Vertical") * sen_y * cDis * cDis , Input.GetAxis("Triggers") * sen_z * cDis);
		//transform.Translate(posAxis);

		//右スティックVerticalで本を傾ける
		float dturn_x = Input.GetAxis("RVertical") * sen_xRot * Mathf.PI /180;
		// 現在の回転角度を0～360から-180～180に変換
		float rotateX = (transform.eulerAngles.x > 180) ? transform.eulerAngles.x - 360 : transform.eulerAngles.x;
		// 現在の回転角度に入力(turn)を加味した回転角度をMathf.Clamp()を使いminAngleからMaxAngle内に収まるようにする
		float angleX = Mathf.Clamp(rotateX + dturn_x, xRot_min, xRot_max);
		// 回転角度を-180～180から0～360に変換
		angleX = (angleX < 0) ? angleX + 360 : angleX;
		// 回転角度をオブジェクトに適用
		transform.rotation = Quaternion.Euler(angleX,0,0);

		//左右スワップ機能
		//左右のQuadのPositionとRotationを交換する。操作による移動・回転方向も逆にするのが望ましいか？
		//スプライトのPivotをあとから変更できない関係で、中心合わせを自動化した場合にはページカウントを増減することで対応する予定。
		//その場合左右スワップ機能はBookControllerではなくSpriteChangerで行う。

		/*
		if (Input.GetButtonDown ("swap")) {
			Vector3 LPpos = Lpanel.transform.localPosition;
			Quaternion LProt = Lpanel.transform.localRotation;
			Vector3 RPpos = Rpanel.transform.localPosition;
			Quaternion RProt = Rpanel.transform.localRotation;

			Lpanel.transform.localPosition = RPpos;
			Rpanel.transform.localPosition = LPpos;
			Lpanel.transform.localRotation = RProt;
			Rpanel.transform.localRotation = LProt;
		}
		*/

		//画面数切り替え
		//左ページをオン・オフすることで実現している。
		if (Input.GetButtonDown ("dual")){
			Lsprite.renderer.enabled = !Lsprite.renderer.enabled;
		}



			//バンパーで回転
	/*	if(Input.GetButtonDown("Left Bumper")&Input.GetButtonDown("Right Bumper")){
			}else if(Input.GetButtonDown("Left Bumper")){
			angAxis.Set(0,0,sen_rz);
			transform.Rotate(angAxis);
			}else if(Input.GetButtonDown("Right Bumper")){
			angAxis.Set(0,0, -1f * sen_rz);
			transform.Rotate(angAxis);
			}
	*/
	}
}
