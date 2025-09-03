﻿using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;

namespace GitForce
{
    public partial class FormHttpsAuth : Form
    {
		/// <summary>
		/// Keep user name and password fields as typed in the control
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Username
        {
            set { textUsername.Text = value.Trim(); }
            get { return textUsername.Text.Trim(); }
		}
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Password
        {
            set { textPassword.Text = value.Trim(); }
            get { return textPassword.Text.Trim(); }
        }

        /// <summary>
        /// Form constructor
        /// </summary>
        public FormHttpsAuth(bool enableUsername)
        {
            InitializeComponent();
            ClassWinGeometry.Restore(this);

            textUsername.Enabled = enableUsername;
        }

        /// <summary>
        /// Form is closing
        /// </summary>
        private void FormHttpsAuth_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClassWinGeometry.Save(this);
        }

        /// <summary>
        /// Show or hide password field
        /// </summary>
        private void CheckShowCheckedChanged(object sender, System.EventArgs e)
        {
            textPassword.UseSystemPasswordChar = !checkReveal.Checked;
        }

        /// <summary>
        /// Enables or disables OK button based on validity of the user name and password fields
        /// </summary>
        private void ValidateOk(object sender, System.EventArgs e)
        {
            // Validator for the user name field: must start with a letter and ...
            string username = textUsername.Text.Trim();
            Regex r = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");

            // This is a really lame validator for the password field
            // We simply want to avoid some 'dangerous' characters, including spaces
            string password = textPassword.Text.Trim();

            btOK.Enabled = (!textUsername.Enabled || r.IsMatch(username)) && (password.Length > 0) && password.IndexOfAny(@" \/".ToCharArray()) == -1;
        }
    }
}
