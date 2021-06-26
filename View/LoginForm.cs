using NeoParser.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoParser
{
    public partial class LoginForm : Form
    {

        MainForm mf;
        public static CookieContainer cookie = new CookieContainer();

        public LoginForm()
        {
            InitializeComponent();
        }

        //비밀번호 보기
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                textBox2.PasswordChar = default(char);
            }
            else
            {
                textBox2.PasswordChar = '*';
            }
        }

        //인트라넷 바로가기
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://net.neograph.co.kr/login/login.do");
        }

        //로그인 처리
        public static string mainLogin(string id, string pw)
        {
            //REQUEST 설정
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://net.neograph.co.kr/login/login_proc.do");
            req.Method = "POST";
            req.UserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.106 Safari/535.2";
            string s = String.Format("id={0}&pw={1}", id, pw); //아이디 비번 설정
            req.CookieContainer = cookie;
            req.ContentLength = s.Length;
            req.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
            //req.ContentType = "text/plain";
            //req.ContentType = "application/xml, text/xml, */*; q=0.01";
            req.KeepAlive = true;

            //POST값 전송
            TextWriter w = (TextWriter)new System.IO.StreamWriter(req.GetRequestStream());
            w.Write(s);
            w.Close();

            //사이트 긁어오기
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            TextReader r = (TextReader)new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            string result = r.ReadToEnd().ToString();

            return result;
        }

        //로그인 버튼
        private void button1_Click(object sender, EventArgs e)
        {
            string id = textBox1.Text;
            string pw = textBox2.Text;
            string result = mainLogin(id, pw);

            if (result.Contains("alert(\"아이디와 비밀번호가 일치하지 않습니다.\\n확인 후 다시 시도 해주세요.\");"))
            {
                MessageBox.Show("로그인에 실패하였습니다. 아이디와 비밀번호를 확인해주세요.", "로그인 실패");
            }
            else
            {
                //Console.WriteLine("로그인 성공");
                textBox1.Text = "";
                textBox2.Text = "";

                this.Hide();
                mf = new MainForm(this, id);
                mf.Show();
            }
        }

        //비밀번호 엔터
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                button1_Click(sender, e);
            }
        }
    }
}
