using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotController : MonoBehaviour
{
	Rigidbody rb;

	//カメラのスクリプト
	[SerializeField]CameraController cameraScript;
	//着地エフェクト
	[SerializeReference] ParticleSystem m_shockWave;
	//ジャンプの溜めゲージのスライダー
	[SerializeField]Slider m_jumpSlider;
	//ブースト容量のスライダー
	[SerializeField]Slider m_boostCapSlider;
	//ブースト容量のゲージの画像
	[SerializeField]Image m_boostGageImage;

	//ジャンプ力
	[SerializeField] float JUMP_FORCE;
	//着地硬直のタイマー
	float m_landingRigidityTimer = 0.0f;
	//着地硬直の時間
	[SerializeField] float LANDING_RIGIDITY_TIME = 0.25f;
	//ジャンプの溜めゲージの最大値
	[SerializeField] float JUMP_GAGE_MAX = 2.0f;
	//ジャンプの溜めの上昇値
	[SerializeField] float JUMP_CHARGE_UP_VALUE = 0.5f;
	//ジャンプのゲージ消費量
	[SerializeField] float JUMP_GAGE_CONSUMPTION = 10.0f;
	//カメラ振動の時間
	[SerializeField] float SHAKE_DURATION = 0.25f;
	//カメラ振動の強さ
	[SerializeField] float SHAKE_MAGNITUDE = 0.1f;
	//旋回速度(一秒間に回転する角度)
	[SerializeField] float TRUN_SPEED = 1.0f;
	//歩行スピード
	[SerializeField] float WALK_SPEED = 10.0f;
	//最大ブースト速度
	[SerializeField] float BOOST_SPEED_MAX;
	//ブースト移動の速度の上昇値
	[SerializeField] float BOOST_SPEED_UP_VALUE;
	//ブースト容量のゲージの最大値
	[SerializeField] float BOOST_GAGE_MAX = 100.0f;
	//ブースト容量ゲージの減少量
	[SerializeField] float BOOST_GAGE_DOWN_VALUE = 1.0f;
	//ブースト容量ゲージの上昇量
	[SerializeField] float BOOST_GAGE_UP_VALUE = 3.0f;
	//初期座標(リスポーン座標)
	[SerializeField] Vector3 START_POS = new Vector3(0, 20.0f, 0);

	//X方向の移動量
	float m_velX;
	//Z方向の移動量
	float m_velZ;

	//ジャンプしているかどうかのフラグ
	bool m_isJump;
	//着地フラグ
	bool m_isStanding;
	//着地硬直のタイマーのフラグ
	bool m_isRigidity = false;
	//オーバーヒートのフラグ
	bool m_isOverHeat = false;


	// Start is called before the first frame update
	void Start()
	{
		//Rigitbodyの取得
		rb = this.GetComponent<Rigidbody>();

		//変数の初期化
		m_isJump = false;
		m_landingRigidityTimer = LANDING_RIGIDITY_TIME;
		m_isRigidity = false;
		m_jumpSlider.maxValue = JUMP_GAGE_MAX;
		m_jumpSlider.gameObject.SetActive(false);
		this.transform.position = START_POS;
		m_boostCapSlider.value = m_boostCapSlider.maxValue = BOOST_GAGE_MAX;

	}

	// Update is called once per frame
	void Update()
	{
		//var rot = this.transform.rotation;

		Transform trans = this.transform;

		//オーバーヒート状態でなければジャンプ
		if (m_isOverHeat == false)
		{
			Jump();
		}

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
			if (Input.GetKey(KeyCode.LeftShift) != true || m_isOverHeat == true)
			{
				Walk();

			}

			//LShift入力かつオーバーヒート状態でなければブースト移動
			else if (Input.GetKey(KeyCode.LeftShift) == true && m_isOverHeat == false)
			{
				Boost();
			}

			//右旋回
			if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.RightArrow))
			{
				///////////////////////////////////
				//ココ↓に記述
				trans.Rotate(new Vector3(0.0f, TRUN_SPEED * Time.deltaTime, 0.0f));
				///////////////////////////////////


			}
			//左旋回
			if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow))
			{
				///////////////////////////////////
				//ココ↓に記述
				trans.Rotate(new Vector3(0.0f, -TRUN_SPEED * Time.deltaTime, 0.0f));
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

		//ブーストゲージの処理
		BoostGage();

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
			m_velX = WALK_SPEED;
			//////////////////////////////
		}
		//左方向(-)に移動量を代入する
		else if (Input.GetKey(KeyCode.A))
		{
			//////////////////////////////
			//ココ↓に記述
			m_velX = -WALK_SPEED;
			//////////////////////////////
		}
		else m_velX = 0.0f;

		//前後移動
		//前方向(+)に移動量を代入する
		if (Input.GetKey(KeyCode.W))
		{
			//////////////////////////////
			//ココ↓に記述
			m_velZ = WALK_SPEED;
			//////////////////////////////

		}
		//後方向(-)に移動量を代入する
		else if (Input.GetKey(KeyCode.S))
		{
			//////////////////////////////
			//ココ↓に記述
			m_velZ = -WALK_SPEED;
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
			m_velX += BOOST_SPEED_UP_VALUE * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velX >= BOOST_SPEED_MAX) m_velX = BOOST_SPEED_MAX;


		}

		//左移動　X方向の移動速度を減算する
		else if (Input.GetKey(KeyCode.A))

		{
			m_velX -= BOOST_SPEED_UP_VALUE * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velX <= -BOOST_SPEED_MAX) m_velX = -BOOST_SPEED_MAX;

		}
		//左右どちらかの入力がなければ減速
		else
		{
			if(m_velX < 0.0f)
			{
				m_velX += 10.0f * Time.deltaTime;
				if (m_velX >= 0.0f) m_velX = 0.0f;
			}
			else if (m_velX > 0.0f)
			{
				m_velX -= 10.0f * Time.deltaTime;
				if (m_velX <= 0.0f) m_velX = 0.0f;
			}

		}

		//前移動　Z方向の移動速度を加算する
		if (Input.GetKey(KeyCode.W))
		{
			m_velZ += BOOST_SPEED_UP_VALUE * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velZ >= BOOST_SPEED_MAX) m_velZ = BOOST_SPEED_MAX;

		}
		//後移動　Z方向の移動速度を減算する
		else if (Input.GetKey(KeyCode.S))
		{
			m_velZ -= BOOST_SPEED_UP_VALUE * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velZ <= -BOOST_SPEED_MAX) m_velZ = -BOOST_SPEED_MAX;

		}
		//前後どちらかの入力がなければ減速
		else
		{
			if (m_velZ < 0.0f)
			{
				m_velZ += 10.0f * Time.deltaTime;
				if (m_velZ >= 0.0f) m_velZ = 0.0f;
			}
			else if (m_velZ > 0.0f)
			{
				m_velZ -= 10.0f * Time.deltaTime;
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
			m_jumpSlider.value += JUMP_CHARGE_UP_VALUE * Time.deltaTime;
			//溜めゲージを表示する
			m_jumpSlider.gameObject.SetActive(true);

		}

		//ジャンプ力を計算する
		float jumpForce = JUMP_FORCE * (1.0f + m_jumpSlider.value);

		//スペースキーリリースでジャンプ
		if (Input.GetKeyUp(KeyCode.Space) && m_isJump == false && m_isStanding == true && m_isRigidity == false)
		{
			//////////////////////////////
			//ココ↓に記述
			rb.AddForce(0, jumpForce, 0, ForceMode.Impulse);
			//////////////////////////////

			//ブースト容量を消費
			m_boostCapSlider.value -= JUMP_GAGE_CONSUMPTION;
		}
	}

		private void FixedUpdate()
	{

	}

	private void OnTriggerEnter(Collider other)
	{

		//ジャンプのフラグを消す
		m_isJump = false;

		///////////////////////////////
		//カメラを揺らす
		//ココ↓に記述
		cameraScript.Shake(SHAKE_DURATION, SHAKE_MAGNITUDE);
		///////////////////////////////

		//着地エフェクトの再生
		//m_shockWave.Play();

		//着地硬直のフラグを立てる
		m_isRigidity = true;

		//溜めゲージを非表示にする
		m_jumpSlider.gameObject.SetActive(false);

		//溜めゲージを0にする
		m_jumpSlider.value = 0.0f;

	}

	private void OnTriggerStay(Collider other)
	{
		m_isStanding = true;
	}

	private void OnTriggerExit(Collider other)
	{
		m_isStanding = false;
	}


	///////////////////////////////
	//ブーストゲージの処理
	//引数：なし
	//戻り値：なし
	//////////////////////////////
	void BoostGage()
	{
		//ブースト移動中はゲージを減らす
		if (Input.GetKey(KeyCode.LeftShift) && m_isStanding == true && m_isOverHeat == false)
		{
			if (Input.GetKey(KeyCode.W) ||
				Input.GetKey(KeyCode.A) ||
				Input.GetKey(KeyCode.S) ||
				Input.GetKey(KeyCode.D))
			{
				m_boostCapSlider.value -= BOOST_GAGE_DOWN_VALUE * Time.deltaTime;
			}
			//ブースト移動解除でゲージ回復
			else m_boostCapSlider.value += BOOST_GAGE_UP_VALUE * Time.deltaTime;
		}
		//ブースト移動解除でゲージ回復
		else if (Input.GetKey(KeyCode.LeftShift) == false && m_isStanding == true)
		{
			m_boostCapSlider.value += BOOST_GAGE_UP_VALUE * Time.deltaTime;

		}

		//ゲージが0になったらオーバーヒートさせる
		if(m_boostCapSlider.value <= 0.0f)
		{
			m_isOverHeat = true;
		}

		//オーバーヒート状態になったらブースト容量ゲージを赤色にする
		if (m_isOverHeat == true)
		{
			m_boostGageImage.color = Color.red;
			m_boostCapSlider.value += (BOOST_GAGE_UP_VALUE * 2.0f) * Time.deltaTime;
			if (m_boostCapSlider.value >= BOOST_GAGE_MAX) m_isOverHeat = false;
		}
		else
		{
			m_boostGageImage.color = Color.yellow;
		}

	}
}
