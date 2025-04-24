﻿using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace PurgeChar
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : UserControl
    {
        private XmlDocument _xmldocument;
        public Viewer()
        {
            InitializeComponent();
        }

        public XmlDocument xmlDocument
        {
            get { return _xmldocument; }
            set
            {
                _xmldocument = value;
                BindXMLDocument();
            }
        }

        private void BindXMLDocument()
        {
            if (_xmldocument == null)
            {
                xmlTree.ItemsSource = null;
                return;
            }

            XmlDataProvider provider = new XmlDataProvider();
            provider.Document = _xmldocument;
            Binding binding = new Binding();
            binding.Source = provider;
            binding.XPath = "child::node()";
            xmlTree.SetBinding(TreeView.ItemsSourceProperty, binding);
        }

        public bool? LoadContent(string validXml)
        {
            if (string.IsNullOrWhiteSpace(validXml))
            {
                xmlDocument = null;
                return false;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(validXml);
                xmlDocument = doc;
                return true;
            }
            catch (XmlException)
            {
                xmlDocument = null;
                return false;
            }
        }
    }
}