using UnityEngine;
using System;
using System.Collections;

namespace FlowInLib
{
    public class LevelLoadingUnit : LoadingUnit
    {
        private AsyncOperation _asyncOp = null;
        private string _levelName = null;

        public LevelLoadingUnit(string levelName = null)
        {
            _levelName = levelName;
        }

        public override int Weight
        {
            get { return 50; }
        }

        public override float Progress
        {
            get { return _asyncOp == null ? 0 : _asyncOp.progress; }
        }

        public override IEnumerator DoWork()
        {
            float beginTime = Time.unscaledTime;
            while (_levelName == null)
            {
                if (Time.unscaledTime - beginTime > 5.0f)
                {
                    LogManager.Debug("[LevelLoadingUnit::DoWork]未指定场景");
                    yield break;
                }
                else
                    yield return null;
            }

            _asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_levelName);
            yield return _asyncOp;
        }

        public override void OnEvent(string strEvent, object param)
        {
            if (strEvent == "levelName")
                _levelName = (string)param;
        }
    }
}
