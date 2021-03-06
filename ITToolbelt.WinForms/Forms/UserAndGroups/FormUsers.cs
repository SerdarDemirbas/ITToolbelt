using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ITToolbelt.Bll.Managers;
using ITToolbelt.Entity.Db;
using ITToolbelt.WinForms.ExtensionMethods;
using ITToolbelt.WinForms.Forms.ControlSpesifications;

namespace ITToolbelt.WinForms.Forms.UserAndGroups
{
    public partial class FormUsers : Form
    {
        private WorkerStatus wStatus;
        public FormUsers()
        {
            InitializeComponent();
        }

        private void backgroundWorkerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            switch (wStatus)
            {
                case WorkerStatus.RefreshData:
                    RefreshData();
                    break;
                case WorkerStatus.GetFromDc:
                    SyncUsersWithAd();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void backgroundWorkerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBarStatus.StartStopMarque();
        }

        private void RefreshData()
        {
            UserManager userManager = new UserManager(GlobalVariables.ConnectInfo);
            List<User> users = userManager.GetAll();

            if (dataGridViewUsers.InvokeRequired)
            {
                dataGridViewUsers.Invoke(new Action(delegate
                {
                    userBindingSource.DataSource = users;
                }));
            }
            else
            {
                userBindingSource.DataSource = users;
            }
        }

        enum WorkerStatus
        {
            RefreshData,
            GetFromDc
        }

        private void FormUsers_Load(object sender, EventArgs e)
        {
            dataGridViewUsers.LoadGridColumnStatus();

            wStatus = WorkerStatus.RefreshData;
            toolStripProgressBarStatus.StartStopMarque();
            backgroundWorkerWorker.RunWorkerAsync();
        }

        private void buttonResfresh_Click(object sender, EventArgs e)
        {
            wStatus = WorkerStatus.RefreshData;
            toolStripProgressBarStatus.StartStopMarque();
            backgroundWorkerWorker.RunWorkerAsync();
        }

        private void FormUsers_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataGridViewUsers.SaveGridColumnStatus();
        }

        private void buttonColumnSelection_Click(object sender, EventArgs e)
        {
            FormGridColumnSelection formGridColumn = new FormGridColumnSelection(dataGridViewUsers.Columns);
            formGridColumn.ShowDialog();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormUser formUser = new FormUser();
            formUser.ShowDialog();

            wStatus = WorkerStatus.RefreshData;
            toolStripProgressBarStatus.StartStopMarque();
            backgroundWorkerWorker.RunWorkerAsync();

        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 0 || !(dataGridViewUsers.SelectedRows[0].DataBoundItem is User))
            {
                return;
            }

            User user = dataGridViewUsers.SelectedRows[0].DataBoundItem as User;
            if (user == null)
            {
                return;
            }

            FormUser formUser = new FormUser(user);
            formUser.ShowDialog();

            wStatus = WorkerStatus.RefreshData;
            toolStripProgressBarStatus.StartStopMarque();
            backgroundWorkerWorker.RunWorkerAsync();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsers.SelectedRows.Count == 0 || !(dataGridViewUsers.SelectedRows[0].DataBoundItem is User))
            {
                return;
            }

            User user = dataGridViewUsers.SelectedRows[0].DataBoundItem as User;
            if (user == null)
            {
                return;
            }
            if (GlobalMethods.DeleteConfirm(user.Fullname) != DialogResult.Yes)
            {
                return;
            }
            UserManager userManager = new UserManager(GlobalVariables.ConnectInfo);
            Tuple<bool, List<string>> delete = userManager.Delete(user.Id);
            delete.ShowDialog();
            if (delete.Item1)
            {
                wStatus = WorkerStatus.RefreshData;
                toolStripProgressBarStatus.StartStopMarque();
                backgroundWorkerWorker.RunWorkerAsync();
            }
        }

        private void buttonfromAd_Click(object sender, EventArgs e)
        {
            wStatus = WorkerStatus.GetFromDc;
            toolStripProgressBarStatus.StartStopMarque();
            backgroundWorkerWorker.RunWorkerAsync();
        }

        void SyncUsersWithAd()
        {
            UserManager userManager = new UserManager(GlobalVariables.ConnectInfo);
            Tuple<bool, List<string>> syncUsersWithAd = userManager.SyncUsersWithAd();
            syncUsersWithAd.ShowDialog();

            if (syncUsersWithAd.Item1)
            {
                RefreshData();
            }
        }
    }
}
