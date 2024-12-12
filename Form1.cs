using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.pdf.parser;
using Org.BouncyCastle.Utilities.Encoders;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Drawing.Text;


namespace DES
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //-----Objects-------------

        private PdfFiles pdfhandler = new PdfFiles();
        private key round_key = new key();
        private DEScs des= new DEScs();
        string file_path = string.Empty;

        //-----------choose file----------------
        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                file_path = openFileDialog1.FileName;
            }
            else
            {
                MessageBox.Show("No file selected. Please select a file.");
                return;
            }
        }
        //***************************** ENCRYPTION ****************************************************************************
        private void button1_Click(object sender, EventArgs e)
        {
            //-----------Key method-------------------
          
            string[] roundKeys = round_key.GenerateKeyRound();

            //-----------Data method-------------------
           
            string data = pdfhandler.ExtractTextFromPdf(file_path);
           List<string> encrypted = new List<string>();
            encrypted = des.EncryptData(data, roundKeys);      
                string new_path = null;
            new_path = pdfhandler.SelectFolder();
            PdfFiles.GeneratePdfTesting(new_path, encrypted);

        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }
        private void label1_Click(object sender, EventArgs e)
        {
        }
        
        //*************************************** DECRYPTION *********************************************************************************
        private void button2_Click(object sender, EventArgs e)
        {

            //-----------Key method-------------------

            string[] roundKeys = round_key.GenerateKeyRound();

            //-----------Data method-------------------

            string data = pdfhandler.ExtractTextFromPdf(file_path);
            List<string> decrypted = new List<string>();
            decrypted = des.Decrypt(data, roundKeys);
            string new_path = null;
            new_path = pdfhandler.SelectFolder();
            PdfFiles.GeneratePdfTesting(new_path, decrypted);
        }
      

    }
} 