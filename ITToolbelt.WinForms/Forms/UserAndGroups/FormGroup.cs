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

namespace ITToolbelt.WinForms.Forms.UserAndGroups
{
    public partial class FormGroup : Form
    {
        private Group Group;
        public FormGroup()
        {
            InitializeComponent();

            Group = new Group();
        }
        public FormGroup(Group group)
        {
            InitializeComponent();

            Group = group;
            textBoxName.Text = group.Name;
            textBoxDesc.Text = group.Description;

            UserManager groupManager = new UserManager(GlobalVariables.ConnectInfo);
            List<User> userGroups = groupManager.GetUserGroups(group.Id);
            userBindingSource.DataSource = userGroups;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Group.Name = textBoxName.Text;
            Group.Description = textBoxDesc.Text;

            List<User> users = userBindingSource.DataSource as List<User>;
            if (users != null)
            {
                Group.UserGroups = new List<UserGroup>();
                foreach (User group in users)
                {
                    UserGroup userGroup = new UserGroup { UserId = group.Id };
                    if (Group.Id > 0)
                    {
                        userGroup.GroupId = Group.Id;
                    }
                    Group.UserGroups.Add(userGroup);
                }
            }

            GroupManager userManager = new GroupManager(GlobalVariables.ConnectInfo);
            Tuple<bool, List<string>> add = Group.Id > 0 ? userManager.Update(Group) : userManager.Add(Group);
            add.ShowDialog();
            if (add.Item1)
            {
                Close();
            }
        }

        private void buttonGroupAdd_Click(object sender, EventArgs e)
        {
            List<User> users = userBindingSource.DataSource as List<User> ?? new List<User>();

            List<int> list = users.Select(g => g.Id).ToList();

            FormGetUsers formGetGroups = new FormGetUsers(list);
            formGetGroups.ShowDialog();


            foreach (User user in formGetGroups.Users)
            {
                if (users.All(g => g.Id != user.Id))
                {
                    users.Add(user);
                }
            }

            userBindingSource.DataSource = users;
            dataGridViewGroups.DataSource = null;
            dataGridViewGroups.DataSource = userBindingSource;
        }

        private void buttonRemoveGroup_Click(object sender, EventArgs e)
        {
            if (dataGridViewGroups.SelectedRows.Count == 0)
            {
                return;
            }

            User user = dataGridViewGroups.SelectedRows[0].DataBoundItem as User;
            if (user == null)
            {
                return;
            }

            List<User> users = userBindingSource.DataSource as List<User>;

            users.Remove(user);

            userBindingSource.DataSource = users;
            dataGridViewGroups.DataSource = null;
            dataGridViewGroups.DataSource = userBindingSource;
        }
    }
}
