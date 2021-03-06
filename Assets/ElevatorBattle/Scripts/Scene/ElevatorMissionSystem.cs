﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OperationTrident.Util;
using System;

namespace OperationTrident.Elevator
{
    public class ElevatorMissionSystem : MonoBehaviour
    {

        // 图标离目标有多高
        public float missionLabelOffset = 3.0f;

        // 任务系统！！！的显示字符串的位置
        private Vector3 UIPosition;

        // 是否显示（任务点)，背对着的时候不显示
        private bool toDisplayTheMissionPoint = true;

        // 显示任务目标更新
        //private bool toDisplayNewMission;

        // 在显示的任务目标索引
        private int missionContentsIndex;

        public string[] missionContents = {
            "",
            "开启电梯门",
            "等待电梯启动",
            "请抵御来袭的敌人，活下去！",
            "逃出电梯",
            "找到逃生舱"
        };

        // 任务目标的内容
        private string missionContent;

        // 目标的世界坐标
        private Vector3 targetWorldPosition;

        // 字幕每个字显示的时间
        public float timePerSubTitleWord = 1.0f;

        // 任务目标每个字出现的速度
        public float appearInterval = 0.5f;

        // 任务目标每个乱码闪烁的速度
        public float blingInterval = 0.3f;

        // 任务目标是随机的生成正确的还是顺序
        public bool sequentClear = true;

        private float nowDistance;

        // 是否显示距离
        private bool display;

        string[] missionDetail =
{
                "2048年8月1日 16：35",
                "鲲内部 中央电梯",
                "三叉戟行动"
        };

        // Use this for initialization
        void Start()
        {
            missionContent = String.Empty;
            missionContentsIndex = 0;
            display = false;
        }

        // Update is called once per frame
        void Update()
        {
            // 准备传入任务目标的世界坐标
            targetWorldPosition = new Vector3();
            // 准备传入的任务目标的下标
            switch (SceneController.state)
            {
                case SceneController.ElevatorState.Initing:
                    missionContentsIndex = 1;
                    display = true;
                    targetWorldPosition = new Vector3(7.683f, -4.0818f, 5.3866f);
                    if(Door.state == true)
                    {
                        targetWorldPosition = new Vector3(-10f, -3.8844f, -0.4f);
                    }
                    break;
                case SceneController.ElevatorState.Ready:
                    missionContentsIndex = 2;
                    display = false;
                    break;
                case SceneController.ElevatorState.Start_Fighting:
                    display = false;
                    break;
                case SceneController.ElevatorState.Fighting:
                    missionContentsIndex = 3;
                    display = false;
                    break;
                case SceneController.ElevatorState.End:
                    missionContentsIndex = 4;
                    display = false;
                    break;
                case SceneController.ElevatorState.Escape:
                    missionContentsIndex = 5;
                    display = true;
                    targetWorldPosition = new Vector3(150.96f, -13.647f, 78.372f);
                    break;
            }

            missionContent = missionContents[missionContentsIndex]; // 设置要显示的任务目标内容
            //UIPosition = camera.WorldToScreenPoint(targetWorldPosition);
        }

        private void DisplayNewMission()
        {
            //TODO:
            return;
        }

        //onGUI在每帧被渲染之后执行
        private void OnGUI()
        {
            // 显示任务目标

            if (missionContent != string.Empty)
            {
                //GUIUtil.DisplayMissionTargetDefaultSequently(missionContent, camera,
                //    GUIUtil.brightGreenColor, interval: 0.4f, fontSize: 16, inLeft: true);
                GUIUtil.DisplayMissionTargetInMessSequently(
                    missionContent,
                    Room1.Util.GetCamera(),
                    GUIUtil.whiteColor,
                    interval: appearInterval,
                    blingInterval: blingInterval,
                    fontSize: 16,
                    sequentClear: sequentClear);
            }


            GUIUtil.DisplayMissionDetailDefault(missionDetail, OperationTrident.Room1.Util.GetCamera(), Color.white);
            GUIUtil.DisplayMissionPoint(targetWorldPosition, Room1.Util.GetCamera(), GUIUtil.missionPointColor);




            //string subtitle = "^w你好,^r一勺^w,我是^b鸡哥^w,我们要找到^y飞奔的啦啦啦";
            //GUIUtil.DisplaySubtitleInGivenGrammar(subtitle, camera, 20, 0.8f, subtitle.Length * timePerSubTitleWord);
            //string[] subtitles ={
            //    "^b地球指挥官:^w 根据情报显示，开启电源室入口的^y智能感应芯片^w在仓库里的几个可能位置",
            //    "^b地球指挥官:^w 你们要拿到它，小心里面的^r巡逻机器人"
            //};
            //GUIUtil.DisplaySubtitlesInGivenGrammar(
            //    subtitles, 
            //    camera, 
            //    fontSize: 16,
            //    subtitleRatioHeight: 0.9f, 
            //    secondOfEachWord: 0.5f, 
            //    secondBetweenLine: 3.0f);
        }
    }
}