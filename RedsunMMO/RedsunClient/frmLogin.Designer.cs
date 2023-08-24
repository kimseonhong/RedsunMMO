namespace RedsunClient
{
    partial class frmLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnTitle = new Panel();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // pnTitle
            // 
            pnTitle.Dock = DockStyle.Top;
            pnTitle.Location = new Point(0, 0);
            pnTitle.Name = "pnTitle";
            pnTitle.Size = new Size(800, 100);
            pnTitle.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Location = new Point(534, 285);
            panel1.Name = "panel1";
            panel1.Size = new Size(200, 100);
            panel1.TabIndex = 1;
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Controls.Add(pnTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmLogin";
            Text = "Redsun MMO Login";
            ResumeLayout(false);
        }

        #endregion

        private Panel pnTitle;
        private Panel panel1;
    }
}