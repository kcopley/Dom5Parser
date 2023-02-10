using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dom5Edit.Props
{
    public class SpellDamage : Reference
    {
        private string _val;

        private MonsterOrMontagRef _monRef;
        private EnchIDRef _enchRef;
        private EventEffectCodeRef _eventEffectRef;


        public static Property Create()
        {
            return new SpellDamage();
        }

        public override void Parse(Command c, string v, string comment)
        {
            _val = v;
            Comment = comment;
            Command = c;
            Parent.ParentMod.SpellDamages.Add(this);
        }

        public override void Resolve()
        {
            //is this a summon?
            Spell parent = (Spell)Parent;
            if (parent.IsSummon()) ResolveAsSummon();
            else if (parent.IsEnchant()) ResolveAsEnchant();
            else if (parent.IsEventEffect()) ResolveAsEventEffect();

            //currently string
            //resolve to an ID
            //or a bitmask
            //if the bitmask, leave it as a string export w/ no quotes
        }

        public override bool TryGetEntity(out IDEntity e)
        {
            e = null;
            if (_monRef != null)
            {
                return _monRef.TryGetEntity(out e);
            }
            return false;
        }

        void ResolveAsSummon()
        {
            _monRef = new MonsterOrMontagRef();
            _monRef.Parent = this.Parent;
            _monRef.Parse(this.Command, _val, Comment);
            _monRef.Resolve();
        }

        void ResolveAsEnchant()
        {
            _enchRef = new EnchIDRef();
            _enchRef.Parent = this.Parent;
            _enchRef.Parse(this.Command, _val, Comment);
            _enchRef.Resolve();
        }

        void ResolveAsEventEffect()
        {
            _eventEffectRef = new EventEffectCodeRef();
            _eventEffectRef.Parent = this.Parent;
            _eventEffectRef.Parse(this.Command, _val, Comment);
            _eventEffectRef.Resolve();
        }

        public override string ToExportString()
        {
            if (_monRef != null)
            {
                return _monRef.ToExportString();
            }

            if (_enchRef != null)
            {
                return _enchRef.ToExportString();
            }

            if (_eventEffectRef != null)
            {
                return _eventEffectRef.ToExportString();
            }

            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    return s + " " + _val + " -- " + Comment;
                }
                else
                {
                    return s + " " + _val;
                }
            }
            else return "";
        }

        internal override EntityType GetEntityType()
        {
            throw new NotImplementedException();
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            
            return false;
        }
    }
}
