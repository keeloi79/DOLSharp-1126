using System.Collections.Generic;
using DOL.Database;
using DOL.Language;
using log4net;

namespace DOL.GS.RealmAbilities
{
    /// <summary>
    ///
    /// </summary>
    public abstract class RAPropertyEnhancer : L5RealmAbility
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // property to modify
        private readonly eProperty[] _property;

        public RAPropertyEnhancer(DBAbility dba, int level, eProperty[] property)
            : base(dba, level)
        {
            _property = property;
        }

        public RAPropertyEnhancer(DBAbility dba, int level, eProperty property)
            : base(dba, level)
        {
            _property = new[] { property };
        }

        public override IList<string> DelveInfo
        {
            get
            {
                var list = new List<string>
                {
                    m_description, string.Empty
                };

                for (int i = 1; i <= MaxLevel; i++)
                {
                    list.Add(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "RAPropertyEnhancer.DelveInfo.Info1", i, GetAmountForLevel(i), ValueUnit));
                }

                return list;
            }
        }

        /// <summary>
        /// Get the Amount of Bonus for this RA at a particular level
        /// </summary>
        /// <param name="level">The level</param>
        /// <returns>The amount</returns>
        public virtual int GetAmountForLevel(int level)
        {
            return 0;
        }

        /// <summary>
        /// The bonus amount at this RA's level
        /// </summary>
        public int Amount => GetAmountForLevel(Level);

        /// <summary>
        /// send updates about the changes
        /// </summary>
        /// <param name="target"></param>
        protected virtual void SendUpdates(GameLiving target)
        {
        }

        /// <summary>
        /// Unit for values like %
        /// </summary>
        protected virtual string ValueUnit => string.Empty;

        public override void Activate(GameLiving living, bool sendUpdates)
        {
            if (m_activeLiving == null)
            {
                foreach (eProperty property in _property)
                {
                    living.AbilityBonus[(int)property] += GetAmountForLevel(Level);
                }

                m_activeLiving = living;
                if (sendUpdates)
                {
                    SendUpdates(living);
                }
            }
            else
            {
                Log.Warn($"ability {Name} already activated on {living.Name}");
            }
        }

        public override void Deactivate(GameLiving living, bool sendUpdates)
        {
            if (m_activeLiving != null)
            {
                foreach (eProperty property in _property)
                {
                    living.AbilityBonus[(int)property] -= GetAmountForLevel(Level);
                }

                if (sendUpdates)
                {
                    SendUpdates(living);
                }

                m_activeLiving = null;
            }
            else
            {
                Log.Warn($"ability {Name} already deactivated on {living.Name}");
            }
        }

        public override void OnLevelChange(int oldLevel, int newLevel = 0)
        {
            if (newLevel == 0)
            {
                newLevel = Level;
            }

            foreach (eProperty property in _property)
            {
                m_activeLiving.AbilityBonus[(int)property] += GetAmountForLevel(newLevel) - GetAmountForLevel(oldLevel);
            }

            SendUpdates(m_activeLiving);
        }
    }

    public abstract class L3RAPropertyEnhancer : RAPropertyEnhancer
    {
        public L3RAPropertyEnhancer(DBAbility dba, int level, eProperty property)
            : base(dba, level, property)
        {
        }

        public L3RAPropertyEnhancer(DBAbility dba, int level, eProperty[] properties)
            : base(dba, level, properties)
        {
        }

        public override int CostForUpgrade(int level)
        {
            if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
            {
                switch (level)
                {
                    case 0: return 5;
                    case 1: return 5;
                    case 2: return 5;
                    case 3: return 7;
                    case 4: return 8;
                    default: return 1000;
                }
            }

            return (level + 1) * 5;
        }

        public override int MaxLevel
        {
            get
            {
                if (ServerProperties.Properties.USE_NEW_ACTIVES_RAS_SCALING)
                {
                    return 5;
                }

                return 3;
            }
        }
    }
}