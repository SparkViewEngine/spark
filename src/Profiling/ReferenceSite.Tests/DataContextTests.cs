using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using ReferenceSite.Model;

namespace ReferenceSite.Tests
{
    [TestFixture]
    public class DataContextTests
    {
        private DataContext _dataContext;

        [SetUp]
        public void SetUp()
        {
            _dataContext = new DataContext();
        }

        [Test]
        public void Name_should_end_with_storydata_xml()
        {
            Assert.That(_dataContext.DataFileLocation(), Text.EndsWith("StoryData.xml"));
        }

        [Test]
        public void Any_stories_are_available()
        {
            _dataContext.Initialize();
            Assert.That(_dataContext.Story, Is.Not.Empty);
            Assert.That(_dataContext.Story,
                        Has.Some.Property("Title").EqualTo("Motivational Thoughts From The Motivated Man."));
        }


        [Test]
        public void Any_users_are_available()
        {
            _dataContext.Initialize();
            Assert.That(_dataContext.User, Is.Not.Empty);
            Assert.That(_dataContext.User.ToArray(), Has.Some.Property("FullName").EqualTo("Ugis Pundurs"));

        }
    }
}
