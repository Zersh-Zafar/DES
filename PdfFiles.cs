using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting.Channels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Org.BouncyCastle.Utilities.Encoders;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace DES
{
    internal class PdfFiles
    {
        public string ExtractTextFromPdf(string file_path)
        {

            StringBuilder sb = new StringBuilder();
            string file = file_path;

            try
            {
                using (PdfReader reader = new PdfReader(file))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string text = PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                        sb.Append(text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading PDF: " + ex.Message);
                return null;
            }
            return sb.ToString();
        }

        public static void GeneratePdfTesting(string appPhysicalPath, List<string> asciiOutput)
        {
            string filePath = null;
           

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.DefaultExt = "pdf";
                saveFileDialog.AddExtension = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = saveFileDialog.FileName;
                }
                else
                {
                    filePath = appPhysicalPath + "\\" + DateTime.Now.Ticks.ToString() + ".pdf";
                }
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                var doc1 = new Document();
                var streamObj = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
                PdfWriter.GetInstance(doc1, streamObj);
                doc1.Open();
                string combinedOutput = string.Join("", asciiOutput);

                doc1.Add(new Paragraph(combinedOutput));
                doc1.Close();
            }
        }
        public string SelectFolder()
        {
            string selectedPath = null;

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedPath = folderBrowserDialog.SelectedPath;
                }
                else
                {
                    MessageBox.Show("No folder selected. Please select a folder.");
                }
            }

            return selectedPath;
        }

    }
}
