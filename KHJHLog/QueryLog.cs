using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Campus.Configuration;
using FISCA.Data;
using FISCA.LogAgent;
using FISCA.UDT;
using KHJHLog;
using System.Diagnostics;


namespace KHJHLog
{
    public partial class QueryLog : FISCA.Presentation.Controls.BaseForm
    {
        //private List<string> Actions = new List<string>() {"自動轉入","調整班級","鎖定班級","解除鎖定班級","變更特殊身分","匯入新增學生", "匯入更新班級" };
        //private List<string> VerifyActions = new List<string>() { "匯入更新班級", "調整班級", "變更特殊身分", "鎖定班級" };
        private AccessHelper accesshelper = new AccessHelper();
        private QueryHelper queryhelper = new QueryHelper();
        private ConfigData config = Campus.Configuration.Config.User["Option"];

        List<string> _VerifyString = new List<string>();
        
        private Color UpdateColor = Color.FromArgb(255, 255, 192);
        private bool IsUpdate = false;

        public QueryLog()
        {
            InitializeComponent();
        }

        private void labelX2_Click(object sender, EventArgs e)
        {

        }

        private string GetContentFormat(string Action, string Content)
        {
            StringBuilder strBuilder = new StringBuilder();
            XElement elmContent;

            try
            {
                elmContent = XElement.Load(new StringReader("<root>" + Content + "</root>"));
            }
            catch
            {
                return Content;
            }

            if (Action.Equals("自動轉入"))
            {
                //自動轉入
                // <Content>
                //     <IDNumber>   </IDNumber>
                //     <StudentNumber> </StudentNumber>
                //     <StudentName>   </StudentName>
                //     <NewClassName> </NewClassName>
                //     <SeatNo> </SeatNo>
                //     <ScheduleClassDate> </ScheduleClassDate>
                //     <Reason> </Reason>
                //  ...其他需要和異動相關的欄位
                //  </Content>

                strBuilder.AppendLine(string.Format("身分證「{0}」", elmContent.ElementText("IDNumber")));
                strBuilder.AppendLine(string.Format("學號「{0}」", elmContent.ElementText("StudentNumber")));
                strBuilder.AppendLine(string.Format("姓名「{0}」", elmContent.ElementText("StudentName")));
                strBuilder.AppendLine(string.Format("班級「{0}」", elmContent.ElementText("NewClassName")));
                strBuilder.AppendLine(string.Format("座號「{0}」", elmContent.ElementText("SeatNo")));
                strBuilder.AppendLine(string.Format("備註「{0}」", elmContent.ElementText("Reason")));

                return strBuilder.ToString();
            }
            else if (Action.Equals("調整班級"))
            {
                //調整班級
                //  <Content>
                //      <IDNumber></IDNumber>
                //      <StudentNumber></StudentNumber>
                //      <StudentName></StudentName>
                //      <GradeYear></GradeYear>
                //      <ClassName></ClassName>
                //      <NewClassName></NewClassName>
                //      <Reason></Reason>
                //  </Content>

                strBuilder.AppendLine(string.Format("學生「{0}」從「{1}」調整班級到「{2}」", elmContent.ElementText("StudentName"), elmContent.ElementText("ClassName"), elmContent.ElementText("NewClassName")));
                strBuilder.AppendLine(string.Format("身分證「{0}」", elmContent.ElementText("IDNumber")));
                strBuilder.AppendLine(string.Format("學號「{0}」", elmContent.ElementText("StudentNumber")));
                strBuilder.AppendLine(string.Format("編班委員會會議日期「{0}」", elmContent.ElementText("ScheduleClassDate")));
                strBuilder.AppendLine(string.Format("備註「{0}」", elmContent.ElementText("Reason")));
                strBuilder.AppendLine(string.Format("第一優先順位班級「{0}」", elmContent.ElementText("FirstPriorityClassName")));
                strBuilder.AppendLine(string.Format("第二優先順位班級「{0}」", elmContent.ElementText("SecondPriorityClassName")));
                strBuilder.AppendLine(string.Format("相關證明文件網址「{0}」", elmContent.ElementText("EDoc")));
                return strBuilder.ToString();

                //return 
            }
            else if (Action.Equals("鎖定班級") ||
                     Action.Equals("解除鎖定班級"))
            {
                //鎖定／解除鎖定班級
                // <Content>
                //      <ClassName>資一忠</ClassName>
                //      <GradeYear>1</GradeYear>
                //      <Reason></Reason>
                // </Content>

                strBuilder.AppendLine(string.Format("{0}「{1}」", Action, elmContent.ElementText("ClassName")));
                strBuilder.AppendLine(string.Format("年級「{0}」", elmContent.ElementText("GradeYear")));
                strBuilder.AppendLine(string.Format("備註「{0}」", elmContent.ElementText("Comment")));

                // 顯示第二、三優先順位班級
                if (elmContent.Element("SecondPriorityClassName") != null)
                {
                    strBuilder.AppendLine(string.Format("第二優先順位班級「{0}」", elmContent.ElementText("SecondPriorityClassName")));
                }
                if (elmContent.Element("ThridPriorityClassName") != null)
                {
                    strBuilder.AppendLine(string.Format("第三優先順位班級「{0}」", elmContent.ElementText("ThridPriorityClassName")));
                }
                strBuilder.AppendLine(string.Format("相關證明文件網址「{0}」", elmContent.ElementText("EDoc")));

                return strBuilder.ToString();
            }
            else if (Action.Equals("變更特殊身分"))
            {
                //<Content>
                // <IDNumber>Q101000099</IDNumber>
                // <StudentNumber>11009</StudentNumber>
                // <StudentName>林九寶</StudentName>
                // <ClassName>330</ClassName>
                // <SeatNo>9</SeatNo>
                // <NumberReduce>0</NumberReduce>
                // <DocNo>456</DocNo>
                //</Content>

                strBuilder.AppendLine(string.Format("變更特殊身分學生「{0}」", elmContent.ElementText("StudentName")));
                strBuilder.AppendLine(string.Format("身分證「{0}」", elmContent.ElementText("IDNumber")));
                strBuilder.AppendLine(string.Format("學號「{0}」", elmContent.ElementText("StudentNumber")));
                strBuilder.AppendLine(string.Format("班級「{0}」", elmContent.ElementText("ClassName")));
                strBuilder.AppendLine(string.Format("座號「{0}」", elmContent.ElementText("SeatNo")));
                strBuilder.AppendLine(string.Format("文號「{0}」", elmContent.ElementText("DocNo")));
                strBuilder.AppendLine(string.Format("減免人數「{0}」", elmContent.ElementText("NumberReduce")));
                strBuilder.AppendLine(string.Format("相關證明文件網址「{0}」", elmContent.ElementText("EDoc")));

                return strBuilder.ToString();
            }
            else if (Action.Equals("取消特殊身分"))
            {
                //<Content>
                // <IDNumber>Q101000099</IDNumber>
                // <StudentNumber>11009</StudentNumber>
                // <StudentName>林九寶</StudentName>
                // <ClassName>330</ClassName>
                // <SeatNo>9</SeatNo>
                // <NumberReduce>0</NumberReduce>
                // <DocNo>456</DocNo>
                //</Content>

                strBuilder.AppendLine(string.Format("取消特殊身分學生「{0}」", elmContent.ElementText("StudentName")));
                strBuilder.AppendLine(string.Format("身分證「{0}」", elmContent.ElementText("IDNumber")));
                strBuilder.AppendLine(string.Format("學號「{0}」", elmContent.ElementText("StudentNumber")));
                strBuilder.AppendLine(string.Format("班級「{0}」", elmContent.ElementText("ClassName")));
                strBuilder.AppendLine(string.Format("座號「{0}」", elmContent.ElementText("SeatNo")));
                strBuilder.AppendLine(string.Format("文號「{0}」", elmContent.ElementText("DocNo")));
                strBuilder.AppendLine(string.Format("減免人數「{0}」", elmContent.ElementText("NumberReduce")));
                strBuilder.AppendLine(string.Format("相關證明文件網址「{0}」", elmContent.ElementText("EDoc")));

                return strBuilder.ToString();
            }
            else if (Action.Equals("特殊狀態變更"))
            {

                strBuilder.AppendLine(string.Format("狀態變更學生「{0}」", elmContent.ElementText("StudentName")));
                strBuilder.AppendLine(string.Format("身分證「{0}」", elmContent.ElementText("IDNumber")));
                strBuilder.AppendLine(string.Format("學號「{0}」", elmContent.ElementText("StudentNumber")));
                strBuilder.AppendLine(string.Format("班級「{0}」", elmContent.ElementText("ClassName")));
                strBuilder.AppendLine(string.Format("狀態變更前「{0}」", elmContent.ElementText("StudentStatus")));
                strBuilder.AppendLine(string.Format("狀態變更後「{0}」", elmContent.ElementText("NewStudentStatus")));
                strBuilder.AppendLine(string.Format("備註「{0}」", elmContent.ElementText("Reason")));
                return strBuilder.ToString();
            }
            else if (Action.Equals("一般狀態變更"))
            {

                strBuilder.AppendLine(string.Format("狀態變更學生「{0}」", elmContent.ElementText("StudentName")));
                strBuilder.AppendLine(string.Format("身分證「{0}」", elmContent.ElementText("IDNumber")));
                strBuilder.AppendLine(string.Format("學號「{0}」", elmContent.ElementText("StudentNumber")));
                strBuilder.AppendLine(string.Format("班級「{0}」", elmContent.ElementText("ClassName")));
                strBuilder.AppendLine(string.Format("狀態變更前「{0}」", elmContent.ElementText("StudentStatus")));
                strBuilder.AppendLine(string.Format("狀態變更後「{0}」", elmContent.ElementText("NewStudentStatus")));
                strBuilder.AppendLine(string.Format("備註「{0}」", elmContent.ElementText("Reason")));

                return strBuilder.ToString();
            }
            else if (Action.Equals("調整班級導師"))
            {
                strBuilder.AppendLine(string.Format("班級「{0}」班導師從「{1}」調整為「{2}」", elmContent.ElementText("ClassName"), elmContent.ElementText("OldTeacherName"), elmContent.ElementText("NewTeacherName")));                
                strBuilder.AppendLine(string.Format("備註「{0}」", elmContent.ElementText("Reason")));                
                strBuilder.AppendLine(string.Format("相關證明文件網址「{0}」", elmContent.ElementText("EDoc")));

                return strBuilder.ToString();
            }

            else if (Action.Equals("匯入更新班級"))
                return elmContent.ElementText("Summary");
            else if (Action.Equals("匯入更新"))
                return elmContent.ElementText("Summary");
            else if (Action.Equals("匯入新增學生"))
                return elmContent.ElementText("Summary");
            else if (Action.Equals("匯入特殊身分"))
                return elmContent.ElementText("Summary");
            else
                return Content;
        }
                      
        
        private bool IsKeywordContent(string Keyword,string Content)
        {
            if (string.IsNullOrEmpty(Keyword))
                return true;

            //逗號是OR
            if (Keyword.Contains(","))
            {
                string[] Keywords = Keyword.Split(new char[] { ',' });

                for (int i = 0; i < Keywords.Length; i++)
                {
                    if (Keywords[i] == "通過" || Keywords[i] == "不通過" || Keywords[i] == "待修正")
                        if (!_VerifyString.Contains(Keywords[i]))
                            _VerifyString.Add(Keywords[i]);
                }


                //當是OR的情況，只要有其中一個符合即傳回true
                for (int i = 0; i < Keywords.Length; i++)
                {
                    if (Content.Contains(Keywords[i]))
                        return true;
                    
                }

                return false;
            }
            //空白是AND
            {
                string[] Keywords = Keyword.Split(new char[] {' '});
                for (int i = 0; i < Keywords.Length; i++)
                {
                    if (Keywords[i] == "通過" || Keywords[i] == "不通過" || Keywords[i] == "待修正")
                        if (!_VerifyString.Contains(Keywords[i]))
                            _VerifyString.Add(Keywords[i]);
                }


                //當是And的情況，只要有其中一個不符合即傳回false
                for (int i = 0; i < Keywords.Length; i++)
                {
                    if (!Content.Contains(Keywords[i]))
                        return false;
                }

                return true;
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            Query();
        }

        private void Query()
        {
            if (IsUpdate)
            {
                IsUpdate = false;
                if (MessageBox.Show("提醒您有資料尚未儲存，是否先儲存後再查詢？", "自動編班查詢", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    Save();
            }

            string StartDate = dateStart.Value.ToShortDateString();

            config["start_date"] = StartDate;

            config.SaveAsync();

            grdLog.Rows.Clear();

            List<string> SelectedActions = new List<string>();

            foreach (ListViewItem Item in lstAction.Items)
            {
                if (Item.Checked)
                {
                    SelectedActions.Add(Item.Name);
                }
            }

            //若沒有選取任何動作，則傳回空資料
            if (SelectedActions.Count == 0)
                return;

            string strStartDate = dateStart.Value.ToShortDateString();
            string strEndDate = dateEnd.Value.ToShortDateString();

            StringBuilder strSQLBuilder = new StringBuilder();

            strSQLBuilder.Append("select uid,date_time,dsns,action,content,is_verify,comment from $school_log where date_time>='" + strStartDate + " 00:00:00' and date_time<='" + strEndDate + " 23:59:59' ");

            if (SelectedActions.Count > 0)
            {
                string strCondition = string.Join(",", SelectedActions.Select(x => "'" + x.Trim() + "'").ToArray());
                strSQLBuilder.Append(" and trim(action) in (" + strCondition + ")");
            }

            strSQLBuilder.Append(" order by date_time desc");

            string strSQL = strSQLBuilder.ToString();

            DataTable tblSchoolLog = queryhelper.Select(strSQL);
            List<School> Schools = accesshelper.Select<School>();

            StringBuilder sb = new StringBuilder();
            
            foreach (DataRow row in tblSchoolLog.Rows)
            {                
                string UID = row.Field<string>("uid");
                string Date = DateTime.Parse(row.Field<string>("date_time")).ToString("yyyy/MM/dd HH:mm");
                string DSNS = row.Field<string>("dsns");
                string Action = row.Field<string>("action").Trim();
                string Content = GetContentFormat(Action, row.Field<string>("content"));
                string IsVerify = row.Field<string>("is_verify");
                string Comment = row.Field<string>("comment");
                string EDoc="";
                // 處理連結位置
                sb.Clear();
                sb.Append("<root>");
                if(row["content"]!=null)
                    sb.Append(row["content"].ToString());
                sb.Append("</root>");
                try
                {
                    XElement elm = XElement.Parse(sb.ToString());
                    if (elm.Element("EDoc") != null)
                        EDoc = elm.Element("EDoc").Value;
                }
                catch(Exception ex)
                {}

                School vSchool = Schools
                    .Find(x => x.DSNS.Equals(DSNS));

                string SchoolName = vSchool != null ? vSchool.Title : DSNS;

                string SearchContent = SchoolName + string.Empty + Content+string.Empty+IsVerify;
                string Keyword = txtKeyword.Text;
                _VerifyString.Clear();
                if (IsKeywordContent(Keyword, SearchContent))
                {
                    // 當有輸入審核結果 Keyword 完全比對
                    if(_VerifyString.Count>0)             
                    {
                        if(_VerifyString.Contains(IsVerify))
                        {
                            int RowIndex = grdLog.Rows.Add(
                                UID,
                                Date,
                                SchoolName,
                                Action,
                                Content,
                                EDoc,
                                IsVerify,
                                Comment);
                        }

                    }else
                    {
                        int RowIndex = grdLog.Rows.Add(
                                UID,
                                Date,
                                SchoolName,
                                Action,
                                Content,
                                EDoc,
                                IsVerify,
                                Comment);
                    }                
                }
            }
        }

        private void QueryLog_Load(object sender, EventArgs e)
        {
            //Version version = Assembly.GetExecutingAssembly().GetName().Version;

            //this.Text += "『" + version.ToString() + "』";
            //this.TitleText += "『" + version.ToString() + "』";

            DateTime dteStart = DateTime.Today.AddDays(-7);
            dateStart.Value = dteStart;
            dateEnd.Value = DateTime.Today;

            string StartDate = config["start_date"];

            if (!string.IsNullOrWhiteSpace(StartDate))
            {
                DateTime.TryParse(StartDate, out dteStart);
                dateStart.Value = dteStart;
            }

            lstAction.Items.Clear();

            List<Action> Actions = Utility.AccessHelper
                .Select<Action>();

            foreach (Action Action in Actions)
            {
                ListViewItem vItem = new ListViewItem();
                vItem.Name = Action.Name;
                vItem.Text = Action.Verify ? vItem.Name + "（需審核）" : vItem.Name;
                vItem.Checked = true;
                lstAction.Items.Add(vItem);
            }
        }

        private void grdLog_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 處理連結
            if (e.RowIndex>=0 && e.ColumnIndex == colEDoc.Index && grdLog.Rows[e.RowIndex].Cells[colEDoc.Index].Value != null)
            {
                string url = grdLog.Rows[e.RowIndex].Cells[colEDoc.Index].Value.ToString();

                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        if (url.Contains("://"))
                        {
                            ProcessStartInfo info = new ProcessStartInfo(url);
                            Process.Start(info);
                        }
                        else
                        {
                            FISCA.Presentation.Controls.MsgBox.Show("網址不完整。");
                        }
                    }
                    catch (Exception ex)
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("網址無法解析，" + ex.Message);
                    }
                }
            }
        }

