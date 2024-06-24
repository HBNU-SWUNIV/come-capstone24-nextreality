using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Windows.Forms;
using Ookii.Dialogs;
using TMPro;
using Button = UnityEngine.UI.Button;
using NextReality;
using NextReality.Asset;
using NextReality.Networking.Request;
using System;
using NextReality.Networking.Response;
using NextReality.Asset.UI;
using NextReality.Data;

namespace NextReality.Game.UI
{
	public class UploadPopup : MainMenuPopup
	{

        HttpRequests httpRequests;
        MessageSetter messageSetter;
        UserManager userManager;

        // upload 창 내부의 요소들
        public Button closeBtn;

		public TMP_InputField nameField;
		public AssetCategoryDropBoxContainer categoryField;
		public TMP_InputField priceField;

        public Button imgUploadBtn;
        public Button modelUploadBtn;

		public TMP_Text imgUrlTxt;
        public TMP_Text modelUrlTxt;

        public Toggle publicToggle;

        public Image imagePreview;
		// model preview 필요

        public TMP_Text stateTxt;

		public TextAsset categoryJson;


        // 외부 요소들
        VistaOpenFileDialog dialog;
		Stream openStream = null;

		string imageExt;
		byte[] imageFile;
		byte[] modelFile;

		public ModelPreviewer previewer;
		public ModelPreviewPanel previewPanel;

		// Start is called before the first frame update
		void Start()
		{
			dialog = new VistaOpenFileDialog();

			categoryField.RequireCategoryList(categoryJson.text);
            httpRequests = Utilities.HttpUtil;
            messageSetter = Utilities.MessageUtil;
            userManager = Managers.User;

			previewPanel.previewer = previewer;


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
			// 초기화 과정
			this.gameObject.SetActive(false);
            messageSetter.ResetText(stateTxt);
			nameField.text = string.Empty;
			categoryField.Clear();
			priceField.text = string.Empty;
			imgUrlTxt.text = string.Empty;
			modelUrlTxt.text = string.Empty;
			imagePreview.sprite = null;
			imageExt = string.Empty;
			imageFile = null;
			modelFile = null;
			publicToggle.isOn = true;

			previewer.ClearPreview();
		}

		public void OnClickImgUpload()
		{
			string imagePath = OpenFile((int)FileType.Image);

			if (imagePath != null)
			{
                imageExt = imagePath.Substring(imagePath.Length - 3).ToLower();
                Debug.Log("Image File Path : " + imagePath);
                Debug.Log("Image File Extension : " + imageExt);

                imgUrlTxt.SetText(imagePath);
				imgUrlTxt.gameObject.SetActive(true);
				imageFile = loadFile(imagePath);
				Texture2D texture = new Texture2D(0, 0);
				texture.LoadImage(imageFile);
				Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

				float newHeight = (float)texture.height / (float)texture.width*imagePreview.rectTransform.rect.width;

				Debug.Log("[UploadPopUp] " + texture.width + "/" + texture.height + "/" + imagePreview.rectTransform.rect.width + "/" + imagePreview.rectTransform.sizeDelta.x+"/"+newHeight);

				imagePreview.sprite = sprite;
				imagePreview.rectTransform.sizeDelta = new Vector2(imagePreview.rectTransform.sizeDelta.x, newHeight);
			}
			else
			{
				imgUrlTxt.SetText((string)null);
				imgUrlTxt.gameObject.SetActive(false);
			}

		}

		public void OnClickModelUpload()
		{

			string modelPath = OpenFile((int)FileType.Model);

			if (modelPath != null)
			{
                Debug.Log("Model File Path : " + modelPath);
				modelUrlTxt.SetText(modelPath);
				modelUrlTxt.gameObject.SetActive(true);
				modelFile = loadFile(modelPath);

				previewer.InstantiatePreviewObject(modelFile);

			}
			else
			{
				modelUrlTxt.SetText((string)null);
				modelUrlTxt.gameObject.SetActive(false);
			}

		}

