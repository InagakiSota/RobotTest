using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    //Rigidbosy
    Rigidbody rb;
    //カメラのスクリプト
    public CameraController cameraScript;

    //ジャンプ力
    [SerializeField] float JUMP_FORCE;
    //ジャンプ中のフラグ
    bool m_isJump;
    //ジャンプの溜め時間
    [SerializeField] float JUMP_CHARGE_TIME = 1.0f;
    //ジャンプの溜め時間のタイマー
    float m_jumpChargeTimer = 1.0f;
    //カメラ振動の時間
    [SerializeField] float CAMERA_SHAKE_DURATION = 0.25f;
    //カメラ振動の強さ
    [SerializeField] float CAMERA_SHAKE_MAGUNITUDE = 0.1f;
    //旋回速度
    [SerializeField] float TRUN_SPEED = 1.0f;
    //着地エフェクト
    [SerializeReference] ParticleSystem m_shockWave;
    //歩行スピード
    [SerializeField] float WALK_SPEED = 10.0f;
    //ブースト移動スピード
    [SerializeField] float BOOST_MOVE_SPEED = 15.0f;
    //着地硬直のタイマー
    float m_landingRigidityTimer = 0.0f;
    //着地硬直の時間
    [SerializeField] float LANDING_RIGIDITY_TIME = 0.25f;
    //着地硬直のタイマーのフラグ
    bool m_isRigidity = false;

    //X方向の移動量
    float m_velX;
    //Z方向の移動量
    float m_velZ;

    //着地フラグ
    bool m_isStanding;

    private CharacterController charaController;



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
            //ジャンプフラグを立てる
            m_isJump = true;
        }
        //ジャンプフラグが立ったら
        if(m_isJump == true)
        {
            //ジャンプのチャージ時間のタイマーを減算
            m_jumpChargeTimer -= Time.deltaTime;
            //タイマーが0になったら
            if(m_jumpChargeTimer < 0.0f)
            {
                //上方向に力を加える
                rb.AddForce(0, JUMP_FORCE, 0, ForceMode.Impulse);
                //ジャンプのフラグを消す
                m_isJump = false;
            }
        }

        //着地硬直
        if(m_isRigidity == true)
        {
            //着地したら移動量を無くす
            rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
            
            //着地硬直のタイマーを減算
            m_landingRigidityTimer -= Time.deltaTime;
            //タイマーが0になったら
            if(m_landingRigidityTimer <= 0.0f)
            {
                //着地硬直を解除
                m_isRigidity = false;
                //タイマーの時間をを再設定
                m_landingRigidityTimer = LANDING_RIGIDITY_TIME;
            }
        }

        //着地しているかつ着地硬直がなければ移動、または旋回
        if (m_isStanding == true && m_isRigidity == false)
        {
            //右移動
            if (Input.GetKey(KeyCode.D)) m_velX = 1;
            //左移動
            else if (Input.GetKey(KeyCode.A)) m_velX = -1;
            else m_velX = 0.0f;

            //前移動
            if (Input.GetKey(KeyCode.W)) m_velZ = 1;
            //後移動
            else if (Input.GetKey(KeyCode.S)) m_velZ = -1;
            else m_velZ = 0.0f;

            //右旋回
            if (Input.GetKey(KeyCode.E))
            {
                trans.Rotate(new Vector3(0.0f, TRUN_SPEED * Time.deltaTime, 0.0f), Space.World);

            }
            //左旋回
            if (Input.GetKey(KeyCode.Q))
            {
                trans.Rotate(new Vector3(0.0f, -TRUN_SPEED * Time.deltaTime, 0.0f), Space.World);
            }

        }

        //var angles = new Vector3(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z);
        //var direction = Quaternion.Euler(this.transform.eulerAngles) * Vector3.forward; 

        //前方向のベクトル
        Vector3 moveForward = this.transform.forward * m_velZ + this.transform.right * m_velX;

        //vel = new Vector3(
        //    m_velX,
        //    0.0f,
        //    m_velZ);

        //前方向のベクトルの正規化
        moveForward.Normalize();

        //歩行
        if (Input.GetKey(KeyCode.LeftShift) != true)
        {
            rb.velocity = moveForward * WALK_SPEED + new Vector3(0, rb.velocity.y, 0);
        }
        //ブースト移動
        else if (Input.GetKey(KeyCode.LeftShift) == true)
        {
            rb.AddForce(moveForward * BOOST_MOVE_SPEED, ForceMode.Force);
        }



        //this.transform.position += this.transform.forward * m_velZ * m_walkSpeed * Time.deltaTime;
        //this.transform.position += this.transform.right * m_velX * m_walkSpeed * Time.deltaTime;


        //rb.velocity = vel;

    }

    private void FixedUpdate()
    {
        //rb.AddForce(vel, ForceMode.Force);
    }

    
    //着地した瞬間の処理
    private void OnTriggerEnter(Collider other)
    {

        //ジャンプのフラグを消す
        m_isJump = false;

        //カメラを揺らす
        cameraScript.Shake(CAMERA_SHAKE_DURATION, CAMERA_SHAKE_MAGUNITUDE);

        //着地エフェクトの再生
        m_shockWave.Play();

        //ジャンプの溜め時間の設定
        m_jumpChargeTimer = JUMP_CHARGE_TIME;

        //着地硬直のフラグを立てる
        m_isRigidity = true;

    }

    //着地時の処理
    private void OnTriggerStay(Collider other)
    {
        m_isStanding = true;
    }

    //地面から脚が離れた瞬間の処理
    private void OnTriggerExit(Collider other)
    {
        m_isStanding = false;
    }
}
