namespace ClientSimpleSoft
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _output= new RichTextBox() ;
            SuspendLayout();
            // 
            // _output
            // 
            _output.Dock= DockStyle.Fill ;
            _output.Location= new Point( 0, 0 ) ;
            _output.Name= "_output" ;
            _output.Size= new Size( 800, 450 ) ;
            _output.TabIndex= 0 ;
            _output.Text= "" ;
            // 
            // Form1
            // 
            AutoScaleDimensions= new SizeF( 10F, 25F ) ;
            AutoScaleMode= AutoScaleMode.Font ;
            ClientSize= new Size( 800, 450 ) ;
            Controls.Add( _output );
            Name= "Form1" ;
            Text= "Синхронизация" ;
            ResumeLayout( false );
        }

        #endregion

        private RichTextBox _output;
    }
}