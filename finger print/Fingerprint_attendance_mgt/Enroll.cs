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

namespace Fingerprint_attendance_mgt
{
    public partial class Enroll : Form, DPFP.Capture.EventHandler
    {
        private String name, id_num, level, category, department, gender, position, email, phone, fp_string;
        private byte[] img_arr;
        public DPFP.Capture.Capture Capturer;

        public delegate void OnTemplateEventHandler(DPFP.Template template);

        public event OnTemplateEventHandler On_Template;

        //private string path = Path.GetFullPath(Environment.CurrentDirectory);
        //private string dbName = "Attendance_mgt.mdf";

        #region member variables
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

        public string Id_num
        {
            get
            {
                return id_num;
            }

            set
            {
                id_num = value;
            }
        }

        public string Department
        {
            get
            {
                return department;
            }

            set
            {
                department = value;
            }
        }

        public string Gender
        {
            get
            {
                return gender;
            }

            set
            {
                gender = value;
            }
        }

        public string Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public string Email
        {
            get
            {
                return email;
            }

            set
            {
                email = value;
            }
        }

        public string Phone
        {
            get
            {
                return phone;
            }

            set
            {
                phone = value;
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
        #endregion member variables
        public Enroll()
        {
            InitializeComponent();
            On_Template += this.OnTemplate;
            //string path = Path.GetFullPath(Environment.CurrentDirectory);
        }

        private void OnTemplate(DPFP.Template template)
        {
            this.Invoke(new Function(delegate ()
            {
                Template = template;
                //VerifyButton.Enabled = SaveButton.Enabled = (Template != null);
                if (Template != null)
                    MessageBox.Show("The fingerprint template is ready for fingerprint verification.", "Fingerprint Enrollment");
                else
                    MessageBox.Show("The fingerprint template is not valid. Repeat fingerprint enrollment.", "Fingerprint Enrollment");
            }));
        }


        protected virtual void Init()
        {
            Capturer = new DPFP.Capture.Capture();                  // Create a capture operation.
            Capturer.EventHandler = this;                           // Subscribe for capturing events.
            Enroller = new DPFP.Processing.Enrollment();            // Create an enrollment.
            UpdateStatus();
        }

        protected virtual void Process(DPFP.Sample Sample)
        {
            // Draw fingerprint sample image.
            DrawPicture(ConvertSampleToBitmap(Sample));
            // Process the sample and create a feature set for the enrollment purpose.
            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Enrollment);

            // Check quality of the sample and add to enroller if it's good
            if (features != null)
                try
                {
                    MakeReport("The fingerprint feature set was created.");
                    Enroller.AddFeatures(features);     // Add feature set to template.
                }
                finally
                {
                    UpdateStatus();

                    // Check if template has been created.
                    switch (Enroller.TemplateStatus)
                    {
                        case DPFP.Processing.Enrollment.Status.Ready:   // report success and stop capturing
                            OnTemplate(Enroller.Template);
                            SetPrompt("Click Close, and then click Fingerprint Verification.");
                            Stop();
                            break;

                        case DPFP.Processing.Enrollment.Status.Failed:  // report failure and restart capturing
                            Enroller.Clear();
                            Stop();
                            UpdateStatus();
                            OnTemplate(null);
                            Start();
                            break;
                    }
                }
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

        #region Form EventHandler
        private void Enroll_Load(object sender, EventArgs e)
        {
            Init();
            Start();
            
        }

        private void Enroll_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
           
        }
        #endregion

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
            try
            {
                Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);
            }
            catch (DPFP.Error.SDKException)
            {
                MessageBox.Show("Place Finger on the scanner surface");
            }
                       // TODO: return features as a result?
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }


        protected void SetStatus(string status)
        {
            this.Invoke(new Function(delegate () {
                StatusLine.Text = status;
            }));
        }

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

        




        //public void staffDetails()
        //{
        //    using (var conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + path + @"\" + dbName + ";Integrated Security=True;Connect Timeout=30"))
        //    {
        //        conn.Open();
        //        SqlCommand cmd = new SqlCommand("INSERT INTO Staff_Enroll() VALUES ()", conn);
        //    }
        //}




        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpeg;*.bmp;*.png;*.jpg)|*.jpeg;*.bmp;*.png;*.jpg";
            if(open.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Load(open.FileName);
            }
       
            Image img = pictureBox1.Image;
            //byte[] img_arr;
            using(var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                img_arr = ms.ToArray();
            }

        }

       

        private void update_button_Click(object sender, EventArgs e)
        {
            if(Name_text.Text == string.Empty)
            {
                MessageBox.Show("Enter Name");
            }
            else if(Id_text.Text == string.Empty)
            {
                MessageBox.Show("Enter ID");
            }
            
            else if (Dept_text.Text == string.Empty)
            {
                MessageBox.Show("Enter Department");
            }
            else if (Gender_comboBox.Text == string.Empty)
            {
                MessageBox.Show("Enter Gender");
            }
            else if (Position_comboBox.Text == string.Empty)
            {
                MessageBox.Show("Enter Position");
            }
            else if (Phone_text.Text == string.Empty)
            {
                MessageBox.Show("Enter Phone Number");
            }
            else if (Email_text.Text == string.Empty)
            {
                MessageBox.Show("Enter email");
            }

            Name1 = Name_text.Text;
            Id_num = Id_text.Text;
            Department = Dept_text.Text;
            Gender = Gender_comboBox.Text;
            Position = Position_comboBox.Text;
            Phone = Phone_text.Text;
            Email = Email_text.Text;
            Level = comboBox_level.Text;
            Category = comboBox_category.Text;

            byte[] bytes = new byte[1632];
            try
            {
                Template.Serialize(ref bytes);
                fp_string = Convert.ToBase64String(bytes);

                dbUpdate();
            }
            catch(NullReferenceException)
            {
                MessageBox.Show("Enroll Fingerprint");
            }
            
           

        }

        private void dbUpdate()
        {
            Database db = new Database();
            db.dbEnroll(Id_num, Name1, Department, Gender, Position, Phone, Email, img_arr, fp_string, Level, Category);
            MessageBox.Show("User Enrolled");
        }

        public void sqlexception_handle()
        {
            MessageBox.Show("User already exists");
            return;
        }

        private void UpdateStatus()
        {
            // Show number of samples needed.
            SetStatus(String.Format("Fingerprint samples needed: {0}", Enroller.FeaturesNeeded));
        }

        private void clear_button_Click(object sender, EventArgs e)
        {
            Init();
            Start();
            Name_text.Text = null;
            Id_text.Text = null;
            Dept_text.Text = null;
            Phone_text.Text = null;
            Email_text.Text = null;
            pictureBox1.Image = pictureBox1.BackgroundImage;

            StatusText.Text = null;
            Picture.Image = null;
        }

        private DPFP.Processing.Enrollment Enroller;
        private DPFP.Template Template;
    }
}
