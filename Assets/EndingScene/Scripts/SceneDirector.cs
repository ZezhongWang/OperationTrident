﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OperationTrident.Util;

namespace OperationTrident.EndingScene
{
    public class SceneDirector : MonoBehaviour
    {
        //逃生舱
        public GameObject m_EscapingCabin;

        //鲲
        public GameObject m_Kun;

        //太空垃圾铁板
        public GameObject m_SpaceRubbish;
        private Vector3 m_SpaceRubbishInitialPos;//铁板的起始位置，记录下来才方便插值
        private float m_SpaceRubbishPosLerpFactor;

        //爆炸生成器
        public ExplosionGenerator m_ExplosionGenerator;

        enum CameraState
        {
            ROAMING,//一开始缓慢移动,和靠近，用Timeline
            THIRD_PERSON,//第三人称看着逃生舱（可以控制）
            LOOKING_AT_KUN,//脱离第三人称绑定，看着鲲爆炸，不能控制
            VIDEO,//播放视频
        }

        //BGM 4小节的时间
        private const float m_BgmBarTime = 60.0f / 140.0f * 4.0f;

        //当前Camera的状态
        private CameraState m_CamState;

        //场景流逝总时间
        public UnityEngine.Playables.PlayableDirector m_TimeLineDirector;
        private double m_Time = 0.0f;

        //Timeline控制一个Camera_Directed，第三人称控制一个CameraThirdPerson
        //在Timeline里的activation track控制两个camera的enabled
        public Camera m_CamDirected;
        public Camera m_CamFree;

        //第三人称环视的Camera信息
        public float m_MouseLookSensitivity;
        private Vector3 m_ThirdPersonCamOffsetEuler;//第三人称offset向量的欧拉角
        private float m_ThirdPersonCamRotateRadius;
        private Vector3 m_DestCamPos;//真正的Cam.transform要线性插值跟随这个target pos
        private Vector3 m_DestLookat;
        private float m_LookingAtKunCamShakingAmp;//鲲爆炸的时候镜头的抖动幅度

        // Use this for initialization
        void Start()
        {
            m_CamState = CameraState.ROAMING;
            m_SpaceRubbishInitialPos = m_SpaceRubbish.transform.position;
            m_LookingAtKunCamShakingAmp = 1.0f;
        }

        // Update is called once per frame
        void Update()
        {
            m_Time = m_TimeLineDirector.time;
            switch (m_CamState)
            {
                case CameraState.ROAMING:
                    Update_Roaming();
                    break;

                case CameraState.THIRD_PERSON:
                    Update_ThirdPerson();
                    break;

                case CameraState.LOOKING_AT_KUN:
                    Update_LookingAtKun();
                    break;

                case CameraState.VIDEO:
                    Update_Video();
                    break;
            }

        }

        void OnGUI()
        {
            switch (m_CamState)
            {
                case CameraState.ROAMING:

                    break;

                case CameraState.THIRD_PERSON:
                    GUIUtil.DisplayMissionTargetInMessSequently("任务完成，返回基地.", m_CamDirected, Color.white,0.1f);
                    string[] subtitles =
                    {
                        "",//等几秒先
                        "^g蓝星陆战队：^w指挥部，已取回托卡马克之心",
                        "^g蓝星陆战队：^w陆战队所有成员均已登上逃生舱，任务完成",
                        "^g地球指挥部：^w收到，尽快返回海神号进行任务简报。",
                        "^g蓝星陆战队：^w收到。建军节快乐。",
                    };

                    float[] subtitleTime = { 5.0f, 4.0f, 7.0f, 6.0f,3.0f };
                    float[] intervals = {5.0f, 0.5f, 2.0f,  2.0f,1.0f};
                    GUIUtil.DisplaySubtitlesInGivenGrammarWithTimeStamp(
                        subtitles,m_CamFree,GUIUtil.DefaultFontSize, GUIUtil.DefaultSubtitleRatioHeight,subtitleTime, intervals);

                    break;

                case CameraState.LOOKING_AT_KUN:

                    break;

                case CameraState.VIDEO:

                    break;
            }

        }



        /************************************************
         *                           PRIVATE
         * **********************************************/
        private void Update_Roaming()
        {
            if (m_Time > m_BgmBarTime * 8)
            {
                //切换至第三人称状态
                m_CamState = CameraState.THIRD_PERSON;
                //初始化第三人称观察的参数
                m_DestLookat = m_CamDirected.transform.position + m_CamDirected.transform.forward;
                m_DestCamPos = m_CamDirected.transform.position;
                //m_CamFree.transform.position = m_CamDirected.transform.position;
                m_ThirdPersonCamRotateRadius = (m_DestCamPos - m_EscapingCabin.transform.position).magnitude;
                m_ThirdPersonCamOffsetEuler = m_CamFree.transform.eulerAngles;
            }
        }

