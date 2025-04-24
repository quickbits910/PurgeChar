using System.Threading;
using NUnit.Framework;
using PurgeChar;
using System.Xml;
using System.Windows.Controls;
using System.Windows;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("System.Windows.Controls")] // Add reference to PresentationFramework.dll

namespace PurgeCharTests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ViewerTests
    {
        private Viewer _viewer;

        [SetUp]
        public void Setup()
        {
            _viewer = new Viewer();
        }

        [Test]
        public void Viewer_Constructor_InitializesCorrectly()
        {
            // Assert
            Assert.IsNotNull(_viewer);
            Assert.IsNull(_viewer.xmlDocument);
        }

        [Test]
        public void Viewer_LoadContent_WithValidXml_LoadsSuccessfully()
        {
            // Arrange
            string validXml = "<root><child>Test</child></root>";

            // Act
            bool? result = _viewer.LoadContent(validXml);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(_viewer.xmlDocument);
            Assert.AreEqual("root", _viewer.xmlDocument.DocumentElement.Name);
        }

        [Test]
        public void Viewer_LoadContent_WithInvalidXml_ReturnsFalse()
        {
            // Arrange
            string invalidXml = "<root><child>Test</child>";

            // Act
            bool? result = _viewer.LoadContent(invalidXml);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(_viewer.xmlDocument);
        }

        [Test]
        public void Viewer_LoadContent_WithEmptyString_ReturnsFalse()
        {
            // Arrange
            string emptyXml = "";

            // Act
            bool? result = _viewer.LoadContent(emptyXml);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(_viewer.xmlDocument);
        }

        [Test]
        public void Viewer_LoadContent_WithNullInput_ReturnsFalse()
        {
            // Act
            bool? result = _viewer.LoadContent(null);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(_viewer.xmlDocument);
        }

        [Test]
        public void Viewer_LoadContent_WithComplexXml_LoadsSuccessfully()
        {
            // Arrange
            string complexXml = @"
                <root>
                    <child id='1'>
                        <subchild>Value1</subchild>
                        <subchild>Value2</subchild>
                    </child>
                    <child id='2'>
                        <subchild>Value3</subchild>
                    </child>
                </root>";

            // Act
            bool? result = _viewer.LoadContent(complexXml);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(_viewer.xmlDocument);
            Assert.AreEqual(2, _viewer.xmlDocument.SelectNodes("//child").Count);
        }

        [Test]
        public void Viewer_LoadContent_WithWhitespaceOnly_ReturnsFalse()
        {
            // Arrange
            string whitespaceXml = "    \t\n";

            // Act
            bool? result = _viewer.LoadContent(whitespaceXml);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(_viewer.xmlDocument);
        }

        [Test]
        public void Viewer_LoadContent_WithXmlDeclaration_LoadsSuccessfully()
        {
            // Arrange
            string xmlWithDeclaration = "<?xml version='1.0' encoding='UTF-8'?><root><child>Test</child></root>";

            // Act
            bool? result = _viewer.LoadContent(xmlWithDeclaration);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(_viewer.xmlDocument);
            Assert.AreEqual("root", _viewer.xmlDocument.DocumentElement.Name);
        }
        
          [Test]
        public void Viewer_LoadContent_MultipleCalls_OverwritesSuccessfully()
        {
            // Arrange
            string xml1 = "<root><child>Test1</child></root>";
            string xml2 = "<newroot><child>Test2</child></newroot>";

            // Act
            bool? result1 = _viewer.LoadContent(xml1);
            bool? result2 = _viewer.LoadContent(xml2);

            // Assert
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.AreEqual("newroot", _viewer.xmlDocument.DocumentElement.Name);
        }

        [Test]
        public void Viewer_LoadContent_InvalidThenValidContent_ResetsXmlDocument()
        {
            // Arrange
            string invalidXml = "<root><child></root>";
            string validXml = "<root><child>Test</child></root>";

            // Act
            bool? invalidResult = _viewer.LoadContent(invalidXml);
            bool? validResult = _viewer.LoadContent(validXml);

            // Assert
            Assert.IsFalse(invalidResult);
            Assert.IsTrue(validResult);
            Assert.IsNotNull(_viewer.xmlDocument);
            Assert.AreEqual("root", _viewer.xmlDocument.DocumentElement.Name);
        }

        [Test]
        public void Viewer_LoadContent_LargeXml_LoadsSuccessfully()
        {
            // Arrange
            string largeXml = "<root>";
            for (int i = 0; i < 1000; i++)
            {
                largeXml += $"<child id='{i}'>Value{i}</child>";
            }
            largeXml += "</root>";

            // Act
            bool? result = _viewer.LoadContent(largeXml);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(_viewer.xmlDocument);
            Assert.AreEqual(1000, _viewer.xmlDocument.SelectNodes("//child").Count);
        }

        [Test]
        public void Viewer_LoadContent_WithNamespaces_LoadsSuccessfully()
        {
            // Arrange
            string xmlWithNs = @"<root xmlns:ns='http://example.com/ns'>
                                    <ns:child>Value</ns:child>
                                 </root>";

            // Act
            bool? result = _viewer.LoadContent(xmlWithNs);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(_viewer.xmlDocument);
            Assert.AreEqual("root", _viewer.xmlDocument.DocumentElement.Name);
        }

        [Test]
        public void Viewer_LoadContent_WithSpecialCharacters_LoadsSuccessfully()
        {
            // Arrange
            string xmlSpecialChars = "<root><child>Special &amp; Characters &lt;Test&gt;</child></root>";

            // Act
            bool? result = _viewer.LoadContent(xmlSpecialChars);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(_viewer.xmlDocument);
            Assert.AreEqual("root", _viewer.xmlDocument.DocumentElement.Name);
            Assert.AreEqual("Special & Characters <Test>", _viewer.xmlDocument.SelectSingleNode("//child").InnerText);
        }
    }
}