        private void grdLog_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            grdLog.Export("自動編班查詢記錄");
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem Item in lstAction.Items)
            {
                Item.Checked = chkSelectAll.Checked;
            }
        }

        private void grdLog_DoubleClick(object sender, EventArgs e)
        {
            if (grdLog.SelectedRows.Count == 1)
            {
                string Action =  ""+grdLog.Rows[grdLog.SelectedCells[0].RowIndex].Cells[3].Value;


                if (Action.Equals("匯入新增學生") || 
                    Action.Equals("匯入更新班級") ||
                    Action.Equals("匯入更新") ||
                    Action.Equals("匯入特殊身分"))
                    new frmDetailLog2(grdLog.Rows[grdLog.SelectedCells[0].RowIndex]).ShowDialog();
                else
                    new frmDetailLog(grdLog.Rows[grdLog.SelectedCells[0].RowIndex]).ShowDialog();
            }
        }

        private void grdLog_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (grdLog.Columns[e.ColumnIndex].Name.Equals("colComment"))
            {
                grdLog.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(255,255,192);
                IsUpdate = true;
            } else if (grdLog.Columns[e.ColumnIndex].Name.Equals("colVerify"))
            {
                grdLog.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(255, 255, 192);
                IsUpdate = true;
            }
        }

        private void txtKeyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                Query();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            UpdateHelper updateHelper = new UpdateHelper();

            List<string> updateSQLs = new List<string>();
            List<DataGridViewRow> Rows = new List<DataGridViewRow>();
            StringBuilder strLog = new StringBuilder();

            foreach (DataGridViewRow Row in grdLog.Rows)
            {
                if (Row.Cells["colComment"].Style.BackColor.Equals(UpdateColor) ||
                    Row.Cells["colVerify"].Style.BackColor.Equals(UpdateColor))
                {
                    string UID = "" + Row.Cells["colID"].Value;

                    string Date = "" + Row.Cells["colDate"].Value;
                    string School = "" + Row.Cells["colSchool"].Value;
                    string Action = "" + Row.Cells["colAction"].Value;
                    string Content = "" + Row.Cells["colContent"].Value;

                    string Comment = "" + Row.Cells["colComment"].Value;
                    string Verify = "" + Row.Cells["colVerify"].Value;

                    Rows.Add(Row);
                    updateSQLs.Add("UPDATE $school_log SET is_verify = '" + Verify + "', comment = '" + Comment + "' WHERE uid =" + UID);

                    strLog.AppendLine("日期時間「" + Date + "」學校「" + School + "」動作「" + Action + "」審核結果「" + Verify +"」備註「" + Comment +"」");
                }
            }

            if (updateSQLs.Count > 0)
            {
                try
                {
                    updateHelper.Execute(updateSQLs);

                    foreach (DataGridViewRow Row in Rows)
                    {
                        Row.Cells["colComment"].Style.BackColor = Color.White;
                        Row.Cells["colVerify"].Style.BackColor = Color.White;
                    }

                    IsUpdate = false;

                    ApplicationLog.Log("高雄市自動編班", "修改審核及備註", strLog.ToString());

                    MessageBox.Show("已成功更新" + updateSQLs.Count + "筆記錄！");
                }
                catch (Exception ve)
                {
                    MessageBox.Show("更新錯誤，錯誤訊息如下：" + System.Environment.NewLine + ve.Message);
                }
            }
        }

        private void 批次調整審核結果ToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            frmChangeVeritfySubForm cvsf = new frmChangeVeritfySubForm();
            if(cvsf.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
            {
                string SelectString = "";
                SelectString = cvsf.GetSelectItem();

                foreach(DataGridViewRow drv in grdLog.SelectedRows)
                {
                    if (drv.IsNewRow)
                        continue;

                    drv.Cells[colVerify.Index].Value = SelectString;
                }                
            }
        }
    }
}