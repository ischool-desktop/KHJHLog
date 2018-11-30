using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KHJHLog
{
    public partial class frmActionList : FISCA.Presentation.Controls.BaseForm
    {
        // 2018/11/30 穎驊註記，這邊程式碼很怪，其實根本不給新增，不確定當初設計者思維，新增資料先使用 Insert Table 處理

        private List<Action> Actions = null;

        public frmActionList()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool HasError = false;

            List<int> SurveyYears = new List<int>();

            foreach (DataGridViewRow Row in grdOpenDate.Rows)
            {
                if (!Row.IsNewRow)
                {
                    foreach (DataGridViewCell Cell in Row.Cells)
                    {

                    }
                }
            }

            if (HasError)
            {
                MessageBox.Show("輸入資料有誤，請檢查後再儲存！");
                return;
            }

            try
            {
                #region 先將全部資料刪除
                List<Action> DeleteRecords = Utility
                    .AccessHelper.Select<Action>();

                DeleteRecords.ForEach(x => x.Deleted = true);
                DeleteRecords.SaveAll();
                #endregion

                #region 新增資料
                Actions.Clear();

                foreach (DataGridViewRow Row in grdOpenDate.Rows)
                {
                    if (!Row.IsNewRow)
                    {
                        string Action = "" + Row.Cells[0].Value;
                        string Verify = string.IsNullOrEmpty("" + Row.Cells[1].Value) ?
                            "false" : "" + Row.Cells[1].Value;

                        Action vAction = new Action();

                        vAction.Name = Action;
                        vAction.Verify = Verify.ToLower().Equals("false") ? false : true;

                        Actions.Add(vAction);
                    }
                }

                Utility.AccessHelper.SaveAll(Actions);

                MessageBox.Show("儲存成功！");
                #endregion
            }
            catch (Exception ve)
            {
                MessageBox.Show(ve.Message);
            }
        }

        private void frmActionList_Load(object sender, EventArgs e)
        {
            grdOpenDate.Rows.Clear();            

            Actions = Utility.AccessHelper
               .Select<Action>();

            foreach (Action vAction in Actions)
            {
                grdOpenDate.Rows.Add(
                    vAction.Name,
                    vAction.Verify);
            }
        }
    }
}