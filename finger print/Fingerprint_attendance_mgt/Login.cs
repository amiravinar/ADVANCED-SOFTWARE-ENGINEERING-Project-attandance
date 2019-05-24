using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fingerprint_attendance_mgt
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        public void valid()
        {
            
            MessageBox.Show("Login Successful");
            
            var log = new Log();
            
            log.ShowDialog();
            
        }

        public void invalid()
        {
            MessageBox.Show("Incorrect Username or Password");
        }

        private void button_login_Click(object sender, EventArgs e)
        {
            var db = new Database();
            if (textBox_user.Text != string.Empty && textBox_pass.Text != string.Empty)
            {
                
                db.loginCheck(textBox_user.Text, textBox_pass.Text);
                this.Close();
            }
            else if (textBox_user.Text == string.Empty)
            {
                MessageBox.Show("Enter Username");
            }
            else
            {
                MessageBox.Show("Enter Password");
            }
        }
    }
}
