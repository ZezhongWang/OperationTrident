﻿using UnityEngine;
using System.Collections;
using OperationTrident.Common;

namespace OperationTrident.FPS.Weapons {
    public enum ProjectileType {
        Standard,
        //追踪特定Tag的物体
        Seeker,
        ClusterBomb
    }
    public enum DamageType {
        Direct,
        Explosion
    }

    public class Projectile : MonoBehaviour {
        [Tooltip("抛射物种类")]
        public ProjectileType SelectedProjectileType = ProjectileType.Standard; 
        [Tooltip("伤害类型")]
        public DamageType SelectedDamageType = DamageType.Direct;
        [Tooltip("伤害大小")]
        public float Damage = 100.0f;
        [Tooltip("移动速度")]
        public float Speed = 10.0f;
        [Tooltip("发射导弹时给导弹施加的力大小,速度为0时设置有效")]
        public float InitialForce = 1000.0f;
        [Tooltip("最长飞行时间")]
        public float Lifetime = 30.0f;

        [Tooltip("追踪目标时的拐弯速率")]
        public float SeekRate = 1.0f;
        [Tooltip("追踪目标的Tag")]
        public string SeekTag = "Enemy";
        [Tooltip("爆炸特效")]
        public GameObject Explosion;
        [Tooltip("更新追踪目标的速率")]
        public float TargetListUpdateRate = 1.0f;

        [Tooltip("炸弹模型")]
        public GameObject ClusterBomb;
        [Tooltip("单次射击发射炸弹的数量")]
        public int ClusterBombNum = 6;
        
        

        //计时器
        private float m_LifeTimer = 0.0f;
        private float m_TargetListUpdateTimer = 0.0f;
        //保存可能的追踪目标
        private GameObject[] m_EnemyList;

        void Start() {
            //更新追踪目标List
            UpdateEnemyList();

            //发射时添加一个初始的力
            GetComponent<Rigidbody>().AddRelativeForce(0, 0, InitialForce);
        }

        // Update is called once per frame
        void Update() {
            //计时
            m_LifeTimer += Time.deltaTime;

            //超过飞行时间进行销毁
            if (m_LifeTimer >= Lifetime) {
                Explode(transform.position);
            }

            //如果没有设置力就给导弹一个初始速度
            if (InitialForce == 0) 
                GetComponent<Rigidbody>().velocity = transform.forward * Speed;

            //追踪型
            if (SelectedProjectileType == ProjectileType.Seeker) {
                //计时
                m_TargetListUpdateTimer += Time.deltaTime;

                //更新目标
                if (m_TargetListUpdateTimer >= TargetListUpdateRate) {
                    UpdateEnemyList();
                }

                if (m_EnemyList != null) {
                    //选择一个最接近飞行方向的目标进行追踪
                    float greatestDotSoFar = -1.0f;
                    Vector3 target = transform.forward * 1000;
                    foreach (GameObject enemy in m_EnemyList) {
                        if (enemy != null) {
                            Vector3 direction = enemy.transform.position - transform.position;
                            float dot = Vector3.Dot(direction.normalized, transform.forward);
                            if (dot > greatestDotSoFar) {
                                target = enemy.transform.position;
                                greatestDotSoFar = dot;
                            }
                        }
                    }

                    //旋转导弹
                    Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * SeekRate);
                }
            }
        }

        void UpdateEnemyList() {
            m_EnemyList = GameObject.FindGameObjectsWithTag(SeekTag);
            m_TargetListUpdateTimer = 0.0f;
        }

        void OnCollisionEnter(Collision col) {
            //撞击到物体
            Hit(col);
        }

        void Hit(Collision col) {
            //播放爆炸特效
            Explode(col.contacts[0].point);

            //让被击中物受到伤害
            if (SelectedDamageType == DamageType.Direct) {
                ReactiveTarget target = col.collider.gameObject.GetComponent<ReactiveTarget>();
                if (target) {
                    //第二个参数设置为true，不分敌我进行伤害
                    target.OnHit(this.gameObject.name, true, Damage);
                } else {
                    target = col.collider.transform.root.GetComponent<ReactiveTarget>();
                    if (target) {
                        //第二个参数设置为true，不分敌我进行伤害
                        target.OnHit(this.gameObject.name, false, Damage);
                    }
                }
            }
        }

        void Explode(Vector3 position) {
            //实例化爆炸特效
            if (Explosion != null) {
                GameObject explosion = Instantiate(Explosion, position, Quaternion.identity);
                explosion.name = this.gameObject.name;

                //Instantiate(Explosion, position, Quaternion.identity);
            }

            //爆炸之后创建新的炸弹
            if (SelectedProjectileType == ProjectileType.ClusterBomb) {
                if (ClusterBomb != null) {
                    for (int i = 0; i <= ClusterBombNum; i++) {
                        Instantiate(ClusterBomb, transform.position, transform.rotation);
                    }
                }
            }

            //销毁炸弹
            Destroy(gameObject);
        }

        //伤害函数
        public void MultiplyDamage(float amount) {
            Damage *= amount;
        }

        //修改初始的力
        public void MultiplyInitialForce(float amount) {
            InitialForce *= amount;
        }
    }
}