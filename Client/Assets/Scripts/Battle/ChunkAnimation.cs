﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkAnimation : MonoBehaviour
{
    public class Define
    {
        static public float TIME_IDLE = 2.5f;
        static public float TIME_ATTACK_START = 0.1f;
        static public float TIME_ATTACK_DO = 0.05f;
        static public float TIME_ATTACK_END = 0.1f;
        static public float TIME_HITTED = 0.1f;
        static public float TIME_DEFEND = 0.2f;
        static public float TIME_SKIPED = 0.0f;

        static public float TIME_ATTACK = TIME_ATTACK_START + TIME_ATTACK_DO + TIME_ATTACK_END / 2.0f;
    };

    public List<GameObject> ChunkNodeRef;

    List<GameObject> ChunkMapRef;
    GameObject ChunkRef = null;
    GameObject RootRef = null;
    float m_ScaleFactor = 1.0f;
    float m_ElapsedTime = 0.0f;
    TransformCache m_TransformRef;
    AnimationState m_State = AnimationState.InValid;
    AnimationState m_NextState = AnimationState.InValid;

    IEnumerator PerformChangeAnimationState(AnimationState _State, float _DelayTime, bool _RestoreTransform)
    {
        yield return new WaitForSeconds(_DelayTime);

        // perform change
        m_State = AnimationState.InValid;
        m_ElapsedTime = 0.0f;

        if (_RestoreTransform)
        {
            foreach (GameObject go in ChunkMapRef)
            {
                go.transform.parent = RootRef.transform;
            }
            // ChunkMapRef.Clear();
            ChunkRef = null;
            m_TransformRef.Restore();
        }
    }

    public void DoAnimation(
        Character _CharRef,
        string _ChunkNode,
        string _ChunkIndex,
        AnimationState _NextState,
        float _DelayTime = 0.0f)
    {
        RootRef = _CharRef.gameObject;
        ChunkRef = GlobalSingleton.Find(_ChunkNode, true);
        Mesh_VoxelChunk Mesh = _CharRef.mesh as Mesh_VoxelChunk;
        
        // ChunkRef
        if (null != ChunkRef
         && null != RootRef
         && Mesh.Chunks.ContainsKey(_ChunkIndex))
        {
            ChunkMapRef = Mesh.Chunks[_ChunkIndex];
            GlobalSingleton.DEBUG("Ready to change parent:"+ ChunkMapRef.Count);

            foreach (GameObject go in ChunkMapRef)
            {
                // go.transform = null;
                go.transform.SetParent(ChunkRef.transform);
            }
        }

        ChangeAnimationState(_NextState, _DelayTime, true);
    }

    public void ChangeAnimationState(
        AnimationState _NextState, 
        float _DelayTime, 
        bool _RestoreTransform = true)
    {
        m_TransformRef = new TransformCache(ChunkRef.transform);
        // m_ScaleFactor = (-m_RootRef.transform.localScale.x) / m_RootRef.transform.localScale.x;
        m_State = _NextState;

        m_NextState = AnimationState.InValid;
        StartCoroutine(PerformChangeAnimationState(_NextState, _DelayTime, _RestoreTransform));
    }


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (null == ChunkRef)
            return;

        m_ElapsedTime += Time.deltaTime;
        Vector3 pos = ChunkRef.transform.position;

        // GlobalSingleton.DEBUG("Update State:" + m_State);
        switch (m_State)
        {
            case AnimationState.Idle:
                pos.y = 0.5f * Mathf.Sin(Define.TIME_IDLE * m_ElapsedTime);
                ChunkRef.transform.position = pos;
                break;

            case AnimationState.Attack:
                {
                    float dx = -ChunkRef.transform.localScale.x / ChunkRef.transform.localScale.x;
                    float Speed = -dx * Time.deltaTime * 10.0f;
                    ChunkRef.transform.Translate(new Vector3(Speed, 0, 0)); 
                }
                break;

            case AnimationState.Defend:
                
                ChangeAnimationState(AnimationState.Idle, Define.TIME_DEFEND, true);
                break;
        }
    }
}
