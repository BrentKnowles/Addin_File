using System;
using CoreUtilities;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using CoreUtilities.Links;
using System.Data;
using Layout;
using System.IO;



namespace ADD_Facts
{
	public class NoteDataXML_File:  NoteDataXML_RichText
	{
		#region constants
		
		//	public const string NotUsed = "Modifier";
#endregion
		#region interface
		Panel BottomInfo;
		Label LastUpdateDate;
		Label LastExportDate;
		Label FileShown;
#endregion
		
		#region properties


		private string linkedFile=Constants.BLANK;

		public string LinkedFile {
			get {
				return linkedFile;
			}
			set {
				linkedFile = value;
			}
		}

		private DateTime lastExport;

		public DateTime LastExport {
			get {
				return lastExport;
			}
			set {
				lastExport = value;
			}
		}

		private DateTime lastRefresh;

		public DateTime LastRefresh {
			get {
				return lastRefresh;
			}
			set {
				lastRefresh = value;
			}
		}

#endregion
		
		
		public NoteDataXML_File () : base()
		{
			CommonConstructorBehavior ();
		}
		public NoteDataXML_File(int height, int width):base(height, width)
		{
			CommonConstructorBehavior ();
			
		}
		public NoteDataXML_File(NoteDataInterface Note) : base(Note)
		{
			//this.Notelink = ((NoteDataXML_Checklist)Note).Notelink;
		}
		protected override void CommonConstructorBehavior ()
		{
			base.CommonConstructorBehavior ();
			Caption = Loc.Instance.GetString("File");
			
		}
		
		/// <summary>
		/// Registers the type.
		/// </summary>
		public override string RegisterType()
		{
			return Loc.Instance.GetString("File");
		}

		string ImportTime ()
		{
			return Loc.Instance.GetStringFmt("Last Import: {0}", lastRefresh.ToString ());
		}		

		string ExportTime()
		{
			return Loc.Instance.GetStringFmt("Last Export: {0}", lastExport.ToString ());
		}
		protected override void DoBuildChildren (LayoutPanelBase Layout)
		{
			base.DoBuildChildren (Layout);
			try {
				BottomInfo = new Panel();
				BottomInfo.Dock = DockStyle.Bottom;
				BottomInfo.Height = 75;
				
				ParentNotePanel.Controls.Add (BottomInfo);
				
				
				ToolStripMenuItem SelectItem = new ToolStripMenuItem();
				SelectItem.Text = Loc.Instance.GetString ("Select File");
				SelectItem.Click+= HandleSelectFile;

				ToolStripMenuItem RefreshItem = new ToolStripMenuItem();
				RefreshItem.Text = Loc.Instance.GetString ("Get From File");
				RefreshItem.Click+= HandleRefreshClick;

				ToolStripMenuItem SendTo = new ToolStripMenuItem();
				SendTo.Text = Loc.Instance.GetString ("Send To File");
				SendTo.Click += HandleSendToClick;

				LastUpdateDate = new Label();
				LastUpdateDate.Text = ImportTime();//lastRefresh.ToString ();
				LastUpdateDate.Dock = DockStyle.Top;
				LastUpdateDate.Click+= (object sender, EventArgs e) => HandleRefresh();

				LastExportDate = new Label();
				LastExportDate.Text = ExportTime ();
				LastExportDate.Dock = DockStyle.Top;
				LastExportDate.Click+= (object sender, EventArgs e) =>  this.SendTo();

				FileShown = new Label();
				FileShown.Click+= HandleFileShownClick;
				//FileShown.Text = LinkedFile;
				FileShown.Dock = DockStyle.Top;

				// not actually changing the name but updated the label with a shortened version of it
				ChangeName(LinkedFile);

				BottomInfo.Controls.Add (LastUpdateDate);
				BottomInfo.Controls.Add (LastExportDate);
				BottomInfo.Controls.Add (FileShown);

				properties.DropDownItems.Add (new ToolStripSeparator());
				properties.DropDownItems.Add (SelectItem);
				properties.DropDownItems.Add(RefreshItem);
				properties.DropDownItems.Add(SendTo);
			} catch (Exception ex) {
				NewMessage.Show (ex.ToString ());
			}
			
			
			
		}

