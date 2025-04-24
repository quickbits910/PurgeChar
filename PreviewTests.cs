using Microsoft.VisualStudio.TestTools.UnitTesting;
using PurgeChar;

namespace PurgeChar.Tests
{
    [TestClass]
    public class PreviewTests
    {
        private Preview _preview;

        [TestInitialize]
        public void Setup()
        {
            _preview = new Preview();
        }

        [TestMethod]
        public void Preview_Constructor_InitializesCorrectly()
        {
            // Assert
            Assert.IsNotNull(_preview);
            Assert.IsNotNull(_preview.PreviewText);
        }

        [TestMethod]
        public void Preview_SetPreviewText_UpdatesTextCorrectly()
        {
            // Arrange
            string testText = "Test preview text";

            // Act
            _preview.PreviewText = testText;

            // Assert
            Assert.AreEqual(testText, _preview.PreviewText);
        }
    }
}