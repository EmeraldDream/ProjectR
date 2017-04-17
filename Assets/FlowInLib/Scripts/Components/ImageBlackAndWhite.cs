using UnityEngine;
using System.Collections;

namespace FlowInLib
{
    public class ImageBlackAndWhite : MonoBehaviour
    {
        public Shader mShader;
        private Material mMat;
        private float mLuminosity;
        
        void Start()
        {
            if (mShader == null || !mShader.isSupported || !SystemInfo.supportsImageEffects)
                enabled = false;
        }

        void OnEnable()
        {
            mLuminosity = 0f;

            mMat = new Material(mShader);
            if (mMat == null)
                enabled = false;
        }

        void OnDisable()
        {
            if (mMat != null)
                DestroyImmediate(mMat);
        }
        
        void Update()
        {
            if (mLuminosity < 1f)
                mLuminosity = Mathf.Clamp(mLuminosity + Time.deltaTime, 0f, 1f);
        }

        void OnRenderImage(RenderTexture srcTexture, RenderTexture destTexture)
        {
            if (mMat != null)
            {
                mMat.SetFloat("_Luminosity", mLuminosity);
                Graphics.Blit(srcTexture, destTexture, mMat);
            }
            else
            {
                Graphics.Blit(srcTexture, destTexture);
            }
        }
    }
}