﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotController : MonoBehaviour
{
	Rigidbody rb;
	//ジャンプ力
	[SerializeField] float JUMP_FORCE;
	bool m_isJump;

	public CameraController cameraScript;

	//カメラ振動の時間
	[SerializeField] float m_shakeDuration = 0.25f;
	//カメラ振動の強さ
	[SerializeField] float m_shakeMagnitude = 0.1f;
	//旋回速度(一秒間に回転する角度)
	[SerializeField] float m_trunSpeed = 1.0f;
	//着地エフェクト
	[SerializeReference] ParticleSystem m_shockWave;
	//歩行スピード
	[SerializeField] float m_walkSpeed = 10.0f;
	//着地硬直のタイマー
	float m_landingRigidityTimer = 0.0f;
	//着地硬直の時間
	[SerializeField] float LANDING_RIGIDITY_TIME = 0.25f;

	//最大ブースト速度
	[SerializeField] float BOOST_SPEED_MAX;

	//ジャンプの溜めゲージのスライダー
	[SerializeField] Slider m_boostSlider;


	//X方向の移動量
	float m_velX;
	//Z方向の移動量
	float m_velZ;

	//着地フラグ
	bool m_isStanding;

	//移動量
	Vector3 vel;

	//着地硬直のタイマーのフラグ
	bool m_isRigidity = false;

	//初期座標(リスポーン座標)
	[SerializeField] Vector3 START_POS = new Vector3(0, 20.0f, 0);


	// Start is called before the first frame update
	void Start()
	{
		//Rigitbodyの取得
		rb = this.GetComponent<Rigidbody>();

		//変数の初期化
		m_isJump = false;
		m_landingRigidityTimer = LANDING_RIGIDITY_TIME;
		m_isRigidity = false;
		m_boostSlider.maxValue = 2.0f;
		m_boostSlider.gameObject.SetActive(false);
		this.transform.position = START_POS;

	}

	// Update is called once per frame
	void Update()
	{
		//var rot = this.transform.rotation;

		Transform trans = this.transform;

		//ジャンプ関数
		Jump();

		//着地硬直
		if (m_isRigidity == true)
		{
			//着地したら移動量を無くす
			rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
			//着地硬直のタイマーを減らす
			m_landingRigidityTimer -= Time.deltaTime;
			//着地硬直のタイマーが0になったら着地硬直を解除
			if(m_landingRigidityTimer <= 0.0f)
			{
				///////////////////////////////////
				//着地硬直解除の処理
				//ココ↓に記述
				m_isRigidity = false;
				m_landingRigidityTimer = LANDING_RIGIDITY_TIME;
				///////////////////////////////////

			}
		}

		//着地しているかつ着地硬直がなければ移動、または旋回
		if (m_isStanding == true && m_isRigidity == false)
		{
			//歩行
			if (Input.GetKey(KeyCode.LeftShift) != true)
			{
				Walk();

				Debug.Log("Walk");
			}

			//LShift入力でブースト移動
			else if (Input.GetKey(KeyCode.LeftShift) == true)
			{
				Boost();
				Debug.Log("Boost");
			}

			//右旋回
			if (Input.GetKey(KeyCode.E))
			{
				///////////////////////////////////
				//ココ↓に記述
				trans.Rotate(new Vector3(0.0f, m_trunSpeed * Time.deltaTime, 0.0f));
				///////////////////////////////////


			}
			//左旋回
			if (Input.GetKey(KeyCode.Q))
			{
				///////////////////////////////////
				//ココ↓に記述
				trans.Rotate(new Vector3(0.0f, -m_trunSpeed * Time.deltaTime, 0.0f));
				///////////////////////////////////
			}

		}

		//前方向のベクトルを求める
		Vector3 moveForward = this.transform.forward * m_velZ + this.transform.right * m_velX;

		//移動量に値を代入する
		rb.velocity = moveForward + new Vector3(0, rb.velocity.y, 0);

		//場外に落ちたらリスポーン
		if (this.transform.position.y < -10.0f)
		{
			this.transform.position = START_POS;
			rb.velocity = Vector3.zero;
		}
	}

	//////////////////////////////////////
	/// 歩行
	/// 引数：なし
	/// 戻り値：なし
	/// /////////////////////////////////
	void Walk()
	{
		//左右移動
		//右方向(+)に移動量を代入する
		if (Input.GetKey(KeyCode.D))
		{
			//////////////////////////////
			//ココ↓に記述
			m_velX = m_walkSpeed;
			//////////////////////////////
		}
		//左方向(-)に移動量を代入する
		else if (Input.GetKey(KeyCode.A))
		{
			//////////////////////////////
			//ココ↓に記述
			m_velX = -m_walkSpeed;
			//////////////////////////////
		}
		else m_velX = 0.0f;

		//前後移動
		//前方向(+)に移動量を代入する
		if (Input.GetKey(KeyCode.W))
		{
			//////////////////////////////
			//ココ↓に記述
			m_velZ = m_walkSpeed;
			//////////////////////////////

		}
		//後方向(-)に移動量を代入する
		else if (Input.GetKey(KeyCode.S))
		{
			//////////////////////////////
			//ココ↓に記述
			m_velZ = -m_walkSpeed;
			//////////////////////////////
		}
		else m_velZ = 0.0f;
	}


	//////////////////////////////////////
	/// ブースト移動
	/// 引数：なし
	/// 戻り値：なし
	/// /////////////////////////////////
	void Boost()
	{
		//右移動　X方向の移動速度を加算する
		if (Input.GetKey(KeyCode.D))
		{
			m_velX += 10.0f * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velX >= BOOST_SPEED_MAX) m_velX = BOOST_SPEED_MAX;
		}

		//左移動　X方向の移動速度を減算する
		else if (Input.GetKey(KeyCode.A))
		{
			m_velX -= 10.0f * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velX <= -BOOST_SPEED_MAX) m_velX = -BOOST_SPEED_MAX;
		}
		//左右どちらかの入力がなければ減速
		else
		{
			if(m_velX < 0.0f)
			{
				m_velX += 1.0f;
				if (m_velX >= 0.0f) m_velX = 0.0f;
			}
			else if (m_velX > 0.0f)
			{
				m_velX -= 1.0f;
				if (m_velX <= 0.0f) m_velX = 0.0f;
			}

		}

		//前移動　Z方向の移動速度を加算する
		if (Input.GetKey(KeyCode.W))
		{
			m_velZ += 10.0f * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velZ >= BOOST_SPEED_MAX) m_velZ = BOOST_SPEED_MAX;
		}
		//後移動　Z方向の移動速度を減算する
		else if (Input.GetKey(KeyCode.S))
		{
			m_velZ -= 10.0f * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velZ <= -BOOST_SPEED_MAX) m_velZ = -BOOST_SPEED_MAX;
		}
		//前後どちらかの入力がなければ減速
		else
		{
			if (m_velZ < 0.0f)
			{
				m_velZ += 1.0f;
				if (m_velZ >= 0.0f) m_velZ = 0.0f;
			}
			else if (m_velZ > 0.0f)
			{
				m_velZ -= 1.0f;
				if (m_velZ <= 0.0f) m_velZ = 0.0f;
			}

		}

	}

	///////////////////////////////
	//ジャンプ
	//引数：なし
	//戻り値：なし
	//////////////////////////////
	void Jump()
	{
		//スペースキー入力でジャンプのゲージ溜め
		if (Input.GetKey(KeyCode.Space) && m_isJump == false && m_isStanding == true && m_isRigidity == false)
		{
			m_boostSlider.value += 0.5f * Time.deltaTime;
			//溜めゲージを表示する
			m_boostSlider.gameObject.SetActive(true);

		}

		//ジャンプ力を計算する
		float jumpForce = JUMP_FORCE * (1.0f + m_boostSlider.value);

		//スペースキーリリースでジャンプ
		if (Input.GetKeyUp(KeyCode.Space) && m_isJump == false && m_isStanding == true && m_isRigidity == false)
		{
			//////////////////////////////
			//ココ↓に記述
			rb.AddForce(0, jumpForce, 0, ForceMode.Impulse);
			//////////////////////////////

		}
	}

		private void FixedUpdate()
	{
		//rb.AddForce(vel, ForceMode.Force);
	}

	private void OnTriggerEnter(Collider other)
	{

		//ジャンプのフラグを消す
		m_isJump = false;

		///////////////////////////////
		//カメラを揺らす
		//ココ↓に記述
		cameraScript.Shake(m_shakeDuration, m_shakeMagnitude);
		///////////////////////////////


		//着地エフェクトの再生
		//m_shockWave.Play();

		//着地硬直のフラグを立てる
		m_isRigidity = true;

		//溜めゲージを非表示にする
		m_boostSlider.gameObject.SetActive(false);

		//溜めゲージを0にする
		m_boostSlider.value = 0.0f;

	}

	private void OnTriggerStay(Collider other)
	{
		m_isStanding = true;
	}

	private void OnTriggerExit(Collider other)
	{
		m_isStanding = false;
	}
}
