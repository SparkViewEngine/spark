using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;


namespace Castle.MonoRail.Views.Spark.Tests
{
    public static class Contains
    {
        public static Constraint InOrder(params string[] items)
        {
            return new ContainsInOrderConstraint(items);
        }
    }

    internal class ContainsInOrderConstraint : Constraint
    {
        private readonly string[] items;

        public ContainsInOrderConstraint(params string[] items)
        {
            this.items = items;
        }

        public override bool Matches(object actual)
        {
            if (actual == null)
            {
                return this.items.Length == 0;
            }

            var actualString = actual.ToString();

            int index = 0;
            foreach (string value in this.items)
            {
                int nextIndex = actualString.IndexOf(value, index);
                if (nextIndex < 0)
                {
                    return false;
                }

                index = nextIndex + value.Length;
            }

            return true;
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
        }
    }
}
