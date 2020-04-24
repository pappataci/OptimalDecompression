namespace Continuation_Optimal_Deco
{
    partial class ContOptimalDeco
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose ( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose ( );
            }
            base.Dispose ( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent ( )
        {
            this.textBoxMaxDepth = new System.Windows.Forms.TextBox();
            this.labelMaxDepth = new System.Windows.Forms.Label();
            this.textBoxDescentRate = new System.Windows.Forms.TextBox();
            this.labelDescentRate = new System.Windows.Forms.Label();
            this.textBoxBottomTime = new System.Windows.Forms.TextBox();
            this.labelBottomTime = new System.Windows.Forms.Label();
            this.textBoxMaxAscentRate = new System.Windows.Forms.TextBox();
            this.labelMaxAscentRate = new System.Windows.Forms.Label();
            this.textBoxSurfaceTime = new System.Windows.Forms.TextBox();
            this.labelSurfaceTime = new System.Windows.Forms.Label();
            this.textBoxTargetPDCS = new System.Windows.Forms.TextBox();
            this.labelTargetPDCS = new System.Windows.Forms.Label();
            this.textBoxClearTime = new System.Windows.Forms.TextBox();
            this.labelClearTime = new System.Windows.Forms.Label();
            this.textBoxActualPDCS = new System.Windows.Forms.TextBox();
            this.labelActualPDCS = new System.Windows.Forms.Label();
            this.buttonCalculate = new System.Windows.Forms.Button();
            this.textBoxFractionO2 = new System.Windows.Forms.TextBox();
            this.labelFractionO2 = new System.Windows.Forms.Label();
            this.textBoxExponent = new System.Windows.Forms.TextBox();
            this.labelExponent = new System.Windows.Forms.Label();
            this.textBoxBreakFraction = new System.Windows.Forms.TextBox();
            this.labelBreakFraction = new System.Windows.Forms.Label();
            this.buttonIntersection = new System.Windows.Forms.Button();
            this.checkBoxSaveData = new System.Windows.Forms.CheckBox();
            this.buttonEvaluate = new System.Windows.Forms.Button();
            this.buttonBoundary = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxMaxDepth
            // 
            this.textBoxMaxDepth.Location = new System.Drawing.Point(12, 12);
            this.textBoxMaxDepth.Name = "textBoxMaxDepth";
            this.textBoxMaxDepth.Size = new System.Drawing.Size(100, 20);
            this.textBoxMaxDepth.TabIndex = 0;
            this.textBoxMaxDepth.Text = "120.0";
            // 
            // labelMaxDepth
            // 
            this.labelMaxDepth.AutoSize = true;
            this.labelMaxDepth.Location = new System.Drawing.Point(118, 15);
            this.labelMaxDepth.Name = "labelMaxDepth";
            this.labelMaxDepth.Size = new System.Drawing.Size(74, 13);
            this.labelMaxDepth.TabIndex = 1;
            this.labelMaxDepth.Text = "Max Depth (ft)";
            // 
            // textBoxDescentRate
            // 
            this.textBoxDescentRate.Location = new System.Drawing.Point(12, 39);
            this.textBoxDescentRate.Name = "textBoxDescentRate";
            this.textBoxDescentRate.Size = new System.Drawing.Size(100, 20);
            this.textBoxDescentRate.TabIndex = 2;
            this.textBoxDescentRate.Text = "60.0";
            // 
            // labelDescentRate
            // 
            this.labelDescentRate.AutoSize = true;
            this.labelDescentRate.Location = new System.Drawing.Point(118, 43);
            this.labelDescentRate.Name = "labelDescentRate";
            this.labelDescentRate.Size = new System.Drawing.Size(109, 13);
            this.labelDescentRate.TabIndex = 3;
            this.labelDescentRate.Text = "Descent Rate (ft/min)";
            // 
            // textBoxBottomTime
            // 
            this.textBoxBottomTime.Location = new System.Drawing.Point(13, 66);
            this.textBoxBottomTime.Name = "textBoxBottomTime";
            this.textBoxBottomTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxBottomTime.TabIndex = 4;
            this.textBoxBottomTime.Text = "30.0";
            // 
            // labelBottomTime
            // 
            this.labelBottomTime.AutoSize = true;
            this.labelBottomTime.Location = new System.Drawing.Point(118, 70);
            this.labelBottomTime.Name = "labelBottomTime";
            this.labelBottomTime.Size = new System.Drawing.Size(91, 13);
            this.labelBottomTime.TabIndex = 5;
            this.labelBottomTime.Text = "Bottom Time (min)";
            // 
            // textBoxMaxAscentRate
            // 
            this.textBoxMaxAscentRate.Location = new System.Drawing.Point(13, 93);
            this.textBoxMaxAscentRate.Name = "textBoxMaxAscentRate";
            this.textBoxMaxAscentRate.Size = new System.Drawing.Size(100, 20);
            this.textBoxMaxAscentRate.TabIndex = 6;
            this.textBoxMaxAscentRate.Text = "30.0";
            // 
            // labelMaxAscentRate
            // 
            this.labelMaxAscentRate.AutoSize = true;
            this.labelMaxAscentRate.Location = new System.Drawing.Point(118, 97);
            this.labelMaxAscentRate.Name = "labelMaxAscentRate";
            this.labelMaxAscentRate.Size = new System.Drawing.Size(125, 13);
            this.labelMaxAscentRate.TabIndex = 7;
            this.labelMaxAscentRate.Text = "Max Ascent Rate (ft/min)";
            // 
            // textBoxSurfaceTime
            // 
            this.textBoxSurfaceTime.Location = new System.Drawing.Point(302, 12);
            this.textBoxSurfaceTime.Name = "textBoxSurfaceTime";
            this.textBoxSurfaceTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxSurfaceTime.TabIndex = 8;
            this.textBoxSurfaceTime.Text = "50.0";
            // 
            // labelSurfaceTime
            // 
            this.labelSurfaceTime.AutoSize = true;
            this.labelSurfaceTime.Location = new System.Drawing.Point(409, 16);
            this.labelSurfaceTime.Name = "labelSurfaceTime";
            this.labelSurfaceTime.Size = new System.Drawing.Size(95, 13);
            this.labelSurfaceTime.TabIndex = 9;
            this.labelSurfaceTime.Text = "Surface Time (min)";
            // 
            // textBoxTargetPDCS
            // 
            this.textBoxTargetPDCS.Location = new System.Drawing.Point(13, 120);
            this.textBoxTargetPDCS.Name = "textBoxTargetPDCS";
            this.textBoxTargetPDCS.Size = new System.Drawing.Size(100, 20);
            this.textBoxTargetPDCS.TabIndex = 10;
            this.textBoxTargetPDCS.Text = "0.033";
            // 
            // labelTargetPDCS
            // 
            this.labelTargetPDCS.AutoSize = true;
            this.labelTargetPDCS.Location = new System.Drawing.Point(118, 124);
            this.labelTargetPDCS.Name = "labelTargetPDCS";
            this.labelTargetPDCS.Size = new System.Drawing.Size(70, 13);
            this.labelTargetPDCS.TabIndex = 11;
            this.labelTargetPDCS.Text = "Target PDCS";
            // 
            // textBoxClearTime
            // 
            this.textBoxClearTime.Enabled = false;
            this.textBoxClearTime.Location = new System.Drawing.Point(302, 39);
            this.textBoxClearTime.Name = "textBoxClearTime";
            this.textBoxClearTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxClearTime.TabIndex = 12;
            // 
            // labelClearTime
            // 
            this.labelClearTime.AutoSize = true;
            this.labelClearTime.Location = new System.Drawing.Point(409, 43);
            this.labelClearTime.Name = "labelClearTime";
            this.labelClearTime.Size = new System.Drawing.Size(82, 13);
            this.labelClearTime.TabIndex = 13;
            this.labelClearTime.Text = "Clear Time (min)";
            // 
            // textBoxActualPDCS
            // 
            this.textBoxActualPDCS.Enabled = false;
            this.textBoxActualPDCS.Location = new System.Drawing.Point(302, 66);
            this.textBoxActualPDCS.Name = "textBoxActualPDCS";
            this.textBoxActualPDCS.Size = new System.Drawing.Size(100, 20);
            this.textBoxActualPDCS.TabIndex = 14;
            // 
            // labelActualPDCS
            // 
            this.labelActualPDCS.AutoSize = true;
            this.labelActualPDCS.Location = new System.Drawing.Point(409, 70);
            this.labelActualPDCS.Name = "labelActualPDCS";
            this.labelActualPDCS.Size = new System.Drawing.Size(69, 13);
            this.labelActualPDCS.TabIndex = 15;
            this.labelActualPDCS.Text = "Actual PDCS";
            // 
            // buttonCalculate
            // 
            this.buttonCalculate.Location = new System.Drawing.Point(647, 4);
            this.buttonCalculate.Name = "buttonCalculate";
            this.buttonCalculate.Size = new System.Drawing.Size(75, 23);
            this.buttonCalculate.TabIndex = 16;
            this.buttonCalculate.Text = "Calculate";
            this.buttonCalculate.UseVisualStyleBackColor = true;
            this.buttonCalculate.Click += new System.EventHandler(this.buttonCalculate_Click);
            // 
            // textBoxFractionO2
            // 
            this.textBoxFractionO2.Location = new System.Drawing.Point(13, 147);
            this.textBoxFractionO2.Name = "textBoxFractionO2";
            this.textBoxFractionO2.Size = new System.Drawing.Size(100, 20);
            this.textBoxFractionO2.TabIndex = 17;
            this.textBoxFractionO2.Text = "0.21";
            // 
            // labelFractionO2
            // 
            this.labelFractionO2.AutoSize = true;
            this.labelFractionO2.Location = new System.Drawing.Point(118, 151);
            this.labelFractionO2.Name = "labelFractionO2";
            this.labelFractionO2.Size = new System.Drawing.Size(62, 13);
            this.labelFractionO2.TabIndex = 18;
            this.labelFractionO2.Text = "Fraction O2";
            // 
            // textBoxExponent
            // 
            this.textBoxExponent.Location = new System.Drawing.Point(302, 93);
            this.textBoxExponent.Name = "textBoxExponent";
            this.textBoxExponent.Size = new System.Drawing.Size(100, 20);
            this.textBoxExponent.TabIndex = 19;
            this.textBoxExponent.Text = "4.0";
            // 
            // labelExponent
            // 
            this.labelExponent.AutoSize = true;
            this.labelExponent.Location = new System.Drawing.Point(409, 97);
            this.labelExponent.Name = "labelExponent";
            this.labelExponent.Size = new System.Drawing.Size(52, 13);
            this.labelExponent.TabIndex = 20;
            this.labelExponent.Text = "Exponent";
            // 
            // textBoxBreakFraction
            // 
            this.textBoxBreakFraction.Location = new System.Drawing.Point(302, 120);
            this.textBoxBreakFraction.Name = "textBoxBreakFraction";
            this.textBoxBreakFraction.Size = new System.Drawing.Size(100, 20);
            this.textBoxBreakFraction.TabIndex = 21;
            this.textBoxBreakFraction.Text = "0.8";
            // 
            // labelBreakFraction
            // 
            this.labelBreakFraction.AutoSize = true;
            this.labelBreakFraction.Location = new System.Drawing.Point(409, 124);
            this.labelBreakFraction.Name = "labelBreakFraction";
            this.labelBreakFraction.Size = new System.Drawing.Size(76, 13);
            this.labelBreakFraction.TabIndex = 22;
            this.labelBreakFraction.Text = "Break Fraction";
            // 
            // buttonIntersection
            // 
            this.buttonIntersection.Location = new System.Drawing.Point(647, 34);
            this.buttonIntersection.Name = "buttonIntersection";
            this.buttonIntersection.Size = new System.Drawing.Size(75, 23);
            this.buttonIntersection.TabIndex = 23;
            this.buttonIntersection.Text = "Intersection";
            this.buttonIntersection.UseVisualStyleBackColor = true;
            //this.buttonIntersection.Click += new System.EventHandler(this.buttonIntersection_Click);
            // 
            // checkBoxSaveData
            // 
            this.checkBoxSaveData.AutoSize = true;
            this.checkBoxSaveData.Location = new System.Drawing.Point(647, 97);
            this.checkBoxSaveData.Name = "checkBoxSaveData";
            this.checkBoxSaveData.Size = new System.Drawing.Size(77, 17);
            this.checkBoxSaveData.TabIndex = 24;
            this.checkBoxSaveData.Text = "Save Data";
            this.checkBoxSaveData.UseVisualStyleBackColor = true;
            this.checkBoxSaveData.CheckedChanged += new System.EventHandler(this.checkBoxSaveData_CheckedChanged);
            // 
            // buttonEvaluate
            // 
            this.buttonEvaluate.Location = new System.Drawing.Point(647, 64);
            this.buttonEvaluate.Name = "buttonEvaluate";
            this.buttonEvaluate.Size = new System.Drawing.Size(75, 23);
            this.buttonEvaluate.TabIndex = 25;
            this.buttonEvaluate.Text = "Evaluate";
            this.buttonEvaluate.UseVisualStyleBackColor = true;
            this.buttonEvaluate.Click += new System.EventHandler(this.buttonEvaluate_Click);
            // 
            // buttonBoundary
            // 
            this.buttonBoundary.Location = new System.Drawing.Point(647, 120);
            this.buttonBoundary.Name = "buttonBoundary";
            this.buttonBoundary.Size = new System.Drawing.Size(75, 23);
            this.buttonBoundary.TabIndex = 26;
            this.buttonBoundary.Text = "Boundary";
            this.buttonBoundary.UseVisualStyleBackColor = true;
            //this.buttonBoundary.Click += new System.EventHandler(this.buttonBoundary_Click);
            // 
            // ContOptimalDeco
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 463);
            this.Controls.Add(this.buttonBoundary);
            this.Controls.Add(this.buttonEvaluate);
            this.Controls.Add(this.checkBoxSaveData);
            this.Controls.Add(this.buttonIntersection);
            this.Controls.Add(this.labelBreakFraction);
            this.Controls.Add(this.textBoxBreakFraction);
            this.Controls.Add(this.labelExponent);
            this.Controls.Add(this.textBoxExponent);
            this.Controls.Add(this.labelFractionO2);
            this.Controls.Add(this.textBoxFractionO2);
            this.Controls.Add(this.buttonCalculate);
            this.Controls.Add(this.labelActualPDCS);
            this.Controls.Add(this.textBoxActualPDCS);
            //this.Controls.Add(this.labelClearTlime);
            this.Controls.Add(this.textBoxClearTime);
            this.Controls.Add(this.labelTargetPDCS);
            this.Controls.Add(this.textBoxTargetPDCS);
            this.Controls.Add(this.labelSurfaceTime);
            this.Controls.Add(this.textBoxSurfaceTime);
            this.Controls.Add(this.labelMaxAscentRate);
            this.Controls.Add(this.textBoxMaxAscentRate);
            this.Controls.Add(this.labelBottomTime);
            this.Controls.Add(this.textBoxBottomTime);
            this.Controls.Add(this.labelDescentRate);
            this.Controls.Add(this.textBoxDescentRate);
            this.Controls.Add(this.labelMaxDepth);
            this.Controls.Add(this.textBoxMaxDepth);
            this.Name = "ContOptimalDeco";
            this.Text = "Optimal Ascent Planner";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxMaxDepth;
        private System.Windows.Forms.Label labelMaxDepth;
        private System.Windows.Forms.TextBox textBoxDescentRate;
        private System.Windows.Forms.Label labelDescentRate;
        private System.Windows.Forms.TextBox textBoxBottomTime;
        private System.Windows.Forms.Label labelBottomTime;
        private System.Windows.Forms.TextBox textBoxMaxAscentRate;
        private System.Windows.Forms.Label labelMaxAscentRate;
        private System.Windows.Forms.TextBox textBoxSurfaceTime;
        private System.Windows.Forms.Label labelSurfaceTime;
        private System.Windows.Forms.TextBox textBoxTargetPDCS;
        private System.Windows.Forms.Label labelTargetPDCS;
        private System.Windows.Forms.TextBox textBoxClearTime;
        private System.Windows.Forms.Label labelClearTime;
        private System.Windows.Forms.TextBox textBoxActualPDCS;
        private System.Windows.Forms.Label labelActualPDCS;
        private System.Windows.Forms.Button buttonCalculate;
        private System.Windows.Forms.TextBox textBoxFractionO2;
        private System.Windows.Forms.Label labelFractionO2;
        private System.Windows.Forms.TextBox textBoxExponent;
        private System.Windows.Forms.Label labelExponent;
        private System.Windows.Forms.TextBox textBoxBreakFraction;
        private System.Windows.Forms.Label labelBreakFraction;
        private System.Windows.Forms.Button buttonIntersection;
        private System.Windows.Forms.CheckBox checkBoxSaveData;
        private System.Windows.Forms.Button buttonEvaluate;
        private System.Windows.Forms.Button buttonBoundary;
    }
}

