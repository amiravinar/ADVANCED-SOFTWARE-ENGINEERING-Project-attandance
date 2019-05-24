using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Windows.Forms;

namespace Fingerprint_attendance_mgt
{
    class Database
    {
        private string id, name, dept, gender, position;
        private byte[] photo;
        private string path, dbName;
        private SqlConnection conn;
        string timeIn_, timeOut_, date_;

        #region member variables
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

        public string Name
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

        public string Dept
        {
            get
            {
                return dept;
            }

            set
            {
                dept = value;
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

        public byte[] Photo
        {
            get
            {
                return photo;
            }

            set
            {
                photo = value;
            }
        }
        #endregion

        public Database()
        {
            path = Path.GetFullPath(Environment.CurrentDirectory);
            dbName = "Attendance_mgt.mdf";
            conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + path + @"\" + dbName + ";Integrated Security=True;Connect Timeout=30");
        }

     
        public void dbEnroll(string id,string fullname, string dept, string gender, string pos, string phone, string email, byte[] photo, string fprint, string level, string category)
        {
            using (conn)
            {
                try
                {
                    var cmd = new SqlCommand("INSERT INTO [Staff_Enroll] VALUES (@id, @name, @dept, @gender, @position,@phone, @email, @photo, @fprint, @level, @category)", conn);
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("name", fullname);
                    cmd.Parameters.AddWithValue("dept", dept);
                    cmd.Parameters.AddWithValue("gender", gender);
                    cmd.Parameters.AddWithValue("position", pos);
                    cmd.Parameters.AddWithValue("phone", phone);
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("photo", photo);
                    cmd.Parameters.Add("fprint", SqlDbType.VarChar).Value = fprint;
                    cmd.Parameters.AddWithValue("level", level);
                    cmd.Parameters.AddWithValue("category", category);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch(SqlException ex)
                {
                    if(ex.Number == 2627 || ex.Number == 2601)
                    {
                        Enroll enroll = new Enroll();
                        enroll.sqlexception_handle();
                        
                    }
                }
                
            }
        }

        public List<string> db_retrieve()
        {
            List<string> list = new List<string>();
            using (conn)
            {
                var cmd = new SqlCommand("SELECT Fingerprint FROM [Staff_Enroll]",conn);
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(Convert.ToString(reader["Fingerprint"]));
                }

                return list;
            
            }
        }

        //public void getStaff_Details()
        //{
        //    var atten = new attendance_form();
        //    //atten.test(f_print);
        //    var print = atten.Staff_finger;

        //    using (conn)
        //    {
        //        var cmd = new SqlCommand("select [full name] from [staff_enroll] where Department = '@dept'", conn);
        //        //if(print == string.Empty)
        //        //{
        //        //    cmd.Parameters.AddWithValue("fingerprint", DBNull.Value);
        //        //}
        //        //else
        //        //{
        //        //    cmd.Parameters.AddWithValue("fingerprint", print);
        //        //}
        //        cmd.Parameters.AddWithValue("dept", "eee");
        //        conn.Open();
        //        var reader = cmd.ExecuteReader();

        //        while (reader.Read())
        //        {
        //            name = (string)reader["full name"];
        //            //id = reader["id"].ToString();
        //            //dept = (string)reader["department"];
        //            //gender = (string)reader["gender"];
        //            //position = (string)reader["position"];
        //            //photo = (byte[])reader["photo"];
        //        }
        //    }
        //}

        public void updateStaff_Attendance(string id, string date, string timeIn, string name, string level, string category)
        {
            
            using (conn)
            {
                var cmd = new SqlCommand("INSERT INTO [Staff_Attendance](Id, Date, TimeIn, Name, Level, Category) VALUES (@Id, @Date, @TimeIn, @name, @level, @category)", conn);
                cmd.Parameters.AddWithValue("Id", id);
                cmd.Parameters.AddWithValue("Date", date);
                cmd.Parameters.AddWithValue("TimeIn", timeIn);
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("level", level);
                cmd.Parameters.AddWithValue("category", category);
                //cmd.Parameters.AddWithValue("TimeOut", null);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void updatetimeOut(string id, string date, string time)
        {
            using (conn)
            {
                var cmd = new SqlCommand("UPDATE [Staff_Attendance] SET TimeOut = @time WHERE Id = @id AND Date = @date", conn);
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("date", date);
                cmd.Parameters.AddWithValue("time", time);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void timeIn_retrieve(string id, string date, out string timeIn, out string timeOut, out string out_date)
        {
            //string time;
            using (conn)
            {
                var cmd = new SqlCommand("SELECT TimeIn, Date, TimeOut FROM [Staff_Attendance] WHERE Id = @id AND Date = @date",conn);
                cmd.Parameters.AddWithValue("id", Int32.Parse(id));
                cmd.Parameters.AddWithValue("date", date);
                conn.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    timeIn_ = reader["TimeIn"].ToString();
                    timeOut_ = reader["TimeOut"].ToString();
                    date_ = reader["Date"].ToString();
                }
            }
            timeIn = timeIn_;
            timeOut = timeOut_;
            out_date = date_;
            //return time;
            //return date;
        }

        public DataTable dailylog_retrieve(string date_from, string date_to)
        {
            DataTable dt = new DataTable();
            using (conn)
            {
                conn.Open();
                var adapter = new SqlDataAdapter("SELECT Id, Name, Level, Category, Date, TimeIn, TimeOut FROM [Staff_Attendance] WHERE Date BETWEEN @date_from AND @date_to", conn);
                adapter.SelectCommand.Parameters.AddWithValue("@date_from", date_from);
                adapter.SelectCommand.Parameters.AddWithValue("@date_to", date_to);
                adapter.Fill(dt);
            }
            return dt;
        }

        public void loginCheck(string username, string password)
        {
            var login = new Login();
            using (conn)
            {
                var cmd = new SqlCommand("SELECT COUNT(*) FROM LoginTable WHERE Username = @username AND Password = @password", conn);
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);

                conn.Open();
                if ((int)cmd.ExecuteScalar() > 0)
                {
                    login.valid();
                }
                else
                {
                    login.invalid();
                }
            }
        }

        public void remove_staff(string id)
        {
            using (conn)
            {
                var cmd = new SqlCommand("DELETE FROM Staff_Enroll WHERE Id = @id ", conn);
                cmd.Parameters.AddWithValue("id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void clear_attendance()
        {
            using (conn)
            {
                var cmd = new SqlCommand("DELETE FROM Staff_Attendance ", conn);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            
        }

        public void deleteAll()
        {
            using (conn)
            {
                var cmd = new SqlCommand("DELETE FROM Staff_Enroll ", conn);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