		// 파일 경로 읽어서 byte 읽어오는 함수 필요.
		// 8일 제작 예정

		private byte[] loadFile(string filePath)
		{
			byte[] data = File.ReadAllBytes(filePath);
			return data;
		}

		enum FileType
		{
			Image,
			Model
		}


		private string OpenFile(int fileType)
		{
			switch(fileType)
			{
                case (int)FileType.Image:
                    dialog.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png";
                    dialog.Title = "Select Image File";
					break;

				case (int)FileType.Model:
                    dialog.Filter = "glb files (*.glb)|*.glb";
                    dialog.Title = "Select 3D Model File";
					break;
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

		public void OnCliCkUpload()
		{
			if (nameField.text == string.Empty)
			{
                messageSetter.SetText(stateTxt, "Name is required!", Color.red);
				return;
			}
			if (categoryField.SelectedCategory == null)
			{
				messageSetter.SetText(stateTxt, "Category is required!", Color.red);
				return;
			}
			if (imageFile == null)
			{
                messageSetter.SetText(stateTxt, "Thumbnail is required!", Color.red);
                return;
            }
			if (modelFile == null)
			{
				messageSetter.SetText(stateTxt, "Model File is required!", Color.red);
                return;
            }
			if (!previewer.IsAppropriateModel)
			{
				messageSetter.SetText(stateTxt, "Model is out of specifications!", Color.red);
				return;
			}



			AssetUpload assetUpload = new AssetUpload();

			assetUpload.name = nameField.text;
			assetUpload.categoryid = categoryField.SelectedCategory?.id ?? 0;

			// assetUpload.thumbnail = imageFile; (byte array 성능 저하 심함)
			// Base64로 해도 1.36배 파일이 커진 것을 확인할 수 있었음
			assetUpload.thumbnail = Convert.ToBase64String(imageFile);
            // assetUpload.thumbnail = null;
            // assetUpload.thumbnail = "testing";
            assetUpload.thumbnailext = imageExt;

            assetUpload.file = Convert.ToBase64String(modelFile);
            //assetUpload.file = null;
            //assetUpload.file = "testing2";

            // assetUpload.file = modelFile;
            // assetUpload.uploaddate = DateTime.Now.ToString("yyyy-MM-dd");
			// assetUpload.uploaddate = DateTime.Now.Ticks;
			assetUpload.downloadcount = 0;
			assetUpload.price = (priceField.text != string.Empty) ? int.Parse(priceField.text) : 0;
			assetUpload.isdisable = !publicToggle.isOn;

			string assetUploadJson = JsonUtility.ToJson(assetUpload);

			// 로컬 저장 확인용
			string localPath = Path.Combine(UnityEngine.Application.persistentDataPath, "test.json"); 
			File.WriteAllText(localPath, assetUploadJson);

            Debug.Log(assetUploadJson);

			messageSetter.SetText(stateTxt, "Uploading...", Color.grey);

			StartCoroutine(httpRequests.RequestPost
				(httpRequests.GetServerUrl
				(HttpRequests.ServerEndpoints.AssetUpload), assetUploadJson, (callback) =>
			{
				Debug.Log("RequestPost Callback : " + callback);

				if (callback == null)
				{
					messageSetter.SetText(stateTxt, "Upload Failed", Color.red);
				}
				else
				{
					try
					{
						ResponseData responseData = JsonUtility.FromJson<ResponseData>(callback);
						if (callback != null)
						{
							if (Utilities.HttpUtil.CheckResult(callback))
							{
								Debug.Log(responseData.message);
								messageSetter.SetText(stateTxt, "Upload Success!", Color.green);
							}
							else
							{
                                Debug.Log(responseData.message);
                                messageSetter.SetText(stateTxt, "Upload Failed", Color.red);
                            }
						}
					}
					catch(Exception e)
					{
						Debug.Log("UploadPopup.RequestPost Error : " + e.ToString());
                        messageSetter.SetText(stateTxt, "Upload Failed", Color.red);
                    }
				}
			}));

        }

	}

}
