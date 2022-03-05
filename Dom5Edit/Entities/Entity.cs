using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public abstract class Entity
    {
        private string _startComment;
        private string _endComment;

        public virtual void SetEndComment(string s)
        {
            _endComment = s;
        }

        public virtual void SetStartComment(string s)
        {
            _endComment = s;
        }

        public abstract void Parse(string command, string value, string comment);
    }
}
