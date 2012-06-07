using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HebMorph.CorpusReaders;
using HebMorph.CorpusReaders.Common;
using HebMorph.CorpusReaders.Wikipedia;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using LuceneNeatThings.Core;
using Directory = System.IO.Directory;

namespace IndexBuilder
{
	public partial class Form1 : Form
	{
		private WikiDumpReader cr;
		private IndexWriter writer;

		/// <summary>
		/// Creates the index in the specified path, using the corpusReader object
		/// as the documents feed
		/// </summary>
		/// <param name="corpusReader"></param>
		/// <param name="indexPath"></param>
		public void CreateIndex(WikiDumpReader corpusReader, string indexPath)
		{
			cr = corpusReader;

			var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);

			writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(indexPath)), analyzer, true,
										 IndexWriter.MaxFieldLength.UNLIMITED);
			writer.SetUseCompoundFile(false);

			// This will be called whenever a document is read by the provided ICorpusReader
			corpusReader.OnDocument += corpusDoc =>
			{
				if (corpusReader.AbortReading)
					return;

				// Blaaaah that's ugly. Make sure parsing doesn't stick us in an infinite loop
				var t = Task.Factory.StartNew(() => corpusDoc.AsHtml());
				var timeout = t.Wait(TimeSpan.FromMinutes(2));
				var content = timeout ? t.Result : string.Empty;

				// skip blank documents, they are worthless to us (even though they have a title we could index)
				if (string.IsNullOrEmpty(content))
					return;

				// Create a new index document
				var doc = new Document();
				doc.Add(new Field("Id", corpusDoc.Id, Field.Store.YES,
					Field.Index.NOT_ANALYZED_NO_NORMS));

				// Add title field
				var titleField = new Field("Title", corpusDoc.Title, Field.Store.YES,
					Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
				titleField.SetBoost(3.0f);
				doc.Add(titleField);

				doc.Add(new Field("Content", content, Field.Store.COMPRESS,
					Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));

				writer.AddDocument(doc);
			};

			// Progress reporting
			corpusReader.OnProgress += (percentage, status, isRunning) =>
			{
				var pi = new ProgressInfo { IsStillRunning = true, Status = string.Format("{0} ({1}%)", status, percentage) };
				Invoke(new ProgressChangedDelegate(UpdateProgress), null, new ProgressChangedEventArgs(percentage, pi));
			};

			// Execute corpus reading, which will trigger indexing for each document found
			corpusReader.Read();
			cr = null;

			// Clean up and close
			writer.SetUseCompoundFile(true);
			writer.Optimize();
			writer.Close();
			writer = null;

			var pi1 = new ProgressInfo { IsStillRunning = false, Status = "Ready" };
			Invoke(new ProgressChangedDelegate(UpdateProgress), null, new ProgressChangedEventArgs(100, pi1));
		}

		#region Boring UI stuff

		public Form1()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the ProgressChanged event from indexers
		/// </summary>
		/// <param name="sender">Indexer</param>
		/// <param name="e">Progress event</param>
		private delegate void ProgressChangedDelegate(object sender, ProgressChangedEventArgs e);

		private Thread workerThread;

		private void btnExecute_Click(object sender, EventArgs e)
		{
			if (workerThread != null)
			{
				btnExecute.Enabled = false;

				if (cr != null)
				{
					Task.Factory.StartNew(() =>
					{
						Thread.Sleep(TimeSpan.FromMinutes(10));
						
						if (writer != null)
						{
							writer.Close();
							writer = null;
						}
					}); // fail safe

					cr.AbortReading = true;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(txbCorpusPath.Text))
				{
					MessageBox.Show("Valid path is required");
					return;
				}

				if (!(Directory.Exists(txbCorpusPath.Text) || File.Exists(txbCorpusPath.Text)))
				{
					MessageBox.Show("Valid path is required");
					return;
				}

				var indexPath = Path.Combine(Path.GetDirectoryName(txbCorpusPath.Text), "idx2");

				panel1.Enabled = false;
				lblStatus.Show();
				progressBar1.Show();
				btnExecute.Text = "Stop";
				workerThread = new Thread(delegate()
				{
					var wikiDumpReader = new WikiDumpReader(txbCorpusPath.Text);
					CreateIndex(wikiDumpReader, indexPath);
				});
				workerThread.Start();
			}
		}

		private void UpdateProgress(object sender, ProgressChangedEventArgs e)
		{
			var pi = (ProgressInfo)e.UserState;
			if (!pi.IsStillRunning)
			{
				workerThread.Abort();
				workerThread = null;

				panel1.Enabled = true;
				progressBar1.Hide();
				lblStatus.Hide();
				btnExecute.Text = "Start";
				btnExecute.Enabled = true;

				return;
			}

			progressBar1.Value = e.ProgressPercentage;
			if (!string.IsNullOrEmpty(pi.Status))
				lblStatus.Text = pi.Status;
		}

		private void btnSelectCorpusWikiDump_Click(object sender, EventArgs e)
		{
			var ofd = new OpenFileDialog();

			DialogResult dr = ofd.ShowDialog();
			if (dr == DialogResult.OK && !string.IsNullOrEmpty(ofd.FileName))
			{
				txbCorpusPath.Text = ofd.FileName;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (workerThread != null && workerThread.IsAlive)
			{
				workerThread.Join();
			}

			base.OnClosing(e);
		}

		private bool paused;
		private void button1_Click(object sender, EventArgs e)
		{
			if (paused)
			{
				cr.SuspendEvent.Set();
				paused = false;
			}
			else
			{
				paused = true;
				cr.SuspendEvent.Reset();
			}
		}

		#endregion
	}
}
