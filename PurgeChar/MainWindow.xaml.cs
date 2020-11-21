using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Threading;

namespace PurgeChar
{

    public static class GenericListExtensions
    {
        public static string ToString<T>(this IList<T> list)
        {
            return string.Join(", ", list);
        }
    }

    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            using (_timeoutTimer)
                System.Windows.MessageBox.Show(text, caption);
        }
        public static void Show(string text, string caption, int timeout)
        {
            new AutoClosingMessageBox(text, caption, timeout);
        }
        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string readyToRenameLog;
        private List<string> readyList;
        private string ErrorLogName = "Error.xml";
        private string thePathTxtText;
        private bool processResult;
        private bool showPrompts = false;
        private static AutoResetEvent resetEvent = new AutoResetEvent(false);

        /// <summary>
        /// The backgroundworker object on which the time consuming operation 
        /// shall be executed
        /// </summary>
        BackgroundWorker m_oWorker;

        /// <summary>
        /// The backgroundworker object on which the time consuming operation 
        /// shall be executed
        /// </summary>
        BackgroundWorker m_oWorker2;

        /// <summary>
        /// The backgroundworker object on which the time consuming operation 
        /// shall be executed
        /// </summary>
        BackgroundWorker m_oWorker3;


        public MainWindow()
        {
            InitializeComponent();
            chkBtn.IsEnabled = false;
            cancelBtn.Visibility = Visibility.Hidden;
            cancelBtn.IsEnabled = false;
        }

        private void browseBtn_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (dialog.SelectedPath != "")
            {
                thePathTxt.Text = dialog.SelectedPath.ToString();
                purgeTxt.IsEnabled = true;
                delimTxt.IsEnabled = true;
                if (purgeTxt.Text == "\\,/,:,*,?,\",<,>,|,#,{,},%,~,&,@")
                {
                    chkBtn.IsEnabled = true;
                }

            }



        }


        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void chkBtn_Click(object sender, RoutedEventArgs e)
        {
            progressBar1.Value = 0;
            var delimChar = delimTxt.Text;
            if (delimChar == "" || delimChar == string.Empty)
            {
                delimChar = ",";
                delimTxt.Text = ",";
            }

            //Calculate purge groups
            List<string> theList = new List<string>();

            string[] purgeVars;
            theList.Clear();
            char[] theSplitter = delimChar.ToCharArray();

            purgeVars = purgeTxt.Text.Split(theSplitter[0]);


            foreach (string str in purgeVars)
            {
                if (!theList.Contains(str))
                {
                    theList.Add(str);
                }
            }



            //Checking for
            if (showPrompts == true)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(("Replace Count: " + theList.Count + ". \r" + GenericListExtensions.ToString(theList)), "Proceed? No files will be changed.", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ShowPreview(theList);
                }
            }
            else
            {
                ShowPreview(theList);
            }


        }

        private void ShowPreview(List<string> theList)
        {
            thePathTxtText = thePathTxt.Text;
            string LogName = "Test_" + DateTime.Now.Year + "_" + DateTime.Now.DayOfWeek + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;
            ErrorLogName = LogName + "_ERROR" + ".xml";
            LogName = LogName + ".xml";
            readyToRenameLog = LogName;
            readyList = new List<string>();
            readyList = theList;
            //Loop recursively through all files and folders

            m_oWorker2 = new BackgroundWorker();

            // Create a background worker thread that ReportsProgress &
            // SupportsCancellation
            // Hook up the appropriate events.
            m_oWorker2.DoWork += new DoWorkEventHandler(m_oWorker2_DoWork);
            m_oWorker2.ProgressChanged += new ProgressChangedEventHandler
                    (m_oWorker2_ProgressChanged);
            m_oWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    (m_oWorker2_RunWorkerCompleted);
            m_oWorker2.WorkerReportsProgress = true;
            m_oWorker2.WorkerSupportsCancellation = true;

            cancelBtn.Visibility = Visibility.Visible;
            cancelBtn.IsEnabled = true;

            m_oWorker2.RunWorkerAsync();
            resetEvent.WaitOne();

            if (processResult == true)
            {


                //Load XML file into preview
                XmlDocument XMLdoc = new XmlDocument();
                string LogNameTrimmed = readyToRenameLog.Substring(0, readyToRenameLog.LastIndexOf("_"));
                var currentPath = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

                var myFile = (from f in currentPath.GetFiles() orderby f.LastWriteTime descending select f).First();

                if (!File.Exists(readyToRenameLog) && myFile.Exists)
                {
                    if (myFile.LastWriteTime > DateTime.Now.AddSeconds(-2))
                    {
                        readyToRenameLog = myFile.Name;
                    }
                }

                if (File.Exists(readyToRenameLog))
                {

                    // Create a background worker thread that ReportsProgress &
                    // SupportsCancellation
                    // Hook up the appropriate events.
                    //m_oWorker3.DoWork += new DoWorkEventHandler(m_oWorker3_DoWork);
                    //m_oWorker3.ProgressChanged += new ProgressChangedEventHandler
                    //        (m_oWorker3_ProgressChanged);
                    //m_oWorker3.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    //        (m_oWorker3_RunWorkerCompleted);
                    //m_oWorker3.WorkerReportsProgress = true;
                    //m_oWorker3.WorkerSupportsCancellation = true;

                    //cancelBtn.Visibility = Visibility.Visible;
                    //cancelBtn.IsEnabled = true;

                    //m_oWorker3.RunWorkerAsync();

                    try
                    {
                        XMLdoc.Load(readyToRenameLog);
                    }
                    catch (XmlException)
                    {
                        MessageBoxResult result2 = System.Windows.MessageBox.Show("XML is invalid. Check for error logs.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

                    }

                    Viewer vXMLViewer = new Viewer();
                    vXMLViewer.xmlDocument = XMLdoc;

                    Window window = new Window
                    {
                        Title = "Preview Renames",
                        Width = 800,
                        Height = 600,
                        Content = vXMLViewer

                    };

                    window.ShowDialog();


                    //Enable Rename button
                    renameBtn.IsEnabled = true;
                }
                else
                {
                    AutoClosingMessageBox.Show("Nothing to rename, all clean.", "Success!", 3000);
                    //MessageBoxResult result2 = System.Windows.MessageBox.Show("Nothing to rename, all clean.", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                    renameBtn.IsEnabled = false;

                }

            }
            else
            {
                //Do Nothing
            }

            //bool processResult = ProcessDir(thePathTxt.Text, readyToRenameLog, theList);

            //if (processResult == true)
            //{


            //    //Load XML file into preview
            //    XmlDocument XMLdoc = new XmlDocument();
            //    string LogNameTrimmed = LogName.Substring(0, LogName.LastIndexOf("_"));
            //    var currentPath = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            //    var myFile = (from f in currentPath.GetFiles() orderby f.LastWriteTime descending select f).First();

            //    if (!File.Exists(LogName) && myFile.Exists)
            //    {
            //        if (myFile.LastWriteTime > DateTime.Now.AddSeconds(-2))
            //        {
            //            LogName = myFile.Name;
            //            readyToRenameLog = LogName;
            //        }
            //    }

            //    if (File.Exists(LogName))
            //    {
            //        try
            //        {
            //            XMLdoc.Load(LogName);
            //        }
            //        catch (XmlException)
            //        {
            //            MessageBoxResult result2 = System.Windows.MessageBox.Show("XML is invalid. Check for error logs.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            //        }

            //        Viewer vXMLViewer = new Viewer();
            //        vXMLViewer.xmlDocument = XMLdoc;

            //        Window window = new Window
            //        {
            //            Title = "Preview Renames",
            //            Width = 800,
            //            Height = 600,
            //            Content = vXMLViewer

            //        };

            //        window.ShowDialog();


            //        //Enable Rename button
            //        renameBtn.IsEnabled = true;
            //    }
            //    else
            //    {
            //        MessageBoxResult result2 = System.Windows.MessageBox.Show("Nothing to rename, all clean.", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
            //        renameBtn.IsEnabled = false;

            //    }

            //}
            //else
            //{
            //    //Do Nothing
            //}
        }

        private void renameBtn_Click(object sender, RoutedEventArgs e)
        {
            //Take XML extract and rename each file
            try
            {
                exitBtn.IsEnabled = false;
                chkBtn.IsEnabled = false;
                browseBtn.IsEnabled = false;
                progressBar1.Value = 0;

                m_oWorker = new BackgroundWorker();

                // Create a background worker thread that ReportsProgress &
                // SupportsCancellation
                // Hook up the appropriate events.
                m_oWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
                m_oWorker.ProgressChanged += new ProgressChangedEventHandler
                        (m_oWorker_ProgressChanged);
                m_oWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                        (m_oWorker_RunWorkerCompleted);
                m_oWorker.WorkerReportsProgress = true;
                m_oWorker.WorkerSupportsCancellation = true;

                cancelBtn.Visibility = Visibility.Visible;
                cancelBtn.IsEnabled = true;

                m_oWorker.RunWorkerAsync();

                ////Load XML file into preview
                //XmlDocument XMLdoc = new XmlDocument();
                //try
                //{
                //    XMLdoc.Load(readyToRenameLog);
                //}
                //catch (XmlException)
                //{
                //    System.Windows.Forms.MessageBox.Show("The XML file is invalid. Check for error logs.");
                //    return;
                //}

                //if (XMLdoc.HasChildNodes)
                //{
                //    var xmlEntities = new List<XmlEntity>();

                //    //Rename Files
                //    foreach (XmlNode item in XMLdoc.ChildNodes)
                //    {
                //         try
                //         {
                //            GetChildren(item, "File");
                //         }
                //         catch (Exception exFile)
                //         {
                //            LogError(ErrorLogName, exFile.Message.ToString());
                //         }
                //    }

                //    //Rename Folders
                //    foreach (XmlNode item in XMLdoc.ChildNodes)
                //    {
                //        try
                //        {
                //            GetChildren(item, "Directory");
                //        }
                //        catch (Exception exDir)
                //        {
                //            LogError(ErrorLogName, exDir.Message.ToString());
                //        }
                //    }

                //    //Success
                //    MessageBoxResult result2 = System.Windows.MessageBox.Show("Renamed all files and folders. Check for error logs.", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                //}

                ////XML has no child nodes
            }
            catch (Exception ex)
            {
                //
            }
            finally
            {
                exitBtn.IsEnabled = true;
                chkBtn.IsEnabled = true;
                browseBtn.IsEnabled = true;
            }
        }

        /// <summary>
        /// On completed do the appropriate task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // The background process is complete. We need to inspect
            // our response to see if an error occurred, a cancel was
            // requested or if we completed successfully.  
            if (e.Cancelled)
            {
                //lblStatus.Text = "Task Cancelled.";
            }

            // Check to see if an error occurred in the background process.

            else if (e.Error != null)
            {
                //lblStatus.Text = "Error while performing background operation.";
            }
            else
            {
                // Everything completed normally.
                // lblStatus.Text = "Task Completed...";
            }

            //Change the status of the buttons on the UI accordingly
            //btnStartAsyncOperation.Enabled = true;
            //btnCancel.Enabled = false;
            cancelBtn.Visibility = Visibility.Hidden;
            cancelBtn.IsEnabled = false;
        }

        /// <summary>
        /// Notification is performed here to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            // This function fires on the UI thread so it's safe to edit

            // the UI control directly, no funny business with Control.Invoke :)

            // Update the progressBar with the integer supplied to us from the

            // ReportProgress() function.  

            progressBar1.Value = e.ProgressPercentage;
            //lblStatus.Text = "Processing......" + progressBar1.Value.ToString() + "%";
        }

        /// <summary>
        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            //Load XML file into preview
            XmlDocument XMLdoc = new XmlDocument();
            try
            {
                XMLdoc.Load(readyToRenameLog);
            }
            catch (XmlException)
            {
                System.Windows.Forms.MessageBox.Show("The XML file is invalid. Check for error logs.");
                return;
            }

            if (XMLdoc.HasChildNodes)
            {
                var xmlEntities = new List<XmlEntity>();

                int fileCounter = 0;
                //Rename Files
                foreach (XmlNode item in XMLdoc.ChildNodes)
                {
                    try
                    {
                        //REM Thread.Sleep(1);                      

                        GetChildren(item, "File");

                        // Periodically report progress to the main thread so that it can
                        // update the UI.  In most cases you'll just need to send an
                        // integer that will update a ProgressBar                    
                        m_oWorker.ReportProgress(fileCounter / XMLdoc.ChildNodes.Count);
                    }
                    catch (Exception exFile)
                    {
                        LogError(ErrorLogName, exFile.Message.ToString());
                    }
                }

                //Rename Folders
                foreach (XmlNode item in XMLdoc.ChildNodes)
                {
                    try
                    {
                        //REM  Thread.Sleep(1);
                        GetChildren(item, "Directory");

                    }
                    catch (Exception exDir)
                    {
                        LogError(ErrorLogName, exDir.Message.ToString());
                    }
                }

                //Success
                AutoClosingMessageBox.Show("Renamed all files and folders. Check for error logs.", "Success!", 3000);
                //MessageBoxResult result2 = System.Windows.MessageBox.Show("Renamed all files and folders. Check for error logs.", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            //XML has no child nodes

            // Periodically check if a cancellation request is pending.
            // If the user clicks cancel the line
            // m_AsyncWorker.CancelAsync(); if ran above.  This
            // sets the CancellationPending to true.
            // You must check this flag in here and react to it.
            // We react to it by setting e.Cancel to true and leaving
            if (m_oWorker.CancellationPending)
            {
                // Set the e.Cancel flag so that the WorkerCompleted event
                // knows that the process was cancelled.
                e.Cancel = true;
                m_oWorker.ReportProgress(0);
                return;
            }

            //Report 100% completion on operation completed
            m_oWorker.ReportProgress(100);
        }

        /// <summary>
        /// On completed do the appropriate task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // The background process is complete. We need to inspect
            // our response to see if an error occurred, a cancel was
            // requested or if we completed successfully.  
            if (e.Cancelled)
            {
                //lblStatus.Text = "Task Cancelled.";
            }

            // Check to see if an error occurred in the background process.

            else if (e.Error != null)
            {
                //lblStatus.Text = "Error while performing background operation.";
            }
            else
            {
                // Everything completed normally.
                // lblStatus.Text = "Task Completed...";
            }

            //Change the status of the buttons on the UI accordingly
            //btnStartAsyncOperation.Enabled = true;
            //btnCancel.Enabled = false;
            cancelBtn.Visibility = Visibility.Hidden;
            cancelBtn.IsEnabled = false;
        }

        /// <summary>
        /// Notification is performed here to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            // This function fires on the UI thread so it's safe to edit

            // the UI control directly, no funny business with Control.Invoke :)

            // Update the progressBar with the integer supplied to us from the

            // ReportProgress() function.  

            progressBar1.Value = e.ProgressPercentage;
            //lblStatus.Text = "Processing......" + progressBar1.Value.ToString() + "%";
        }

        /// <summary>
        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker2_DoWork(object sender, DoWorkEventArgs e)
        {

            //REM Thread.Sleep(100);
            processResult = ProcessDir(thePathTxtText, readyToRenameLog, readyList);

            int counter = 1;

            // Periodically report progress to the main thread so that it can
            // update the UI.  In most cases you'll just need to send an
            // integer that will update a ProgressBar  

            m_oWorker2.ReportProgress(counter);

            counter++;
            // Periodically check if a cancellation request is pending.
            // If the user clicks cancel the line
            // m_AsyncWorker.CancelAsync(); if ran above.  This
            // sets the CancellationPending to true.
            // You must check this flag in here and react to it.
            // We react to it by setting e.Cancel to true and leaving
            if (m_oWorker2.CancellationPending)
            {
                // Set the e.Cancel flag so that the WorkerCompleted event
                // knows that the process was cancelled.
                e.Cancel = true;
                m_oWorker2.ReportProgress(0);
                return;
            }

            //Report 100% completion on operation completed
            m_oWorker2.ReportProgress(100);

            resetEvent.Set();
        }

        /// <summary>
        /// On completed do the appropriate task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // The background process is complete. We need to inspect
            // our response to see if an error occurred, a cancel was
            // requested or if we completed successfully.  
            if (e.Cancelled)
            {
                //lblStatus.Text = "Task Cancelled.";
            }

            // Check to see if an error occurred in the background process.

            else if (e.Error != null)
            {
                //lblStatus.Text = "Error while performing background operation.";
            }
            else
            {
                // Everything completed normally.
                // lblStatus.Text = "Task Completed...";
            }

            //Change the status of the buttons on the UI accordingly
            //btnStartAsyncOperation.Enabled = true;
            //btnCancel.Enabled = false;
            cancelBtn.Visibility = Visibility.Hidden;
            cancelBtn.IsEnabled = false;
        }

        /// <summary>
        /// Notification is performed here to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            // This function fires on the UI thread so it's safe to edit

            // the UI control directly, no funny business with Control.Invoke :)

            // Update the progressBar with the integer supplied to us from the

            // ReportProgress() function.  

            progressBar1.Value = e.ProgressPercentage;
            //lblStatus.Text = "Processing......" + progressBar1.Value.ToString() + "%";
        }

        /// <summary>
        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker3_DoWork(object sender, DoWorkEventArgs e)
        {

            //REM Thread.Sleep(1);
            XmlDocument XMLdoc = new XmlDocument();
            try
            {
                XMLdoc.Load(readyToRenameLog);
            }
            catch (XmlException)
            {
                MessageBoxResult result2 = System.Windows.MessageBox.Show("XML is invalid. Check for error logs.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            }

            Viewer vXMLViewer = new Viewer();
            vXMLViewer.xmlDocument = XMLdoc;

            Window window = new Window
            {
                Title = "Preview Renames",
                Width = 800,
                Height = 600,
                Content = vXMLViewer

            };

            window.ShowDialog();


            // Periodically report progress to the main thread so that it can
            // update the UI.  In most cases you'll just need to send an
            // integer that will update a ProgressBar                    
            //m_oWorker.ReportProgress(fileCounter / XMLdoc.ChildNodes.Count);


            // Periodically check if a cancellation request is pending.
            // If the user clicks cancel the line
            // m_AsyncWorker.CancelAsync(); if ran above.  This
            // sets the CancellationPending to true.
            // You must check this flag in here and react to it.
            // We react to it by setting e.Cancel to true and leaving
            if (m_oWorker3.CancellationPending)
            {
                // Set the e.Cancel flag so that the WorkerCompleted event
                // knows that the process was cancelled.
                e.Cancel = true;
                m_oWorker3.ReportProgress(0);
                return;
            }

            //Report 100% completion on operation completed
            m_oWorker3.ReportProgress(100);

        }


        private void GetChildren(XmlNode node, string nodeName)
        {
            if (node.LocalName == "File" && nodeName == "File")
            {
                //<you get the element here and work with it>

                //Full Path
                //node.FirstChild.InnerText;

                string folderPath = System.IO.Path.GetDirectoryName(node.FirstChild.InnerText);
                System.IO.File.Move(node.FirstChild.InnerText, (folderPath + "\\" + node.LastChild.InnerText));

                //Rename Item


            }
            else if (node.LocalName == "Directory" && nodeName == "Directory")
            {
                //<you get the element here and work with it>

                //Full Path
                //node.FirstChild.InnerText;

                string folderPath = System.IO.Path.GetDirectoryName(node.FirstChild.InnerText);
                System.IO.Directory.Move(node.FirstChild.InnerText, (folderPath + "\\" + node.LastChild.InnerText));

                //Rename Item

            }
            else
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    GetChildren(item, nodeName);
                }
            }
        }

        private void purgeTxt_Changed(object sender, TextChangedEventArgs e)
        {
            if (purgeTxt.Text.Length > 0)
            {
                chkBtn.IsEnabled = true;
            }
            else
            {
                chkBtn.IsEnabled = false;
            }
        }

        public string RemoveChar(string input, List<string> ExceptionStr)
        {
            StringBuilder replaceStr = new StringBuilder();
            try
            {
                replaceStr.Append(input);
                Parallel.ForEach(ExceptionStr, remStr =>
                {
                    if (input.Contains(remStr))
                    {
                        replaceStr = replaceStr.Replace(remStr, "");
                    }
                });

                //foreach (string remStr in ExceptionStr)
                //{
                //    if (input.Contains(remStr))
                //    {
                //        replaceStr = replaceStr.Replace(remStr, "");
                //    }
                //}

                if (replaceStr.Length > 0)
                {
                    return replaceStr.ToString();
                }
                else
                {
                    throw new Exception("Resulting File or folder name would have been zero length.");
                }
            }
            catch (Exception ex)
            {
                LogError(ErrorLogName, ex.ToString());
                return input;
            }
        }

        private void LogError(string ErrorLogName, string exMsg)
        {
            #region Error Log
            if (File.Exists(ErrorLogName) == false)
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.NewLineOnAttributes = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(ErrorLogName, xmlWriterSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Exception");

                    xmlWriter.WriteStartElement("Error");
                    xmlWriter.WriteElementString("Result", exMsg.ToString());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else
            {
                XDocument xDocument = XDocument.Load(ErrorLogName);
                XElement root = xDocument.Element("Exception");
                IEnumerable<XElement> rows = root.Descendants("Error");
                XElement firstRow = rows.First();
                firstRow.AddBeforeSelf(
                   new XElement("Error",
                   new XElement("Result", exMsg.ToString())));
                xDocument.Save(ErrorLogName);
            }
            #endregion
        }

        // How much deep to scan. (of course you can also pass it to the method)
        //const int HowDeepToScan = 255;

        public bool ProcessDir(string sourceDir, string LogName, List<string> ExceptionStr)
        {
            int totalFilecounter = 1;
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;

            try
            {

                // Process the list of files found in the directory.
                //string[] fileEntries = Directory.GetFiles(sourceDir);
                var fileEntries = Directory.EnumerateFiles(sourceDir);

                //string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);    
                foreach (string fileName in fileEntries)
                {
                    // do something with fileName
                    string isolateFilename = System.IO.Path.GetFileName(fileName);
                    string renamedFile = RemoveChar(isolateFilename, ExceptionStr);

                    if (isolateFilename != renamedFile)
                    {

                        // Periodically report progress to the main thread so that it can
                        // update the UI.  In most cases you'll just need to send an
                        // integer that will update a ProgressBar                    
                        //m_oWorker2.ReportProgress(totalFilecounter / files.Length);

                        #region Write File Changes
                        if (File.Exists(LogName) == false)
                        {
                            using (XmlWriter xmlWriter = XmlWriter.Create(LogName, xmlWriterSettings))
                            {
                                xmlWriter.WriteStartDocument();
                                xmlWriter.WriteStartElement("Files_Folders");

                                xmlWriter.WriteStartElement("File");
                                xmlWriter.WriteElementString("Full_Path", fileName);
                                xmlWriter.WriteElementString("Proposed_Name", renamedFile);
                                xmlWriter.WriteEndElement();

                                xmlWriter.WriteEndElement();
                                xmlWriter.WriteEndDocument();
                                xmlWriter.Flush();
                                xmlWriter.Close();
                            }
                        }
                        else
                        {
                            XDocument xDocument = XDocument.Load(LogName);
                            XElement root = xDocument.Element("Files_Folders");
                            IEnumerable<XElement> rows = root.Descendants("File");
                            XElement firstRow = rows.First();
                            firstRow.AddBeforeSelf(
                               new XElement("File",
                               new XElement("Full_Path", fileName),
                               new XElement("Proposed_Name", renamedFile)));
                            xDocument.Save(LogName);
                        }
                        #endregion
                    }
                    totalFilecounter++;
                }

                //Parallel.ForEach(fileEntries, fileName => {
                //    // do something with fileName
                //    string isolateFilename = System.IO.Path.GetFileName(fileName);
                //    string renamedFile = RemoveChar(isolateFilename, ExceptionStr);

                //    if (isolateFilename != renamedFile)
                //    {

                //        // Periodically report progress to the main thread so that it can
                //        // update the UI.  In most cases you'll just need to send an
                //        // integer that will update a ProgressBar                    
                //        //m_oWorker2.ReportProgress(totalFilecounter / files.Length);

                //        #region Write File Changes
                //        if (File.Exists(LogName) == false)
                //        {
                //            using (XmlWriter xmlWriter = XmlWriter.Create(LogName, xmlWriterSettings))
                //            {
                //                xmlWriter.WriteStartDocument();
                //                xmlWriter.WriteStartElement("Files_Folders");

                //                xmlWriter.WriteStartElement("File");
                //                xmlWriter.WriteElementString("Full_Path", fileName);
                //                xmlWriter.WriteElementString("Proposed_Name", renamedFile);
                //                xmlWriter.WriteEndElement();

                //                xmlWriter.WriteEndElement();
                //                xmlWriter.WriteEndDocument();
                //                xmlWriter.Flush();
                //                xmlWriter.Close();
                //            }
                //        }
                //        else
                //        {
                //            XDocument xDocument = XDocument.Load(LogName);
                //            XElement root = xDocument.Element("Files_Folders");
                //            IEnumerable<XElement> rows = root.Descendants("File");
                //            XElement firstRow = rows.First();
                //            firstRow.AddBeforeSelf(
                //               new XElement("File",
                //               new XElement("Full_Path", fileName),
                //               new XElement("Proposed_Name", renamedFile)));
                //            xDocument.Save(LogName);
                //        }
                //        #endregion
                //    }
                //    totalFilecounter++;
                //});

                // Recurse into subdirectories of this directory.
                //string[] subdirEntries = Directory.GetDirectories(sourceDir);
                var subdirEntries = Directory.EnumerateDirectories(sourceDir);

                Parallel.ForEach(subdirEntries, subdir =>
                {

                    // Do not iterate through reparse points
                    if ((File.GetAttributes(subdir) &
                             FileAttributes.ReparsePoint) !=
                                 FileAttributes.ReparsePoint)
                    {
                        //REM Thread.Sleep(1);

                        ProcessDir(subdir, LogName, ExceptionStr);

                        string isolateDirectory = System.IO.Path.GetFileName(subdir);
                        string renamedDirectory = RemoveChar(isolateDirectory, ExceptionStr);

                        if (isolateDirectory != renamedDirectory)
                        {
                            #region Write Directory Changes
                            if (File.Exists(LogName) == false)
                            {


                                using (XmlWriter xmlWriter = XmlWriter.Create(LogName, xmlWriterSettings))
                                {
                                    xmlWriter.WriteStartDocument();
                                    xmlWriter.WriteStartElement("Files_Folders");

                                    xmlWriter.WriteStartElement("Directory");
                                    xmlWriter.WriteElementString("Full_Path", subdir);
                                    xmlWriter.WriteElementString("Proposed_Name", renamedDirectory);
                                    xmlWriter.WriteEndElement();

                                    xmlWriter.WriteEndElement();
                                    xmlWriter.WriteEndDocument();
                                    xmlWriter.Flush();
                                    xmlWriter.Close();
                                }
                            }
                            else
                            {
                                XDocument xDocument = XDocument.Load(LogName);
                                XElement root = xDocument.Element("Files_Folders");
                                IEnumerable<XElement> rows = root.Descendants("File");
                                XElement firstRow = rows.First();
                                firstRow.AddBeforeSelf(
                                   new XElement("Directory",
                                   new XElement("Full_Path", subdir),
                                   new XElement("Proposed_Name", renamedDirectory)));
                                xDocument.Save(LogName);
                            }
                            #endregion
                        }
                        //recursionLvl++;
                    }

                });

                return true;
            }
            catch (Exception ex)
            {
                LogError(ErrorLogName, ex.Message.ToString());
                return false;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RichTextBox richText = new System.Windows.Controls.RichTextBox();
            richText.FontSize = 12;

            richText.AppendText("This program comes with no warranty.");
            richText.AppendText("\r\r");
            richText.AppendText("It doesn't offer much in the way of help.");
            richText.AppendText("\r\r");
            richText.AppendText("It will not stop you doing stupid things.");
            richText.AppendText("\r\r");
            richText.AppendText("It will happily rename whatever you tell it to, without warnings or errors.");
            richText.AppendText("\r\r");
            richText.AppendText("There is no undo option.");
            richText.AppendText("\r\r");
            richText.AppendText("Use entirely at your own risk.");
            richText.AppendText("\r\r");
            richText.AppendText("Doesn't yet handle file/folder rename clashes. Check the error logs!");
            richText.AppendText("\r\r");

            Window window = new Window
            {
                Title = "USE AT YOUR OWN RISK",
                Width = 420,
                Height = 255,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = richText

            };

            window.ShowDialog();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (m_oWorker.IsBusy)
            {

                // Notify the worker thread that a cancel has been requested.

                // The cancel will not actually happen until the thread in the

                // DoWork checks the m_oWorker.CancellationPending flag. 

                m_oWorker.CancelAsync();
            }
        }

        private void ShowPrompts_CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (promptShowChkbox.IsChecked == true)
            {
                showPrompts = true;
            }
            else
            {
                showPrompts = false;
            }
        }

        private void ShowPrompts_Checkbox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (promptShowChkbox.IsChecked == true)
            {
                showPrompts = true;
            }
            else
            {
                showPrompts = false;
            }
        }
    }
}
