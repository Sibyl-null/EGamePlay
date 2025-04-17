using EGamePlay;
using EGamePlay.Combat;
using ET;
using System.Threading;
using Sirenix.OdinInspector;

public class EGamePlayInit : SerializedMonoBehaviour
{
    public static EGamePlayInit Instance { get; private set; }
    public ReferenceCollector ConfigsCollector;

    private void Awake()
    {
        Instance = this;
        SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
        var ecsNode = ECSNode.Create();
        ecsNode.AddChild<TimerManager>();
        ecsNode.AddChild<CombatContext>();
        ecsNode.AddComponent<ConfigManageComponent>(ConfigsCollector);
    }

    private void Update()
    {
        ThreadSynchronizationContext.Instance.Update();
        ECSNode.Instance.Update();
        TimerManager.Instance.Update();
    }

    private void FixedUpdate()
    {
        ECSNode.Instance.FixedUpdate();
    }

    private void OnApplicationQuit()
    {
        ECSNode.Destroy();
    }
}
