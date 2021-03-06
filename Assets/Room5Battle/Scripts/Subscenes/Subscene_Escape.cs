﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OperationTrident.Util;
using System;

namespace OperationTrident.Room5
{
    //第一次进入房间后，
    public class Subscene_Escape : Subscene
    {
        //反应柱（下降）
        public TokamakReactorPillar m_ReactorPillar;
        //核心（下降）
        public TokamakCore m_ReactorCore;

        public GameObject m_EnterNextSceneCube;

        //播放出口门打开的的动画
        public UnityEngine.Playables.PlayableDirector m_ExitDoorOpenDirector;

        //音效的音源（非BGM)
        public AudioSource m_AudioSource_SoundEffect;

        //音效：核心关闭+台词：“逃离反应室”
        public AudioClip m_AudioClip_CoreClose;
        public AudioClip m_AudioClip_Evacuate;
        bool m_isCoreCloseAudioPlayed = false;
        bool m_isEvacuateAudioPlayed = false;
        bool m_isExitDoorOpened=false;

        public override bool isTransitionTriggered()
        {
            //Room5 battle逃脱完就结束啦
            return false;
        }

        //@brief 返回下一个子场景
        public override int GetNextSubscene()
        {
            //Room5 battle逃脱完就结束啦，没有下一个状态了
            return c_InvalidStateId;
        }

        //@brief 子场景的初始化，可以在初始化阶段将所有元素的行为模式改为此状态下的逻辑
        public override void onSubsceneInit()
        {
            m_ReactorCore.Shutdown();
            m_ReactorPillar.Shutdown();

        }

        //@brief 善后工作
        public override void onSubsceneDestory()
        {
        }

        /***************************************************
         *                     Subscene's controller
         * *************************************************/
        private void Start()
        {
            m_isCoreCloseAudioPlayed = false;
            m_isEvacuateAudioPlayed = false;
            m_isExitDoorOpened = false;
        }

        private void Update()
        {
            //如果拿到了反应核心
            if (m_ReactorCore == null && m_isExitDoorOpened==false)
            {
                //出口的门打开
                m_ExitDoorOpenDirector.Play();
                m_isExitDoorOpened = true;
                //下一场景的出口active一下
                m_EnterNextSceneCube.SetActive(true);
            }

            //播放 音效断电+"拿上核心"
            if (!m_isCoreCloseAudioPlayed)
            {
                m_AudioSource_SoundEffect.clip = m_AudioClip_CoreClose;
                m_AudioSource_SoundEffect.Play();
                m_isCoreCloseAudioPlayed = true;
            }

            //拿到核心，播放"撤退"台词
            if (m_ReactorCore == null && !m_isEvacuateAudioPlayed)
            {
                m_AudioSource_SoundEffect.clip = m_AudioClip_Evacuate;
                m_AudioSource_SoundEffect.Play();
                m_isEvacuateAudioPlayed = true;
            }
        }

        private void OnGUI()
        {
            if (m_ReactorCore != null)
            {
                GUIUtil.DisplayMissionTargetDefault("夺回托卡马克之心.", Room5.GetCameraUtil.GetCurrentCamera(), Color.white);
                GUIUtil.DisplayMissionPoint(m_ReactorCore.transform.position, GetCameraUtil.GetCurrentCamera(), Color.white,labelOffsetHeight:5.0f);
            }
            else
            {
                GUIUtil.DisplayMissionTargetDefault("逃离中央控制室.", Room5.GetCameraUtil.GetCurrentCamera(), Color.white);
                GUIUtil.DisplayMissionPoint(m_EnterNextSceneCube.transform.position, GetCameraUtil.GetCurrentCamera(), Color.white);
            }

            //播放 音效断电+"拿上核心"
            if (m_ReactorCore != null && m_isCoreCloseAudioPlayed)
            {
                //字幕
                string[] subtitles ={ "", "^g队长^w：拿上核心，准备撤退", };
                float[] lastingTime = { 4.6f, 1.6f };
                float[] intervals = { 0.0f, 0.0f };
                GUIUtil.DisplaySubtitlesInGivenGrammarWithTimeStamp(subtitles, GetCameraUtil.GetCurrentCamera(), 20, 0.9f, lastingTime, intervals);
            }

            //拿到核心，"撤退”
            if (m_ReactorCore == null && m_isEvacuateAudioPlayed)
            {
                //字幕
                string[] subtitles = { "^g队长^w：鲲的自毁程序即将启动，动作快一点！"};
                float[] lastingTime = { 2.6f };
                float[] intervals = { 0.0f };
                GUIUtil.DisplaySubtitlesInGivenGrammarWithTimeStamp(subtitles, GetCameraUtil.GetCurrentCamera(), 20, 0.9f, lastingTime, intervals);
            }
        }


        /*
        public void RecvData(SyncData data)
        {
            isGetTorcore = (bool)data.Get(typeof(bool));
        }

        public SyncData SendData()
        {
            SyncData data = new SyncData();
            data.Add(isGetTorcore);
            return data;
        }

        public void Init(NetSyncController controller)
        {
            
        }
        */
    }
}