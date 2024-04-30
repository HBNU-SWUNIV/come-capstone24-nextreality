using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Windows.Forms;
using Ookii.Dialogs;
using TMPro;

namespace NextReality.Game.UI
{
	public class UploadPopup : MonoBehaviour
	{
		public UnityEngine.UI.Button closeBtn;

		public UnityEngine.UI.Button imgUploadBtn;
		public UnityEngine.UI.Button modelUploadBtn;

		public GameSceneCanvas gameSceneCanvas;

		public TMP_Text stateTxt;

		VistaOpenFileDialog dialog;
		Stream openStream = null;

		Texture2D image;
		byte[] modelFile;


		// Start is called before the first frame update
		void Start()
		{
			dialog = new VistaOpenFileDialog();
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void Open()
		{
			this.gameObject.SetActive(true);
		}

		public void Close()
		{
			this.gameObject.SetActive(false);
		}

		public void OnClickImgUpload()
		{
			if (OpenFile((int)FileType.Image) != null)
			{

			}

		}

		public void OnClickModelUpload()
		{
			if (OpenFile((int)FileType.Model) != null)
			{

			}

		}


		enum FileType
		{
			Image,
			Model
		}


		private string OpenFile(int fileType)
		{

			if (fileType == (int)FileType.Image)
			{
				dialog.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png";
				dialog.Title = "Select Image File";

			}
			else if (fileType == (int)FileType.Model)
			{
				dialog.Filter = "glb files (*.glb)|*.glb";
				dialog.Title = "Select 3D Model File";
			}


			dialog.FilterIndex = 0;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				if ((openStream = dialog.OpenFile()) != null)
				{
					return dialog.FileName;
				}
			}

			return null;
		}



	}

}
