using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp;

namespace Fingerprint_attendance_mgt
{
    delegate void Function();
    delegate void setText(string text);
    delegate void setPhoto(byte[] photo);
    public partial class attendance_form : Form, DPFP.Capture.EventHandler
    {
        private string staff_finger; //variable to hold staff fingerprint from list for identification
        private string id,staff_Date, timeIn, timeOut, name, level, category;  //variables for storing staff date, timein and timeout
        private string path = Path.GetFullPath(Environment.CurrentDirectory);
        private string dbName = "Attendance_mgt.mdf";
        //SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + path + @"\" + dbName + ";Integrated Security=True;Connect Timeout=30");

        private List<string> list;
        private byte[] f_print;
        public DPFP.Capture.Capture Capturer;
        private DPFP.Template Template;
        private DPFP.Verification.Verification Verificator;


        public string Staff_finger
        {
            get
            {
                return staff_finger;
            }

            set
            {
                staff_finger = value;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public string Staff_Date
        {
            get
            {
                return staff_Date;
            }

            set
            {
                staff_Date = value;
            }
        }

        public string TimeIn
        {
            get
            {
                return timeIn;
            }

            set
            {
                timeIn = value;
            }
        }

        public string TimeOut
        {
            get
            {
                return timeOut;
            }

            set
            {
                timeOut = value;
            }
        }

        public string Name1
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public string Level
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
            }
        }

        public string Category
        {
            get
            {
                return category;
            }

            set
            {
                category = value;
            }
        }

        public attendance_form()
        {
            InitializeComponent();
            
            
        }


