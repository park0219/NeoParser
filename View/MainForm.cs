using NeoParser.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace NeoParser.View
{
    public partial class MainForm : Form
    {
        LoginForm loginForm;
        private static MainForm mainform;
        private static string user_id;
        private static System.Timers.Timer timer;
        private static DataGridView dg = new DataGridView();
        private static CheckBox cb1 = new CheckBox();
        private static CheckBox cb2 = new CheckBox();
        private static TextBox tb = new TextBox();

        public MainForm(LoginForm loginForm, string id)
        {
            InitializeComponent();
            mainform = this;
            this.loginForm = loginForm;
            user_id = id;
            dg = dataGridView1;
            cb1 = checkBox1;
            cb2 = checkBox2;
            tb = textBox1;

            dg.ColumnCount = 6;
            dg.Columns[0].Name = "제목";
            dg.Columns[0].Width = 350;
            dg.Columns[1].Name = "URL";
            dg.Columns[1].Width = 180;
            dg.Columns[2].Name = "작성자";
            dg.Columns[2].Width = 70;
            dg.Columns[3].Name = "작성일자";
            dg.Columns[3].Width = 80;
            dg.Columns[4].Name = "스크랩";
            dg.Columns[4].Width = 70;
            dg.Columns[5].Name = "SEQ";
            dg.Columns[5].Visible = false;

            comboBox1.Items.Add("10분");
            comboBox1.Items.Add("20분");
            comboBox1.Items.Add("30분");
            comboBox1.Items.Add("1시간");
            comboBox1.SelectedIndex = 0;

            timer = new System.Timers.Timer(1000 * 60 * 10);
            timer.Elapsed += startCrawler;
            timer.Enabled = true;

            //최초 실행
            crawler();
            setDataGrid();
        }

        //웹 정보 가져오기
        public static string getMainPage()
        {
            //로그인 (사용자 인증 후에 할 일)
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://net.neograph.co.kr/main/main.do");

            //세션 쿠키 유지
            req.CookieContainer = LoginForm.cookie;

            //사이트 긁어오기
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            TextReader r = (TextReader)new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            //Console.WriteLine(r.ReadToEnd());

            string htmlBuffer = r.ReadToEnd();

            return htmlBuffer;
        }

        //timer 이벤트
        private static void startCrawler(Object source, ElapsedEventArgs e)
        {
            crawler();
        }

        //크롤링
        private static void crawler()
        {
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(getMainPage());

            var boards = new List<board>();
            var lis = htmlDocument.DocumentNode.SelectNodes("//*[@id='mainWork1']/ul/li[*]");
            foreach (var li in lis)
            {
                //댓글  span 있으면 제거하고 가져오기
                if (li.Descendants("p").ElementAt(0).Descendants("a").First().Descendants("span").Count() > 0)
                {
                    li.Descendants("p").ElementAt(0).Descendants("a").First().Descendants("span").First().Remove();
                }
                var board = new board
                {
                    BOARD_TITLE = li.Descendants("p").ElementAt(0).Descendants("a").First().InnerText.Trim(),
                    BOARD_URL = li.Descendants("p").ElementAt(0).Descendants("a").First().GetAttributeValue("href", ""),
                    BOARD_NAME = li.Descendants("p").ElementAt(1).InnerText.Trim(),
                    BOARD_DATE = li.Descendants("p").ElementAt(2).InnerText.Trim().Replace(" ", "")
                };
                boards.Add(board);
            }
            saveDB(boards);
        }

        //DB에 저장
        private static void saveDB(List<board> boards)
        {
            int newCnt = 0;
            int keyCnt = 0;
            string keyword = tb.Text;
            string[] keywordList = keyword.Split(',');

            SqlConnection sqlconSelect = new SqlConnection("server=192.168.0.148,1433;database=NeoParser;uid=NeoParser;pwd=1");
            SqlConnection sqlconInsert = new SqlConnection("server=192.168.0.148,1433;database=NeoParser;uid=NeoParser;pwd=1");
            SqlDataReader rd = null;
            try
            {
                sqlconSelect.Open();

                if (sqlconSelect.State == ConnectionState.Open)
                {
                    foreach (var board in boards)
                    {
                        string strSql_Select = 
                            string.Format("SELECT COUNT(*) AS COUNT FROM board WHERE BOARD_TITLE = '{0}' AND BOARD_URL = '{1}' AND BOARD_NAME = '{2}' AND BOARD_DATE = '{3}' AND USER_ID = '{4}'"
                            , board.BOARD_TITLE, board.BOARD_URL, board.BOARD_NAME, board.BOARD_DATE, user_id);
                        
                        SqlCommand cmd_Select = new SqlCommand(strSql_Select, sqlconSelect);

                        rd = cmd_Select.ExecuteReader();
                        rd.Read();
                        if(rd["COUNT"].ToString() == "0")
                        {
                            //키워드 검색
                            if (cb1.Checked == true)
                            {
                                foreach (string key in keywordList)
                                {
                                    if (board.BOARD_TITLE.Contains(key) || board.BOARD_NAME.Contains(key))
                                    {
                                        keyCnt++;
                                    }
                                }
                            }
                            newCnt++;
                            
                            sqlconInsert.Open();
                            if (sqlconInsert.State == ConnectionState.Open)
                            {
                                board.BOARD_CLIP = "N";
                                string strSql_Insert =
                                    string.Format("INSERT INTO board(BOARD_TITLE, BOARD_URL, BOARD_NAME, BOARD_DATE, BOARD_CLIP, USER_ID) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')"
                                    , board.BOARD_TITLE, board.BOARD_URL, board.BOARD_NAME, board.BOARD_DATE, board.BOARD_CLIP, user_id);
                                SqlCommand cmd_Insert = new SqlCommand(strSql_Insert, sqlconInsert);
                                cmd_Insert.ExecuteNonQuery();
                                sqlconInsert.Close();
                            }
                        }
                        if (rd != null)
                        {
                            rd.Close();
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "error");
            }
            finally
            {
                if(rd != null)
                {
                    rd.Close();
                }
                if(sqlconSelect != null)
                {
                    sqlconSelect.Close();
                }
                if(sqlconInsert != null)
                {
                    sqlconInsert.Close();
                }
            }

            //키워드 검색 활성화 + 검색 결과 있을 때
            if (keyCnt > 0 && cb1.Checked == true)
            {
                FlashWindow.Flash(mainform);
                setDataGrid();
                //MessageBox.Show(keyword + "로 검색된" + keyCnt + "개의 새 글이 등록되었습니다.", "새 글 알림");
                MessageForm mf = new MessageForm(keyword + "로 검색된" + keyCnt + "개의 새 글이 등록되었습니다.", "새 글 알림");
                mf.StartPosition = FormStartPosition.CenterParent; //부모창 가운데
                mf.ShowDialog(mainform);
            }
            //키워드 검색 비 활성화 + 검색 결과 있을 떄
            else if (newCnt > 0 && cb1.Checked == false)
            {
                FlashWindow.Flash(mainform);
                setDataGrid();
                //MessageBox.Show(newCnt + "개의 새 글이 등록되었습니다.", "새 글 알림");
                MessageForm mf = new MessageForm(newCnt + "개의 새 글이 등록되었습니다.", "새 글 알림");
                mf.StartPosition = FormStartPosition.CenterParent; //부모창 가운데
                mf.ShowDialog(mainform);
            }
            //새로운게 있을 때
            else if(newCnt > 0)
            {
                setDataGrid();
            }
        }

        //1: 스크랩만, 0: 모두
        private static void setDataGrid()
        {
            dg.Rows.Clear();
            dg.Refresh();

            SqlConnection sqlconSelect = new SqlConnection("server=192.168.0.148,1433;database=NeoParser;uid=NeoParser;pwd=1");
            SqlDataReader rd = null;
            try
            {
                sqlconSelect.Open();
                if (sqlconSelect.State == ConnectionState.Open)
                {
                    string strSql_Select = "";
                    if (cb2.Checked == false)
                    {
                        strSql_Select = string.Format("SELECT BOARD_SEQ, BOARD_TITLE, BOARD_URL, BOARD_NAME, BOARD_DATE, BOARD_CLIP, USER_ID FROM board WHERE USER_ID = '{0}' ORDER BY BOARD_DATE DESC, BOARD_SEQ DESC", user_id);
                        //DBResult = context.board.OrderBy(p => p.BOARD_SEQ);
                    }
                    else
                    {
                        strSql_Select = string.Format("SELECT BOARD_SEQ, BOARD_TITLE, BOARD_URL, BOARD_NAME, BOARD_DATE, BOARD_CLIP, USER_ID FROM board WHERE BOARD_CLIP = 'Y' AND USER_ID = '{0}' ORDER BY BOARD_DATE DESC, BOARD_SEQ DESC", user_id);
                        //DBResult = context.board.Where(p => p.BOARD_CLIP == "Y").OrderBy(p => p.BOARD_SEQ);
                    }

                    SqlCommand cmd_Select = new SqlCommand(strSql_Select, sqlconSelect);

                    rd = cmd_Select.ExecuteReader();

                    while (rd.Read())
                    {
                        int index = dg.Rows.Add(rd["BOARD_TITLE"].ToString(), "http://net.neograph.co.kr" + rd["BOARD_URL"].ToString(), rd["BOARD_NAME"].ToString(), rd["BOARD_DATE"].ToString(), rd["BOARD_CLIP"].ToString(), rd["BOARD_SEQ"].ToString());
                        string date = DateTime.Now.ToString("yyyy.MM.dd");
                        if (rd["BOARD_DATE"].ToString() == date)
                        {
                            dg.Rows[index].DefaultCellStyle.BackColor = Color.DarkGray;
                        }
                    }
                    if(rd != null)
                    {
                        rd.Close();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "error");
            }
            finally
            {
                if (rd != null)
                {
                    rd.Close();
                }
                if (sqlconSelect != null)
                {
                    sqlconSelect.Close();
                }
            }
        }

        //타이머 변경
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "10분")
            {
                timer.Interval = 1000 * 60 * 10;
            }
            else if (comboBox1.SelectedItem.ToString() == "20분")
            {
                timer.Interval = 1000 * 60 * 20;
            }
            else if (comboBox1.SelectedItem.ToString() == "30분")
            {
                timer.Interval = 1000 * 60 * 30;
            }
            else
            {
                timer.Interval = 1000 * 60 * 60;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            setDataGrid();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            crawler();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Hide();
            loginForm.Show();
        }

        //스크랩 변경
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int r = e.RowIndex;
            SqlConnection sqlconUpdate= new SqlConnection("server=192.168.0.148,1433;database=NeoParser;uid=NeoParser;pwd=1");

            try
            {
                string board_seq = dg.Rows[r].Cells[5].Value.ToString();
                string board_clip = dg.Rows[r].Cells[4].Value.ToString();

                sqlconUpdate.Open();
                if (sqlconUpdate.State == ConnectionState.Open)
                {
                    if (board_clip == "Y")
                    {
                        board_clip = "N";
                        if(cb2.Checked == true)
                        {
                            dg.Rows.Remove(dg.Rows[r]);
                        }
                        else
                        {
                            dg.Rows[r].Cells[4].Value = board_clip;
                        }
                    }
                    else
                    {
                        board_clip = "Y";
                        dg.Rows[r].Cells[4].Value = board_clip;
                    }
                }
                string strSql_Update = string.Format("UPDATE board SET BOARD_CLIP = '{0}' WHERE BOARD_SEQ = '{1}'"
                                    , board_clip, board_seq);
                SqlCommand cmd_Update = new SqlCommand(strSql_Update, sqlconUpdate);
                cmd_Update.ExecuteNonQuery();
                sqlconUpdate.Close();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, "오류");
            }
            finally
            {
                if (sqlconUpdate != null)
                {
                    sqlconUpdate.Close();
                }
            }
        }
    }
}
