using System;
using UnityEngine;

namespace EGamePlay.Combat
{
    public class EntityDeadEvent
    {
        public Entity DeadEntity;
    }

    /// <summary>
    /// 战斗实体
    /// </summary>
    public sealed class CombatEntity : Entity, IPosition
    {
        public GameObject HeroObject { get; set; }
        public Transform ModelTrans { get; set; }
        public HealthPointComponent CurrentHealth { get; private set; }

        //效果赋给行动能力
        public EffectAssignAbility EffectAssignAbility { get; private set; }
        //施法行动能力
        public SpellActionAbility SpellAbility { get; private set; }
        //移动行动能力
        public MotionActionAbility MotionAbility { get; private set; }
        //伤害行动能力
        public DamageActionAbility DamageAbility { get; private set; }
        //治疗行动能力
        public CureActionAbility CureAbility { get; private set; }
        //施加状态行动能力
        public AddStatusActionAbility AddStatusAbility { get; private set; }
        //施法普攻行动能力
        public AttackActionAbility AttackSpellAbility { get; private set; }
        //回合行动能力
        public RoundActionAbility RoundAbility { get; private set; }
        //起跳行动能力
        public JumpToActionAbility JumpToAbility { get; private set; }
        public CollisionActionAbility CollisionAbility { get; private set; }

        //普攻格挡能力
        public AttackBlockActionAbility AttackBlockAbility { get; set; }

        //执行中的执行体
        public AbilityExecution SpellingExecution { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        /// 行为禁制
        public ActionControlType ActionControlType { get; set; }
        /// 禁制豁免
        public ActionControlType ActionControlImmuneType { get; set; }


        public override void Awake()
        {
            AddComponent<AttributeComponent>().InitializeCharacter();
            AddComponent<BehaviourPointComponent>();
            AddComponent<AbilityComponent>();
            //AddComponent<ConditionEventComponent>();
            AddComponent<StatusComponent>();
            AddComponent<SkillComponent>();
            AddComponent<SpellComponent>();
            AddComponent<MotionComponent>();
            CurrentHealth = AddComponent<HealthPointComponent>();
            CurrentHealth.HealthPointNumeric = GetComponent<AttributeComponent>().HealthPoint;
            CurrentHealth.HealthPointMaxNumeric = GetComponent<AttributeComponent>().HealthPointMax;
            CurrentHealth.Reset();

            EffectAssignAbility = AttachAction<EffectAssignAbility>();
            SpellAbility = AttachAction<SpellActionAbility>();
            MotionAbility = AttachAction<MotionActionAbility>();
            DamageAbility = AttachAction<DamageActionAbility>();
            CureAbility = AttachAction<CureActionAbility>();
            AddStatusAbility = AttachAction<AddStatusActionAbility>();
            AttackSpellAbility = AttachAction<AttackActionAbility>();
            RoundAbility = AttachAction<RoundActionAbility>();
            JumpToAbility = AttachAction<JumpToActionAbility>();
            CollisionAbility = AttachAction<CollisionActionAbility>();
        }

        #region 行动点事件
        public void ListenActionPoint(ActionPointType actionPointType, Action<Entity> action)
        {
            GetComponent<BehaviourPointComponent>().AddListener(actionPointType, action);
        }

        public void UnListenActionPoint(ActionPointType actionPointType, Action<Entity> action)
        {
            GetComponent<BehaviourPointComponent>().RemoveListener(actionPointType, action);
        }

        public void TriggerActionPoint(ActionPointType actionPointType, Entity action)
        {
            GetComponent<BehaviourPointComponent>().TriggerActionPoint(actionPointType, action);
        }
        #endregion

        public T AttachAction<T>() where T : Entity, IActionAbility
        {
            var action = AddChild<T>();
            action.AddComponent<ActionComponent>();
            action.Enable = true;
            return action;
        }

        public Ability AttachStatus(object configObject)
        {
            return GetComponent<StatusComponent>().AttachStatus(configObject);
        }

        public void BindSkillInput(Ability abilityEntity, KeyCode keyCode)
        {
            GetComponent<SkillComponent>().InputSkills.Add(keyCode, abilityEntity);
            abilityEntity.TryActivateAbility();
        }

        #region 回合制战斗
        public int SeatNumber { get; set; }
        public int JumpToTime { get; set; }
        public bool IsHero { get; set; }
        public bool IsMonster => IsHero == false;

        public CombatEntity GetEnemy(int seat)
        {
            if (IsHero)
            {
                return GetParent<CombatContext>().GetMonster(seat);
            }
            else
            {
                return GetParent<CombatContext>().GetHero(seat);
            }
        }

        public CombatEntity GetTeammate(int seat)
        {
            if (IsHero)
            {
                return GetParent<CombatContext>().GetHero(seat);
            }
            else
            {
                return GetParent<CombatContext>().GetMonster(seat);
            }
        }
        #endregion
    }

    public class RemoveStatusEvent
    {
        public Entity Entity { get; set; }
        public Ability Status { get; set; }
        public long StatusId { get; set; }
    }
}