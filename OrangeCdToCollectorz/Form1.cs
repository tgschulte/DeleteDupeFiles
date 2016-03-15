using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace DelDupes
{
  internal partial class Form1 : Form
  {
    string _lastPoprerty = string.Empty;
    string _selectedPath = string.Empty;
    int _cnt = 0;

    // This delegate enables asynchronous calls for setting  
    // the text property on a TextBox control.  
    delegate void SetTextCallback(RichTextBox label, string text);

    public Form1()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      //private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;

      DialogResult result = folderBrowserDialog1.ShowDialog();
      if (result == DialogResult.OK) // Test result.
      {
        this._selectedPath = folderBrowserDialog1.SelectedPath;
        richTextBox1.Text = "Loading contents of " + this._selectedPath + Environment.NewLine;

        if (this.checkBoxDoIt.Checked == false)
        {
          richTextBox1.Text = "WILL NOT DELETE" + Environment.NewLine;
        }

        progressBar1.Maximum = 100;
        progressBar1.Step = 1;
        progressBar1.Value = 0;
        backgroundWorker1.WorkerReportsProgress = true;
        backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
        backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
        backgroundWorker1.RunWorkerAsync();
      }
      else
      {
        richTextBox1.Text = result.ToString(); // <-- For debugging use.
      }
    }

    private void Form1_Load(object sender, System.EventArgs e)
    {
      // Start the BackgroundWorker.
      backgroundWorker1.RunWorkerAsync();
    }

    private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      this.progressBar1.Value = e.ProgressPercentage;
      this.progressBar1.Update();
    }

    private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      // write output to a file
      richTextBox1.Text += Environment.NewLine + "COMPLETE!";
    }

    private void DirSearch(string sDir, object sender, DoWorkEventArgs e)
    {
      var backgroundWorker = sender as BackgroundWorker;      // Show the dialog and get result.
      //int albumCnt = orangeCdCollection.Albums.All.Length;
      //this.SetText(this.richTextBox1, "OrangeCd.Collection has " + albumCnt.ToString() + " albums." + Environment.NewLine);

      ////richTextBox1.Text += "OrangeCd.Collection has " + albumCnt.ToString() + " albums." + Environment.NewLine;
      //int cnt = 0;

      try
      {
        //cnt++;
        //TODO: Get progress bar to work
        //double progress = cnt * 100.0 / albumCnt;
        //backgroundWorker.ReportProgress((int)progress);

        //Directory.GetFiles(dir, "*" + this.textSuffix.Text + ".*")

        string searchPattern = "*" + this.textSuffix.Text + ".*";

        foreach (string file in Directory.GetFiles(sDir, searchPattern))
        {
          FileInfo fileInfo = new FileInfo(file);
          long length = fileInfo.Length;
          string name = fileInfo.Name;
          string extension = fileInfo.Extension;

          string targetName = name.Replace(this.textSuffix.Text + fileInfo.Extension, "") + fileInfo.Extension;

          string path = Path.Combine(fileInfo.DirectoryName, targetName);

          if (File.Exists(path))
          {
            FileInfo fileInfoTarget = new FileInfo(path);
            if (fileInfoTarget.Length == length)
            {
              _cnt++;
              this.SetText(this.richTextBox1, Environment.NewLine);
              this.SetText(
                this.richTextBox1,
                string.Format(
                  "Found file {0} with same size of {1} as file {2}",
                  file,
                  length.ToString(),
                  fileInfoTarget.Name));
              this.SetText(this.richTextBox1, Environment.NewLine);

              if (this.checkBoxDoIt.Checked == true)
              {
                File.Delete(path);
                this.SetText(this.richTextBox1, "DELETED: " + file + Environment.NewLine);
              }
            }
          }
          else
          {
            this.SetText(
              this.richTextBox1,
              string.Format(
                "File {0} with size of {1} has no matching file {2}",
                file,
                length.ToString(),
                path));
            this.SetText(this.richTextBox1, Environment.NewLine);
          }
        }

        foreach (string d in Directory.GetDirectories(sDir))
        {
          this.DirSearch(d, sender, e);
        }
      }
      catch (System.Exception ex)
      {
        this.SetText(this.richTextBox1, "ERROR: " + ex.Message + Environment.NewLine);
        // listBox1.Items.Add(ex.Message);
      }      
    }

    private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
    {
      //var backgroundWorker = sender as BackgroundWorker;      // Show the dialog and get result.
      //int albumCnt = orangeCdCollection.Albums.All.Length;
      //this.SetText(this.richTextBox1, "OrangeCd.Collection has " + albumCnt.ToString() + " albums." + Environment.NewLine);

      ////richTextBox1.Text += "OrangeCd.Collection has " + albumCnt.ToString() + " albums." + Environment.NewLine;
      //int cnt = 0;

      try
      {
        //cnt++;
        //double progress = cnt * 100.0 / albumCnt;
        //backgroundWorker.ReportProgress((int)progress);
        DirSearch(this._selectedPath, sender, e);
        this.SetText(
                this.richTextBox1,
                  string.Format(
                          "Total matches {0}",
                          _cnt.ToString(CultureInfo.InvariantCulture)));
      }
      catch (System.Exception ex)
      {
        this.SetText(this.richTextBox1, "ERROR: " + ex.Message + Environment.NewLine);
        // listBox1.Items.Add(ex.Message);
      }
    }

    private void richTextBox1_TextChanged(object sender, EventArgs e)
    {
      richTextBox1.SelectionStart = richTextBox1.Text.Length; //Set the current caret position at the end
      richTextBox1.ScrollToCaret(); //Now scroll it automatically
    }

    // This method demonstrates a pattern for making thread-safe
    // calls on a Windows Forms control. 
    //
    // If the calling thread is different from the thread that
    // created the TextBox control, this method creates a
    // SetTextCallback and calls itself asynchronously using the
    // Invoke method.
    //
    // If the calling thread is the same as the thread that created
    // the TextBox control, the Text property is set directly. 
    private void SetText(RichTextBox label, string text)
    {
      // InvokeRequired required compares the thread ID of the
      // calling thread to the thread ID of the creating thread.
      // If these threads are different, it returns true.
      if (label.InvokeRequired)
      {
        SetTextCallback d = new SetTextCallback(SetText);
        this.Invoke(d, new object[] { label, text });
      }
      else
      {
        label.Text += text;
      }
    }

    private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
    {

    }

    private void label1_Click(object sender, EventArgs e)
    {

    }
  }
}
