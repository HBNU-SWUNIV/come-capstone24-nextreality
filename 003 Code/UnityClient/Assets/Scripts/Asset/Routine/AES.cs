using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace NextReality.Asset.Routine
{
	public class AES : MonoBehaviour
	{
		private static AES instance = null;
		public static AES Instance { get { return instance; } }

		private byte[] aesKey;
		private byte[] aesIv;

		private void Awake()
		{
			if (AES.instance == null)
			{
				AES.instance = this;
			}
			else
			{
				Destroy(AES.instance.gameObject);
			}
		}

		private void OnDestroy()
		{
			if (AES.instance == this)
			{
				AES.instance = null;
			}
		}

		private void SetKeyIV()
		{
			byte[] usernameBytes = Encoding.UTF8.GetBytes(Managers.User.Id);
			aesKey = GenerateAes256Key(usernameBytes);

			aesIv = aesKey[..16];
		}

		public static byte[] GenerateAes256Key(byte[] inputBytes)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(inputBytes); // 입력 바이트 배열을 해시하여 AES-256 비트 키 생성
			}
		}

		public byte[] EncryptByteArray(byte[] inputBytes)
		{
			if (aesKey == null || aesKey.Length <= 0)
				SetKeyIV();
			if (aesIv == null || aesIv.Length <= 0)
				SetKeyIV();

			byte[] encryptedData = EncryptByteArrayToBytes_Aes(inputBytes, aesKey, aesIv);

			return encryptedData;
		}

		private byte[] EncryptByteArrayToBytes_Aes(byte[] data, byte[] Key, byte[] IV)
		{
			if (data == null || data.Length <= 0)
				throw new ArgumentNullException(nameof(data));
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException(nameof(Key));
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException(nameof(IV));

			byte[] encrypted;

			// AES 객체 생성 및 설정
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = Key;
				aesAlg.IV = IV;

				// 암호화기를 생성
				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				// 메모리 스트림과 CryptoStream을 사용하여 데이터를 암호화
				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						csEncrypt.Write(data, 0, data.Length);
						csEncrypt.FlushFinalBlock();
						encrypted = msEncrypt.ToArray();
					}
				}
			}

			// 암호화된 데이터를 반환
			return encrypted;
		}

		public byte[] DecryptByteArray(byte[] inputBytes)
		{
			if (aesKey == null || aesKey.Length <= 0)
				SetKeyIV();
			if (aesIv == null || aesIv.Length <= 0)
				SetKeyIV();

			byte[] decryptedData = DecryptByteArrayFromBytes_Aes(inputBytes, aesKey, aesIv);

			return decryptedData;
		}

		private byte[] DecryptByteArrayFromBytes_Aes(byte[] cipherData, byte[] Key, byte[] IV)
		{
			if (cipherData == null || cipherData.Length <= 0)
				throw new ArgumentNullException(nameof(cipherData));
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException(nameof(Key));
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException(nameof(IV));

			byte[] decrypted;

			// AES 객체 생성 및 설정
			using (Aes aesAlg = Aes.Create())
			{
				aesAlg.Key = Key;
				aesAlg.IV = IV;

				// 복호화기를 생성
				ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

				// 메모리 스트림과 CryptoStream을 사용하여 데이터를 복호화
				using (MemoryStream msDecrypt = new MemoryStream(cipherData))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (MemoryStream msPlain = new MemoryStream())
						{
							csDecrypt.CopyTo(msPlain);
							decrypted = msPlain.ToArray();
						}
					}
				}
			}

			// 복호화된 데이터를 반환
			return decrypted;
		}
	}
}