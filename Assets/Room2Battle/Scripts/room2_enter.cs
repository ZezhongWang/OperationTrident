﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OperationTrident.Util;

using OperationTrident.FPS.Common;

namespace room2Battle
{
    //==================================================
    //================   走出电梯   ====================
    //=================================================

    public class room2_enter : Subscene, NetSyncInterface
    {
        NetSyncController m_controller;

        protected GameObject playerCamera = null;

        protected List<GameObject> playerCameraMirror = new List<GameObject>();

        protected GetCamera getCamera;

        //[SerializeField]
        //protected GameObject player;

        //挂载脚本的shader，包括dark和depth sensor
        [SerializeField]
        protected Shader shader_dark = null;

        [SerializeField]
        protected Shader shader_depthSensor = null;

        //depthSensor的纹理
        [SerializeField]
        protected Texture waveTexture = null;

        [SerializeField]
        protected Texture waveMaskTexture = null;

        protected bool isNear = false;

        protected bool isEnter = false;

        Camera mCamera;

        public string[] missionDetails =
            {
            "2018.6.22  星期五",
            "外太空",
            "三叉戟行动"
        };

        public string[] line =
        {
        };

        public float wordTransparentInterval = 0.005f; // 字变得更加透明的周期
        public float wordAppearanceInterval = 0.1f; // 每行字一个个出现的速度
        public float lineSubsequentlyInterval = 1.236f; // 每行字一行行出现的速度
        public int fontSize = 16; // 字体大小
        //初始化网络玩家
        protected bool isInit = false;

        //房间开始位置
        [SerializeField]
        protected Transform roomPos;
        //显示目标
        protected bool isShowTarget = false;
        //距离
        protected float distance = float.MaxValue;

        //音源
        [SerializeField]
        protected AudioSource source;
        //音频
        [SerializeField]
        protected AudioClip[] clips;

        protected bool playOnce = false;

        ///=======================================================
        ///================ method  ==============================
        ///=======================================================

        public override void onSubsceneInit()
        {
        }

        public override bool isTransitionTriggered()
        {
            return isEnter;
        }

        public override string GetNextSubscene()
        {
            return "room2_powerroom";
        }

        public override void onSubsceneDestory()
        {

        }

        public override void notify(int i)
        {
            if (i == 1)
            {
                isNear = true;
                m_controller.SyncVariables();
                //初始化
                becomeDark();
                m_controller.RPC(this, "becomeDark");
            }
            else if (i == 2)
            {
                isEnter = true;
                m_controller.SyncVariables();
            }
        }

        void OnGUI()
        {
            if (isInit)
            {
                if (GameMgr.instance)
                {
                    mCamera = getCamera.GetCurrentUsedCamera();
                }


                GUIUtil.DisplayMissionDetailDefault(
                                missionDetails,
                                mCamera,
                                GUIUtil.yellowColor,
                                wordTransparentInterval: wordTransparentInterval,
                                wordAppearanceInterval: wordAppearanceInterval,
                                lineSubsequentlyInterval: lineSubsequentlyInterval,
                                fontSize: fontSize);

                if (!isNear)
                {
                    GUIUtil.DisplayMissionTargetInMessSequently("突入电源室！",
                        mCamera,
                        GUIUtil.yellowColor,
                        0.5f, 0.1f, 16);
                    GUIUtil.DisplaySubtitleInGivenGrammar("^y地球指挥部^w：你们已经进入了电源室，你们需要开启电源，电源室才能正常运作。", mCamera, 16, 0.9f, 0.5f, 3.0f);
                }
                else
                {
                    //深度摄像头是否开启
                    bool open = playerCamera.GetComponent<depthSensor>().enabled;
                    GUIUtil.DisplayMissionTargetInMessSequently("任务变化：开启照明开关！",
                        mCamera,
                        GUIUtil.yellowColor,
                        0.5f, 0.1f, 16);
                    if (!open)
                        GUIUtil.DisplaySubtitleInGivenGrammar("^w按^yH^w开启/关闭探测器", mCamera, 12, 0.7f);
                    GUIUtil.DisplaySubtitlesInGivenGrammar(line, mCamera, 16, 0.9f, 0.2f, 1.2f);
                }

                GUIStyle style = GUIUtil.GetDefaultTextStyle(GUIUtil.FadeAColor(GUIUtil.greyColor, 60.0f));
                Rect rect = GUIUtil.GetFixedRectDirectlyFromWorldPosition(roomPos.position, mCamera);
                // 指定颜色
                if (isShowTarget)
                {
                    GUI.Label(rect, (int)distance + "m", style);
                }
            }
        }

        public void RecvData(SyncData data)
        {
            isNear = (bool)data.Get(typeof(bool));
            isEnter = (bool)data.Get(typeof(bool));
        }

