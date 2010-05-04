//-------------------------------------------------------------------------
// <copyright file="Constraints.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Castle.MonoRail.Views.Spark.Tests
{
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    /// <summary>
    /// The Is class is a helper class with properties and methods that supply a
    /// number of constraints used in Asserts.
    /// </summary>
    public static class Contains
    {
        /// <summary>
        /// Contains.InOrder returns a constraint that tests whether the actual value
        /// contains all of the specified items in order.
        /// </summary>
        /// <param name="items">The list of items to test.</param>
        /// <returns>true, if all items are contained within the actual value, in order; false, otherwise.</returns>
        public static Constraint InOrder(params string[] items)
        {
            return new ContainsInOrderConstraint(items);
        }

        /// <summary>
        /// A constraint that tests whether the actual value contains all of the specified items in order.
        /// </summary>
        private class ContainsInOrderConstraint : Constraint
        {
            /// <summary>
            /// Holds the list of items to test.
            /// </summary>
            private readonly string[] items;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContainsInOrderConstraint"/> class.
            /// </summary>
            /// <param name="items">The list of items to test.</param>
            public ContainsInOrderConstraint(params string[] items)
            {
                this.items = items;
            }

            /// <summary>
            /// Tests whether the actual value contains all of the specified items in order. 
            /// </summary>
            /// <param name="actual">The actual value to test.</param>
            /// <returns>true, if all items are contained within the actual value, in order; false, otherwise.</returns>
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

            /// <summary>
            /// Writes the constraint description to a MessageWriter.
            /// </summary>
            /// <param name="writer">The writer on which the description is displayed.</param>
            public override void WriteDescriptionTo(MessageWriter writer)
            {
                writer.WriteLine("Tests whether the actual value contains all of the specified items in order.");
            }
        }
    }
}