        protected  void  Init()
        {
            Capturer = new DPFP.Capture.Capture();                  // Create a capture operation.
            Capturer.EventHandler = this;                           // Subscribe for capturing events.
            Verificator = new DPFP.Verification.Verification();     // Create a fingerprint template verificator
            //UpdateStatus(0);
        }
        #region EventHandler Members:

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            MakeReport("The fingerprint sample was captured.");
            SetPrompt("Scan the same fingerprint again.");
            Process(Sample);
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The finger was removed from the fingerprint reader.");
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was touched.");
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was connected.");
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was disconnected.");
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good)
                MakeReport("The quality of the fingerprint sample is good.");
            else
                MakeReport("The quality of the fingerprint sample is poor.");
        }
        #endregion

        

        protected void SetPrompt(string prompt)
        {
            this.Invoke(new Function(delegate () {
                Prompt.Text = prompt;
            }));
        }



        protected void MakeReport(string message)
        {
            this.Invoke(new Function(delegate () {
                StatusText.AppendText(message + "\r\n");
            }));
        }

        public void DrawPicture(Bitmap bitmap)
        {
            this.Invoke(new Function(delegate () {
                Picture.Image = new Bitmap(bitmap, Picture.Size);   // fit the image into the picture box
            }));
        }

        protected void Process(DPFP.Sample Sample)
        {
            TimeSpan begin = TimeSpan.Parse("07:00");
            TimeSpan stop = TimeSpan.Parse("19:00");
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (now >= begin && now <= stop)
            {
                bool verified = false;
                // Draw fingerprint sample image.
                DrawPicture(ConvertSampleToBitmap(Sample));

                // Process the sample and create a feature set for the enrollment purpose.
                DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

                // Check quality of the sample and start verification if it's good
                // TODO: move to a separate task
                if (features != null)
                {
                    foreach (var val in list)
                    {

                        Staff_finger = val;
                        f_print = Convert.FromBase64String(val);
                        DPFP.Template temp = new DPFP.Template();
                        temp.DeSerialize(f_print);
                        Template = temp;


                        DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();
                        Verificator.Verify(features, Template, ref result);
                        //UpdateStatus(result.FARAchieved);
                        if (result.Verified)
                        {
                            verified = true;
                            MakeReport("The fingerprint was VERIFIED.");
                            checktimeOut();
                            //dbAccess();
                        }

                       
                    }
                    // Compare the feature set with our template
                    if (!verified)
                    {
                        MessageBox.Show("Invalid staff");
                    }

                }
            }
            else
            {
                MessageBox.Show("System Inactive");
            }
        }
        
        

        public void get_fprint()
        {
            Database db = new Database();
            list = new List<string>();
            list = db.db_retrieve();
            
        }

        public void setTextName(string text)
        {
            if (textBox_name.InvokeRequired)
            {
                setText d = new setText(setTextName);
                this.Invoke(d, new object[] { text });
            }
            textBox_name.Text = text;
        }

        public void setTextId(string text)
        {
            if (textBox_name.InvokeRequired)
            {
                setText d = new setText(setTextId);
                this.Invoke(d, new object[] { text });
            }
            textBox_id.Text = text;
        }
        public void setTextDept(string text)
        {
            if (textBox_name.InvokeRequired)
            {
                setText d = new setText(setTextDept);
                this.Invoke(d, new object[] { text });
            }
            textBox_dept.Text = text;
        }
        public void setTextPos(string text)
        {
            if (textBox_name.InvokeRequired)
            {
                setText d = new setText(setTextPos);
                this.Invoke(d, new object[] { text });
            }
            textBox_pos.Text = text;
        }
        public void setTextGender(string text)
        {
            if (textBox_name.InvokeRequired)
            {
                setText d = new setText(setTextGender);
                this.Invoke(d, new object[] { text });
            }
            textBox_gender.Text = text;
        }
        public void setTextTimeIn(string text)
        {
            if (textBox_timeIn.InvokeRequired)
            {
                setText d = new setText(setTextTimeIn);
                this.Invoke(d, new object[] { text });
            }
            textBox_timeIn.Text = text;
        }
        public void setTextTimeOut(string text)
        {
            if (textBox_timeOut.InvokeRequired)
            {
                setText d = new setText(setTextTimeOut);
                this.Invoke(d, new object[] { text });
            }
            textBox_timeOut.Text = text;
        }
        public void setTextCategory(string text)
        {
            if (textBox_category.InvokeRequired)
            {
                setText d = new setText(setTextCategory);
                this.Invoke(d, new object[] { text });
            }
            textBox_category.Text = text;
        }
        public void setTextLevel(string text)
        {
            if (textBox_level.InvokeRequired)
            {
                setText d = new setText(setTextLevel);
                this.Invoke(d, new object[] { text });
            }
            textBox_level.Text = text;
        }


        public void setPhoto(byte[] photo)
        {

            Bitmap image = new Bitmap(new MemoryStream(photo));
            pictureBox1.Image = image;
        }


        public void dbAccess() //db access to update verfication form with name, timeIn, etc
        {
            using (var conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + path + @"\" + dbName + ";Integrated Security=True;Connect Timeout=30"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM [Staff_Enroll] WHERE Fingerprint = @fingerprint ", conn);
                cmd.Parameters.AddWithValue("fingerprint", Staff_finger);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    setTextName(reader["Full Name"].ToString());
                    setTextId(reader["Id"].ToString());
                    setTextDept(reader["Department"].ToString());
                    setTextPos(reader["Position"].ToString());
                    setTextGender(reader["Gender"].ToString());
                    setPhoto((byte[])reader["Photo"]);
                    setTextCategory(reader["Category"].ToString());
                    setTextLevel(reader["Level"].ToString());

                    TimeIn = DateTime.Now.ToString("hh:mm tt");
                    setTextTimeIn(TimeIn);


                    Id = reader["Id"].ToString();
                    Name1 = reader["Full Name"].ToString();
                    Level = reader["Level"].ToString();
                    Category = reader["Category"].ToString();
                    Staff_Date = DateTime.Now.ToString("dd-MMM-yyyy");

                    setTextTimeOut(""); // to clear the time-out field

                }


            }
            Database db = new Database();
            db.updateStaff_Attendance(Id, Staff_Date, TimeIn, Name1, Level, Category);

        }

        public void checktimeOut()
        {
            using (var conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + path + @"\" + dbName + ";Integrated Security=True;Connect Timeout=30"))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM [Staff_Enroll] WHERE Fingerprint = @fingerprint ", conn);
                cmd.Parameters.AddWithValue("fingerprint", Staff_finger);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Id = reader["Id"].ToString();
                    setTextName(reader["Full Name"].ToString());
                    setTextId(reader["Id"].ToString());
                    setTextDept(reader["Department"].ToString());
                    setTextPos(reader["Position"].ToString());
                    setTextGender(reader["Gender"].ToString());
                    setPhoto((byte[])reader["Photo"]);
                    setTextCategory(reader["Category"].ToString());
                    setTextLevel(reader["Level"].ToString());
                }
            }

            Database db = new Database();
            var date = DateTime.Now.ToString("dd-MMM-yyyy");
            //var timeIn = db.timeIn_retrieve(Id, date);
            string timeIn, timeOut, out_date;

            db.timeIn_retrieve(Id, date, out timeIn, out timeOut, out out_date);
            //MessageBox.Show(timeIn);
            if (!string.IsNullOrEmpty(timeOut))
            {
                setTextTimeOut(timeOut);
                setTextTimeIn(timeIn);
                MessageBox.Show("Already Signed Out");
                return;
            }
            if (!string.IsNullOrEmpty(timeIn))
            {
                TimeOut = DateTime.Now.ToString("hh:mm tt");
                setTextTimeOut(TimeOut);
                setTextTimeIn(timeIn);
                Database db_ = new Database();
                db_.updatetimeOut(Id, date, TimeOut);
            }
            else
            {
                dbAccess();
            }
        }





        public void showStaff_Details()
        {
            textBox_name.Text = Staff_finger;
            //var fing = f_print;
            //var db = new Database();
            //db.getStaff_Details();

            //this.textBox_name.Invoke(new setText(showStaff_Details), new object[] { db.Name});
            //textBox_name.Text = db.Name;
            //textBox_id.Text = db.Id;
            //textBox_dept.Text = db.Dept;
            //textBox_gender.Text = db.Gender;
            //textBox_pos.Text = db.Position;
        }

        public void test(string test) {
            MessageBox.Show(test);
        }

        protected void Start()
        {
            Capturer.StartCapture();
            SetPrompt("Using the fingerprint reader, scan your fingerprint.");
        }

        protected void Stop()
        {
            Capturer.StopCapture();
        }

        protected Bitmap ConvertSampleToBitmap(DPFP.Sample Sample)
        {
            DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion();  // Create a sample convertor.
            Bitmap bitmap = null;                                                           // TODO: the size doesn't matter
            Convertor.ConvertToPicture(Sample, ref bitmap);                                 // TODO: return bitmap as a result
            return bitmap;
        }

        protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
        {
            DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();  // Create a feature extractor
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);            // TODO: return features as a result?
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }
        private void staffEnrollmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stop();
            var enrollForm = new Enroll();
            
            enrollForm.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            date.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            time.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var login = new Login();
            login.ShowDialog();
            

        }

       

        private void attendance_form_Load(object sender, EventArgs e)
        {
            get_fprint();
            Init();
            Start();
            
        }

        private void attendance_form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
        }

        private void attendance_form_Activated(object sender, EventArgs e)
        {
            get_fprint();
            Start();
        }
    }
}