        public SyncData SendData()
        {
            SyncData data = new SyncData();
            data.Add(isNear);
            data.Add(isEnter);
            return data;
        }

        public void Init(NetSyncController controller)
        {
            m_controller = controller;
        }


        void LateUpdate()
        {
            //按G打开夜视仪
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (isInit)
                {
                    //通过只有一个后处理，减少post processing的pass
                    if (playerCamera.GetComponent<becomeDark>().enabled)
                    {
                        playerCamera.GetComponent<depthSensor>().enabled = true;
                        playerCamera.GetComponent<becomeDark>().enabled = false;

                        foreach (GameObject mirror in playerCameraMirror)
                        {
                            mirror.GetComponent<depthSensor>().enabled = true;
                            mirror.GetComponent<becomeDark>().enabled = false;
                        }
                    }
                    else
                    {
                        playerCamera.GetComponent<depthSensor>().enabled = false;
                        playerCamera.GetComponent<becomeDark>().enabled = true;

                        foreach (GameObject mirror in playerCameraMirror)
                        {
                            mirror.GetComponent<depthSensor>().enabled = false;
                            mirror.GetComponent<becomeDark>().enabled = true;
                        }

                    }
                }
            }

        }

        void Update()
        {
            if (isInit)
            {
                mCamera = getCamera.GetCurrentUsedCamera();
                if (mCamera)
                {
                    Vector3 point = new Vector3(mCamera.pixelWidth / 2, mCamera.pixelHeight / 2, 0);

                    Ray ray = mCamera.ScreenPointToRay(point);

                    distance = Vector3.Distance(roomPos.position, playerCamera.GetComponent<Transform>().position);

                    Vector3 direction1 = ray.direction; // 摄像头的方向
                    Vector3 direction2 = roomPos.position - playerCamera.GetComponentInParent<Transform>().position; // 到物体的方向

                    if (Vector3.Dot(direction1, direction2) <= 0)
                        isShowTarget = false;
                    else
                        isShowTarget = true;
                }

                if (isNear)
                {
                    if (!playOnce)
                    {
                        source.clip = clips[1];
                        source.Play();
                        playOnce = true;
                    }
                }
            }
            else
            {
                if (this.enabled)
                {
                    if (GameMgr.instance)//联网状态
                    {
                        GameObject PLAYER = (SceneNetManager.instance.list[GameMgr.instance.id]);
                        getCamera = PLAYER.GetComponent<GetCamera>();
                        PLAYER.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
                        playerCamera = getCamera.MainCamera;
                        foreach (GameObject cam in getCamera.MirrorCameras)
                        {
                            playerCameraMirror.Add(cam);
                        }
                    }

                    //给腰射相机加特效
                    playerCamera.AddComponent<becomeDark>();
                    playerCamera.AddComponent<depthSensor>();

                    //初始化脚本参数
                    (playerCamera.GetComponent<becomeDark>() as becomeDark).m_Shader = shader_dark;
                    playerCamera.GetComponent<becomeDark>().enabled = false;

                    (playerCamera.GetComponent<depthSensor>() as depthSensor).m_Shader = shader_depthSensor;
                    (playerCamera.GetComponent<depthSensor>() as depthSensor).m_WaveColorTexture = waveTexture;
                    (playerCamera.GetComponent<depthSensor>() as depthSensor).m_WaveMaskTexture = waveMaskTexture;
                    playerCamera.GetComponent<depthSensor>().enabled = false;

                    //给倍镜加特效
                    foreach (GameObject mirror in playerCameraMirror)
                    {
                        mirror.AddComponent<becomeDark>();
                        mirror.AddComponent<depthSensor>();

                        //初始化脚本参数
                        (mirror.GetComponent<becomeDark>() as becomeDark).m_Shader = shader_dark;
                        mirror.GetComponent<becomeDark>().enabled = false;

                        (mirror.GetComponent<depthSensor>() as depthSensor).m_Shader = shader_depthSensor;
                        (mirror.GetComponent<depthSensor>() as depthSensor).m_WaveColorTexture = waveTexture;
                        (mirror.GetComponent<depthSensor>() as depthSensor).m_WaveMaskTexture = waveMaskTexture;
                        mirror.GetComponent<depthSensor>().enabled = false;
                    }

                    isInit = true;

                    source.clip = clips[0];
                    source.Play();
                }
            }
        }

        public void becomeDark()
        {
            if (isInit)
            {
                playerCamera.GetComponent<becomeDark>().enabled = true;

                foreach (GameObject mirror in playerCameraMirror)
                {
                    mirror.GetComponent<becomeDark>().enabled = true;
                }
            }
        }
    }
}
