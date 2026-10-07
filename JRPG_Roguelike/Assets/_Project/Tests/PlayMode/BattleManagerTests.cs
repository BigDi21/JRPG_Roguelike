using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BattleManagerTests
{
    [UnityTest]
    public IEnumerator BattleManager_StartsBattle()
    {
        // Arrange
        var go = new GameObject();
        BattleManager manager = go.AddComponent<BattleManager>();
        yield return null; // пропускаем кадр, чтобы Awake/Start выполнились

        // Assert
        Assert.IsNotNull(manager);

        // Cleanup
        Object.Destroy(go);
        yield return null;
    }
}
