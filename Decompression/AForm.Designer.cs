namespace Decompression
{
    partial class AForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.button_ProfileViewer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_ProfileViewer
            // 
            this.button_ProfileViewer.Location = new System.Drawing.Point(13, 42);
            this.button_ProfileViewer.Name = "button_ProfileViewer";
            this.button_ProfileViewer.Size = new System.Drawing.Size(75, 23);
            this.button_ProfileViewer.TabIndex = 1;
            this.button_ProfileViewer.Text = "Viewer";
            this.button_ProfileViewer.UseVisualStyleBackColor = true;
            this.button_ProfileViewer.Click += new System.EventHandler(this.button_ProfileViewer_Click);
            // 
            // AForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 264);
            this.Controls.Add(this.button_ProfileViewer);
            this.Controls.Add(this.button1);
            this.Name = "AForm";
            this.Text = "Decompression";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_ProfileViewer;
    }
}

