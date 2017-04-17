using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowInLib
{
    public class CloneTrail : MonoBehaviour
    {
        class CloneInfo
        {
            public Mesh mMesh;
            public MaterialPropertyBlock mProp;
            public float mCreateTime;

            public void Clear()
            {
                if (mMesh != null)
                {
                    mMesh.Clear();
                    mMesh = null;
                }

                if (mProp != null)
                {
                    mProp.Clear();
                    mProp = null;
                }
            }

            public bool Update(float curTime, float duration, float fadeTime)
            {
                float deltaTime = curTime - mCreateTime;
                if (deltaTime >= duration + fadeTime)
                    return false;

                if (deltaTime < duration || fadeTime <= 0f)
                {
                    mProp.SetFloat("Alpha", 1f);
                }
                else
                {
                    mProp.SetFloat("Alpha", 1f - (deltaTime - duration) / fadeTime);
                }
                return true;
            }
        }

        public int mMaxNum;
        public bool mByDistance;
        public bool mOnlySkinnedMesh;
        public float mDistance;
        public float mInterval;

        public float mLifeTime;
        public float mFadeTime;

        public Material mMaterial;

        private List<CloneInfo> mCloneList = new List<CloneInfo>();
        private Vector3 mLastPos;
        private float mLastTime;
        private bool mToggleOn;

        void Awake()
        {
            enabled = false;
            mToggleOn = false;
        }

        void OnEnable()
        {
            Toggle(true);
        }

        void OnDestroy()
        {
            RemoveAllClones();
        }

        public void Toggle(bool on)
        {
            if (mToggleOn == on)
                return;

            mToggleOn = on;

            if (on)
            {
                if (mByDistance)
                    mLastPos = GetComponent<Transform>().position;
                else
                    mLastTime = Time.realtimeSinceStartup;
            }
        }

        void Update()
        {
            if (!mToggleOn)
                return;

            if (mByDistance)
            {
                Vector3 curPos = GetComponent<Transform>().position;
                if (Vector3.Distance(curPos, mLastPos) >= mDistance)
                {
                    AddClone();
                    mLastPos = curPos;
                }
            }
            else
            {
                float curTime = Time.realtimeSinceStartup;
                if (curTime - mLastTime >= mInterval)
                {
                    AddClone();
                    mLastTime = curTime;
                }
            }
        }

        void LateUpdate()
        {
            float curTime = Time.realtimeSinceStartup;

            for (int i = mCloneList.Count - 1; i >= 0; --i)
            {
                CloneInfo clone = mCloneList[i];
                if (clone == null)
                {
                    mCloneList.RemoveAt(i);
                    continue;
                }

                if (!clone.Update(curTime, mLifeTime, mFadeTime))
                {
                    clone.Clear();
                    mCloneList.RemoveAt(i);
                    continue;
                }

                Graphics.DrawMesh(clone.mMesh, Matrix4x4.identity, mMaterial, gameObject.layer, Camera.main, 0, clone.mProp);
            }

            if (!mToggleOn && mCloneList.Count <= 0)
            {
                this.enabled = false;
            }
        }

        void AddClone()
        {
            List<CombineInstance> comInsts = new List<CombineInstance>();

            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < renderers.Length; ++i)
            {
                SkinnedMeshRenderer renderer = renderers[i];
                if (!renderer.enabled || renderer.sharedMesh == null)
                    continue;

                Mesh mesh = new Mesh();
                renderer.BakeMesh(mesh);

                for (int j = renderer.sharedMesh.subMeshCount - 1; j >= 0; --j)
                {
                    CombineInstance inst = new CombineInstance()
                    {
                        mesh = mesh,
                        subMeshIndex = j,
                        transform = renderers[i].transform.localToWorldMatrix
                    };
                    comInsts.Add(inst);
                }
            }

            if (!mOnlySkinnedMesh)
            {
                MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
                for (int i = 0; i < filters.Length; ++i)
                {
                    MeshFilter filter = filters[i];
                    if (filter.sharedMesh == null)
                        continue;

                    MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                    if (renderer == null || !renderer.enabled)
                        continue;

                    for (int j = filter.sharedMesh.subMeshCount - 1; j >= 0; --j)
                    {
                        CombineInstance inst = new CombineInstance()
                        {
                            mesh = filter.sharedMesh,
                            subMeshIndex = j,
                            transform = renderers[i].transform.localToWorldMatrix
                        };
                        comInsts.Add(inst);
                    }
                }
            }

            if (comInsts.Count <= 0)
                return;

            Mesh comMesh = new Mesh();
            comMesh.CombineMeshes(comInsts.ToArray(), true, true);

            CloneInfo clone = new CloneInfo()
            {
                mMesh = comMesh,
                mProp = new MaterialPropertyBlock(),
                mCreateTime = Time.realtimeSinceStartup
            };

            mCloneList.Add(clone);
            if (mCloneList.Count > mMaxNum)
                RemoveClone();
        }

        void RemoveClone(int index = 0)
        {
            if (index >= mCloneList.Count)
                return;

            CloneInfo clone = mCloneList[index];
            if (clone != null)
                clone.Clear();

            mCloneList.RemoveAt(index);
        }

        void RemoveAllClones()
        {
            for (int i = 0; i < mCloneList.Count; ++i)
            {
                if (mCloneList[i] != null)
                    mCloneList[i].Clear();
            }

            mCloneList.Clear();
        }
    }
}