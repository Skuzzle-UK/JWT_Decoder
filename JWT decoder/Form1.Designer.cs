namespace JWT_decoder
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            jwtInputBox = new RichTextBox();
            jwtDecodedPayloadBox = new RichTextBox();
            jwtDecodedHeaderBox = new RichTextBox();
            decodeBtn = new Button();
            splitContainer1 = new SplitContainer();
            jwtInputLabel = new Label();
            decodedPayloadLabel = new Label();
            decodedHeaderLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // jwtInputBox
            // 
            jwtInputBox.BackColor = Color.FromArgb(45, 45, 48);
            jwtInputBox.BorderStyle = BorderStyle.None;
            jwtInputBox.Dock = DockStyle.Fill;
            jwtInputBox.ForeColor = Color.White;
            jwtInputBox.Location = new Point(5, 25);
            jwtInputBox.Margin = new Padding(0, 0, 0, 5);
            jwtInputBox.Name = "jwtInputBox";
            jwtInputBox.Size = new Size(425, 584);
            jwtInputBox.TabIndex = 0;
            jwtInputBox.Text = "";
            // 
            // jwtDecodedPayloadBox
            // 
            jwtDecodedPayloadBox.BackColor = Color.FromArgb(45, 45, 48);
            jwtDecodedPayloadBox.BorderStyle = BorderStyle.None;
            jwtDecodedPayloadBox.Dock = DockStyle.Fill;
            jwtDecodedPayloadBox.ForeColor = Color.FromArgb(220, 220, 220);
            jwtDecodedPayloadBox.Location = new Point(5, 233);
            jwtDecodedPayloadBox.Margin = new Padding(0);
            jwtDecodedPayloadBox.Name = "jwtDecodedPayloadBox";
            jwtDecodedPayloadBox.ReadOnly = true;
            jwtDecodedPayloadBox.Size = new Size(732, 406);
            jwtDecodedPayloadBox.TabIndex = 1;
            jwtDecodedPayloadBox.Text = "";
            // 
            // jwtDecodedHeaderBox
            // 
            jwtDecodedHeaderBox.BackColor = Color.FromArgb(45, 45, 48);
            jwtDecodedHeaderBox.BorderStyle = BorderStyle.None;
            jwtDecodedHeaderBox.Dock = DockStyle.Top;
            jwtDecodedHeaderBox.ForeColor = Color.FromArgb(220, 220, 220);
            jwtDecodedHeaderBox.Location = new Point(5, 25);
            jwtDecodedHeaderBox.Margin = new Padding(0, 0, 0, 5);
            jwtDecodedHeaderBox.Name = "jwtDecodedHeaderBox";
            jwtDecodedHeaderBox.ReadOnly = true;
            jwtDecodedHeaderBox.Size = new Size(732, 188);
            jwtDecodedHeaderBox.TabIndex = 2;
            jwtDecodedHeaderBox.Text = "";
            // 
            // decodeBtn
            // 
            decodeBtn.BackColor = Color.FromArgb(0, 122, 204);
            decodeBtn.Cursor = Cursors.Hand;
            decodeBtn.Dock = DockStyle.Bottom;
            decodeBtn.FlatAppearance.BorderColor = Color.Black;
            decodeBtn.FlatAppearance.BorderSize = 2;
            decodeBtn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 82, 164);
            decodeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 142, 224);
            decodeBtn.FlatStyle = FlatStyle.Flat;
            decodeBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            decodeBtn.ForeColor = Color.White;
            decodeBtn.Location = new Point(5, 609);
            decodeBtn.Name = "decodeBtn";
            decodeBtn.Size = new Size(425, 30);
            decodeBtn.TabIndex = 3;
            decodeBtn.Text = "Decode";
            decodeBtn.UseVisualStyleBackColor = false;
            decodeBtn.Click += decodeBtn_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = Color.FromArgb(30, 30, 30);
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(jwtInputBox);
            splitContainer1.Panel1.Controls.Add(jwtInputLabel);
            splitContainer1.Panel1.Controls.Add(decodeBtn);
            splitContainer1.Panel1.Padding = new Padding(5);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(jwtDecodedPayloadBox);
            splitContainer1.Panel2.Controls.Add(decodedPayloadLabel);
            splitContainer1.Panel2.Controls.Add(jwtDecodedHeaderBox);
            splitContainer1.Panel2.Controls.Add(decodedHeaderLabel);
            splitContainer1.Panel2.Padding = new Padding(5);
            splitContainer1.Size = new Size(1181, 644);
            splitContainer1.SplitterDistance = 435;
            splitContainer1.TabIndex = 4;
            // 
            // jwtInputLabel
            // 
            jwtInputLabel.AutoSize = true;
            jwtInputLabel.Dock = DockStyle.Top;
            jwtInputLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            jwtInputLabel.ForeColor = Color.FromArgb(200, 200, 200);
            jwtInputLabel.Location = new Point(5, 5);
            jwtInputLabel.Margin = new Padding(0, 0, 0, 5);
            jwtInputLabel.Name = "jwtInputLabel";
            jwtInputLabel.Padding = new Padding(0, 0, 0, 5);
            jwtInputLabel.Size = new Size(68, 20);
            jwtInputLabel.TabIndex = 4;
            jwtInputLabel.Text = "JWT Token";
            // 
            // decodedPayloadLabel
            // 
            decodedPayloadLabel.AutoSize = true;
            decodedPayloadLabel.Dock = DockStyle.Top;
            decodedPayloadLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            decodedPayloadLabel.ForeColor = Color.FromArgb(200, 200, 200);
            decodedPayloadLabel.Location = new Point(5, 213);
            decodedPayloadLabel.Margin = new Padding(0, 0, 0, 5);
            decodedPayloadLabel.Name = "decodedPayloadLabel";
            decodedPayloadLabel.Padding = new Padding(0, 0, 0, 5);
            decodedPayloadLabel.Size = new Size(102, 20);
            decodedPayloadLabel.TabIndex = 6;
            decodedPayloadLabel.Text = "Decoded Payload";
            // 
            // decodedHeaderLabel
            // 
            decodedHeaderLabel.AutoSize = true;
            decodedHeaderLabel.Dock = DockStyle.Top;
            decodedHeaderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            decodedHeaderLabel.ForeColor = Color.FromArgb(200, 200, 200);
            decodedHeaderLabel.Location = new Point(5, 5);
            decodedHeaderLabel.Margin = new Padding(0, 0, 0, 5);
            decodedHeaderLabel.Name = "decodedHeaderLabel";
            decodedHeaderLabel.Padding = new Padding(0, 0, 0, 5);
            decodedHeaderLabel.Size = new Size(101, 20);
            decodedHeaderLabel.TabIndex = 5;
            decodedHeaderLabel.Text = "Decoded Header";
            // 
            // Form1
            // 
            AcceptButton = decodeBtn;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(1181, 644);
            Controls.Add(splitContainer1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Name = "Form1";
            Text = "JWT Decoder";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox jwtInputBox;
        private RichTextBox jwtDecodedPayloadBox;
        private RichTextBox jwtDecodedHeaderBox;
        private Button decodeBtn;
        private SplitContainer splitContainer1;
        private Label jwtInputLabel;
        private Label decodedHeaderLabel;
        private Label decodedPayloadLabel;
    }
}
