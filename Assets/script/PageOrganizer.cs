using UnityEngine;
using System.Collections;

//利用しているインプット一覧
//"RHorizontal", "BumperAxis"

public class PageOrganizer : MonoBehaviour {
	//各ページの綴じに対する位置・角度を調節することを目的としたスクリプト
	//別途綴じをターゲットとした座標調節スクリプトを併用する

	//とりあえずページの閉じ具合を右スティック左右で調節・Y軸に対する回転 
	//左右ページの間隔を左右バンパーで調節とする・X軸平行移動


	//private Vector3 posAxis = new Vector3(0,0,0);
	//private Vector3 angAxis = new Vector3(0,0,0);
	//private float cDis;
	//private GameObject MCam;
	
	

	//public float sen_y = 1f;
	//public float sen_z = 1f;

	//public float sen_tilx = 1f;
	//public float sen_tily = 1f;
	//public float sen_rz= -1f;

	public float sen_x = 1f;
	public float x_min = -0.5f;
	public float x_max = 0.5f;


	//public float y_min = -0.5f;
	//public float y_max = 0.5f;

	public float sen_yRot = 20f;
	public float yRot_min = -90f;
	public float yRot_max = 90f;

	public bool x_inv = false;
	public bool yRot_inv = false;


	// Use this for initialization
	void Start () {
		//invert処理
		if(x_inv){sen_x = sen_x * -1;}
		if(yRot_inv){sen_yRot = sen_yRot * -1;}

	//	MCam = GameObject.Find("OVRCameraController");
	}
	
	// Update is called once per frame
	void Update () {

		//バンパーをAxis扱いにしてページの位置を平行移動
		Vector3 slide = new Vector3 (Input.GetAxis ("BumperAxis") * sen_x, 0, 0);
		transform.Translate (slide);
		slide = transform.localPosition; //slide再利用
		transform.localPosition = new Vector3 (Mathf.Clamp (slide.x, x_min, x_max),0,0);



		//cDis = Vector3.Distance(MCam.transform.position, transform.position) * 0.05f;
		//posAxis.Set(Input.GetAxis("Left Horizontal") * sen_x * cDis * cDis , Input.GetAxis("Left Vertical") * sen_y * cDis * cDis , Input.GetAxis("Triggers") * sen_z * cDis);
		//transform.Translate(posAxis);

		//右スティックHorizontalでページを閉じる
		float turn = Input.GetAxis("RHorizontal") * sen_yRot * Mathf.PI / 180;
		// 現在の回転角度を0～360から-180～180に変換
		float rotateY = (transform.localEulerAngles.y > 180) ? transform.localEulerAngles.y - 360 : transform.localEulerAngles.y;
		// 現在の回転角度に入力(turn)を加味した回転角度をMathf.Clamp()を使いminAngleからMaxAngle内に収まるようにする
		float angleY = Mathf.Clamp(rotateY + turn , yRot_min, yRot_max);
		// 回転角度を-180～180から0～360に変換
		angleY = (angleY < 0) ? angleY + 360 : angleY;
		// 回転角度をオブジェクトに適用
		transform.localRotation = Quaternion.Euler(0, angleY, 0);

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
