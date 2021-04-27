using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
	Rigidbody rb;
	[SerializeField] float m_jumpForce;
	bool m_isJump;

	public CameraController cameraScript;

	//ジャンプの溜め時間
	[SerializeField] float JUMP_CHARGE_TIME = 1.0f;
	//ジャンプの溜め時間のタイマー
	float m_jumpChargeTimer = 1.0f;
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


	// Start is called before the first frame update
	void Start()
	{
		//Rigitbodyの取得
		rb = this.GetComponent<Rigidbody>();

		//変数の初期化
		m_isJump = false;
		m_jumpChargeTimer = JUMP_CHARGE_TIME;
		m_landingRigidityTimer = LANDING_RIGIDITY_TIME;
		m_isRigidity = false;
	}

	// Update is called once per frame
	void Update()
	{
		//var rot = this.transform.rotation;

		Transform trans = this.transform;

		//スペースキー入力でジャンプ
		if(Input.GetKeyDown(KeyCode.Space) && m_isJump == false && m_jumpChargeTimer >= JUMP_CHARGE_TIME)
		{
			//ジャンプしたフラグを立てる
			m_isJump = true;
		}
		if(m_isJump == true)
		{
			m_jumpChargeTimer -= Time.deltaTime;

			if(m_jumpChargeTimer < 0.0f)
			{
				rb.AddForce(0, m_jumpForce, 0, ForceMode.Impulse);
				m_isJump = false;
			}
		}

		//着地硬直の時間
		if(m_isRigidity == true)
		{
			//着地したら移動量を無くす
			rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
			
			m_landingRigidityTimer -= Time.deltaTime;
			if(m_landingRigidityTimer <= 0.0f)
			{
				m_isRigidity = false;
				m_landingRigidityTimer = LANDING_RIGIDITY_TIME;
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
				trans.Rotate(new Vector3(0.0f, m_trunSpeed * Time.deltaTime, 0.0f), Space.World);

			}
			//左旋回
			if (Input.GetKey(KeyCode.Q))
			{
				trans.Rotate(new Vector3(0.0f, -m_trunSpeed * Time.deltaTime, 0.0f), Space.World);
			}

		}

		//var angles = new Vector3(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z);
		//var direction = Quaternion.Euler(this.transform.eulerAngles) * Vector3.forward; 

		//前方向のベクトルを求める
		Vector3 moveForward = this.transform.forward * m_velZ + this.transform.right * m_velX;


		//前方向のベクトルの正規化
		//moveForward.Normalize();

		//移動量に値を代入する
		rb.velocity = moveForward + new Vector3(0, rb.velocity.y, 0);
		//スラスター移動
		//else rb.AddForce(moveForward * m_walkSpeed, ForceMode.Force);  


		//this.transform.position += this.transform.forward * m_velZ * m_walkSpeed * Time.deltaTime;
		//this.transform.position += this.transform.right * m_velX * m_walkSpeed * Time.deltaTime;

		//rb.velocity = vel;



	}

	/// <summary>
	/// 歩行
	/// 引数：なし
	/// 戻り値：なし
	/// </summary>
	void Walk()
	{
		//左右移動
		//右方向(+)に移動量を代入する
		if (Input.GetKey(KeyCode.D)) m_velX = m_walkSpeed;
		//左方向(-)に移動量を代入する
		else if (Input.GetKey(KeyCode.A)) m_velX = -m_walkSpeed;
		else m_velX = 0.0f;

		//前後移動
		//前方向(+)に移動量を代入する
		if (Input.GetKey(KeyCode.W)) m_velZ = m_walkSpeed;
		//後方向(-)に移動量を代入する
		else if (Input.GetKey(KeyCode.S)) m_velZ = -m_walkSpeed;
		else m_velZ = 0.0f;
	}


	/// <summary>
	///  ブースト移動
	///  引数：なし
	///  戻り値：なし
	/// </summary>
	void Boost()
	{
		//右移動　X方向の移動速度を加算する
		if (Input.GetKey(KeyCode.D))
		{
			m_velX += 10.0f * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velX >= BOOST_SPEED_MAX) m_velX = BOOST_SPEED_MAX;
		}
		//else
		//{
		//	m_velX -= 1.0f * Time.deltaTime;
		//	if (m_velX <= 0.0f) m_velX = 0.0f;
		//}

		//左移動　X方向の移動速度を減算する
		if (Input.GetKey(KeyCode.A))
		{
			m_velX -= 10.0f * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velX <= -BOOST_SPEED_MAX) m_velX = -BOOST_SPEED_MAX;
		}
		//else
		//{
		//	m_velX += 1.0f * Time.deltaTime;
		//	if (m_velX >= 0.0f) m_velX = 0.0f;
		//}

		//前移動　Z方向の移動速度を加算する
		if (Input.GetKey(KeyCode.W))
		{
			m_velZ += 10.0f * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velZ >= BOOST_SPEED_MAX) m_velZ = BOOST_SPEED_MAX;
		}
		else
		{
			m_velZ -= 10.0f * Time.deltaTime;
			if (m_velZ <= 0.0f) m_velZ = 0.0f;
		}

		//後移動　Z方向の移動速度を減算する
		if (Input.GetKey(KeyCode.S))
		{
			m_velZ -= 10.0f * Time.deltaTime;
			//最大値を越えたら最大値を代入する
			if (m_velZ <= -BOOST_SPEED_MAX) m_velZ = -BOOST_SPEED_MAX;
		}
		else
		{
			m_velZ += 10.0f * Time.deltaTime;
			if (m_velZ >= 0.0f) m_velZ = 0.0f;
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

		//カメラを揺らす
		cameraScript.Shake(m_shakeDuration, m_shakeMagnitude);

		//着地エフェクトの再生
		m_shockWave.Play();

		//ジャンプの溜め時間の設定
		m_jumpChargeTimer = JUMP_CHARGE_TIME;

		//着地硬直のフラグを立てる
		m_isRigidity = true;

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