		void HandleFileShownClick (object sender, EventArgs e)
		{
			ShowFileInFolder ();
		}

		void ShowFileInFolder()
		{
			string argument =String.Format (@"/select, {0}", LinkedFile);
			System.Diagnostics.Process.Start ("explorer.exe", argument);
		}
		void HandleSendToClick (object sender, EventArgs e)
		{
			SendTo();
		}

		void HandleRefreshClick (object sender, EventArgs e)
		{
			HandleRefresh ();
		}

		void HandleRefresh ()
		{
			if (File.Exists (LinkedFile)) {
				// loads the file
				// this will be called from manual refresh press and after open a file

				if (true)
				{

					bool proceed = true;
					if (richBox.Text != Constants.BLANK)
					{
						proceed = false;
						if (NewMessage.Show (Loc.Instance.GetString ("Override existing?"), "You have text already here", MessageBoxButtons.YesNo,null)==DialogResult.Yes)
						{
							proceed = true;
						}

					}
					if (true == proceed)
					{
						LastRefresh = DateTime.Now;
						LastUpdateDate.Text = ImportTime();//lastRefresh.ToString ();
						richBox.LoadFile(LinkedFile, RichTextBoxStreamType.PlainText);
					}
				}
			}

		}
		void ChangeName (string newName)
		{
			LinkedFile = newName;
			if (LinkedFile != Constants.BLANK) {
				FileShown.Text = new FileInfo (LinkedFile).Name;
			}
		}
		void HandleSelectFile (object sender, EventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();
			open.DefaultExt = "txt";
			open.Filter ="Text|*.txt";

			if (open.ShowDialog() == DialogResult.OK)
			{
				ChangeName (open.FileName);
			
				HandleRefresh();
			}
		}
		

		
		protected override void DoChildAppearance (AppearanceClass app)
		{
			base.DoChildAppearance (app);
			
			
		}
		
		
		
		
		private void SendTo()
		{
			// I originally piggybacked onto the save routine, but
			// this was confusing. I th ink its better if this is an explicit action.
			if (LinkedFile != Constants.BLANK) {
				FileInfo f = new FileInfo(LinkedFile);
				
				if (f.Directory.Exists)
				{

					// August 2013
					// LOGIC UPDATE: Check the date-stamp of the file in Dropbox
					//    if that file is NEWER than last "IMPORT" date
					//       means: dropbox file 'might' be newest
					//   HENCE: Prompt
					bool DoWePrompt = false;
					FileInfo LinkedFileDetails = new FileInfo(LinkedFile);
					if (DateTime.Compare ( LinkedFileDetails.LastWriteTime, this.lastRefresh) >0 )
					{
						DoWePrompt = true;
					}

					if (false == DoWePrompt || NewMessage.Show (Loc.Instance.GetString ("External File is NEWER!"), "Do you want to override the text in the file in the folder with this note?", MessageBoxButtons.YesNo,null)==DialogResult.Yes)
					{
					//	NewMessage.Show ("Saving " + LinkedFile);

						// http://msdn.microsoft.com/en-us/library/cc488002(v=vs.90).aspx August 6 2013
						// Trying to: Save as  UTF-8 text instead
						string plainText = richBox.Text;
						System.IO.File.WriteAllText(LinkedFile, plainText);

						//richBox.SaveFile(LinkedFile, RichTextBoxStreamType.PlainText);
						lastExport = DateTime.Now;
						LastExportDate.Text = ExportTime();

						// We also update the IMPORT date, because at this point, the External file has taken precendence over any PAST imports.
						lastRefresh = lastExport;
						LastUpdateDate.Text = this.ImportTime();
					}
				}
			}
		}
	
		protected override AppearanceClass UpdateAppearance ()
		{
			AppearanceClass app = base.UpdateAppearance ();
			if (BottomInfo != null) {
				BottomInfo.BackColor = app.mainBackground;
				BottomInfo.ForeColor = app.secondaryForeground;
			}

			return app;
		}
		
		public override void Save ()
		{
			
			
			
			
			base.Save ();


			
		}
		
	}
}


