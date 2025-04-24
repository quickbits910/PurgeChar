using System.Threading;
using NUnit.Framework;
using PurgeChar;

namespace PurgeCharTests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class PreviewTests
    {
        private Preview _preview;

        [SetUp]
        public void Setup()
        {
            _preview = new Preview();
        }

        [Test]
        public void Preview_SetPreviewText_UpdatesTextCorrectly()
        {
            // Arrange
            string testText = "Test preview text";

            // Act
            _preview.PreviewText = testText;

            // Assert
            Assert.That(_preview.PreviewText, Is.EqualTo(testText));
        }
        
        
        [Test]
        public void Preview_SetPreviewText_NullOrEmpty_UpdatesText()
        {
            // Act & Assert for null
            _preview.PreviewText = null;
            Assert.That(_preview.PreviewText, Is.Null);

            // Act & Assert for empty string
            _preview.PreviewText = "";
            Assert.That(_preview.PreviewText, Is.EqualTo(""));
        }

        [Test]
        public void Preview_SetPreviewText_MultipleSets_UpdatesTextCorrectlyEachTime()
        {
            // Arrange
            string firstText = "First text";
            string secondText = "Second text";

            // Act & Assert
            _preview.PreviewText = firstText;
            Assert.That(_preview.PreviewText, Is.EqualTo(firstText));

            _preview.PreviewText = secondText;
            Assert.That(_preview.PreviewText, Is.EqualTo(secondText));
        }
    }
    }
