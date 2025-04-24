using Microsoft.VisualStudio.TestTools.UnitTesting;
using PurgeChar;

namespace PurgeChar.Tests
{
    [TestClass]
    public class ViewerTests
    {
        private Viewer _viewer;

        [TestInitialize]
        public void Setup()
        {
            _viewer = new Viewer();
        }

        [TestMethod]
        public void Viewer_Constructor_InitializesCorrectly()
        {
            // Assert
            Assert.IsNotNull(_viewer);
        }

        [TestMethod]
        public void Viewer_LoadContent_WithValidXml_LoadsSuccessfully()
        {
            // Arrange
            string validXml = "<root><child>Test</child></root>";

            // Act & Assert
            Assert.IsTrue(_viewer.LoadContent(validXml));
        }

        [TestMethod]
        public void Viewer_LoadContent_WithInvalidXml_ReturnsFalse()
        {
            // Arrange
            string invalidXml = "<root><child>Test</child>";

            // Act & Assert
            Assert.IsFalse(_viewer.LoadContent(invalidXml));
        }
    }
}