using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FlowInLib;

namespace ProjectR
{
    public class GameLevelLoading : Loading
    {
        public override void Awake()
        {
            base.Awake();
        }

        protected override void InitUnitList()
        {
            _unitList.Add(new LevelLoadingUnit("game"));
        }

        protected override IEnumerator PrepareLoadingAnim()
        {
            yield break;
        }

        protected override IEnumerator StartLoadingAnim()
        {
            yield break;
        }

        protected override IEnumerator EndLoadingAnim()
        {
            UIController UI = AppRoot.UI;
            if (UI == null)
                yield break;

            //AssetManager.Instance.AutoHandleAsset("Game.prefab", null, true);
        }
    }
}
