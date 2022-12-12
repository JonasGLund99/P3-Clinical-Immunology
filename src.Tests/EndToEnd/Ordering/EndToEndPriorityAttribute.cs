using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://stackoverflow.com/questions/9210281/how-to-set-the-test-case-sequence-in-xunit

namespace src.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EndToEndPriorityAttribute : Attribute
    {
        public EndToEndPriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; private set; }
    }
}