        private void Update_ThirdPerson()
        {

            if (m_Time > m_BgmBarTime * (8 + 16+ 16))
            {
                //切至下一状态，不再绑定在玩家的第三人称，禁用控制
                //并初始化camera的destPos和destLookat
                m_CamState = CameraState.LOOKING_AT_KUN;
                m_DestLookat = m_EscapingCabin.transform.position;
                m_DestCamPos = m_CamFree.transform.position;
            }
            else
            {
                //鼠标控制target transform的旋转角度
                //实际Camera transform以一定比例Lerp向target transform
                //注意鼠标在x,y上的移动和旋转的欧拉角要对上
                float deltaAngleY = m_MouseLookSensitivity * Input.GetAxis("Mouse X") * Time.deltaTime;
                float deltaAngleX = m_MouseLookSensitivity * Input.GetAxis("Mouse Y") * Time.deltaTime;

                //offset vector的新欧拉角
                m_ThirdPersonCamOffsetEuler.x += deltaAngleX;
                m_ThirdPersonCamOffsetEuler.y += deltaAngleY;
                m_ThirdPersonCamOffsetEuler.z = 0;
                m_ThirdPersonCamOffsetEuler.x = Mathf.Clamp(m_ThirdPersonCamOffsetEuler.x, -89.9f, 89.9f);

                //cam离注视中心的offset vector
                Quaternion rotation = Quaternion.Euler(m_ThirdPersonCamOffsetEuler);
                Vector3 posOffsetFromCenter = rotation * new Vector3(0,0, m_ThirdPersonCamRotateRadius);

                //实际Camera位置向pos/lookat插值
                const float posLerpScale = 10.0f;
                m_DestCamPos = m_EscapingCabin.transform.position + posOffsetFromCenter;
                m_CamFree.transform.position = Vector3.Lerp(m_CamFree.transform.position, m_DestCamPos, posLerpScale * Time.deltaTime);

                //const float lookatLerpScale = 3.0f;
                m_DestLookat = m_EscapingCabin.transform.position;// Vector3.Lerp(m_DestLookat, m_EscapingCabin.transform.position, lookatLerpScale * Time.deltaTime);
                m_CamFree.transform.LookAt(m_DestLookat);
            }
        }

        private void Update_LookingAtKun()
        {
            //计算新的需要插值到的camera pos/lookat
            const float lerpScale = 2.0f;

            //爆炸时镜头加一点震动，显得炸的很激烈
            Vector3 shakePosOffset = new Vector3(
                Random.Range(-m_LookingAtKunCamShakingAmp, m_LookingAtKunCamShakingAmp),
                Random.Range(-m_LookingAtKunCamShakingAmp, m_LookingAtKunCamShakingAmp),
                Random.Range(-m_LookingAtKunCamShakingAmp, m_LookingAtKunCamShakingAmp)
                );
            m_DestCamPos += new Vector3(0, 1.5f, -15.0f) * Time.deltaTime;
            m_CamFree.transform.position = Vector3.Lerp(m_CamFree.transform.position, m_DestCamPos, lerpScale * Time.deltaTime) ;

            m_DestLookat = Vector3.Lerp(m_DestLookat, m_Kun.transform.position, lerpScale * Time.deltaTime);
            m_CamFree.transform.LookAt(m_DestLookat+ shakePosOffset);

            //爆炸特效（越来越密集的爆炸）
            m_ExplosionGenerator.GenerateExplosion();
            //每2个小节改变一点参数
            for(int i=0;i<8;++i)
            {
                if(m_Time > m_BgmBarTime * (8 + 16 + 16 + i*2 ))
                {
                    m_ExplosionGenerator.SetExplodeMaxInterval(0.5f - 0.05f * i);
                    m_LookingAtKunCamShakingAmp = 3f *(i+1);
                }
            }


            //BGM最后8小节，太空垃圾飞过来撞镜头
            if (m_Time > m_BgmBarTime * (8 + 16 +16+ 14))
            {
                m_SpaceRubbishPosLerpFactor += (Time.deltaTime / (m_BgmBarTime * 2));

                //太空垃圾按BGM流逝时间从起始位置插值到camPos
                m_SpaceRubbish.transform.position = Vector3.Lerp(
                    m_SpaceRubbishInitialPos, 
                    m_CamFree.transform.position,
                    m_SpaceRubbishPosLerpFactor);

                //高速自转一下
                m_SpaceRubbish.transform.rotation *= Quaternion.Euler(new Vector3(100f, -80f, 60f) * Time.deltaTime);
            }

            if (m_Time > m_BgmBarTime * (8 + 16 + 16 + 16))
            {
                //切至下一状态，不再绑定在玩家的第三人称，禁用控制
                m_CamState = CameraState.VIDEO;
                return;
            }
        }

        private void Update_Video()
        {

        }
    }

}
